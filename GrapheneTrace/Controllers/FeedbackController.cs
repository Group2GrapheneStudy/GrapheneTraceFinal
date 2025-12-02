using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Patient")]
    public class FeedbackController : Controller
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            var feedbacks = await _context.Feedbacks
                .Where(f => f.PatientId == patient.PatientId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            ViewBag.PatientEmail = patient.UserAccount.Email;
            return View(feedbacks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? rating, string comment, int? dataFileId)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Index");
            }

            var feedback = new Feedback
            {
                PatientId = patient.PatientId,
                ClinicianId = null,
                DataFileId = dataFileId,
                Rating = string.IsNullOrWhiteSpace(rating) ? null : rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow,
                VisibleToClinician = true
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Thank you, your feedback has been submitted.";
            return RedirectToAction("Index");
        }
    }
}
