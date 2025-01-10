using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Alcazar.Playpen.Razor.Pages
{
	public class DateboxModel : PageModel
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxModel construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DateboxModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
			Date1 = DateTime.UtcNow;
		}

		private readonly ILogger<IndexModel> _logger;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxModel actions
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IActionResult OnGet()
		{
			Date1 = DateTime.UtcNow;

			return Page();
		}

		public async Task<IActionResult> OnPostSubmitAsync()
		{
			try
			{
				if (!ModelState.IsValid)
					return Page();

				IFormCollection form = Request.Form;

				var simple = form["Simple date field"];

				var date1_1 = form["Date1"];
				var date1_2 = Date1;

				return Page();
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxModel model properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[BindProperty(SupportsGet = true)]
		public DateTime Date1 { get; set; }

		#endregion
	}
}
