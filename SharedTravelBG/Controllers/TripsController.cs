// Controllers/TripsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;

namespace SharedTravelBG.Controllers
{
	public class TripsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public TripsController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Trips
		public async Task<IActionResult> Index()
		{
			var trips = await _context.Trips.Include(t => t.Organizer).ToListAsync();
			return View(trips);
		}

		// GET: Trips/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var trip = await _context.Trips
				.Include(t => t.Organizer)
				.Include(t => t.Participants)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (trip == null)
				return NotFound();

			return View(trip);
		}

		// GET: Trips/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Trips/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("DepartureTown,DestinationTown,TripDate")] Trip trip)
		{
			if (ModelState.IsValid)
			{
				// Set the organizer as the current logged in user
				trip.OrganizerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
				_context.Add(trip);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(trip);
		}

		// GET: Trips/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return NotFound();

			var trip = await _context.Trips.FindAsync(id);
			if (trip == null)
				return NotFound();
			return View(trip);
		}

		// POST: Trips/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,DepartureTown,DestinationTown,TripDate")] Trip trip)
		{
			if (id != trip.Id)
				return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(trip);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Trips.Any(e => e.Id == trip.Id))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}
			return View(trip);
		}

		// GET: Trips/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();

			var trip = await _context.Trips
				.Include(t => t.Organizer)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (trip == null)
				return NotFound();

			return View(trip);
		}

		// POST: Trips/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var trip = await _context.Trips.FindAsync(id);
			_context.Trips.Remove(trip);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}
