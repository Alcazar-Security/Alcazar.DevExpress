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
	[HtmlTargetElement("data-source", ParentTag = "dx-field", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-control", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-autocomplete", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-select", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-dropdown", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-tag", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("data-source", ParentTag = "dx-radiogroup", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class DataSourceTagHelper : RouteTagHelperBase
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

		/// <summary>
		/// Build the data source. THis method will typically be called by the parent tag.
		/// </summary>
		/// <param name="factory"> The data source factory. </param>
		/// <returns> The data source builder.</returns>
		public OptionsOwnerBuilder BuildDatasource(DataSourceFactory factory)
		{
			switch (DatasourceType)
			{
				default:
				case DataSourceTypes.None:
					throw new NotSupportedException($"Cannot apply data source, {DatasourceType} is not implemented.");

				case DataSourceTypes.StaticJson: return BuildStaticJson(factory);
				case DataSourceTypes.Array: return BuildArray(factory);
				case DataSourceTypes.Mvc: return BuildMvc(factory);
				case DataSourceTypes.OData: return BuildOData(factory);
				case DataSourceTypes.RemoteController: return BuildRemoteController(factory);
			}
		}

		private OptionsOwnerBuilder BuildStaticJson(DataSourceFactory factory)
		{
			var options = factory.StaticJson();

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key);

			return options;
		}

		private OptionsOwnerBuilder BuildArray(DataSourceFactory factory)
		{
			var options = factory.Array();

			options = options.Data(Items);

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key);
			
			return options;
		}

		private OptionsOwnerBuilder BuildMvc(DataSourceFactory factory)
		{
			var options = factory.Mvc().LoadMethod(HttpMethod).LoadAction(Action);

			// Add load parameters
			if (LoadParams.Any())
			{
				ExpandoObject loadParams = new ExpandoObject();
				loadParams.AddRange(LoadParams);
				options = options.LoadParams(loadParams);
			}

			if (!string.IsNullOrEmpty(Controller))
				options = options.Controller(Controller);
			if (!string.IsNullOrEmpty(Area))
				options = options.Area(Area);

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key);

			return options;
		}

		private OptionsOwnerBuilder BuildOData(DataSourceFactory factory)
		{
			var options = factory.OData();
			return options;
		}

		private OptionsOwnerBuilder BuildRemoteController(DataSourceFactory factory)
		{
			var options = factory.RemoteController();
			return options;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the text message to be used for this popup.
		/// </summary>
		[HtmlAttributeName("type")]
		public DataSourceTypes DatasourceType { get; set; }

		/// <summary>
		/// Get or set the name of the key property of datasource items.
		/// </summary>
		[HtmlAttributeName("key")]
		public string Key { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper properties: Array
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the sequence of items to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("asp-items")]
		public System.Collections.IEnumerable Items { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper properties: MVC
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set parameters used for loading of data records from the data source.
		/// </summary>
		[HtmlAttributeName(DictionaryAttributePrefix = "load-param-")]
		public IDictionary<string, object> LoadParams { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Get or set the HTTP method to be used for the Web API call.
		/// </summary>
		[HtmlAttributeName("method")]
		public string HttpMethod { get; set; }

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

		None,
		StaticJson,
		Array,
		Mvc,
		OData,
		RemoteController,

		#endregion
	}
}
