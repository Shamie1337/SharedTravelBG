using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Controllers;
using SharedTravelBG.Data;
using SharedTravelBG.Models;
using Xunit;

namespace SharedTravelBG.Tests.Controllers
{
	public class ReviewsControllerTests
	{
		/// <summary>
		/// Creates an in‐memory ApplicationDbContext with:
		/// - Two users: "org1" (organizer) and "user1" (reviewer)
		/// - Three trips:
		///     100: yesterday (past)
		///     101: tomorrow (future)
		///     102: two days ago (past, by a different organizer)
		/// - One existing review by user2 on trip 100
		/// </summary>
		private ApplicationDbContext CreateContext()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			var ctx = new ApplicationDbContext(options);

			// Seed users
			ctx.Users.AddRange(
				new ApplicationUser { Id = "org1", UserName = "organizer", FullName = "Org One", Email = "org1@x.com" },
				new ApplicationUser { Id = "user1", UserName = "reviewer", FullName = "User One", Email = "user1@x.com" },
				new ApplicationUser { Id = "user2", UserName = "user2", FullName = "User Two", Email = "user2@x.com" }
			);

			var today = DateTime.Today;

			// Seed trips
			ctx.Trips.AddRange(
				new Trip
				{
					Id = 100,
					OrganizerId = "org1",
					DepartureTown = "A",
					DestinationTown = "B",
					TripDate = today.AddDays(-1),  // past
					PlannedStartTime = TimeSpan.Zero,
					MaxParticipants = 5
				},
				new Trip
				{
					Id = 101,
					OrganizerId = "org1",
					DepartureTown = "C",
					DestinationTown = "D",
					TripDate = today.AddDays(1),   // future
					PlannedStartTime = TimeSpan.Zero,
					MaxParticipants = 5
				},
				new Trip
				{
					Id = 102,
					OrganizerId = "user2",
					DepartureTown = "E",
					DestinationTown = "F",
					TripDate = today.AddDays(-2),  // past
					PlannedStartTime = TimeSpan.Zero,
					MaxParticipants = 5
				}
			);

			// Seed existing review by user2 on trip 100
			ctx.Reviews.Add(new Review
			{
				Id = 1,
				TripId = 100,
				ReviewerId = "user2",
				Content = "Existing review",
				Rating = 4
			});

			ctx.SaveChanges();
			return ctx;
		}

