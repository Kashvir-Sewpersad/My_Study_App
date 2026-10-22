using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_Study_App.Data;
using My_Study_App.Models;
using My_Study_App.Services;

namespace My_Study_App.Controllers
{
    [Authorize]
    public class MusicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MusicController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> Index()
        {
            var playlists = await _context.MusicPlaylists
                .Where(m => m.UserId == CurrentUserId)
                .OrderBy(m => m.Name)
                .ToListAsync();
            return View(playlists);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string? name, string? url)
        {
            name = name?.Trim();
            if (string.IsNullOrEmpty(name) || name.Length > 100)
            {
                TempData["Error"] = "Please enter a name (max 100 characters).";
                return RedirectToAction("Index");
            }

            var parsed = MusicEmbed.TryParse(url);
            if (parsed == null || url!.Trim().Length > 500)
            {
                TempData["Error"] = "That link isn't supported. Use a YouTube, Spotify or SoundCloud link.";
                return RedirectToAction("Index");
            }

            _context.MusicPlaylists.Add(new MusicPlaylist
            {
                UserId = CurrentUserId,
                Name = name,
                Url = url.Trim(),
                Provider = parsed.Value.Provider,
                EmbedUrl = parsed.Value.EmbedUrl
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.MusicPlaylists
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == CurrentUserId);
            if (item != null)
            {
                _context.MusicPlaylists.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // The small pop-out window
        public async Task<IActionResult> Player(int id)
        {
            var item = await _context.MusicPlaylists
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == CurrentUserId);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}