using DevExpress.Data.Linq.Helpers;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	[HtmlTargetElement("value-axis", ParentTag = "dx-chart", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("value-axis", ParentTag = "dx-piechart", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ChartValueAxisTagHelper : ChartAxisTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartArgumentAxisTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            ChartContext chartContext = GetContextSafe<ChartContext>(context);

			ChartValueAxisModel axisModel = new ChartValueAxisModel
            {
                Name = Name,
                Position = Position,
				Min = Min,
				Max = Max,
				Offset = Offset,
				IsShowZero = IsShowZero,
				Color = Color,

				TickInterval = TickInterval,
                LabelFormat = LabelFormat
            };

            chartContext.ValueAxes.Add(axisModel);
        }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartValueAxisModel properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("position")]
        public Position? Position { get; set; }

		/// <summary>
		/// Get or set the offset for the value axis.
		/// </summary>
		[HtmlAttributeName("offset")]
		public double? Offset { get; set; }

		/// <summary>
		/// Get or set whether to show zero line on the value axis.
		/// </summary>
		[HtmlAttributeName("zero")]
		public bool IsShowZero { get; set; } = true;
		
		[HtmlAttributeName("interval")]
        public double? TickInterval { get; set; }

        [HtmlAttributeName("format")]
        public Format? LabelFormat { get; set; }

		#endregion
	}

	public class ChartAxisTagHelperBase : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartAxisTagHelperBase properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the name of the axis.
		/// </summary>
		[HtmlAttributeName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Get or set the color of the argument axis.
		/// </summary>
		[HtmlAttributeName("color")]
		public string Color { get; set; }

		/// <summary>
		/// Get or set the minimum value for the axis range.
		/// </summary>
		[HtmlAttributeName("min")]
		public double? Min { get; set; }

		/// <summary>
		/// Get or set the maximum value for the axis range.
		/// </summary>
		[HtmlAttributeName("max")]
		public double? Max { get; set; }

		#endregion
	}

	public class ChartValueAxisModel : ChartAxisModelBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartValueAxisModel properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the position of the value axis.
		/// </summary>
		public Position? Position { get; set; }

		/// <summary>
		/// Get or set the offset for the value axis.
		/// </summary>
		public double? Offset { get; set; }

		/// <summary>
		/// Get or set whether to show zero line on the value axis.
		/// </summary>
		public bool IsShowZero { get; set; } = true;

		/// <summary>
		/// Get or set the tick interval of the value axis.
		/// </summary>
		public double? TickInterval { get; set; }

		/// <summary>
		/// Get or set the label format of the value axis.
		/// </summary>
		public Format? LabelFormat { get; set; }

        #endregion
    }

	public class ChartAxisModelBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartAxisModelBase properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the name of the axis.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Get or set the color of the argument axis.
		/// </summary>
		public string Color { get; set; }

		/// <summary>
		/// Get or set the minimum value for the axis range.
		/// </summary>
		public double? Min { get; set; }

		/// <summary>
		/// Get or set the maximum value for the axis range.
		/// </summary>
		public double? Max { get; set; }

		#endregion
	}
}