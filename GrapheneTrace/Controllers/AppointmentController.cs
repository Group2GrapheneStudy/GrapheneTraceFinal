using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Admin", "Clinician", "Patient")]
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------------
        // VIEW ALL APPOINTMENTS FOR CURRENT USER
        // ----------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            if (userId == null)
                return RedirectToAction("Login", "Account");

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                .Include(a => a.Clinician).ThenInclude(c => c.UserAccount);

            if (role == "Patient")
            {
                var patient = await _context.Patients.FirstAsync(p => p.UserId == userId);
                query = query.Where(a => a.PatientId == patient.PatientId);
            }
            else if (role == "Clinician")
            {
                var clinician = await _context.Clinicians.FirstAsync(c => c.UserId == userId);
                query = query.Where(a => a.ClinicianId == clinician.ClinicianId);
            }

            var list = await query.OrderBy(a => a.StartTime).ToListAsync();
            ViewBag.Role = role;
            return View(list);
        }

        // ----------------------------------------------------------
        // CREATE APPOINTMENT (Patient self-booking + Admin/Clinician)
        // ----------------------------------------------------------
        [HttpGet]
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        public async Task<IActionResult> Create()
        {
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            if (role == "Patient")
            {
                // Only allow logged-in patient
                var patient = await _context.Patients
                    .Include(p => p.UserAccount)
                    .FirstAsync(p => p.UserId == HttpContext.Session.GetInt32(SessionKeys.UserId));

                ViewBag.Patients = new List<Patient> { patient };
            }
            else
            {
                ViewBag.Patients = await _context.Patients.Include(p => p.UserAccount).ToListAsync();
            }

            ViewBag.Clinicians = await _context.Clinicians.Include(c => c.UserAccount).ToListAsync();
            return View();
        }

        [HttpPost]
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int patientId, int clinicianId, DateTime start, DateTime end)
        {
            if (start >= end)
            {
                TempData["Error"] = "End time must be after start time.";
                return RedirectToAction("Create");
            }

            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            // Patient can only create for themselves
            if (role == "Patient")
            {
                patientId = (await _context.Patients.FirstAsync(p => p.UserId == HttpContext.Session.GetInt32(SessionKeys.UserId))).PatientId;
            }

            var createdBy = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var appt = new Appointment
            {
                PatientId = patientId,
                ClinicianId = clinicianId,
                StartTime = start.ToUniversalTime(),
                EndTime = end.ToUniversalTime(),
                Status = role == "Patient" ? "Pending" : "Scheduled",
                CreatedByUserId = createdBy
            };

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appointment created successfully.";
            return RedirectToAction("Index");
        }

        // ----------------------------------------------------------
        // EDIT APPOINTMENT
        // ----------------------------------------------------------
        [HttpGet]
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        public async Task<IActionResult> Edit(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                .Include(a => a.Clinician).ThenInclude(c => c.UserAccount)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appt == null)
                return NotFound();

            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

            // Restrict editing
            if (role == "Patient" && appt.Patient.UserId != userId)
                return Forbid();
            if (role == "Clinician" && appt.Clinician.UserId != userId)
                return Forbid();

            if (role == "Patient")
            {
                ViewBag.Patients = new List<Patient> { appt.Patient };
            }
            else
            {
                ViewBag.Patients = await _context.Patients.Include(p => p.UserAccount).ToListAsync();
            }

            ViewBag.Clinicians = await _context.Clinicians.Include(c => c.UserAccount).ToListAsync();

            return View(appt);
        }

        [HttpPost]
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int appointmentId, int patientId, int clinicianId, DateTime start, DateTime end)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt == null)
                return NotFound();

            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

            // Restrict editing
            if (role == "Patient" && appt.Patient.UserId != userId)
                return Forbid();
            if (role == "Clinician" && appt.Clinician.UserId != userId)
                return Forbid();

            appt.PatientId = patientId;
            appt.ClinicianId = clinicianId;
            appt.StartTime = start.ToUniversalTime();
            appt.EndTime = end.ToUniversalTime();

            await _context.SaveChangesAsync();
            TempData["Message"] = "Appointment updated successfully.";
            return RedirectToAction("Index");
        }

        // ----------------------------------------------------------
        // DELETE APPOINTMENT
        // ----------------------------------------------------------
        [HttpPost]
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appt == null)
                return NotFound();

            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

            if (role == "Patient" && appt.Patient.UserId != userId)
                return Forbid();
            if (role == "Clinician" && appt.Clinician.UserId != userId)
                return Forbid();

            _context.Appointments.Remove(appt);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appointment deleted successfully.";
            return RedirectToAction("Index");
        }

        // ----------------------------------------------------------
        // Clinician Updates Status (Accept / Decline)
        // ----------------------------------------------------------
        [HttpPost]
        [RoleAuthorize("Clinician")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string newStatus)
        {
            var appt = await _context.Appointments
                .Include(a => a.Clinician)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appt == null)
                return NotFound();

            var clinicianId = (await _context.Clinicians.FirstAsync(c => c.UserId == HttpContext.Session.GetInt32(SessionKeys.UserId))).ClinicianId;

            if (appt.ClinicianId != clinicianId)
                return Forbid();

            if (newStatus != "Scheduled" && newStatus != "Cancelled")
                return BadRequest("Invalid status");

            appt.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Appointment status updated to {newStatus}.";
            return RedirectToAction("Index");
        }
    }
}
