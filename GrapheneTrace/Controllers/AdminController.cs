using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
using GrapheneTrace.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                Patients = await _context.Patients
                    .Include(p => p.UserAccount)
                    .ToListAsync(),

                Clinicians = await _context.Clinicians
                    .Include(c => c.UserAccount)
                    .ToListAsync(),

                Admins = await _context.Admins
                    .Include(a => a.UserAccount)
                    .ToListAsync()
            };

            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"]!.ToString();
            }

            return View(vm);
        }

        // Create User (Patient/Clinician/Admin)
        [HttpPost]
        public async Task<IActionResult> CreateUser(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                TempData["Error"] = "Email, password and role are required.";
                return RedirectToAction("Index");
            }

            if (await _context.UserAccounts.AnyAsync(u => u.Email == email))
            {
                TempData["Error"] = "Email already exists.";
                return RedirectToAction("Index");
            }

            var user = new UserAccount
            {
                Email = email,
                PasswordHash = HashPassword(password),
                Role = role,
                IsActive = true
            };

            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            switch (role)
            {
                case "Patient":
                    _context.Patients.Add(new Patient { UserId = user.UserId });
                    break;
                case "Clinician":
                    _context.Clinicians.Add(new Clinician { UserId = user.UserId });
                    break;
                case "Admin":
                    _context.Admins.Add(new Admin { UserId = user.UserId });
                    break;
                default:
                    TempData["Error"] = "Unknown role.";
                    return RedirectToAction("Index");
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
