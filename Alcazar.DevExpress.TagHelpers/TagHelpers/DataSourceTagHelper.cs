using Amaqele.Common.Collections;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DataSourceTagHelper"/> type implements a data source for other tag helpers which do not have all data source properties.
	/// This tag is always a child of other dx-* tag helpers.
	/// </summary>
	[HtmlTargetElement("data-source", ParentTag = "dx-datagrid", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-field", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-control", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-autocomplete", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-select", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-dropdown", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-tag", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-radiogroup", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class DataSourceTagHelper : DataSourceTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DataSourceTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the HTML of this tag, it is used for DX button generation only
			output.SuppressOutput();

			// Process the buttons tag and remember the content, so that the parent can process it
			DataSourceContext sourceContext = GetContextSafe<DataSourceContext>(context);
			if (sourceContext.Datasource != null)
				throw new NotSupportedException($"Cannot apply data source {DatasourceType} '{Action}', {sourceContext.Datasource.DatasourceType} '{sourceContext.Datasource.Action}' is already declared.");

			// Declare the data source
			sourceContext.Datasource = this;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the number of characters to enter before the lookup starts.
		/// </summary>
		[HtmlAttributeName("min-length")]
		public int MinSearchLength { get; set; }

		/// <summary>
		/// Get or set the search timeout (in ms) for calling the datasource.
		/// </summary>
		[HtmlAttributeName("timeout")]
		public int SearchTimeout { get; set; }

		#endregion
	}

	public class DataSourceContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DataSourceTagHelper Datasource { get; set; }

		#endregion
	}

	public enum DataSourceTypes
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTypes values
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Not a valud data source.
		/// </summary>
		None,

		/// <summary>
		/// A data source which received its data from static JSON.
		/// </summary>
		StaticJson,

		/// <summary>
		/// A data source which received its data from an array of model objects.
		/// </summary>
		Array,

		/// <summary>
		/// A data source which received its data from an MVC API call.
		/// </summary>
		Mvc,

		/// <summary>
		/// A data source which received its data from an OData API call.
		/// </summary>
		OData,

		/// <summary>
		/// A data source which received its data from an API call to a remote controller.
		/// </summary>
		RemoteController,

		#endregion
	}
}
