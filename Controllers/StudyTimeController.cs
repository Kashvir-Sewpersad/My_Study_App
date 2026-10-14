using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_Study_App.Data;
using My_Study_App.Models;

namespace My_Study_App.Controllers
{
    [Authorize]
    public class StudyTimeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudyTimeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] SaveTimeRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Use the login stamp from the auth cookie, not from the browser
            if (!long.TryParse(User.FindFirst("LoginTime")?.Value, out var ticks))
                return BadRequest();

            var startedAt = new DateTime(ticks, DateTimeKind.Utc);

            // Active time can never exceed the time since login (small tolerance added)
            var maxSeconds = (int)(DateTime.UtcNow - startedAt).TotalSeconds + 5;
            var seconds = Math.Clamp(request.ActiveSeconds, 0, maxSeconds);

            var session = await _context.StudySessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.LoginTicks == ticks);

            if (session == null)
            {
                _context.StudySessions.Add(new StudySession
                {
                    UserId = userId,
                    LoginTicks = ticks,
                    StartedAt = startedAt,
                    LastUpdated = DateTime.UtcNow,
                    ActiveSeconds = seconds
                });
            }
            else if (seconds > session.ActiveSeconds)
            {
                session.ActiveSeconds = seconds;
                session.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}