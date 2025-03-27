// Models/Trip.cs
using System;
using System.Collections.Generic;

namespace SharedTravelBG.Models
{
	public class Trip
	{
		public int Id { get; set; }
		public string DepartureTown { get; set; }
		public string DestinationTown { get; set; }
		public DateTime TripDate { get; set; }

		// Relationship: Organizer is a user who created the trip
		public string OrganizerId { get; set; }
		public ApplicationUser Organizer { get; set; }

		// Many-to-Many: Participants joining the trip
		public ICollection<ApplicationUser> Participants { get; set; } = new List<ApplicationUser>();
	}
}
