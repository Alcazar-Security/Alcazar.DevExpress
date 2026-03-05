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
	[HtmlTargetElement("argument-axis", ParentTag = "dx-chart", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("argument-axis", ParentTag = "dx-piechart", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ChartArgumentAxisTagHelper : ChartAxisTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartArgumentAxisTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the HTML of this tag, it is used for DX button generation only
			output.SuppressOutput();

			// Obtain the context, so that we can use it here
			ChartContext chartContext = GetContextSafe<ChartContext>(context);

			// Construct the axis model
			ChartArgumentAxisModel axisModel = new ChartArgumentAxisModel
			{
				Name = Name,
				Color = Color,
				Min = Min,
				Max = Max,
				AxisScaleType = AxisScaleType,
				LabelFormat = LabelFormat,
				LabelOverlappingBehavior = LabelOverlappingBehavior,
				IsWorkdays = IsWorkdays,
			};

			chartContext.ArgumentAxis = axisModel;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartArgumentAxisTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartArgumentAxisTagHelper properties: axis
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the scale type of the argument axis.
		/// </summary>
		[HtmlAttributeName("type")]
		public AxisScaleType? AxisScaleType { get; set; }

		/// <summary>
		/// Get or set the label format of the argument axis.
		/// </summary>
		[HtmlAttributeName("label-format")]
		public Format? LabelFormat { get; set; }

		/// <summary>
		/// Get or set the overlapping behaviour of the label of the argument axis.
		/// </summary>
		[HtmlAttributeName("label-overlapping")]
		public OverlappingBehavior? LabelOverlappingBehavior { get; set; }

		/// <summary>
		/// Get or set an indicator if the axis should show workdays only
		/// </summary>
		[HtmlAttributeName("workdays")]
		public bool IsWorkdays { get; set; }

		#endregion
	}

	/// <summary>
	/// The <see cref="ChartArgumentAxisModel"/> model type represents a contained argument chart axis, which is confered to the parent chart control.
	/// </summary>
	public class ChartArgumentAxisModel : ChartAxisModelBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region Argument axis properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the scale type of the argument axis.
		/// </summary>
		public AxisScaleType? AxisScaleType { get; set; }

		/// <summary>
		/// Get or set the label format of the argument axis.
		/// </summary>
		public Format? LabelFormat { get; set; }

		/// <summary>
		/// Get or set the overlapping behaviour of the label of the argument axis.
		/// </summary>
		public OverlappingBehavior? LabelOverlappingBehavior { get; set; }

		/// <summary>
		/// Get or set an indicator if the axis should show workdays only
		/// </summary>
		public bool IsWorkdays { get; set; }

		#endregion
	}
}
