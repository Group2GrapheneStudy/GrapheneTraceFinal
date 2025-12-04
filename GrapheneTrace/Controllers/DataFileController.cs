using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
using GrapheneTrace.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Admin", "Clinician")]
    public class DataFileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IPressureAnalysisService _analysis;

        public DataFileController(
            AppDbContext context,
            IWebHostEnvironment env,
            IPressureAnalysisService analysis)
        {
            _context = context;
            _env = env;
            _analysis = analysis;
        }

        // -----------------------------
        // GET: Upload CSV for patient
        // -----------------------------
        [HttpGet]
        public async Task<IActionResult> Upload(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound();

            ViewBag.PatientId = patient.PatientId;
            ViewBag.PatientEmail = patient.UserAccount?.Email ?? "Unknown";

            return View();
        }

        // -----------------------------
        // POST: Upload CSV for patient
        // -----------------------------
        [HttpPost]
        public async Task<IActionResult> Upload(int patientId, IFormFile csvFile, string? notes)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a CSV file.");
            }

            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = patient.PatientId;
                ViewBag.PatientEmail = patient.UserAccount?.Email ?? "Unknown";
                return View();
            }

            // Ensure HeatData folder exists
            var folder = Path.Combine(_env.ContentRootPath, "HeatData");
            Directory.CreateDirectory(folder);

            // Save CSV with a unique name
            var fileName = $"{patient.PatientId}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = System.IO.File.Create(fullPath))
            {
                await csvFile.CopyToAsync(stream);
            }

            var uploaderId = HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;

            var dataFile = new DataFile
            {
                PatientId = patient.PatientId,
                UploadedByUserId = uploaderId,
                UploadedAt = DateTime.UtcNow,
                FilePath = fullPath,
                Notes = notes
            };

            _context.DataFiles.Add(dataFile);
            await _context.SaveChangesAsync();

            // Process the CSV into PressureFrame records
            await ProcessCsvAsync(dataFile);

            TempData["Message"] = "CSV uploaded and processed successfully.";
            return RedirectToAction("Index", "Admin");
        }

        // -------------------------------------------------
        // Helper: read CSV and generate PressureFrame rows
        //         + create alerts for high peak pressure
        // -------------------------------------------------
        private async Task ProcessCsvAsync(DataFile file)
        {
            // Temporarily keep this super low so you can SEE alerts.
            // After you confirm it's working, set it to something sensible.
            const decimal PeakPressureAlertThreshold = 120m;

            string[] lines = await System.IO.File.ReadAllLinesAsync(file.FilePath);

            if (lines.Length % 32 != 0)
                throw new InvalidOperationException("CSV does not contain a whole number of 32-row frames.");

            int frameCount = lines.Length / 32;
            var baseTime = DateTime.UtcNow;
            var frames = new List<PressureFrame>();

            // 1) Build PressureFrame objects from the CSV
            for (int i = 0; i < frameCount; i++)
            {
                int[,] matrix = new int[32, 32];
                int start = i * 32;

                for (int r = 0; r < 32; r++)
                {
                    var cells = lines[start + r].Split(',', StringSplitOptions.RemoveEmptyEntries);
                    for (int c = 0; c < 32; c++)
                    {
                        matrix[r, c] = int.Parse(cells[c]);
                    }
                }

                var peak = _analysis.CalculatePeakPressure(matrix);
                var avg = _analysis.CalculateAveragePressure(matrix);
                var area = _analysis.CalculateContactAreaPercent(matrix);
                var risk = _analysis.CalculateRiskScore(peak, area);

                frames.Add(new PressureFrame
                {
                    DataFileId = file.DataFileId,
                    FrameIndex = i,
                    CapturedAtUtc = baseTime.AddSeconds(i),
                    PeakPressure = peak,
                    AveragePressure = avg,
                    ContactAreaPercent = area,
                    RiskScore = risk
                });
            }

            // 2) Save frames so they get FrameId values
            _context.PressureFrames.AddRange(frames);
            await _context.SaveChangesAsync();

            // 3) Create alerts for frames above the threshold
            var alerts = new List<Alert>();

            foreach (var frame in frames)
            {
                if (frame.PeakPressure > PeakPressureAlertThreshold)
                {
                    alerts.Add(new Alert
                    {
                        PatientId = file.PatientId,
                        FrameId = frame.FrameId,
                        TriggeredAt = frame.CapturedAtUtc,
                        Status = "Open",
                        Severity = "High",
                        Message = $"Peak pressure {frame.PeakPressure} exceeded threshold {PeakPressureAlertThreshold}.",
                        RaisedByUserId = file.UploadedByUserId
                    });
                }
            }

            if (alerts.Any())
            {
                _context.Alerts.AddRange(alerts);
                await _context.SaveChangesAsync();
            }
        }
    }
}




