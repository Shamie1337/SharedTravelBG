using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Security.Claims;
using System.Threading.Tasks;

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
				// Set the current logged-in user as the renter.
				rentedVehicle.RenterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				_context.RentedVehicles.Add(rentedVehicle);
				await _context.SaveChangesAsync();
				// Redirect back to the Rentals Index page.
				return RedirectToAction(nameof(Index));
			}
			return View(rentedVehicle);
		}

		// (Existing Edit and Delete actions remain unchanged)
	}
}
