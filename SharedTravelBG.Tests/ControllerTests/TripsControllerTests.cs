using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Controllers;
using SharedTravelBG.Data;
using SharedTravelBG.Models;
using Xunit;

namespace SharedTravelBG.Tests.Controllers
{
	public class TripsControllerTests
	{
		private class TestTempDataProvider : ITempDataProvider
		{
			public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
			public void SaveTempData(HttpContext context, IDictionary<string, object> values) { /* no-op */ }
		}

		// Helper: create an in-memory DbContext seeded with users and trips
		private ApplicationDbContext CreateContext()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var ctx = new ApplicationDbContext(options);

			// Seed users: two organizers and two normal users
			ctx.Users.AddRange(
				new ApplicationUser { Id = "org1", UserName = "org1", FullName = "Organizer One", Email = "org1@x.com" },
				new ApplicationUser { Id = "org2", UserName = "org2", FullName = "Organizer Two", Email = "org2@x.com" },
				new ApplicationUser { Id = "user1", UserName = "user1", FullName = "User One", Email = "user1@x.com" },
				new ApplicationUser { Id = "user2", UserName = "user2", FullName = "User Two", Email = "user2@x.com" }
			);

			// Today reference
			var today = DateTime.Today;

			// Seed trips:
			// Trip A: tomorrow (future), organized by org1, max 2 participants
			// Trip B: yesterday (past), organized by org2
			// Trip C: today (counts as future), organized by org1, max 1 participant
			ctx.Trips.AddRange(
				new Trip
				{
					Id = 1,
					DepartureTown = "Sofia",
					DestinationTown = "Plovdiv",
					TripDate = today.AddDays(1),
					PlannedStartTime = new TimeSpan(9, 0, 0),
					MaxParticipants = 2,
					OrganizerId = "org1",
					OrganizerPhoneNumber = "555-0001"
				},
				new Trip
				{
					Id = 2,
					DepartureTown = "Varna",
					DestinationTown = "Burgas",
					TripDate = today.AddDays(-1),
					PlannedStartTime = new TimeSpan(10, 0, 0),
					MaxParticipants = 3,
					OrganizerId = "org2",
					OrganizerPhoneNumber = "555-0002"
				},
				new Trip
				{
					Id = 3,
					DepartureTown = "Sofia",
					DestinationTown = "Burgas",
					TripDate = today,
					PlannedStartTime = new TimeSpan(8, 30, 0),
					MaxParticipants = 1,
					OrganizerId = "org1",
					OrganizerPhoneNumber = "555-0001"
				}
			);

			ctx.SaveChanges();
			return ctx;
		}

		/// <summary>
		/// Creates a TripsController with HttpContext.User set to the given userId,
		/// and with a TempDataDictionary backed by TestTempDataProvider.
		/// </summary>
		private TripsController GetController(ApplicationDbContext ctx, string userId)
		{
			var controller = new TripsController(ctx);

			// 1) Simulate an authenticated user
			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = new ClaimsPrincipal(new ClaimsIdentity(
						new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth"))
				}
			};

			// 2) Give the controller a TempDataDictionary so that TempData["Message"] works
			controller.TempData = new TempDataDictionary(
				controller.HttpContext,
				new TestTempDataProvider()
			);

			return controller;
		}

		// Helper: extract list of trips from a ViewResult
		private List<Trip> ExtractTrips(IActionResult result)
		{
			var vr = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<List<Trip>>(vr.Model);
			return model;
		}

		[Fact]
		public async Task Index_NoFilters_ReturnsOnlyFutureTrips_NotFull()
		{
			// Arrange
			var ctx = CreateContext();
			// Pre‐fill Trip #3 so it's full (maxParticipants=1)
			var trip3 = await ctx.Trips.Include(t => t.Participants).FirstAsync(t => t.Id == 3);
			var user = await ctx.Users.FindAsync("user1");
			trip3.Participants.Add(user);
			await ctx.SaveChangesAsync();

			var controller = GetController(ctx, "user1");

			// Act
			var result = await controller.Index(null, null, null, null, null);
			var trips = ExtractTrips(result);

			// Trip #1 (tomorrow) should appear; Trip #2 (past) excluded; Trip #3 (today but full) excluded
			Assert.Single(trips);
			Assert.Contains(trips, t => t.Id == 1);
		}

		[Fact]
		public async Task Index_FilterByDeparture_ReturnsMatchingTrips()
		{
			// Arrange
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			// Act: filter departure = "Sofia"
			var result = await controller.Index("Sofia", null, null, null, null);
			var trips = ExtractTrips(result);

			// Only Trip #1 (future, dep=“Sofia”) and Trip #3 (today, dep=“Sofia” but unfilled) appear.
			// But since Trip #3 is not yet full, it appears. 
			Assert.Equal(2, trips.Count);
			Assert.All(trips, t => Assert.Contains("Sofia", t.DepartureTown));
		}

		[Fact]
		public async Task Index_FilterByDestination_ReturnsMatchingTrips()
		{
			// Arrange
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			// Act: filter destination = "Burgas"
			var result = await controller.Index(null, "Burgas", null, null, null);
			var trips = ExtractTrips(result);

			// Trip #2 is past—excluded. Trip #3 (today, dest="Burgas") appears if not full.
			Assert.Single(trips);
			Assert.Equal(3, trips[0].Id);
		}

		[Fact]
		public async Task Index_FilterByDate_ReturnsMatchingTrip()
		{
			// Arrange
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");
			var targetDate = DateTime.Today.AddDays(1);

			// Act: filter date = tomorrow
			var result = await controller.Index(null, null, targetDate, null, null);
			var trips = ExtractTrips(result);

			Assert.Single(trips);
			Assert.Equal(1, trips[0].Id);
		}

