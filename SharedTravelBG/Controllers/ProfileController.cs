using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SharedTravelBG.Controllers
{
	public class ProfileController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ProfileController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Profile/Index/{id}
		// 'id' here is the user's Id (organiser's Id)
		public async Task<IActionResult> Index(string id)
		{
			if (string.IsNullOrEmpty(id))
				return NotFound();

			// Retrieve the organiser (user) from the Identity table.
			var organiser = await _context.Users.FindAsync(id);
			if (organiser == null)
				return NotFound();

			// Get IDs of trips organized by this user.
			var tripIds = await _context.Trips
				.Where(t => t.OrganizerId == id)
				.Select(t => t.Id)
				.ToListAsync();

			// Get reviews for these trips.
			var reviews = await _context.Reviews
				.Where(r => tripIds.Contains(r.TripId))
				.ToListAsync();

			double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
			ViewBag.AverageRating = averageRating;
			ViewBag.ReviewCount = reviews.Count;

			return View(organiser);
		}
	}
}
