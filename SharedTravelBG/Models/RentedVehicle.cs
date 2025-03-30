using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SharedTravelBG.Models
{
	public class RentedVehicle
	{
		public int Id { get; set; }

		// Mark as nullable so that validation does not require a value from the form.
		[BindNever]
		public string? RenterId { get; set; }

		[ValidateNever]
		public ApplicationUser? Renter { get; set; }

		[Required]
		[Display(Name = "Vehicle Model")]
		public string VehicleModel { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Renter Phone Number")]
		public string RenterPhoneNumber { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Price Per Day")]
		[DataType(DataType.Currency)]
		public decimal PricePerDay { get; set; }

		[Display(Name = "Description")]
		public string? Description { get; set; }

		[Required]
		[Display(Name = "License Plate Number")]
		public string LicensePlateNumber { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Color")]
		public string Color { get; set; } = string.Empty;
	}
}