		[Fact]
		public async Task Index_MinAvailableSpots_FiltersCorrectly()
		{
			// Arrange
			var ctx = CreateContext();
			// Trip #1 max=2, no participants => available=2
			// Trip #3 max=1, no participants => available=1
			var controller = GetController(ctx, "user1");

			// Act: minAvailableSpots = 2
			var result = await controller.Index(null, null, null, null,2);
			var trips = ExtractTrips(result);

			// Only Trip #1 has available >=2
			Assert.Single(trips);
			Assert.Equal(1, trips[0].Id);
		}

		[Fact]
		public async Task Join_AsOrganizer_ShowsErrorMessage()
		{
			// Arrange: org1 tries to join own trip #1
			var ctx = CreateContext();
			var controller = GetController(ctx, "org1");

			// Act
			var result = await controller.Join(1);
			var redirect = Assert.IsType<RedirectToActionResult>(result);

			// Should redirect to Index and have TempData["Message"]
			Assert.Equal(nameof(TripsController.Index), redirect.ActionName);
			Assert.Equal("Не можете да се присъедините към собственото си пътуване.",
						 controller.TempData["Message"]);
		}

		[Fact]
		public async Task Join_AddsParticipant_WhenSpaceAvailable()
		{
			// Arrange
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			// Act
			var result = await controller.Join(1);
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(TripsController.Index), redirect.ActionName);

			// Verify user1 was added to trip #1 participants
			var trip = await ctx.Trips.Include(t => t.Participants).FirstAsync(t => t.Id == 1);
			Assert.Contains(trip.Participants, p => p.Id == "user1");
		}
		[Fact]
		public async Task Join_WhenFull_ShowsErrorMessage()
		{
			// Arrange: fill Trip #3 (max = 1) first
			var ctx = CreateContext();
			var trip3 = await ctx.Trips
								 .Include(t => t.Participants)
								 .FirstAsync(t => t.Id == 3);
			var userA = await ctx.Users.FindAsync("user1");
			trip3.Participants.Add(userA);
			await ctx.SaveChangesAsync();

			// Now user2 tries to join #3
			var controller = GetController(ctx, "user2");

			// Initialize TempData so that setting TempData["Message"] works
			controller.TempData = new TempDataDictionary(
				controller.HttpContext,
				new TestTempDataProvider()
			);

			// Act
			var result = await controller.Join(3);
			var redirect = Assert.IsType<RedirectToActionResult>(result);

			// Assert redirect goes back to Index
			Assert.Equal(nameof(TripsController.Index), redirect.ActionName);

			// Now TempData["Message"] should contain the “full” message
			Assert.Equal(
				"Съжаляваме, това пътуване е пълно.",
				controller.TempData["Message"]
			);
		}

		[Fact]
		public async Task Leave_RemovesParticipant_AndRedirects()
		{
			// Arrange: add user1 to trip #1
			var ctx = CreateContext();
			var trip1 = await ctx.Trips.Include(t => t.Participants).FirstAsync(t => t.Id == 1);
			var userA = await ctx.Users.FindAsync("user1");
			trip1.Participants.Add(userA);
			await ctx.SaveChangesAsync();

			var controller = GetController(ctx, "user1");
			var result = await controller.Leave(1);
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(TripsController.Index), redirect.ActionName);

			// Verify user1 was removed
			trip1 = await ctx.Trips.Include(t => t.Participants).FirstAsync(t => t.Id == 1);
			Assert.DoesNotContain(trip1.Participants, p => p.Id == "user1");
		}

		[Fact]
		public async Task MyTrips_ReturnsOnlyUserParticipatedTrips()
		{
			// Arrange: user1 participates in trip #1 and #3
			var ctx = CreateContext();
			var trip1 = await ctx.Trips.Include(t => t.Participants).FirstAsync(t => t.Id == 1);
			var trip3 = await ctx.Trips.Include(t => t.Participants).FirstAsync(t => t.Id == 3);
			var userA = await ctx.Users.FindAsync("user1");
			trip1.Participants.Add(userA);
			trip3.Participants.Add(userA);
			await ctx.SaveChangesAsync();

			var controller = GetController(ctx, "user1");
			var result = await controller.MyTrips();
			var view = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<List<Trip>>(view.Model);

			Assert.Equal(2, model.Count);
			Assert.Contains(model, t => t.Id == 1);
			Assert.Contains(model, t => t.Id == 3);
		}

		[Fact]
		public async Task MyOrganized_ReturnsOnlyUserOrganizedTrips()
		{
			// Arrange: org1 organizes trips #1 and #3
			var ctx = CreateContext();
			var controller = GetController(ctx, "org1");

			var result = await controller.MyOrganized();
			var view = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<List<Trip>>(view.Model);

			Assert.Equal(2, model.Count);
			Assert.Contains(model, t => t.Id == 1);
			Assert.Contains(model, t => t.Id == 3);
		}

		[Fact]
		public async Task Old_ReturnsOnlyPastTrips()
		{
			// Arrange
			var ctx = CreateContext();
			var controller = GetController(ctx, "user1");

			var result = await controller.Old();
			var view = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<List<Trip>>(view.Model);

			// Only trip #2 is past (TripDate = yesterday)
			Assert.Single(model);
			Assert.Equal(2, model[0].Id);
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
			var model = Assert.IsType<Trip>(view.Model);
			Assert.Equal(1, model.Id);
			Assert.Equal("Sofia", model.DepartureTown);
		}
	}
}
