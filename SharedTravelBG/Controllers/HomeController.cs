using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;


namespace SharedTravelBG.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

		private readonly ApplicationDbContext _context;

		private readonly UserManager<ApplicationUser> _userManager;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			
			_logger = logger;
			_context = context;
			_userManager = userManager;
		}

		[AllowAnonymous]
		public IActionResult Index()
        {
            return View();
        }

		[AllowAnonymous]
		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

		public async Task<IActionResult> Profile()
		{
			// Get the current user's Id from claims.
			string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				// If not logged in, redirect to the Register page.
				return RedirectToAction("Register", "Account", new { area = "Identity" });
			}

			// Retrieve the user (organiser) from the Identity table.
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound();
			}

			// Get the IDs of trips organized by this user.
			var tripIds = await _context.Trips
				.Where(t => t.OrganizerId == userId)
				.Select(t => t.Id)
			.ToListAsync();

			// Get reviews for those trips.
			var reviews = await _context.Reviews
				.Where(r => tripIds.Contains(r.TripId))
				.ToListAsync();

			// Compute the average rating.
			double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
			ViewBag.AverageRating = averageRating;
			ViewBag.ReviewCount = reviews.Count;

			return View(user);
		}



		[HttpGet]
		public async Task<IActionResult> Edit()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			return View(user);
		}

		// POST: /Home/Edit
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ApplicationUser model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			// Only update the fields we allow
			user.UserName = model.UserName;
			user.FullName = model.FullName;
			user.PhoneNumber = model.PhoneNumber;
			// note: we deliberately do NOT touch user.Email

			var result = await _userManager.UpdateAsync(user);
			if (result.Succeeded)
				return RedirectToAction(nameof(Profile));

			// Add any Identity errors back into ModelState so they display
			foreach (var err in result.Errors)
				ModelState.AddModelError(string.Empty, err.Description);

			return View(model);
		}







	}
} 
