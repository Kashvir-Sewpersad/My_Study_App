using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_Study_App.Data;
using My_Study_App.Models;

namespace My_Study_App.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public IActionResult Index() => View();

        // Returns logins, planned events and study plans for the visible month
        [HttpGet]
        public async Task<IActionResult> Data(int year, int month)
        {
            if (year < 2000 || year > 2100 || month < 1 || month > 12)
                return BadRequest();

            var userId = CurrentUserId;

            // Buffer on both sides covers the days shown from neighbouring months
            // and any time zone difference between UTC and the user's browser
            var first = new DateTime(year, month, 1);
            var from = first.AddDays(-8);
            var to = first.AddMonths(1).AddDays(8);

            var loginTimes = await _context.LoginRecords
                .Where(l => l.UserId == userId && l.LoggedInAtUtc >= from && l.LoggedInAtUtc < to)
                .OrderBy(l => l.LoggedInAtUtc)
                .Select(l => l.LoggedInAtUtc)
                .ToListAsync();

            // Send as UTC ("...Z") so the browser converts to the user's local time
            var logins = loginTimes
                .Select(l => DateTime.SpecifyKind(l, DateTimeKind.Utc).ToString("o"))
                .ToList();

            var eventRows = await _context.CalendarEvents
                .Where(e => e.UserId == userId && e.EventDate >= from && e.EventDate < to)
                .OrderBy(e => e.EventDate).ThenBy(e => e.StartTime)
                .ToListAsync();

            var events = eventRows.Select(e => new
            {
                e.Id,
                e.Title,
                EventDate = e.EventDate.ToString("yyyy-MM-dd"),
                e.StartTime,
                e.Notes,
                e.StudyPlanId,
                e.IsCompleted
            });

            var plans = await _context.StudyPlans
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Topic)
                .Select(p => new { p.Id, p.Topic })
                .ToListAsync();

            var totalLogins = await _context.LoginRecords.CountAsync(l => l.UserId == userId);

            return Json(new { logins, events, plans, totalLogins });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEvent([FromBody] CalendarEventRequest req)
        {
            var title = req.Title?.Trim();
            if (string.IsNullOrEmpty(title) || title.Length > 150)
                return BadRequest("Title is required (max 150 characters).");

            if (!DateTime.TryParseExact(req.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                return BadRequest("Invalid date.");

            string? startTime = null;
            if (!string.IsNullOrWhiteSpace(req.StartTime) && TimeOnly.TryParse(req.StartTime, out var t))
                startTime = t.ToString("HH:mm");

            if (req.StudyPlanId != null)
            {
                var owned = await _context.StudyPlans
                    .AnyAsync(p => p.Id == req.StudyPlanId && p.UserId == CurrentUserId);
                if (!owned) return BadRequest("Study plan not found.");
            }

            _context.CalendarEvents.Add(new CalendarEvent
            {
                UserId = CurrentUserId,
                Title = title,
                EventDate = date.Date,
                StartTime = startTime,
                Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(),
                StudyPlanId = req.StudyPlanId
            });
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var ev = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUserId);
            if (ev == null) return NotFound();

            ev.IsCompleted = !ev.IsCompleted;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUserId);
            if (ev == null) return NotFound();

            _context.CalendarEvents.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}