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

        // ---------------------------------------------
        // LIST PATIENT FEEDBACK
        // ---------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
                if (userId == null)
                {
                    TempData["Error"] = "Your session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var patient = await _context.Patients
                    .Include(p => p.UserAccount)
                    .FirstOrDefaultAsync(p => p.UserId == userId.Value);

                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found for this account.";
                    return RedirectToAction("Login", "Account");
                }

                var feedbacks = await _context.Feedbacks
                    .Where(f => f.PatientId == patient.PatientId)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                ViewBag.PatientEmail = patient.UserAccount.Email;
                return View(feedbacks);
            }
            catch (Exception ex)
            {
				Console.WriteLine(ex);
                // In a real app you would log this somewhere (AuditLogs, file, etc.)
                TempData["Error"] = "Something went wrong while loading your feedback.";
                return RedirectToAction("Index", "Patient");
            }
        }

        // ---------------------------------------------
        // CREATE FEEDBACK
        // ---------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? rating, string comment, int? dataFileId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
                if (userId == null)
                {
                    TempData["Error"] = "Your session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId.Value);

                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found for this account.";
                    return RedirectToAction("Login", "Account");
                }

                if (string.IsNullOrWhiteSpace(comment))
                {
                    TempData["Error"] = "Comment cannot be empty.";
                    return RedirectToAction("Index");
                }

                // Rating is optional but must be between 1 and 5 if provided
                int ratingValue = 0; // 0 = no rating
                if (rating.HasValue)
                {
                    if (rating.Value < 1 || rating.Value > 5)
                    {
                        TempData["Error"] = "Rating must be between 1 and 5.";
                        return RedirectToAction("Index");
                    }

                    ratingValue = rating.Value;
                }

                var feedback = new Feedback
                {
                    PatientId = patient.PatientId,
                    ClinicianId = null,
                    DataFileId = dataFileId,
                    Rating = ratingValue,
                    Comment = comment.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    VisibleToClinician = true
                };

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Thank you, your feedback has been submitted.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // If anything goes wrong (DB issue, schema mismatch, etc.)
                TempData["Error"] = "Sorry, something went wrong while submitting your feedback.";
                return RedirectToAction("Index");
            }
        }
    }
}
