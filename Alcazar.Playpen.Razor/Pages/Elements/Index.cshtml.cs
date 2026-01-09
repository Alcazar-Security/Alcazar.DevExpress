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
    public class IndexModel : PageModel
    {
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region IndexModel construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		private IndexModel()
		{
		}

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		private readonly ILogger<IndexModel> _logger;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region IndexModel actions
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
		#region IndexModel model properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		#endregion
	}
}
