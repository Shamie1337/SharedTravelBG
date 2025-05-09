using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedTravelBG.Models;
using System.Threading.Tasks;

namespace SharedTravelBG.Areas.Identity.Pages.Account
{
	public class RegisterModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ILogger<RegisterModel> _logger;

		public RegisterModel(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ILogger<RegisterModel> logger)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
		}

		// This property is used to redirect the user after registration.
		public string ReturnUrl { get; set; } = string.Empty;

		[BindProperty]
		public InputModel Input { get; set; } = new InputModel();

		public class InputModel
		{
			[Required]
			[Display(Name = "Full Name")]
			public string FullName { get; set; }

			[Required]
			[EmailAddress]
			[Display(Name = "Email")]
			public string Email { get; set; }

			[Required]
			[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
			[DataType(DataType.Password)]
			[Display(Name = "Password")]
			public string Password { get; set; }

			[DataType(DataType.Password)]
			[Display(Name = "Confirm Password")]
			[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
			public string ConfirmPassword { get; set; }
		}

		public void OnGet(string returnUrl = null)
		{
			// Set ReturnUrl to the provided value, or default to home if not provided.
			ReturnUrl = returnUrl ?? Url.Content("~/");
		}

		public async Task<IActionResult> OnPostAsync(string returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");
			ReturnUrl = returnUrl;

			if (!ModelState.IsValid)
			{
				foreach (var key in ModelState.Keys)
				{
					foreach (var error in ModelState[key].Errors)
					{
						System.Diagnostics.Debug.WriteLine($"Field: {key} Error: {error.ErrorMessage}");
					}
				}
				return Page();
			}

			if (!ModelState.IsValid)
			{
				// For debugging purposes: print ModelState errors.
				foreach (var key in ModelState.Keys)
				{
					foreach (var error in ModelState[key].Errors)
					{
						System.Diagnostics.Debug.WriteLine($"Field: {key} Error: {error.ErrorMessage}");
					}
				}
				return Page();
			}

			var user = new ApplicationUser
			{
				UserName = Input.Email,
				Email = Input.Email,
				FullName = Input.FullName // Set the full name from the input.
			};

			var result = await _userManager.CreateAsync(user, Input.Password);
			if (result.Succeeded)
			{
				_logger.LogInformation("User created a new account with password.");
				await _signInManager.SignInAsync(user, isPersistent: false);
				return LocalRedirect(returnUrl);
			}
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
			return Page();
		}
	}
}
