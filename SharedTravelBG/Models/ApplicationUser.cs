using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SharedTravelBG.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string FullName { get; set; }

		// Trips in which the user participates
		public ICollection<Trip> TripsParticipated { get; set; } = new List<Trip>();

		// Trips organized by the user
		public ICollection<Trip> OrganizedTrips { get; set; } = new List<Trip>();
	}
}
