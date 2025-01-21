using Amaqele.Common.Authn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Alcazar.Playpen.Razor.Pages
{
    public class FieldsModel : PageModel
    {
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxModel construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public FieldsModel(ILogger<FieldsModel> logger)
		{
			_logger = logger;
		}

		private readonly ILogger<FieldsModel> _logger;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region FieldsModel actions
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public void OnGet()
        {
			ViewData["AuthenticationSchemes"] = _authnSchemes.Select((s) => new SelectListItem
			{
				Value = s.ToString(),
				Text = s.ToString(),
			});
		}

		public IActionResult OnPostAsync()
		{
			try
			{
				List<string> messages = new List<string>();

				if (!ModelState.IsValid)
				{
					messages.Add("Model state is invalid!");
				}
				else
				{
					IFormCollection form = Request.Form;

					messages.Add($"Scheme array is '{string.Join(" ", SchemesArray)}'");
					messages.Add($"Scheme flags are '{string.Join(" ", SchemesFlags)}'");
					messages.Add($"Scheme array (2) is '{string.Join(" ", SchemesArray2)}'");
					messages.Add($"Scheme flags (2) are '{string.Join(" ", SchemesFlags2)}'");
					messages.Add($"Scheme is '{Scheme}'");
				}

				ViewData["AuthenticationSchemes"] = _authnSchemes.Select((s) => new SelectListItem
				{
					Value = s.ToString(),
					Text = s.ToString(),
				});

				TempData["MessageObject-List"] = messages;
				return Page();
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region FieldsModel model properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[BindProperty]
		[Display(Name = "Schemes in array")]
		public AuthenticationSchemes[] SchemesArray { get; set; }

		[BindProperty]
		[Display(Name = "Schemes in array (2)")]
		public AuthenticationSchemes[] SchemesArray2 { get; set; }

		[BindProperty]
		[Display(Name = "Schemes as flags")]
		public AuthenticationSchemes SchemesFlags { get; set; }

		[BindProperty]
		[Display(Name = "Schemes as flags (2)")]
		public AuthenticationSchemes SchemesFlags2 { get; set; }

		[BindProperty]
		public AuthenticationSchemes Scheme { get; set; }

		static private AuthenticationSchemes[] _authnSchemes = new AuthenticationSchemes[]
		{
			AuthenticationSchemes.Authn,
			AuthenticationSchemes.Windows,
			AuthenticationSchemes.Integrated,
		};

		#endregion
	}
}
