using System;
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
	public class ReviewsControllerTests
	{
		private ApplicationDbContext CreateContext()
		{
			var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var ctx = new ApplicationDbContext(opts);

			// Seed a trip
			ctx.Trips.Add(new Trip
			{
				Id = 10,
				OrganizerId = "org1",
				DepartureTown = "A",
				DestinationTown = "B",
				TripDate = DateTime.Today,
				PlannedStartTime = TimeSpan.Zero,
				MaxParticipants = 3
			});

			// Seed users **with FullName** (and Email, if required)
			ctx.Users.AddRange(
				new ApplicationUser
				{
					Id = "org1",
					UserName = "organizer",
					FullName = "Organizer One",
					Email = "org1@example.com"
				},
				new ApplicationUser
				{
					Id = "user1",
					UserName = "reviewer",
					FullName = "Reviewer One",
					Email = "user1@example.com"
				}
			);

			ctx.SaveChanges();
			return ctx;
		}

		private ReviewsController GetController(ApplicationDbContext ctx, string userId)
		{
			var ctrl = new ReviewsController(ctx);
			ctrl.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = new ClaimsPrincipal(new ClaimsIdentity(
						new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth"))
				}
			};
			return ctrl;
		}

		[Fact]
		public async Task Create_OrganizerCannotReviewOwnTrip_AddsModelError()
		{
			var ctx = CreateContext();
			var ctrl = GetController(ctx, "org1");

			var review = new Review { TripId = 10, Content = "X", Rating = 5 };
			var result = await ctrl.Create(review);

			var view = Assert.IsType<ViewResult>(result);
			Assert.False(view.ViewData.ModelState.IsValid);
			Assert.Contains(
				view.ViewData.ModelState[string.Empty].Errors
					.Select(e => e.ErrorMessage),
				msg => msg.Contains("cannot write a review")
			);
		}

		[Fact]
		public async Task Create_UserCanReview_RedirectsAndSaves()
		{
			var ctx = CreateContext();
			var ctrl = GetController(ctx, "user1");

			var review = new Review { TripId = 10, Content = "Nice", Rating = 4 };
			var result = await ctrl.Create(review);

			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal("Index", redirect.ActionName);

			var saved = ctx.Reviews.SingleOrDefault(r => r.TripId == 10 && r.ReviewerId == "user1");
			Assert.NotNull(saved);
			Assert.Equal("Nice", saved.Content);
			Assert.Equal(4, saved.Rating);
		}
	}
}
