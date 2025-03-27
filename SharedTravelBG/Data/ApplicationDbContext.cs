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
