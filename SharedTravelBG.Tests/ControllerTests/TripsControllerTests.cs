using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedTravelBG.Controllers;
using SharedTravelBG.Models;
using Xunit;

namespace SharedTravelBG.Tests.Controllers
{
	public class TripsControllerTests
	{
		//create in-memory DbContext with exactly three trips
		private ApplicationDbContext CreateContext()
		{
			var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			var ctx = new ApplicationDbContext(opts);
			ctx.Users.AddRange(
		   new ApplicationUser
		   {
			   Id = "org1",
			   UserName = "org1",
			   FullName = "Organizer One",
			   Email = "org1@example.com"
		   },
		   new ApplicationUser
		   {
			   Id = "org2",
			   UserName = "org2",
			   FullName = "Organizer Two",
			   Email = "org2@example.com"
		   },
		   new ApplicationUser
		   {
			   Id = "org3",
			   UserName = "org3",
			   FullName = "Organizer Three",
			   Email = "org3@example.com"
			   }
		   );



			ctx.Trips.AddRange(new[]
			{
				new Trip {
					Id = 1,
					DepartureTown = "Sofia",
					DestinationTown = "Plovdiv",
					TripDate = new DateTime(2025, 6, 1),
					PlannedStartTime = new TimeSpan(9, 0, 0),
					MaxParticipants = 3,
					OrganizerId = "org1",
					Participants = new List<ApplicationUser>()
				},
				new Trip {
					Id = 2,
					DepartureTown = "Varna",
					DestinationTown = "Burgas",
					TripDate = new DateTime(2025, 6, 1),
					PlannedStartTime = new TimeSpan(14, 30, 0),
					MaxParticipants = 4,
					OrganizerId = "org2",
					Participants = new List<ApplicationUser>()
				},
				new Trip {
					Id = 3,
					DepartureTown = "Sofia",
					DestinationTown = "Burgas",
					TripDate = new DateTime(2025, 6, 2),
					PlannedStartTime = new TimeSpan(9, 0, 0),
					MaxParticipants = 5,
					OrganizerId = "org3",
					Participants = new List<ApplicationUser>()
				}
			});
			ctx.SaveChanges();
			return ctx;
		}

		// pull the Trip list out of a ViewResult
		private List<Trip> ExtractTrips(IActionResult result)
		{
			var vr = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<IEnumerable<Trip>>(vr.Model);
			return model.ToList();
		}

		// build a controller, optionally simulating a logged-in user
		private TripsController GetController(ApplicationDbContext ctx, string? userId = null)
		{
			var ctrl = new TripsController(ctx);
			if (userId != null)
			{
				ctrl.ControllerContext = new ControllerContext
				{
					HttpContext = new DefaultHttpContext
					{
						User = new ClaimsPrincipal(new ClaimsIdentity(
							new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "Test"))
					}
				};
			}
			return ctrl;
		}

		[Fact]
		public async Task Index_NoFilters_ReturnsThreeTrips_And_ClearsViewData()
		{
			// Arrange
			var ctx = CreateContext();
			var controller = GetController(ctx);

			// Act
			var result = await controller.Index(null, null, null, null);
			var trips = ExtractTrips(result);

			// Assert
			Assert.Equal(3, trips.Count);
			Assert.Equal("", controller.ViewData["departure"]);
			Assert.Equal("", controller.ViewData["destination"]);
			Assert.Equal("", controller.ViewData["date"]);
			Assert.Equal("", controller.ViewData["time"]);
		}

		[Fact]
		public async Task Index_FilterByDepartureOnly_ReturnsOnlySofiaTrips()
		{
			// Arrange
			var ctx = CreateContext();
			var controller = GetController(ctx);

			// Act
			var result = await controller.Index("Sofia", null, null, null);
			var trips = ExtractTrips(result);

			// Assert
			Assert.Equal(2, trips.Count);
			Assert.All(trips, t => Assert.Contains("Sofia", t.DepartureTown));
			Assert.Equal("Sofia", controller.ViewData["departure"]);
		}

		[Fact]
		public async Task Join_AddsParticipant_WhenNotAlreadyParticipating_AndRedirectsToIndex()
		{
			// Arrange: seed a valid user with required fields
			var ctx = CreateContext();
			var user = new ApplicationUser
			{
				Id = "u1",
				UserName = "user1",
				FullName = "User One",
				Email = "user1@example.com"
			};
			ctx.Users.Add(user);
			ctx.SaveChanges();

			var controller = GetController(ctx, "u1");

			// Act
			var result = await controller.Join(1);
			var trip = await ctx.Trips.Include(t => t.Participants).FirstAsync(t => t.Id == 1);

			// Assert: user was added
			Assert.Contains(trip.Participants, p => p.Id == "u1");

			// And we redirect to Index
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(TripsController.Index), redirect.ActionName);
		}

		[Fact]
		public async Task Leave_RemovesParticipant_WhenParticipating_AndRedirectsToIndex()
		{
			// Arrange: seed a valid user and add them as a participant
			var ctx = CreateContext();
			var user = new ApplicationUser
			{
				Id = "u2",
				UserName = "user2",
				FullName = "User Two",
				Email = "user2@example.com"
			};
			ctx.Users.Add(user);
			var trip = await ctx.Trips.FirstAsync(t => t.Id == 2);
			trip.Participants.Add(user);
			ctx.SaveChanges();

			var controller = GetController(ctx, "u2");

			// Act
			var result = await controller.Leave(2);
			var updated = await ctx.Trips.Include(t => t.Participants).FirstAsync(t => t.Id == 2);

			// Assert: user was removed
			Assert.DoesNotContain(updated.Participants, p => p.Id == "u2");

			// And we redirect to Index
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(TripsController.Index), redirect.ActionName);
		}
	}
}
