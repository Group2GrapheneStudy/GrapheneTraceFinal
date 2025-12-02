using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    // Admin and Clinician can create/manage appointments
    [RoleAuthorize("Admin", "Clinician")]
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------------
        // LIST (Clinician/Admin)
        // -----------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

            if (userId == null) return RedirectToAction("Login", "Account");

            // If clinician, only show clinician’s appointments
            if (role == "Clinician")
            {
                var clinician = await _context.Clinicians
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value);

                if (clinician == null) return RedirectToAction("Login", "Account");

                var appts = await _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                    .Where(a => a.ClinicianId == clinician.ClinicianId)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();

                return View(appts);
            }

            // Admin sees all
            var all = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                .Include(a => a.Clinician).ThenInclude(c => c.UserAccount)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            return View(all);
        }

        // -----------------------------------------------------
        // CREATE GET
        // -----------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = await _context.Patients
                .Include(p => p.UserAccount).ToListAsync();

            ViewBag.Clinicians = await _context.Clinicians
                .Include(c => c.UserAccount).ToListAsync();

            return View();
        }

        // -----------------------------------------------------
        // CREATE POST
        // -----------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int patientId, int clinicianId, DateTime start, DateTime end, string? notes)
        {
            if (end <= start)
            {
                TempData["Error"] = "End time must be after start time.";
                return RedirectToAction("Create");
            }

            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;

            var appt = new Appointment
            {
                PatientId = patientId,
                ClinicianId = clinicianId,
                CreatedByUserId = userId,
                StartTime = start.ToUniversalTime(),
                EndTime = end.ToUniversalTime(),
                Status = "Scheduled",
                Notes = notes
            };

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // -----------------------------------------------------
        // EDIT GET
        // -----------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                .Include(a => a.Clinician).ThenInclude(c => c.UserAccount)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appt == null) return NotFound();

            ViewBag.Patients = await _context.Patients.Include(p => p.UserAccount).ToListAsync();
            ViewBag.Clinicians = await _context.Clinicians.Include(c => c.UserAccount).ToListAsync();

            return View(appt);
        }

        // -----------------------------------------------------
        // EDIT POST
        // -----------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int appointmentId, int patientId, int clinicianId, DateTime start, DateTime end, string? notes)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt == null) return NotFound();

            if (end <= start)
            {
                TempData["Error"] = "Invalid time range.";
                return RedirectToAction("Edit", new { id = appointmentId });
            }

            appt.PatientId = patientId;
            appt.ClinicianId = clinicianId;
            appt.StartTime = start.ToUniversalTime();
            appt.EndTime = end.ToUniversalTime();
            appt.Notes = notes;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // -----------------------------------------------------
        // DELETE
        // -----------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            _context.Appointments.Remove(appt);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
