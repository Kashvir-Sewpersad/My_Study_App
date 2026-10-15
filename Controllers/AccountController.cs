using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_Study_App.Data;
using My_Study_App.Models;

namespace My_Study_App.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool exists = await _context.Users.AnyAsync(u =>
                u.Username == model.Username || u.Email == model.Email);

            if (exists)
            {
                ModelState.AddModelError(string.Empty, "Username or email already in use.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            if (user.PasswordHash == null)
            {
                ModelState.AddModelError(string.Empty, "This account uses Google sign-in. Please use the Google button.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            await SignInUserAsync(user);
            return RedirectToAction("Index", "Home");
        }

        // Step 1: send the user to Google
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GoogleLogin()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Account")
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        // Step 2: Google sends the user back here
        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("External");
            await HttpContext.SignOutAsync("External");

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            if (!result.Succeeded || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Google sign-in failed. Please try again.";
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                var baseName = email.Split('@')[0];
                if (baseName.Length > 40) baseName = baseName.Substring(0, 40);

                var username = baseName;
                int n = 1;
                while (await _context.Users.AnyAsync(u => u.Username == username))
                {
                    username = baseName + n++;
                }

                user = new User { Username = username, Email = email, PasswordHash = null };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            await SignInUserAsync(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // Shared by password login and Google login
        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("LoginTime", DateTime.UtcNow.Ticks.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });
        }
    }
}