using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SharedTravelBG.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Vehicle> Vehicles { get; set; }
		public DbSet<Review> Reviews { get; set; }
		public DbSet<Trip> Trips { get; set; }
		public DbSet<RentedVehicle> RentedVehicles { get; set; }
		public DbSet<RentingCompany> RentingCompanies { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Seed RentingCompanies data.
			modelBuilder.Entity<RentingCompany>().HasData(
				new RentingCompany
				{
					Id = 1,
					Name = "Europcar Bulgaria",
					Address = "16 Tsar Osvoboditel Blvd, Sofia 1000, Bulgaria",
					PhoneNumber = "+359 2 123 4567"
				},
				new RentingCompany
				{
					Id = 3,
					Name = "Sixt Bulgaria",
					Address = "27 Vitosha Blvd, Sofia 1000, Bulgaria",
					PhoneNumber = "+359 2 234 5678"
				},
				new RentingCompany
				{
					Id = 4,
					Name = "Avis Bulgaria",
					Address = "5 Vasil Levski Blvd, Plovdiv 4000, Bulgaria",
					PhoneNumber = "+359 32 123 456"
				},
				new RentingCompany
				{
					Id = 5,
					Name = "Budget Bulgaria",
					Address = "2 Maritime Blvd, Varna 9000, Bulgaria",
					PhoneNumber = "+359 52 765 432"
				},
				new RentingCompany
				{
					Id = 6,
					Name = "OK Rent a Car",
					Address = "10 Georgi Kirkov Blvd, Burgas 8000, Bulgaria",
					PhoneNumber = "+359 56 987 654"
				}
			);

			// Configure one-to-many: One ApplicationUser organizes many Trips
			modelBuilder.Entity<Trip>()
				.HasOne(t => t.Organizer)
				.WithMany(u => u.OrganizedTrips)
				.HasForeignKey(t => t.OrganizerId)
				.OnDelete(DeleteBehavior.Restrict);

			// Configure many-to-many: Trip Participants
			modelBuilder.Entity<Trip>()
				.HasMany(t => t.Participants)
				.WithMany(u => u.TripsParticipated);

			// Configure the relationship for RentedVehicle to use DeleteBehavior.Restrict for Vehicle
			modelBuilder.Entity<RentedVehicle>()
			 .HasOne(rv => rv.Renter)
			 .WithMany() // If you add a collection (e.g., RentedVehicles) in ApplicationUser, use that here.
			 .HasForeignKey(rv => rv.RenterId)
			 .OnDelete(DeleteBehavior.Cascade);

			// (Optional) Configure the Renter relationship if needed:
			// modelBuilder.Entity<RentedVehicle>()
			//    .HasOne(rv => rv.Renter)
			//    .WithMany() // adjust if you have a navigation property on ApplicationUser
			//    .HasForeignKey(rv => rv.RenterId)
			//    .OnDelete(DeleteBehavior.Cascade);
		}

	}
}