		/// <summary>
		/// Constructs a ReviewsController whose HttpContext.User
		/// has NameIdentifier = userId. If isAdmin=true, adds role claim.
		/// </summary>
		private ReviewsController GetController(ApplicationDbContext ctx, string userId, bool isAdmin = false)
		{
			var ctrl = new ReviewsController(ctx);
			var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
			if (isAdmin)
				claims.Add(new Claim(ClaimTypes.Role, "Admin"));

			ctrl.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
				}
			};
			return ctrl;
		}

		// Minimal ITempDataProvider for unit tests
		private class TestTempDataProvider : ITempDataProvider
		{
			public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
			public void SaveTempData(HttpContext context, IDictionary<string, object> values) { /* no-op */ }
		}

		[Fact]
		public async Task Index_ReturnsAllReviews()
		{
			// Arrange
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			// Act
			var result = await controller.Index();
			var view = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<List<Review>>(view.Model);

			// One seeded review should appear
			Assert.Single(model);
			Assert.Equal(1, model[0].Id);
			Assert.Equal("Existing review", model[0].Content);
		}

		[Fact]
		public async Task Details_NullId_ReturnsNotFound()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			var result = await controller.Details(null);
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task Details_InvalidId_ReturnsNotFound()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			var result = await controller.Details(999);
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task Details_ValidId_ReturnsViewWithModel()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			var result = await controller.Details(1);
			var view = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Review>(view.Model);
			Assert.Equal(1, model.Id);
			Assert.Equal("Existing review", model.Content);
		}

		[Fact]
		public async Task CreateGet_PopulatesPastTripsOnly()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			var result = await controller.Create();
			var view = Assert.IsType<ViewResult>(result);

			var selectList = Assert.IsType<SelectList>(view.ViewData["Trips"]);
			var items = selectList.Items.Cast<dynamic>().ToList();

			// Trips with TripDate < today: IDs 100 and 102
			var ids = items.Select(x => (int)x.GetType().GetProperty("Id")!.GetValue(x)!).ToList();
			Assert.Contains(100, ids);
			Assert.Contains(102, ids);
			Assert.DoesNotContain(101, ids); // future trip excluded
		}

		[Fact]
		public async Task CreatePost_TripDoesNotExist_ReturnsViewWithModelError()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			var review = new Review { TripId = 999, Content = "Test", Rating = 3 };

			var result = await controller.Create(review);
			var view = Assert.IsType<ViewResult>(result);
			Assert.False(controller.ModelState.IsValid);
			Assert.Contains(
				controller.ModelState[string.Empty].Errors.Select(e => e.ErrorMessage),
				msg => msg.Contains("does not exist")
			);
		}

		[Fact]
		public async Task CreatePost_FutureTrip_RedirectsToCreateWithTempData()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			// Initialize TempData
			controller.TempData = new TempDataDictionary(
				controller.HttpContext,
				new TestTempDataProvider()
			);

			var review = new Review { TripId = 101, Content = "Future test", Rating = 2 };

			var result = await controller.Create(review);
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(ReviewsController.Create), redirect.ActionName);

			Assert.Equal(
				"You cannot write a review for a trip that has not yet occurred.",
				controller.TempData["ErrorMessage"]
			);
		}

		[Fact]
		public async Task CreatePost_OrganizerOwnTrip_RedirectsToCreateWithTempData()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "org1");

			// Initialize TempData
			controller.TempData = new TempDataDictionary(
				controller.HttpContext,
				new TestTempDataProvider()
			);

			var review = new Review { TripId = 100, Content = "Own trip", Rating = 5 };

			var result = await controller.Create(review);
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(ReviewsController.Create), redirect.ActionName);

			Assert.Equal(
				"You cannot write a review for your own trip.",
				controller.TempData["ErrorMessage"]
			);
		}

		[Fact]
		public async Task CreatePost_InvalidModelState_ReturnsView()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			controller.ModelState.AddModelError("Content", "Required");

			var review = new Review { TripId = 100, Content = "", Rating = 0 };

			var result = await controller.Create(review);
			var view = Assert.IsType<ViewResult>(result);
			Assert.False(controller.ModelState.IsValid);
		}

		[Fact]
		public async Task CreatePost_Success_AddsReviewAndRedirects()
		{
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			// Initialize TempData (not strictly needed here, but safe)
			controller.TempData = new TempDataDictionary(
				controller.HttpContext,
				new TestTempDataProvider()
			);

			var review = new Review { TripId = 100, Content = "Good trip", Rating = 4 };

			var result = await controller.Create(review);
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(ReviewsController.Index), redirect.ActionName);

			var saved = ctx.Reviews.SingleOrDefault(r => r.TripId == 100 && r.ReviewerId == "user1");
			Assert.NotNull(saved);
			Assert.Equal("Good trip", saved.Content);
			Assert.Equal(4, saved.Rating);
		}

		[Fact]
		public async Task MyReviews_ReturnsOnlyCurrentUsersReviews()
		{
			var ctx = CreateContext();

			// Add a second review by user1 on trip 102
			ctx.Reviews.Add(new Review
			{
				TripId = 102,
				ReviewerId = "user1",
				Content = "Another review",
				Rating = 5
			});
			ctx.SaveChanges();

			var controllerA = GetController(ctx, "user1");
			var resultA = await controllerA.MyReviews();
			var viewA = Assert.IsType<ViewResult>(resultA);
			var listA = Assert.IsAssignableFrom<List<Review>>(viewA.Model);

			Assert.Single(listA);
			Assert.Equal("Another review", listA[0].Content);

			var controllerB = GetController(ctx, "user2");
			var resultB = await controllerB.MyReviews();
			var viewB = Assert.IsType<ViewResult>(resultB);
			var listB = Assert.IsAssignableFrom<List<Review>>(viewB.Model);

			Assert.Single(listB);
			Assert.Equal(1, listB[0].Id);
		}
	}
}
