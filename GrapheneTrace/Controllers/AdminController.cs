using System.Security.Cryptography;
using System.Text;
using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
using GrapheneTrace.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------
        // DASHBOARD OVERVIEW
        // ---------------------------------------------
        public async Task<IActionResult> Index()
        {
            var users = await _context.UserAccounts.CountAsync();
            var patients = await _context.Patients.CountAsync();
            var clinicians = await _context.Clinicians.CountAsync();
            var alerts = await _context.Alerts.CountAsync();

            // Appointments table might not exist yet on an old database.
            // We wrap in try/catch so the dashboard still loads.
            int appointments = 0;
            try
            {
                appointments = await _context.Appointments.CountAsync();
            }
            catch
            {
                // Swallow error and show 0 – real fix is to update the DB schema.
                appointments = 0;
            }

            var feedbacks = await _context.Feedbacks.CountAsync();

            var recentAlerts = await _context.Alerts
                .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                .OrderByDescending(a => a.TriggeredAt)
                .Take(10)
                .ToListAsync();

            var recentFeedback = await _context.Feedbacks
                .Include(f => f.Patient).ThenInclude(p => p.UserAccount)
                .OrderByDescending(f => f.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentAppointments = new List<Appointment>();
            try
            {
                recentAppointments = await _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                    .Include(a => a.Clinician).ThenInclude(c => c.UserAccount)
                    .OrderByDescending(a => a.StartTime)
                    .Take(10)
                    .ToListAsync();
            }
            catch
            {
                // If table is missing, just leave recentAppointments empty.
            }

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = users,
                TotalPatients = patients,
                TotalClinicians = clinicians,
                TotalAlerts = alerts,
                TotalAppointments = appointments,
                TotalFeedback = feedbacks,
                RecentAlerts = recentAlerts,
                RecentAppointments = recentAppointments,
                RecentFeedback = recentFeedback
            };

            return View(vm);
        }

        // ---------------------------------------------
        // CREATE USER (ADMIN / CLINICIAN / PATIENT)
        // ---------------------------------------------
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            email = email.Trim().ToLower();

            if (await _context.UserAccounts.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "A user with that email already exists.";
                return View();
            }

            var hashed = HashPassword(password);

            var user = new UserAccount
            {
                Email = email,
                PasswordHash = hashed,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            if (role == "Patient")
            {
                _context.Patients.Add(new Patient
                {
                    UserId = user.UserId,
                    ContactEmail = email
                });
            }
            else if (role == "Clinician")
            {
                _context.Clinicians.Add(new Clinician
                {
                    UserId = user.UserId,
                    Specialty = "General",
                    IsAvailable = true
                });
            }
            else if (role == "Admin")
            {
                _context.Admins.Add(new Admin
                {
                    UserId = user.UserId
                });
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "User created successfully.";
            return RedirectToAction("Index");
        }

        // ---------------------------------------------
        // TOGGLE ACTIVE / INACTIVE (for any user)
        // ---------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserActive(int id, string? returnTo)
        {
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";

            if (!string.IsNullOrEmpty(returnTo) && Url.IsLocalUrl(returnTo))
            {
                return Redirect(returnTo);
            }

            return RedirectToAction("Index");
        }

        // ---------------------------------------------
        // DELETE USER (ONLY IF NO DIRECT RELATIONS)
        // ---------------------------------------------
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.UserAccounts.FindAsync(id);
            if (user == null)
                return NotFound();

            bool hasPatient = await _context.Patients.AnyAsync(p => p.UserId == id);
            bool hasClinician = await _context.Clinicians.AnyAsync(c => c.UserId == id);
            bool hasAdmin = await _context.Admins.AnyAsync(a => a.UserId == id);
            bool hasUploadedFiles = await _context.DataFiles.AnyAsync(d => d.UploadedByUserId == id);
            bool hasResolvedAlerts = await _context.Alerts.AnyAsync(a => a.ResolvedByUserId == id);

            bool hasRelations = hasPatient || hasClinician || hasAdmin ||
                                hasUploadedFiles || hasResolvedAlerts;

            if (hasRelations)
            {
                TempData["Error"] = "User has related records and cannot be deleted. Please deactivate instead.";
                return RedirectToAction("Index");
            }

            _context.UserAccounts.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "User deleted successfully.";
            return RedirectToAction("Index");
        }

        // ---------------------------------------------
        // VIEW ALL PATIENTS
        // ---------------------------------------------
        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Patients
                .Include(p => p.UserAccount)
                .OrderBy(p => p.PatientId)
                .ToListAsync();

            return View(patients);
        }

        // ---------------------------------------------
        // VIEW ALL CLINICIANS
        // ---------------------------------------------
        public async Task<IActionResult> Clinicians()
        {
            var clinicians = await _context.Clinicians
                .Include(c => c.UserAccount)
                .OrderBy(c => c.ClinicianId)
                .ToListAsync();

            return View(clinicians);
        }

        // ---------------------------------------------
        // VIEW ALL ALERTS
        // ---------------------------------------------
        public async Task<IActionResult> Alerts()
        {
            var alerts = await _context.Alerts
                .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync();

            return View(alerts);
        }

        // ---------------------------------------------
        // VIEW ALL FEEDBACK
        // ---------------------------------------------
        public async Task<IActionResult> Feedback()
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.Patient).ThenInclude(p => p.UserAccount)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(feedback);
        }

        // ---------------------------------------------
        // VIEW TRENDS (REUSE CLINICIAN DATA VIEW)
        // ---------------------------------------------
        public IActionResult Trend(int patientId)
        {
            return Redirect($"/ClinicianData/Trend?patientId={patientId}");
        }

        // ---------------------------------------------
        // VIEW PATIENT HEATMAP (REUSE CLINICIAN DATA VIEW)
        // ---------------------------------------------
        public IActionResult Heatmap(int patientId)
        {
            return Redirect($"/ClinicianData/Heatmap?patientId={patientId}");
        }

        // ---------------------------------------------
        // PASSWORD HASH
        // ---------------------------------------------
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
