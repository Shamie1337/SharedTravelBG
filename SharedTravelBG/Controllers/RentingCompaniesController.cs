using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Threading.Tasks;

namespace SharedTravelBG.Controllers
{
	// Do not mark the whole controller with [Authorize(Roles="Admin")]
	// so that the Index and Details actions remain accessible to all authenticated users.
	public class RentingCompaniesController : Controller
	{
		private readonly ApplicationDbContext _context;

		public RentingCompaniesController(ApplicationDbContext context)
		{
			_context = context;
		}

		// Index action is now protected by the global authorization filter.
		public async Task<IActionResult> Index()
		{
			return View(await _context.RentingCompanies.ToListAsync());
		}

		// Details action: accessible to all authenticated users.
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var company = await _context.RentingCompanies.FirstOrDefaultAsync(m => m.Id == id);
			if (company == null) return NotFound();

			return View(company);
		}

		// Only Admin can create a new Renting Company.
		[Authorize(Roles = "Admin")]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([Bind("Name,Address,PhoneNumber")] RentingCompany company)
		{
			if (ModelState.IsValid)
			{
				_context.Add(company);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(company);
		}

		// Only Admin can edit a Renting Company.
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var company = await _context.RentingCompanies.FindAsync(id);
			if (company == null) return NotFound();
			return View(company);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,PhoneNumber")] RentingCompany company)
		{
			if (id != company.Id) return NotFound();

			if (ModelState.IsValid)
			{
				_context.Update(company);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(company);
		}

		// Only Admin can delete a Renting Company.
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var company = await _context.RentingCompanies.FirstOrDefaultAsync(m => m.Id == id);
			if (company == null) return NotFound();

			return View(company);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var company = await _context.RentingCompanies.FindAsync(id);
			_context.RentingCompanies.Remove(company);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}
