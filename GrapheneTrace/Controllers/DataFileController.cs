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

            // Fetch the patient record
            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound();

            // Pass values to the view using ViewBag
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

            // Validate that a CSV file was uploaded
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a CSV file.");
            }


            // Load the patient along with the related UserAccount
            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            // If the patient ID does not exist
            if (patient == null)
                return NotFound();

            // If validation failed
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


            // Create the file and write the uploaded CSV content to it
            using (var stream = System.IO.File.Create(fullPath))
            {
                await csvFile.CopyToAsync(stream);
            }

            // Get the ID of the user
            var uploaderId = HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;


            // Create a record
            var dataFile = new DataFile
            {
                PatientId = patient.PatientId,
                UploadedByUserId = uploaderId,
                UploadedAt = DateTime.UtcNow,
                FilePath = fullPath,
                Notes = notes
            };


            // Save the record to the database
            _context.DataFiles.Add(dataFile);
            await _context.SaveChangesAsync();

            // Process the CSV into PressureFrame records
            await ProcessCsvAsync(dataFile);

            TempData["Message"] = "CSV uploaded and processed successfully.";
            return RedirectToAction("Index", "Admin");
        }

        // -------------------------------------------------
        // Read CSV and generate PressureFrame rows
        // -------------------------------------------------
        private async Task ProcessCsvAsync(DataFile file)
        {
            // Read all lines from the CSV file
            string[] lines = await System.IO.File.ReadAllLinesAsync(file.FilePath);

            // Ensure file contains complete 32-row frames
            if (lines.Length % 32 != 0)
                throw new InvalidOperationException("CSV does not contain a whole number of 32-row frames.");

            int frameCount = lines.Length / 32;
            var baseTime = DateTime.UtcNow;       // Timestamp used for frames
            var frames = new List<PressureFrame>();

            for (int i = 0; i < frameCount; i++)
            {
                // Allocate 32×32 matrix for the current frame
                int[,] matrix = new int[32, 32];
                int start = i * 32;

                // Fill the matrix by parsing 32 CSV rows
                for (int r = 0; r < 32; r++)
                {
                    var cells = lines[start + r].Split(',', StringSplitOptions.RemoveEmptyEntries);

                    for (int c = 0; c < 32; c++)
                    {
                        matrix[r, c] = int.Parse(cells[c]);
                    }
                }

                // Perform pressure analysis calculations
                var peak = _analysis.CalculatePeakPressure(matrix);
                var avg = _analysis.CalculateAveragePressure(matrix);
                var area = _analysis.CalculateContactAreaPercent(matrix);
                var risk = _analysis.CalculateRiskScore(peak, area);

                // Add calculated frame to the list
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

            // Save generated frames to the database
            _context.PressureFrames.AddRange(frames);
            await _context.SaveChangesAsync();
        }
    }
}