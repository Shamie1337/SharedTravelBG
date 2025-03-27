using System;
using System.ComponentModel.DataAnnotations;

namespace SharedTravelBG.Models
{
	public class RentedVehicle
	{
		public int Id { get; set; }

		// Removed VehicleId, RentalDate, and ReturnDate

		public string RenterId { get; set; }
		public ApplicationUser Renter { get; set; }

		[Required]
		[Display(Name = "Vehicle Model")]
		public string VehicleModel { get; set; }

		[Required]
		[Display(Name = "Renter Phone Number")]
		public string RenterPhoneNumber { get; set; }

		[Required]
		[Display(Name = "Price Per Day")]
		[DataType(DataType.Currency)]
		public decimal PricePerDay { get; set; }

		[Display(Name = "Description")]
		public string Description { get; set; }

		[Required]
		[Display(Name = "License Plate Number")]
		public string LicensePlateNumber { get; set; }

		[Required]
		[Display(Name = "Color")]
		public string Color { get; set; }
	}
}
