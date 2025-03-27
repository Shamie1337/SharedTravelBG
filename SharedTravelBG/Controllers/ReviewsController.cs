using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SharedTravelBG.Controllers
{
	public class ReviewsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ReviewsController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Reviews
		public async Task<IActionResult> Index()
		{
			var reviews = _context.Reviews.Include(r => r.Reviewer).Include(r => r.Trip);
			return View(await reviews.ToListAsync());
		}

		// GET: Reviews/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var review = await _context.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.Trip)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (review == null) return NotFound();

			return View(review);
		}

		// GET: Reviews/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Reviews/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Content,Rating,TripId")] Review review)
		{
			if (ModelState.IsValid)
			{
				// Set the reviewer to the current user
				review.ReviewerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
				_context.Add(review);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(review);
		}

		// GET: Reviews/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var review = await _context.Reviews.FindAsync(id);
			if (review == null) return NotFound();
			return View(review);
		}

		// POST: Reviews/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Content,Rating,TripId")] Review review)
		{
			if (id != review.Id) return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(review);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ReviewExists(review.Id))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}
			return View(review);
		}

		// GET: Reviews/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var review = await _context.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.Trip)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (review == null) return NotFound();

			return View(review);
		}

		// POST: Reviews/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var review = await _context.Reviews.FindAsync(id);
			_context.Reviews.Remove(review);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ReviewExists(int id)
		{
			return _context.Reviews.Any(r => r.Id == id);
		}
	}
}
