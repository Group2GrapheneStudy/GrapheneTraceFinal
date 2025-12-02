using System.Security.Cryptography;
using System.Text;
using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ------------- LOGIN GET -------------
        [HttpGet]
        public IActionResult Login()
        {
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            if (!string.IsNullOrEmpty(role))
            {
                return role switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Clinician" => RedirectToAction("Index", "Clinician"),
                    "Patient" => RedirectToAction("Index", "Patient"),
                    _ => RedirectToAction("Index", "Landing")
                };
            }

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"]!.ToString();
            }

            return View();
        }

        // ------------- LOGIN POST -------------
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            email = email.Trim().ToLowerInvariant();
            var hashed = HashPassword(password);

            var user = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email &&
                                          u.PasswordHash == hashed &&
                                          u.IsActive);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
            HttpContext.Session.SetString(SessionKeys.UserRole, user.Role);
            HttpContext.Session.SetString(SessionKeys.UserEmail, user.Email);

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return user.Role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Clinician" => RedirectToAction("Index", "Clinician"),
                "Patient" => RedirectToAction("Index", "Patient"),
                _ => RedirectToAction("Index", "Landing")
            };
        }

        // ------------- LOGOUT -------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Landing");  // back to homepage
        }

        // ------------- REGISTER GET -------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ------------- REGISTER POST -------------
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            email = email.Trim().ToLowerInvariant();

            if (role != "Patient" && role != "Clinician" && role != "Admin")
            {
                ViewBag.Error = "Please select a valid role.";
                return View();
            }

            bool exists = await _context.UserAccounts
                .AnyAsync(u => u.Email.ToLower() == email);

            if (exists)
            {
                ViewBag.Error = "An account with that email already exists.";
                return View();
            }

            var user = new UserAccount
            {
                Email = email,
                PasswordHash = HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            switch (role)
            {
                case "Patient":
                    _context.Patients.Add(new Patient
                    {
                        UserId = user.UserId,
                        ContactEmail = user.Email,
                        RiskNotes = "New patient."
                    });
                    break;

                case "Clinician":
                    _context.Clinicians.Add(new Clinician
                    {
                        UserId = user.UserId,
                        Specialty = "General",
                        IsAvailable = true
                    });
                    break;

                case "Admin":
                    _context.Admins.Add(new Admin
                    {
                        UserId = user.UserId
                    });
                    break;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Account created. Please log in.";
            return RedirectToAction("Login");
        }

        // ------------- HELPER -------------
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
