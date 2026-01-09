using Alcazar.Web.Utilities;
using Amaqele.Common.Base;
using Amaqele.Common.Types;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace Alcazar.Playpen.Razor.Pages.Elements
{
    public class IconsModel : PageModel
    {
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region IconsModel construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		private IconsModel()
		{
		}

		public IconsModel(ILogger<IconsModel> logger)
		{
			_logger = logger;
		}

		private readonly ILogger<IconsModel> _logger;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region IconsModel actions
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IActionResult OnGet()
		{
			return Page();
		}

		public IActionResult OnPost()
		{
			return Page();
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region IconsModel model properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		#endregion
	}
}
