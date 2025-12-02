using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Patient")]
    public class PatientDataController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHeatmapService _heatmaps;
        private readonly ITrendService _trends;

        public PatientDataController(
            AppDbContext context,
            IHeatmapService heatmaps,
            ITrendService trends)
        {
            _context = context;
            _heatmaps = heatmaps;
            _trends = trends;
        }

        // -------------------------
        // HEATMAP VIEW
        // -------------------------
        public async Task<IActionResult> Heatmap()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            var result = await _heatmaps.GetLatestHeatmapAsync(patient.PatientId);

            // result can be null; the view should handle a "no data" state
            return View(result);
        }

        // -------------------------
        // TREND VIEW
        // -------------------------
        public async Task<IActionResult> Trend()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            // Uses the convenience wrapper in ITrendService / TrendService
            var points = await _trends.GetPatientPressureTrendAsync(patient.PatientId);

            // Handy for showing context in the Trend view
            ViewBag.PatientEmail = patient.UserAccount?.Email ?? "Unknown";

            return View(points);
        }
    }
}
