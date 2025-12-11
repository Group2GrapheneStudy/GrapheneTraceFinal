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
        // CREATE APPOINTMENT
        // ----------------------------------------------------------
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            if (role == "Patient")
            {
                var patient = await _context.Patients
                    .Include(p => p.UserAccount)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                ViewBag.Patients = new List<Patient> { patient };
            }
            else
            {
                ViewBag.Patients = await _context.Patients
                    .Include(p => p.UserAccount)
                    .ToListAsync();
            }

            ViewBag.Clinicians = await _context.Clinicians
                .Include(c => c.UserAccount)
                .ToListAsync();

            return View();
        }

        [RoleAuthorize("Admin", "Clinician", "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int patientId, int clinicianId, DateTime start, DateTime end)
        {
            if (patientId == 0 || clinicianId == 0)
            {
                TempData["Error"] = "Please select both patient and clinician.";
                return RedirectToAction("Create");
            }

            if (start >= end)
            {
                TempData["Error"] = "End time must be after start time.";
                return RedirectToAction("Create");
            }

            var createdBy = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var appt = new Appointment
            {
                PatientId = patientId,
                ClinicianId = clinicianId,
                StartTime = start.ToUniversalTime(),
                EndTime = end.ToUniversalTime(),
                Status = "Scheduled",
                CreatedByUserId = createdBy
            };

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appointment created successfully.";
            return RedirectToAction("Index");
        }

        // ----------------------------------------------------------
        // EDIT APPOINTMENT (SECURE FOR PATIENTS)
        // ----------------------------------------------------------
        [HttpGet]
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appt == null)
                return NotFound();

            // PATIENT SECURITY CHECK
            if (role == "Patient")
            {
                var patient = await _context.Patients.FirstAsync(p => p.UserId == userId);

                if (appt.PatientId != patient.PatientId)
                    return Unauthorized(); // <---- BLOCK EDITING OTHER PEOPLE'S APPOINTMENTS

                ViewBag.Patients = new List<Patient> { patient };
            }
            else
            {
                ViewBag.Patients = await _context.Patients
                    .Include(p => p.UserAccount)
                    .ToListAsync();
            }

            ViewBag.Clinicians = await _context.Clinicians
                .Include(c => c.UserAccount)
                .ToListAsync();

            return View(appt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        public async Task<IActionResult> Edit(int appointmentId, int patientId, int clinicianId, DateTime start, DateTime end)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt == null)
                return NotFound();

            // PATIENT SECURITY CHECK
            if (role == "Patient")
            {
                var patient = await _context.Patients.FirstAsync(p => p.UserId == userId);
                if (appt.PatientId != patient.PatientId)
                    return Unauthorized();
            }

            appt.PatientId = patientId;
            appt.ClinicianId = clinicianId;
            appt.StartTime = start.ToUniversalTime();
            appt.EndTime = end.ToUniversalTime();

            await _context.SaveChangesAsync();
            TempData["Message"] = "Appointment updated successfully.";
            return RedirectToAction("Index");
        }

        // ----------------------------------------------------------
        // DELETE APPOINTMENT (SECURE FOR PATIENTS)
        // ----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Clinician", "Patient")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null)
                return NotFound();

            // PATIENT SECURITY CHECK
            if (role == "Patient")
            {
                var patient = await _context.Patients.FirstAsync(p => p.UserId == userId);

                if (appt.PatientId != patient.PatientId)
                    return Unauthorized();
            }

            _context.Appointments.Remove(appt);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appointment deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
