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
        // CREATE APPOINTMENT (ADMIN + CLINICIAN)
        // ----------------------------------------------------------
        [RoleAuthorize("Admin", "Clinician")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = await _context.Patients.Include(p => p.UserAccount).ToListAsync();
            ViewBag.Clinicians = await _context.Clinicians.Include(c => c.UserAccount).ToListAsync();
            return View();
        }

        [RoleAuthorize("Admin", "Clinician")]
        [HttpPost]
        public async Task<IActionResult> Create(int patientId, int clinicianId, DateTime start, DateTime end)
        {
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

            return RedirectToAction("Index");
        }

        // ----------------------------------------------------------
        // EDIT APPOINTMENT (ALL ROLES)
        // ----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.UserAccount)
                .Include(a => a.Clinician).ThenInclude(c => c.UserAccount)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appt == null)
                return NotFound();

            ViewBag.Patients = await _context.Patients.Include(p => p.UserAccount).ToListAsync();
            ViewBag.Clinicians = await _context.Clinicians.Include(c => c.UserAccount).ToListAsync();

            return View(appt);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int appointmentId, int patientId, int clinicianId, DateTime start, DateTime end)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);

            if (appt == null)
                return NotFound();

            appt.PatientId = patientId;
            appt.ClinicianId = clinicianId;
            appt.StartTime = start.ToUniversalTime();
            appt.EndTime = end.ToUniversalTime();

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // ----------------------------------------------------------
        // DELETE APPOINTMENT (ALL ROLES)
        // ----------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);

            if (appt == null)
                return NotFound();

            _context.Appointments.Remove(appt);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
