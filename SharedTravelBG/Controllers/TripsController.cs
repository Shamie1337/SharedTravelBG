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

		
		/// Shows only upcoming trips (TripDate >= today). 
		/// If any search filters are provided, applies them; otherwise returns all future trips.
		
		public async Task<IActionResult> Index(
			string departure,
			string destination,
			DateTime? date,
			int? minAvailableSpots    // optional filter for “Мин. места”
		)
		{
			// 1) Make today’s date for comparisons
			var today = DateTime.Today;

			// 2) Figure out if the user actually provided any search parameter
			bool anySearch =
				!string.IsNullOrWhiteSpace(departure)
				|| !string.IsNullOrWhiteSpace(destination)
				|| date.HasValue
				|| (minAvailableSpots.HasValue && minAvailableSpots.Value > 0);

			ViewBag.AnySearch = anySearch;
			ViewBag.MinAvailableSpots = minAvailableSpots; // so the form can re‐populate it

			// 3) Base query: only trips with TripDate >= today
			var baseQuery = _context.Trips
				.Include(t => t.Organizer)
				.Include(t => t.Participants)
				.Where(t => t.TripDate >= today);

			// 4) If no search terms, just fetch all future trips
			if (!anySearch)
			{
				var allFuture = await baseQuery
					.OrderBy(t => t.TripDate)
					.ThenBy(t => t.PlannedStartTime)
					.ToListAsync();

				// Pass that as the model to the view:
				return View(allFuture);
			}

			// 5) Otherwise, apply each filter on top of baseQuery
			if (!string.IsNullOrWhiteSpace(departure))
			{
				baseQuery = baseQuery.Where(t =>
					t.DepartureTown.Contains(departure));
			}

			if (!string.IsNullOrWhiteSpace(destination))
			{
				baseQuery = baseQuery.Where(t =>
					t.DestinationTown.Contains(destination));
			}

			if (date.HasValue)
			{
				baseQuery = baseQuery.Where(t =>
					t.TripDate == date.Value.Date);
			}

			// Only trips with at least one free seat
			baseQuery = baseQuery.Where(t =>
				t.Participants.Count < t.MaxParticipants);

			// “Min Available Spots” filter
			if (minAvailableSpots.HasValue && minAvailableSpots.Value > 0)
			{
				baseQuery = baseQuery.Where(t =>
					(t.MaxParticipants - t.Participants.Count)
					>= minAvailableSpots.Value);
			}

			var filteredResults = await baseQuery
				.OrderBy(t => t.TripDate)
				.ThenBy(t => t.PlannedStartTime)
				.ToListAsync();

			// 6) Pass the filtered list as the model
			return View(filteredResults);
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

			// NEW: Prevent the organizer from joining their own trip.
			if (trip.OrganizerId == currentUserId)
			{
				TempData["Message"] = "Не можете да се присъедините към собственото си пътуване.";
				return RedirectToAction(nameof(Index));
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
					TempData["Message"] = "Съжаляваме, това пътуване е пълно.";
				}
			}

			// Redirect back to the Index page so the updated status (or error) is visible.
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


		
		/// Shows all past trips (TripDate &lt; today).
		
		public async Task<IActionResult> Old()
		{
			var today = DateTime.Today;
			var pastTrips = await _context.Trips
				.Include(t => t.Organizer)
				.Include(t => t.Participants)
				.Where(t => t.TripDate < today)
				.OrderByDescending(t => t.TripDate)
				.ThenByDescending(t => t.PlannedStartTime)
				.ToListAsync();

			return View(pastTrips);
		}

		// GET: /Trips/Details/5
		public async Task<IActionResult> Details(int id)
		{
			// Fetch the trip from the database
			var trip = await _context.Trips
				.Include(t => t.Organizer)
				.Include(t => t.Participants)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (trip == null)
			{
				return NotFound();
			}

			return View(trip);
		}
	}
}
