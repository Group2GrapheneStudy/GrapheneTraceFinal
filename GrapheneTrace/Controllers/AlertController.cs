using System;
using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Patient", "Clinician")]
    public class AlertController : Controller
    {
        private readonly AppDbContext _context;

        public AlertController(AppDbContext context)
        {
            _context = context;
        }

        // -------------------
        // PATIENT VIEW
        // -------------------
        public async Task<IActionResult> Patient()
        {
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            if (!string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase))
            {
                // If someone tries to hit this without being a patient, bounce them.
                return RedirectToAction("Clinician");
            }

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null)
            {
                TempData["ErrorMessage"] = "No patient profile linked to this account.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var alerts = await _context.Alerts
                    .Where(a => a.PatientId == patient.PatientId)
                    .OrderByDescending(a => a.TriggeredAt)
                    .ToListAsync();

                return View(alerts);
            }
            catch (Exception ex)
            {
				// Loggin Error (Can be replaced with ILogger later)
				Console.WriteLine(ex);
                // Avoid crashing the app if the DB schema is out of sync
                TempData["ErrorMessage"] =
                    "There was a problem loading your alerts. Please contact the system administrator.";
                // Optional: log ex here in a real system
                return View(Enumerable.Empty<object>());
            }
        }

        // -------------------
        // CLINICIAN VIEW
        // -------------------
        public async Task<IActionResult> Clinician()
        {
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            if (!string.Equals(role, "Clinician", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Patient");
            }

            try
            {
                var alerts = await _context.Alerts
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.UserAccount)
                    .OrderByDescending(a => a.TriggeredAt)
                    .ToListAsync();

                return View(alerts);
            }
            catch (Exception ex)
            {
				Console.WriteLine(ex);
                TempData["ErrorMessage"] =
                    "There was a problem loading alerts. Please contact the system administrator.";
                // Optional: log ex
                return View(Enumerable.Empty<object>());
            }
        }

        // -------------------
        // RESOLVE ALERT (CLINICIAN)
        // -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id)
        {
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            if (!string.Equals(role, "Clinician", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Clinician");
            }

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var alert = await _context.Alerts.FindAsync(id);
                if (alert == null)
                {
                    return NotFound();
                }

                // These properties must exist on your Alert entity & in the DB
                alert.Status = "Resolved";
                alert.ResolvedByUserId = userId.Value;
                alert.ResolvedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
				Console.WriteLine(ex);
                TempData["ErrorMessage"] =
                    "There was a problem resolving the alert. Please contact the system administrator.";
            }

            return RedirectToAction("Clinician");
        }
    }
}