using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SharedTravelBG.Models;
using System.Threading.Tasks;

namespace SharedTravelBG.Controllers
{
	public class AccountController : Controller
	{
		private readonly SignInManager<ApplicationUser> _signInManager;

		public AccountController(SignInManager<ApplicationUser> signInManager)
		{
			_signInManager = signInManager;
		}

		// GET: /Account/Logout
		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			// After logging out, redirect to the Home page.
			return RedirectToAction("Index", "Home");
		}
	}
}
