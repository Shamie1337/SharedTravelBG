using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SharedTravelBG.Models
{
	public class Review
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Review content is required.")]
		public string Content { get; set; } = string.Empty;

		[Range(1, 5, ErrorMessage = "Please enter a rating between 1 and 5.")]
		public int Rating { get; set; }

		[Required(ErrorMessage = "Please select a Trip.")]
		public int TripId { get; set; }
		public Trip? Trip { get; set; }

		[BindNever]
		public string? ReviewerId { get; set; }

		[ValidateNever]
		public ApplicationUser? Reviewer { get; set; }
	}
}
