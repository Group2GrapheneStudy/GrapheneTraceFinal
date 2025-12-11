using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    // Only users with the Patient role can access this controller
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
            // Get the logged-in user’s ID from session
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

            // Retrieve the patient record associated with this user
            var patient = await _context.Patients
                .FirstAsync(p => p.UserId == userId);

            // Load all alerts for this patient, newest first
            var alerts = await _context.Alerts
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync();

            // Return alerts to the view
            return View(alerts);
        }
    }
}

