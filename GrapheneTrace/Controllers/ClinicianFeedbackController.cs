using System;
using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
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

        // ---------------------------------------------
        // LIST FEEDBACK VISIBLE TO THIS CLINICIAN
        // ---------------------------------------------
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

            // For now: show all feedback that is visible to clinicians.
            // (If you later add an explicit patient–clinician assignment table,
            // you can filter by those patient IDs here.)
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Patient)
                    .ThenInclude(p => p.UserAccount)
                .Where(f => f.VisibleToClinician)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            ViewBag.ClinicianName = clinician.UserAccount.Email;
            return View(feedbacks);
        }

        // ---------------------------------------------
        // CLINICIAN REPLY TO FEEDBACK
        // ---------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int feedbackId, string reply)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(reply))
            {
                TempData["Error"] = "Reply cannot be empty.";
                return RedirectToAction("Index");
            }

            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (clinician == null)
                return RedirectToAction("Login", "Account");

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

            if (feedback == null)
            {
                TempData["Error"] = "Feedback item not found.";
                return RedirectToAction("Index");
            }

            // Attach reply to this clinician
            feedback.ClinicianId = clinician.ClinicianId;
            feedback.ClinicianReply = reply.Trim();
            feedback.ClinicianReplyAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Reply sent to patient.";
            return RedirectToAction("Index");
        }
    }
}
