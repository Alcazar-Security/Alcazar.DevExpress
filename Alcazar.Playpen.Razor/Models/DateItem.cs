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
		/// Get or set a date/time, display as local time (not the browser timezone, but the Alcazar timezone)
		/// </summary>
		[DateTimeUsage]
		[Display(Name = "Local date/time")]
		public DateTime LocalDateTime { get; set; }

		/// <summary>
		/// Get or set a date/time, display as UTC
		/// </summary>
		[DateTimeUsage(ViewAs = DateTimeKind.Utc)]
		[Display(Name = "Universal date/time")]
		public DateTime UniversalDateTime { get; set; }

		/// <summary>
		/// Get or set a date (no time), display as local time (not the browser timezone, but the Alcazar timezone)
		/// </summary>
		[DateTimeUsage]
		[Display(Name = "Local date")]
		public DateTime LocalDate { get; set; }

		/// <summary>
		/// Get or set a date (no time), display as UTC
		/// </summary>
		[DateTimeUsage(ViewAs = DateTimeKind.Utc)]
		[Display(Name = "Universal date")]
		public DateTime UniversalDate { get; set; }

		/// <summary>
		/// Get or set a date: day/only (no time), display timezone agnostic
		/// </summary>
		[DateTimeUsage(ViewAs = DateTimeKind.Utc, IsDateOnly = true)]
		[Display(Name = "Day only")]
		public DateTime Day { get; set; }

		/// <summary>
		/// Get or set a time (no date), display as local time (not the browser timezone, but the Alcazar timezone)
		/// </summary>
		[DateTimeUsage]
		[Display(Name = "Local time")]
		public DateTime LocalTime { get; set; }

		/// <summary>
		/// Get or set a time (no date), display as UTC
		/// </summary>
		[DateTimeUsage(ViewAs = DateTimeKind.Utc)]
		[Display(Name = "Universal time")]
		public DateTime UniversalTime { get; set; }

		/// <summary>
		/// Get or set a time (no date), display as local time (not the browser timezone, but the Alcazar timezone)
		/// </summary>
		[DateTimeUsage]
		[Display(Name = "Local timespan")]
		public TimeSpan LocalTimespan { get; set; }

		/// <summary>
		/// Get or set a time (no date), display as UTC
		/// </summary>
		[DateTimeUsage(ViewAs = DateTimeKind.Utc)]
		[Display(Name = "Universal timespan")]
		public TimeSpan UniversalTimespan { get; set; }

		/// <summary>
		/// Get or set a date/time, without an attribute
		/// </summary>
		[Display(Name = "Unspecified date/time")]
		public DateTime DateTime { get; set; }

		/// <summary>
		/// Get or set a date, without an attribute
		/// </summary>
		[Display(Name = "Unspecified date")]
		public DateTime Date { get; set; }

	}
}