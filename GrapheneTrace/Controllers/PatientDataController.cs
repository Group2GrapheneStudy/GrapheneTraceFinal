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
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            var latestFile = await _context.DataFiles
                .Where(df => df.PatientId == patient.PatientId)
                .OrderByDescending(df => df.UploadedAt)
                .FirstOrDefaultAsync();

            var vm = new HeatmapPlayerVm
            {
                PatientId = patient.PatientId,
                Frames = new List<HeatmapFrameVm>()
            };

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

        public class HeatmapFrameVm
        {
            public int FrameId { get; set; }
            public int FrameIndex { get; set; }
            public DateTime CapturedAtUtc { get; set; }
        }

        public class HeatmapPlayerVm
        {
            public int PatientId { get; set; }
            public List<HeatmapFrameVm> Frames { get; set; } = new();
        }

    }
}
