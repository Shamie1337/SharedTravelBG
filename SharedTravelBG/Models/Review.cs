// Models/Review.cs
namespace SharedTravelBG.Models
{
	public class Review
	{
		public int Id { get; set; }
		public string Content { get; set; }
		public int Rating { get; set; }  // e.g., 1-5 rating

		// Reviewer relationship (User who writes the review)
		public string ReviewerId { get; set; }
		public ApplicationUser Reviewer { get; set; }

		// Optional: A review can be linked to a trip
		public int? TripId { get; set; }
		public Trip Trip { get; set; }
	}
}
