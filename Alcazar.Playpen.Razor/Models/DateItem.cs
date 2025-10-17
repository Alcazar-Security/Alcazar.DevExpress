using Amaqele.Common.Types;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Alcazar.Playpen.Models
{
	// Define the DateItem model for OData (if not already in Program.cs)
	public class DateItem
	{
		public int Id { get; set; }

		/// <summary>
		/// Get or set a date: date/time, display as local time (not the browser timezone, but the Alcazar timezone)
		/// </summary>
		[DateTimeUsage]
		[Display(Name = "Local date/time")]
		public DateTime LocalDateTime { get; set; }

		/// <summary>
		/// Get or set a date: date/time, display as UTC
		/// </summary>
		[DateTimeUsage(ViewAs = DateTimeKind.Utc)]
		[Display(Name = "Universal date/time")]
		public DateTime UniversalDateTime { get; set; }

		/// <summary>
		/// Get or set a date: date (no time), display as local time (not the browser timezone, but the Alcazar timezone)
		/// </summary>
		[DateTimeUsage]
		public DateTime LocalDate { get; set; }

		/// <summary>
		/// Get or set a date: date (no time), display as UTC
		/// </summary>
		[DateTimeUsage(ViewAs = DateTimeKind.Utc)]
		public DateTime UniversalDate { get; set; }

		/// <summary>
		/// Get or set a date: day/only (no time), display timezone agnostic
		/// </summary>
		[DateTimeUsage(ViewAs = DateTimeKind.Utc, IsDateOnly = true)]
		public DateTime Day { get; set; }

	}
}