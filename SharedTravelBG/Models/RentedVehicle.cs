using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SharedTravelBG.Models
{
	public class RentedVehicle
	{
		public int Id { get; set; }

		// This will be set in the controller and not bound from the form.
		[BindNever]
		public string? RenterId { get; set; }

		[ValidateNever]
		public ApplicationUser? Renter { get; set; }

		[Required(ErrorMessage = "Vehicle Model is required.")]
		[Display(Name = "Vehicle Model")]
		public string VehicleModel { get; set; } = string.Empty;

		[Required(ErrorMessage = "Renter Phone Number is required.")]
		[Display(Name = "Renter Phone Number")]
		public string RenterPhoneNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "Price Per Day is required.")]
		[Display(Name = "Price Per Day")]
		[DataType(DataType.Currency)]
		public decimal PricePerDay { get; set; }

		[Display(Name = "Description")]
		public string? Description { get; set; }

		[Required(ErrorMessage = "License Plate Number is required.")]
		[Display(Name = "License Plate Number")]
		public string LicensePlateNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "Color is required.")]
		[Display(Name = "Color")]
		public string Color { get; set; } = string.Empty;
	}
}
