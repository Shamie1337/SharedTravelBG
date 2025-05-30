// Controllers/TripsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Security.Claims;

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
		public async Task<IActionResult> Index(string? departure,
		string? destination,
		DateTime? date,
		TimeSpan? time )
		{
			// Include the Participants navigation property so that each trip's list is populated.

			var trips = _context.Trips
			.Include(t => t.Organizer)
			.Include(t => t.Participants)
			.AsQueryable();

			if (!string.IsNullOrWhiteSpace(departure))
				trips = trips.Where(t => t.DepartureTown.Contains(departure));

			if (!string.IsNullOrWhiteSpace(destination))
				trips = trips.Where(t => t.DestinationTown.Contains(destination));

			if (date.HasValue)
				trips = trips.Where(t => t.TripDate.Date == date.Value.Date);

			if (time.HasValue)
				trips = trips.Where(t => t.PlannedStartTime == time.Value);

			var result = await trips.ToListAsync();

			ViewData["departure"] = departure ?? "";
			ViewData["destination"] = destination ?? "";
			ViewData["date"] = date?.ToString("yyyy-MM-dd") ?? "";
			ViewData["time"] = time?.ToString(@"hh\:mm") ?? "";

			return View(result);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var trip = await _context.Trips
				.Include(t => t.Organizer)
				.Include(t => t.Participants)
				.FirstOrDefaultAsync(t => t.Id == id);
			if (trip == null)
			{
				return NotFound();
			}

			// Calculate available spots.
			int availableSpots = trip.MaxParticipants - (trip.Participants?.Count ?? 0);

			// Compute organiser's overall rating:
			// 1. Get all trips organized by this organiser.
			var organiserTripIds = await _context.Trips
				.Where(t => t.OrganizerId == trip.OrganizerId)
				.Select(t => t.Id)
				.ToListAsync();

			// 2. Get all reviews for those trips.
			var organiserReviews = await _context.Reviews
				.Where(r => organiserTripIds.Contains(r.TripId))
				.ToListAsync();

			// 3. Compute average rating (if there are any reviews)
			double averageRating = organiserReviews.Any() ? organiserReviews.Average(r => r.Rating) : 0;

			// Pass the computed values to the view via ViewBag.
			ViewBag.AvailableSpots = availableSpots;
			ViewBag.OrganiserRating = averageRating;
			ViewBag.ReviewCount = organiserReviews.Count;

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
		public async Task<IActionResult> Create([Bind("DepartureTown,DestinationTown,TripDate,MaxParticipants,OrganizerPhoneNumber,PlannedStartTime")] Trip trip)
		{

			if (!ModelState.IsValid)
			{
				// Loop through all keys in ModelState and print out errors.
				foreach (var key in ModelState.Keys)
				{
					foreach (var error in ModelState[key].Errors)
					{
						// Output the error message to the Debug console.
						System.Diagnostics.Debug.WriteLine($"Field: {key} Error: {error.ErrorMessage}");
						// Alternatively, you could use Console.WriteLine if running in an environment where console output is visible.
						Console.WriteLine($"Field: {key} Error: {error.ErrorMessage}");
					}
				}
				// Optionally, add a breakpoint here to inspect ModelState in the debugger.
				return View(trip);
			}

			if (ModelState.IsValid)
			{
				// Set OrganizerId using the current logged-in user's Id.
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
		public async Task<IActionResult> Edit(int id, [Bind("Id,DepartureTown,DestinationTown,TripDate,MaxParticipants,OrganizerPhoneNumber,PlannedStartTime")] Trip trip)
		{
			if (id != trip.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					// Preserve OrganizerId from the original trip
					var original = await _context.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
					if (original != null)
						trip.OrganizerId = original.OrganizerId;

					_context.Update(trip);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Trips.Any(e => e.Id == id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
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



		// POST: Trips/Join/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Join(int id)
		{
			// Retrieve the trip including its Participants collection.
			var trip = await _context.Trips
									 .Include(t => t.Participants)
									 .FirstOrDefaultAsync(t => t.Id == id);
			if (trip == null)
			{
				return NotFound();
			}

			// Get the current user's ID.
			string currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(currentUserId))
			{
				return Forbid();
			}

			// If the user is not already a participant, add them (if there's space).
			if (!trip.Participants.Any(p => p.Id == currentUserId))
			{
				if (trip.Participants.Count < trip.MaxParticipants)
				{
					var user = await _context.Users.FindAsync(currentUserId);
					if (user != null)
					{
						trip.Participants.Add(user);
						await _context.SaveChangesAsync();
					}
				}
				else
				{
					TempData["Message"] = "Sorry, this trip is full.";
				}
			}

			// Redirect back to the Index page so the updated status is visible.
			return RedirectToAction(nameof(Index));

		}

		//Adding the MyTrips
		public async Task<IActionResult> MyTrips()
		{
			// get current user
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Challenge(); // or RedirectToAction("Index", "Home");

			// find all trips where this user is a participant
			var myTrips = await _context.Trips
				.Include(t => t.Organizer)
				.Include(t => t.Participants)
				.Where(t => t.Participants.Any(p => p.Id == userId))
				.ToListAsync();

			return View(myTrips);
		}

		// POST: Trips/Leave/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Leave(int id)
		{
			// Retrieve the trip including its participants.
			var trip = await _context.Trips
				.Include(t => t.Participants)
				.FirstOrDefaultAsync(t => t.Id == id);
			if (trip == null)
			{
				return NotFound();
			}

			// Get the current user's ID.
			string currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(currentUserId))
			{
				return Forbid();
			}

			// Find the current user in the trip's participants.
			var user = trip.Participants.FirstOrDefault(p => p.Id == currentUserId);
			if (user != null)
			{
				trip.Participants.Remove(user);
				await _context.SaveChangesAsync();
			}

			
			return RedirectToAction(nameof(Index));
		}


		// GET: Trips/MyOrganized
		[Authorize]
		public async Task<IActionResult> MyOrganized()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var organized = await _context.Trips
				.Include(t => t.Organizer)
				.Where(t => t.OrganizerId == userId)
				.ToListAsync();
			return View(organized);
		}

		

	}
}
