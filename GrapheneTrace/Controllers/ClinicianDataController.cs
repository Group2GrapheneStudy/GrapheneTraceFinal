using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Clinician", "Admin")]
    public class ClinicianDataController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITrendService _trends;
        private readonly IHeatmapService _heatmaps;

        public ClinicianDataController(
            AppDbContext context,
            ITrendService trends,
            IHeatmapService heatmaps)
        {
            _context = context;
            _trends = trends;
            _heatmaps = heatmaps;
        }

        // ---------------------------------------------
        // SELECT PATIENT
        // ---------------------------------------------
        public async Task<IActionResult> SelectPatient()
        {
            var patients = await _context.Patients
                .Include(p => p.UserAccount)
                .OrderBy(p => p.PatientId)
                .ToListAsync();

            return View(patients);
        }

        // ---------------------------------------------
        // TREND FOR A SPECIFIC PATIENT
        // ---------------------------------------------
        public async Task<IActionResult> Trend(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound();

            var points = await _trends.GetPatientTrendAsync(patient.PatientId);

            ViewBag.PatientEmail = patient.UserAccount.Email;
            ViewBag.PatientId = patient.PatientId;

            return View(points); // Uses ClinicianData/Trend.cshtml
        }

        // ---------------------------------------------
        // HEATMAP FOR A SPECIFIC PATIENT
        // ---------------------------------------------
        public async Task<IActionResult> Heatmap(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound();

            var heatmap = await _heatmaps.GetLatestHeatmapAsync(patient.PatientId);

            ViewBag.PatientEmail = patient.UserAccount.Email;
            ViewBag.PatientId = patient.PatientId;

            // View name is explicit so you can have a dedicated clinician heatmap view
            return View("ClinicianHeatmap", heatmap);
        }
    }
}
