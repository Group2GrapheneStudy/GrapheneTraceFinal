using Microsoft.AspNetCore.Mvc;
using GrapheneTrace.Models;
using GrapheneTrace.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using GrapheneTrace.Helpers;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Clinician")]
    public class PrescriptionsController : Controller
    {
        private readonly AppDbContext _context;

        public PrescriptionsController(AppDbContext context)
        {
            _context = context;
        }

        // -------------------------------
        // LIST ALL PRESCRIPTIONS OF CLINICIAN
        // -------------------------------
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null) return Unauthorized();

            var clinician = await _context.Clinicians.FirstOrDefaultAsync(c => c.UserId == userId.Value);
            if (clinician == null) return BadRequest("Logged-in user is not a clinician.");

            var prescriptions = await _context.Prescriptions
                .Where(p => p.ClinicianId == clinician.ClinicianId)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.UserAccount)
                .OrderByDescending(p => p.DatePrescribed)
                .ToListAsync();

            return View(prescriptions);
        }

        // -------------------------------
        // SHOW CREATE PRESCRIPTION PAGE
        // -------------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null) return Unauthorized();

            var clinician = await _context.Clinicians.FirstOrDefaultAsync(c => c.UserId == userId.Value);
            if (clinician == null) return BadRequest("Logged-in user is not a clinician.");

            // Load all patients
            var patients = await _context.Patients
                .Include(p => p.UserAccount)
                .ToListAsync();

            ViewBag.Patients = patients;
            return View();
        }

        // -------------------------------
        // CREATE PRESCRIPTION POST
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int patientId, string drugName, string dosage, string frequency, int quantity, string? instructions)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null) return Unauthorized();

            var clinician = await _context.Clinicians.FirstOrDefaultAsync(c => c.UserId == userId.Value);
            if (clinician == null) return BadRequest("Logged-in user is not a clinician.");

            if (string.IsNullOrWhiteSpace(drugName) || quantity < 1)
            {
                TempData["Error"] = "Drug name and quantity are required.";
                return RedirectToAction("Create");
            }

            var prescription = new Prescription
            {
                PatientId = patientId,
                ClinicianId = clinician.ClinicianId,
                DrugName = drugName,
                Dosage = dosage,
                Frequency = frequency,
                Quantity = quantity,
                Instructions = instructions,
                DatePrescribed = DateTime.Now
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // -------------------------------
        // SHOW EDIT PAGE
        // -------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                    .ThenInclude(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return NotFound();

            // Load patients for dropdown
            ViewBag.Patients = await _context.Patients
                .Include(p => p.UserAccount)
                .ToListAsync();

            return View(prescription);
        }

        // -------------------------------
        // EDIT PRESCRIPTION POST
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int patientId, string drugName, string dosage, string frequency, int quantity, string? instructions)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null) return NotFound();

            prescription.PatientId = patientId;
            prescription.DrugName = drugName;
            prescription.Dosage = dosage;
            prescription.Frequency = frequency;
            prescription.Quantity = quantity;
            prescription.Instructions = instructions;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // -------------------------------
        // DELETE PRESCRIPTION
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null) return NotFound();

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
