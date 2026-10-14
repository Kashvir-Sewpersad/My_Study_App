using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_Study_App.Data;
using My_Study_App.Models;

namespace My_Study_App.Controllers
{
    [Authorize]
    public class StudyPlanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudyPlanController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /StudyPlan
        public async Task<IActionResult> Index()
        {
            var plans = await _context.StudyPlans
                .Where(p => p.UserId == CurrentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(plans);
        }

        // GET: /StudyPlan/Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: /StudyPlan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudyPlanEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var plan = new StudyPlan
            {
                Topic = model.Topic,
                Notes = model.Notes,
                UserId = CurrentUserId
            };

            _context.StudyPlans.Add(plan);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = plan.Id });
        }

        // GET: /StudyPlan/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _context.StudyPlans
                .Include(p => p.Flashcards)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);

            if (plan == null) return NotFound();

            var model = new StudyPlanEditViewModel
            {
                Id = plan.Id,
                Topic = plan.Topic,
                Notes = plan.Notes,
                Flashcards = plan.Flashcards
            };

            return View(model);
        }

        // POST: /StudyPlan/Edit  (saves topic + notes)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudyPlanEditViewModel model)
        {
            var plan = await _context.StudyPlans
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.UserId == CurrentUserId);

            if (plan == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Flashcards = await _context.Flashcards
                    .Where(f => f.StudyPlanId == model.Id)
                    .ToListAsync();
                return View(model);
            }

            plan.Topic = model.Topic;
            plan.Notes = model.Notes;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Progress saved.";
            return RedirectToAction("Edit", new { id = plan.Id });
        }

        // POST: /StudyPlan/AddFlashcard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFlashcard(FlashcardCreateViewModel model)
        {
            var planOwned = await _context.StudyPlans
                .AnyAsync(p => p.Id == model.StudyPlanId && p.UserId == CurrentUserId);

            if (!planOwned) return NotFound();

            if (!string.IsNullOrWhiteSpace(model.Question) && !string.IsNullOrWhiteSpace(model.Answer))
            {
                _context.Flashcards.Add(new Flashcard
                {
                    StudyPlanId = model.StudyPlanId,
                    Question = model.Question,
                    Answer = model.Answer
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Edit", new { id = model.StudyPlanId });
        }

        // POST: /StudyPlan/DeleteFlashcard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFlashcard(int flashcardId, int studyPlanId)
        {
            var card = await _context.Flashcards
                .Include(f => f.StudyPlan)
                .FirstOrDefaultAsync(f => f.Id == flashcardId && f.StudyPlan!.UserId == CurrentUserId);

            if (card != null)
            {
                _context.Flashcards.Remove(card);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Edit", new { id = studyPlanId });
        }

        // POST: /StudyPlan/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.StudyPlans
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);

            if (plan != null)
            {
                _context.StudyPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}