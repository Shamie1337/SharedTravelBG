using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

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
			var reviews = await _context.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.Trip)
					.ThenInclude(t => t.Organizer)
				.ToListAsync();
			return View(reviews);
		}


		// GET: Reviews/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var review = await _context.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.Trip)
				.FirstOrDefaultAsync(r => r.Id == id);
			if (review == null)
				return NotFound();

			return View(review);
		}

		// GET: Reviews/Create
		// GET: Reviews/Create
		public async Task<IActionResult> Create()
		{
			// Only past trips (date < today)
			var pastTrips = await _context.Trips
				.Include(t => t.Organizer)
				.Where(t => t.TripDate < DateTime.Today)
				.OrderByDescending(t => t.TripDate)
				.ToListAsync();

			// Build SelectList with organizer name + email
			ViewBag.Trips = new SelectList(
				pastTrips.Select(t => new
				{
					t.Id,
					Display = $"{t.DepartureTown} → {t.DestinationTown} on {t.TripDate:dd.MM.yyyy} by {t.Organizer.FullName} ({t.Organizer.Email})"
				}),
				"Id",
				"Display"
			);

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Review review)
		{
			// Reload past trips for dropdown
			var pastTrips = await _context.Trips
				.Include(t => t.Organizer)
				.Where(t => t.TripDate < DateTime.Today)
				.OrderByDescending(t => t.TripDate)
				.ToListAsync();

			ViewBag.Trips = new SelectList(
				pastTrips.Select(t => new
				{
					t.Id,
					Display = $"{t.DepartureTown} → {t.DestinationTown} on {t.TripDate:dd.MM.yyyy} by {t.Organizer.FullName} ({t.Organizer.Email})"
				}),
				"Id",
				"Display"
			);

			// Ensure the trip exists
			var trip = await _context.Trips
				.Include(t => t.Organizer)
				.FirstOrDefaultAsync(t => t.Id == review.TripId);

			if (trip == null)
			{
				ModelState.AddModelError("", "Selected trip does not exist.");
				return View(review);
			}

			// Prevent reviewing future trips
			if (trip.TripDate >= DateTime.Today)
			{
				TempData["ErrorMessage"] = "You cannot write a review for a trip that has not yet occurred.";
				return RedirectToAction(nameof(Create));
			}

			// Prevent organizer reviewing own trip
			string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (trip.OrganizerId == currentUserId)
			{
				TempData["ErrorMessage"] = "You cannot write a review for your own trip.";
				return RedirectToAction(nameof(Create));
			}

			// Validate other fields
			if (!ModelState.IsValid)
			{
				return View(review);
			}

			// Save the review
			review.ReviewerId = currentUserId;
			_context.Reviews.Add(review);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", "Reviews");
		}

		// GET: Reviews/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var review = await _context.Reviews.FindAsync(id);
			if (review == null)
			{
				return NotFound();
			}

			// Only allow editing if the current user is the creator
			string currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (review.ReviewerId != currentUserId && !User.IsInRole("Admin"))
			{
				return Forbid();
			}

			// If using a dropdown for Trip selection, load it here if needed
			return View(review);
		}



		// POST: Reviews/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Review review)
		{
			if (id != review.Id)
				return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					// Preserve the original ReviewerId.
					var original = await _context.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
					if (original != null)
						review.ReviewerId = original.ReviewerId;

					_context.Update(review);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Reviews.Any(r => r.Id == review.Id))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}
			// Reload trips in case ModelState is invalid.
			var trips = _context.Trips.ToList();
			ViewBag.Trips = new SelectList(trips.Select(t => new {
				t.Id,
				Display = $"{t.DepartureTown} to {t.DestinationTown} on {t.TripDate.ToShortDateString()}"
			}), "Id", "Display", review.TripId);
			return View(review);
		}

		// GET: Reviews/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();

			var review = await _context.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.Trip)
				.FirstOrDefaultAsync(r => r.Id == id);
			if (review == null)
				return NotFound();

			// Allow deletion if the user is the creator or is an admin.
			string currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (review.ReviewerId != currentUserId && !User.IsInRole("Admin"))
				return Forbid();

			return View(review);
		}

		// POST: Reviews/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var review = await _context.Reviews.FindAsync(id);
			if (review == null)
				return NotFound();

			string currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (review.ReviewerId != currentUserId && !User.IsInRole("Admin"))
				return Forbid();

			_context.Reviews.Remove(review);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		[Authorize]
		public async Task<IActionResult> MyReviews()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var myReviews = await _context.Reviews
				.Include(r => r.Trip)
				.Where(r => r.ReviewerId == userId)
				.ToListAsync();
			return View(myReviews);
		}
	}
}
