using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Patient")]
    public class PatientPrescriptionsController : Controller
    {
        private readonly AppDbContext _context;

        public PatientPrescriptionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /PatientPrescriptions/Index
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null) return Unauthorized();

            // Get patient by userId
            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Clinician)
                        .ThenInclude(c => c.UserAccount)
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null) return NotFound("Patient not found.");

            var prescriptions = patient.Prescriptions
                .OrderByDescending(p => p.DatePrescribed)
                .ToList();

            return View(prescriptions);
        }
    }
}
