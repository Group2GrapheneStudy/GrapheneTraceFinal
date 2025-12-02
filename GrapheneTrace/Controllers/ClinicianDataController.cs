using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Clinician")]
    public class ClinicianDataController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITrendService _trends;
        private readonly IHeatmapService _heatmaps;

        public ClinicianDataController(AppDbContext context, ITrendService trends, IHeatmapService heatmaps)
        {
            _context = context;
            _trends = trends;
            _heatmaps = heatmaps;
        }

        // Simple patient list to choose from
        public async Task<IActionResult> SelectPatient()
        {
            var patients = await _context.Patients
                .Include(p => p.UserAccount)
                .OrderBy(p => p.PatientId)
                .ToListAsync();

            return View(patients);
        }

        // Trend for a specific patient
        public async Task<IActionResult> Trend(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null) return NotFound();

            var points = await _trends.GetPatientTrendAsync(patient.PatientId);
            ViewBag.PatientEmail = patient.UserAccount.Email;
            ViewBag.PatientId = patient.PatientId;

            return View(points);
        }
    }
}
