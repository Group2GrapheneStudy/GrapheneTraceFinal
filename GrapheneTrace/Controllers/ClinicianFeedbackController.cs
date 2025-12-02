using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Clinician")]
    public class ClinicianFeedbackController : Controller
    {
        private readonly AppDbContext _context;

        public ClinicianFeedbackController(AppDbContext context)
        {
            _context = context;
        }

        // List feedback that needs attention (or has replies)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var clinician = await _context.Clinicians
                .Include(c => c.UserAccount)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (clinician == null)
                return RedirectToAction("Login", "Account");

            // Show feedback that is either unassigned OR assigned to this clinician
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Patient)
                    .ThenInclude(p => p.UserAccount)
                .Where(f => f.VisibleToClinician &&
                            (f.ClinicianId == null || f.ClinicianId == clinician.ClinicianId))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            ViewBag.ClinicianName = clinician.UserAccount.Email;
            return View(feedbacks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int feedbackId, string reply)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (clinician == null)
                return RedirectToAction("Login", "Account");

            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(reply))
            {
                TempData["Error"] = "Reply cannot be empty.";
                return RedirectToAction("Index");
            }

            feedback.ClinicianId = clinician.ClinicianId; // claim ownership
            feedback.ClinicianReply = reply.Trim();
            feedback.ClinicianReplyAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Reply sent.";
            return RedirectToAction("Index");
        }
    }
}
