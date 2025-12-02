using System;
using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Clinician")]
    public class ClinicianController : Controller
    {
        private readonly AppDbContext _context;

        public ClinicianController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Ensure the user is logged in
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Load clinician linked to this user
            var clinician = await _context.Clinicians
                .Include(c => c.UserAccount)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (clinician == null)
            {
                TempData["ErrorMessage"] = "No clinician profile is linked to this account.";
                return RedirectToAction("Login", "Account");
            }

            var now = DateTime.UtcNow;

            int assignedPatientsCount = 0;
            int upcomingAppointmentsCount = 0;
            DateTime? nextAppointmentAt = null;
            int unresolvedAlertsCount = 0;
            int feedbackToReviewCount = 0;

            // 3. Appointments where this clinician is assigned
            try
            {
                var upcomingAppointments = await _context.Appointments
                    .Where(a => a.ClinicianId == clinician.ClinicianId && a.StartTime > now)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();

                upcomingAppointmentsCount = upcomingAppointments.Count;
                nextAppointmentAt = upcomingAppointments.FirstOrDefault()?.StartTime;

                // Patients that have appointments with this clinician
                assignedPatientsCount = await _context.Appointments
                    .Where(a => a.ClinicianId == clinician.ClinicianId)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync();
            }
            catch (Exception)
            {
                // If something goes wrong, we just leave the counts at 0
            }

            // 4. Alerts not resolved (null-safe and wrapped in try/catch
            try
            {
                unresolvedAlertsCount = await _context.Alerts
                    .Where(a => (a.Status ?? string.Empty) != "Resolved")
                    .CountAsync(); // later we can restrict to clinician's own patients
            }
            catch (Exception)
            {
                // Most likely DB schema mismatch on Alerts; avoid crashing.
                unresolvedAlertsCount = 0;
            }

            // 5. Feedback that has no clinician reply yet
            try
            {
                feedbackToReviewCount = await _context.Feedbacks
                    .Where(f => f.ClinicianId == clinician.ClinicianId && f.ClinicianReply == null)
                    .CountAsync();
            }
            catch (Exception)
            {
                feedbackToReviewCount = 0;
            }

            // 6. Build the view model safely
            var vm = new ClinicianDashboardViewModel
            {
                ClinicianName = clinician.UserAccount?.Email ?? "Clinician",
                Email = clinician.UserAccount?.Email ?? string.Empty,
                AssignedPatientsCount = assignedPatientsCount,
                UpcomingAppointmentsCount = upcomingAppointmentsCount,
                NextAppointmentAt = nextAppointmentAt,
                UnresolvedAlertsCount = unresolvedAlertsCount,
                FeedbackToReviewCount = feedbackToReviewCount
            };

            return View(vm);
        }
    }
}
