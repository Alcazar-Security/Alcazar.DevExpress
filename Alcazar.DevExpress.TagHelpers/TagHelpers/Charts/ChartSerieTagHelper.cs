using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="ColumnTagHelper"/> tag helper defines columns of a data grid, tree lists, and other users of data sources.
	/// </summary>
	[HtmlTargetElement("serie", ParentTag = "dx-chart", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("serie", ParentTag = "dx-piechart", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ChartSerieTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartSerieTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the HTML of this tag, it is used for DX button generation only
			output.SuppressOutput();

			// Obtain the context, so that we can use it here
			ChartContext chartContext = GetContextSafe<ChartContext>(context);

			// Construct the axis model
			ChartSerieModel serieModel = new ChartSerieModel
			{
				Name = Name,
				Type = Type,
				ValueField = ValueField,
				ArgumentField = ArgumentField,
				Axis = Axis,
				Color = Color,
			};

			chartContext.Series.Add(serieModel);
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartSerieTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		/// <summary>
		/// Get or set the name of the serie.
		/// </summary>
		[HtmlAttributeName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Get or set the series type of the serie.
		/// </summary>
		[HtmlAttributeName("type")]
		public SeriesType? Type { get; set; }

		/// <summary>
		/// Get or set the name of the field which contains the data of the serie.
		/// </summary>
		[HtmlAttributeName("value")]
		public string ValueField { get; set; }

		/// <summary>
		/// Get or set the name of the field which contains the argument of the serie.
		/// </summary>
		[HtmlAttributeName("argument")]
		public string ArgumentField { get; set; }

		/// <summary>
		/// Get or set the name of the axis of the serie.
		/// </summary>
		[HtmlAttributeName("axis")]
		public string Axis { get; set; }

		/// <summary>
		/// Get or set the color of the serie.
		/// </summary>
		[HtmlAttributeName("color")]
		public string Color { get; set; }

		#endregion
	}

	/// <summary>
	/// The <see cref="ChartSerieModel"/> model type represents a contained chart serie of data, which is confered to the parent chart control.
	/// </summary>
	public class ChartSerieModel
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartSerieModel properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the name of the serie.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Get or set the series type of the serie.
		/// </summary>
		public SeriesType? Type { get; set; }

		/// <summary>
		/// Get or set the name of the field which contains the data of the serie.
		/// </summary>
		public string ValueField { get; set; }

		/// <summary>
		/// Get or set the name of the field which contains the argument of the serie.
		/// </summary>
		public string ArgumentField { get; set; }

		/// <summary>
		/// Get or set the name of the axis of the serie.
		/// </summary>
		public string Axis { get; set; }

		/// <summary>
		/// Get or set the color of the serie.
		/// </summary>
		public string Color { get; set; }

		#endregion
	}
}
