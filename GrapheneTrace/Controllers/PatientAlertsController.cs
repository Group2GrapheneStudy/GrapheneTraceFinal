using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Patient")]
    public class PatientAlertsController : Controller
    {
        private readonly AppDbContext _context;

        public PatientAlertsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var patient = await _context.Patients.FirstAsync(p => p.UserId == userId);

            var alerts = await _context.Alerts
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync();

            return View(alerts);
        }
    }
}
