// Models/Vehicle.cs
namespace SharedTravelBG.Models
{
	public class Vehicle
	{
		public int Id { get; set; }
		public string Make { get; set; }      // e.g., Toyota, BMW
		public string Model { get; set; }     // e.g., Corolla, 3 Series
		public int Year { get; set; }
		public string Color { get; set; }

		// Relationship: Each vehicle is owned by an ApplicationUser
		public string OwnerId { get; set; }
		public ApplicationUser Owner { get; set; }
	}
}
