using Alcazar.Web.Utilities;
using Amaqele.Common.Base;
using Amaqele.Common.Types;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace Alcazar.Playpen.Razor.Pages.DataGrid
{
    public class DatesModel : PageModel
    {
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DatesModel construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		private DatesModel()
		{
		}

		public DatesModel(ILogger<DatesModel> logger)
		{
			_logger = logger;
		}

		private readonly ILogger<DatesModel> _logger;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DatesModel actions
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IActionResult OnGet()
		{
			LocalDateTime = DateTime.ParseExact("2000-02-14 12:00", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
			UniversalDateTime = DateTime.ParseExact("2000-02-14 12:00", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

			LocalDate = DateTime.ParseExact("2000-02-14", "yyyy-MM-dd", CultureInfo.InvariantCulture);
			UniversalDate = DateTime.ParseExact("2000-02-14", "yyyy-MM-dd", CultureInfo.InvariantCulture);

			Day = DateTime.ParseExact("2000-02-14", "yyyy-MM-dd", CultureInfo.InvariantCulture);

			return Page();
		}

		public IActionResult OnPost()
		{
			return Page();
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DatesModel data calls
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DatesModel model properties: dates
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set a date: date/time, display as local time (not the browser timezone, but the Alcazar timezone)
		/// </summary>
		[BindProperty]
		[DateTimeUsage]
		public DateTime LocalDateTime { get; set; }

		/// <summary>
		/// Get or set a date: date/time, display as UTC
		/// </summary>
		[BindProperty]
		[DateTimeUsage(ViewAs = DateTimeKind.Utc)]
		public DateTime UniversalDateTime { get; set; }

		/// <summary>
		/// Get or set a date: date (no time), display as local time (not the browser timezone, but the Alcazar timezone)
		/// </summary>
		[BindProperty]
		[DateTimeUsage]
		public DateTime LocalDate { get; set; }

		/// <summary>
		/// Get or set a date: date (no time), display as UTC
		/// </summary>
		[BindProperty]
		[DateTimeUsage(ViewAs = DateTimeKind.Utc)]
		public DateTime UniversalDate { get; set; }

		/// <summary>
		/// Get or set a date: day/only (no time), display timezone agnostic
		/// </summary>
		[BindProperty]
		[DateTimeUsage(IsDateOnly = true)]
		public DateTime Day { get; set; }

		#endregion
	}
}
