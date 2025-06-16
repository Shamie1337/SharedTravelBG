using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedTravelBG.Models;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
	private readonly UserManager<ApplicationUser> _userManager;

	public AdminController(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	// GET: /Admin/Users
	public IActionResult Users()
	{
		var users = _userManager.Users.ToList();
		return View(users);
	}

	// POST: /Admin/Ban/5
	[HttpPost]
	public async Task<IActionResult> Ban(string id)
	{
		var user = await _userManager.FindByIdAsync(id);
		if (user != null)
		{
			// Enable lockout and set far-future date
			await _userManager.SetLockoutEnabledAsync(user, true);
			await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
		}
		return RedirectToAction(nameof(Users));
	}

	// POST: /Admin/Unban/5
	[HttpPost]
	public async Task<IActionResult> Unban(string id)
	{
		var user = await _userManager.FindByIdAsync(id);
		if (user != null)
		{
			await _userManager.SetLockoutEndDateAsync(user, null);
		}
		return RedirectToAction(nameof(Users));
	}

	// POST: /Admin/Delete/5
	[HttpPost]
	public async Task<IActionResult> Delete(string id)
	{
		var user = await _userManager.FindByIdAsync(id);
		if (user != null)
		{
			await _userManager.DeleteAsync(user);
		}
		return RedirectToAction(nameof(Users));
	}
}
