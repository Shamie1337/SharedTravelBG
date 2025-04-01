using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SharedTravelBG.Models
{
	public class Trip
	{
		public int Id { get; set; }

		[Required]
		public string DepartureTown { get; set; } = string.Empty;

		[Required]
		public string DestinationTown { get; set; } = string.Empty;

		[DataType(DataType.Date)]
		public DateTime TripDate { get; set; }

		// OrganizerId is required but will be set in the controller, so we don't want the model binder to expect it from the form.
		
		[BindNever]
		public string OrganizerId { get; set; } = string.Empty;

		[ValidateNever]
		public ApplicationUser Organizer { get; set; } = default!;

		// Collection of participants
		public ICollection<ApplicationUser> Participants { get; set; } = new List<ApplicationUser>();

		// New fields
		[Display(Name = "Organizer Phone Number")]
		[DataType(DataType.PhoneNumber)]
		public string OrganizerPhoneNumber { get; set; } = string.Empty;

		[Display(Name = "Planned Start Time")]
		[DataType(DataType.DateTime)]
		public TimeSpan PlannedStartTime { get; set; }

		[Required]
		public int MaxParticipants { get; set; }
	}
}
