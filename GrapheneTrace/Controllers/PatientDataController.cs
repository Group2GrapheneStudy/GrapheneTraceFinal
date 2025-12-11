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

        // -------------------------
        // HEATMAP VIEW
        // -------------------------
        public async Task<IActionResult> Heatmap()
        {
            // Get logged-in user's ID from session
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Load patient associated with this user account
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            // Get latest uploaded data file for this patient
            var latestFile = await _context.DataFiles
                .Where(df => df.PatientId == patient.PatientId)
                .OrderByDescending(df => df.UploadedAt)
                .FirstOrDefaultAsync();

            // Prepare the view model
            var vm = new HeatmapPlayerVm
            {
                PatientId = patient.PatientId,
                Frames = new List<HeatmapFrameVm>()
            };

            // If a file exists, load all heatmap frames for it
            if (latestFile != null)
            {
                vm.Frames = await _context.PressureFrames
                    .Where(f => f.DataFileId == latestFile.DataFileId)
                    .OrderBy(f => f.FrameIndex)
                    .Select(f => new HeatmapFrameVm
                    {
                        FrameId = f.FrameId,
                        FrameIndex = f.FrameIndex,
                        CapturedAtUtc = f.CapturedAtUtc
                    })
                    .ToListAsync();
            }

            return View(vm);
        }

        // Represents a single heatmap frame displayed in the viewer
        public class HeatmapFrameVm
        {
            // Unique ID of the frame record
            public int FrameId { get; set; }

            // Index of the frame within the sequence
            public int FrameIndex { get; set; }

            // Timestamp when the frame was captured
            public DateTime CapturedAtUtc { get; set; }
        }

        // View model for the full heatmap player
        public class HeatmapPlayerVm
        {
            // ID of the patient whose frames are being viewed
            public int PatientId { get; set; }

            // List of frames available for playback
            public List<HeatmapFrameVm> Frames { get; set; } = new();
        }
    }
}
