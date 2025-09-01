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
	public class ChartValueAxisTagHelper : TagHelperBase
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

        [HtmlAttributeName("name")]
        public string Name { get; set; }

        [HtmlAttributeName("position")]
        public Position? Position { get; set; }

        [HtmlAttributeName("interval")]
        public double? TickInterval { get; set; }

        [HtmlAttributeName("format")]
        public Format? LabelFormat { get; set; }

		#endregion
	}


	public class ChartValueAxisModel
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartValueAxisModel properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the name of the value axis.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Get or set the position of the value axis.
		/// </summary>
		public Position? Position { get; set; }

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

}