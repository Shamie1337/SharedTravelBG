using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SharedTravelBG.Controllers
{
	public class VehiclesController : Controller
	{
		private readonly ApplicationDbContext _context;

		public VehiclesController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Vehicles
		public async Task<IActionResult> Index()
		{
			var vehicles = _context.Vehicles.Include(v => v.Owner);
			return View(await vehicles.ToListAsync());
		}

		// GET: Vehicles/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var vehicle = await _context.Vehicles
				.Include(v => v.Owner)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (vehicle == null) return NotFound();

			return View(vehicle);
		}

		// GET: Vehicles/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Vehicles/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Make,Model,Year,Color")] Vehicle vehicle)
		{
			if (ModelState.IsValid)
			{
				// Set the current user as the owner
				vehicle.OwnerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
				_context.Add(vehicle);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(vehicle);
		}

		// GET: Vehicles/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var vehicle = await _context.Vehicles.FindAsync(id);
			if (vehicle == null) return NotFound();
			return View(vehicle);
		}

		// POST: Vehicles/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Make,Model,Year,Color")] Vehicle vehicle)
		{
			if (id != vehicle.Id) return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(vehicle);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!VehicleExists(vehicle.Id))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}
			return View(vehicle);
		}

		// GET: Vehicles/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var vehicle = await _context.Vehicles
				.Include(v => v.Owner)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (vehicle == null) return NotFound();

			return View(vehicle);
		}

		// POST: Vehicles/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var vehicle = await _context.Vehicles.FindAsync(id);
			_context.Vehicles.Remove(vehicle);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool VehicleExists(int id)
		{
			return _context.Vehicles.Any(v => v.Id == id);
		}
	}
}
