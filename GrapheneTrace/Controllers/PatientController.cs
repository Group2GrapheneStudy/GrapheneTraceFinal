using System;
using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Patient")]
    public class PatientController : Controller
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Make sure the user is logged in
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
            {
                // No session – send to login
                return RedirectToAction("Login", "Account");
            }

            // Load the patient linked to this user
            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null)
            {
                // No patient row linked to this account – fail gracefully
                TempData["ErrorMessage"] = "No patient profile is linked to this account.";
                return RedirectToAction("Login", "Account");
            }

            var now = DateTime.UtcNow;

            // Safeguard values so that any data issue doesn't crash the dashboard
            int alertCount = 0;
            int openAlertCount = 0;
            int feedbackCount = 0;
            int upcomingAppointmentsCount = 0;
            DateTime? nextAppointmentAt = null;
            bool hasPressureData = false;

            try
            {
                // Total alerts for this patient
                alertCount = await _context.Alerts
                    .Where(a => a.PatientId == patient.PatientId)
                    .CountAsync();

                // Only non-resolved alerts (Status may be null, so make it null-safe)
                openAlertCount = await _context.Alerts
                    .Where(a => a.PatientId == patient.PatientId &&
                                (a.Status ?? string.Empty) != "Resolved")
                    .CountAsync();

                // Feedback count
                feedbackCount = await _context.Feedbacks
                    .Where(f => f.PatientId == patient.PatientId)
                    .CountAsync();

                // Upcoming appointments
                var upcomingAppointments = await _context.Appointments
                    .Where(a => a.PatientId == patient.PatientId && a.StartTime > now)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();

                upcomingAppointmentsCount = upcomingAppointments.Count;
                nextAppointmentAt = upcomingAppointments.FirstOrDefault()?.StartTime;

                // 7. Any pressure data at all?
                hasPressureData = await _context.DataFiles
                    .AnyAsync(d => d.PatientId == patient.PatientId);
            }
            catch (Exception)
            {
         
            }

            // Build the view model 
            var vm = new PatientDashboardViewModel
            {
                PatientName = patient.UserAccount?.Email ?? "Patient",
                Email = patient.UserAccount?.Email ?? string.Empty,
                AlertCount = alertCount,
                OpenAlertCount = openAlertCount,
                FeedbackCount = feedbackCount,
                UpcomingAppointmentsCount = upcomingAppointmentsCount,
                NextAppointmentAt = nextAppointmentAt,
                HasPressureData = hasPressureData
            };

            return View(vm);
        }   
    }
}
