using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SharedTravelBG.Controllers
{
	public class RentedVehiclesController : Controller
	{
		private readonly ApplicationDbContext _context;

		public RentedVehiclesController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: RentedVehicles/Index
		public async Task<IActionResult> Index()
		{
			var rentals = await _context.RentedVehicles
				.Include(rv => rv.Renter)
				.ToListAsync();
			return View(rentals);
		}

		// GET: RentedVehicles/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: RentedVehicles/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(RentedVehicle rentedVehicle)
		{
			if (ModelState.IsValid)
			{
				// Automatically set the RenterId using the current logged-in user.
				rentedVehicle.RenterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				_context.Add(rentedVehicle);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(rentedVehicle);
		}

		// GET: RentedVehicles/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return NotFound();

			var rentedVehicle = await _context.RentedVehicles.FindAsync(id);
			if (rentedVehicle == null)
				return NotFound();

			string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			// Allow editing if the current user is the creator or an admin.
			if (rentedVehicle.RenterId != currentUserId && !User.IsInRole("Admin"))
				return Forbid();

			return View(rentedVehicle);
		}

		// POST: RentedVehicles/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, RentedVehicle rentedVehicle)
		{
			if (id != rentedVehicle.Id)
				return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					// Preserve the original RenterId
					var original = await _context.RentedVehicles.AsNoTracking().FirstOrDefaultAsync(rv => rv.Id == id);
					if (original != null)
						rentedVehicle.RenterId = original.RenterId;

					_context.Update(rentedVehicle);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.RentedVehicles.Any(rv => rv.Id == rentedVehicle.Id))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}
			return View(rentedVehicle);
		}

		// GET: RentedVehicles/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var rentedVehicle = await _context.RentedVehicles
				.Include(rv => rv.Renter)
				.FirstOrDefaultAsync(rv => rv.Id == id);
			if (rentedVehicle == null)
				return NotFound();
			return View(rentedVehicle);
		}

		// GET: RentedVehicles/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();

			var rentedVehicle = await _context.RentedVehicles
				.Include(rv => rv.Renter)
				.FirstOrDefaultAsync(rv => rv.Id == id);
			if (rentedVehicle == null)
				return NotFound();

			string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			// Allow deletion if the user is the creator or if the user is an admin.
			if (rentedVehicle.RenterId != currentUserId && !User.IsInRole("Admin"))
				return Forbid();

			return View(rentedVehicle);
		}

		// POST: RentedVehicles/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var rentedVehicle = await _context.RentedVehicles.FindAsync(id);
			if (rentedVehicle == null)
				return NotFound();

			string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (rentedVehicle.RenterId != currentUserId && !User.IsInRole("Admin"))
				return Forbid();

			_context.RentedVehicles.Remove(rentedVehicle);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		
	}
}
