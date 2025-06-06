using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Models;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Retrieve the connection string from configuration.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register the ApplicationDbContext.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.ConfigureApplicationCookie(options =>
{
	// When a user is not authenticated, redirect to the Register page.
	options.LoginPath = "/Identity/Account/Register";
});



// Add Identity services with role support.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;
})
	.AddRoles<IdentityRole>()  // Enable role support
	.AddEntityFrameworkStores<ApplicationDbContext>();

// Register the custom claims factory
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();


// Add controllers with views.
builder.Services.AddControllersWithViews(options =>
{
	// Add a global authorization policy if needed.
	// var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
	// options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddRazorPages(options =>
{
	// Allow access to Identity pages (Register, Login, etc.)
	options.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");
});

builder.Services.AddControllersWithViews(options =>
{
	// This policy requires that all actions (by default) need an authenticated user.
	var policy = new AuthorizationPolicyBuilder()
					 .RequireAuthenticatedUser()
					 .Build();
	options.Filters.Add(new AuthorizeFilter(policy));
});


builder.Services.AddSignalR();




var app = builder.Build();



// (Standard middleware configuration below)
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error/500");  // Redirects to /Error/500 for exceptions.
	app.UseStatusCodePagesWithReExecute("/Error/{0}");  // For non-success status codes (e.g., 404).
}
else
{
	app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");



app.MapRazorPages();


// Seed roles and admin user after building the app.
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

	// Define the admin role.
	string adminRole = "Admin";
	if (!await roleManager.RoleExistsAsync(adminRole))
	{
		await roleManager.CreateAsync(new IdentityRole(adminRole));
	}

	

	// Create an admin user (update email/password as desired).
	string adminEmail = "nikolashalamanov@abv.bg";
	var adminUser = await userManager.FindByEmailAsync(adminEmail);
	if (adminUser == null)
	{
		adminUser = new ApplicationUser
		{
			UserName = adminEmail,
			Email = adminEmail,
			FullName = "Admin User"
		};
		var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
		if (createResult.Succeeded)
		{
			await userManager.AddToRoleAsync(adminUser, adminRole);
		}
	}
}

app.Run();
