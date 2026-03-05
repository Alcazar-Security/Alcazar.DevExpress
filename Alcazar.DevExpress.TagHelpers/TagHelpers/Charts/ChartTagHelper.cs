using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="ChartTagHelper"/> type implements a chart.
	/// </summary>
	[HtmlTargetElement("dx-chart")]
	public class ChartTagHelper : DataSourceTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public ChartTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Asynchronously executes the tag with the given <paramref name="context"/> and <paramref name="output"/>.
		/// </summary>
		/// <param name="context"> Contains information associated with the current HTML tag. </param>
		/// <param name="output"> A stateful HTML element used to generate an HTML tag. </param>
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// To avoid: InvalidOperationException: Must call 'Contextualize' method before using this HtmlHelper instance.
			_htmlHelper.Contextualize(ViewContext);

			// Create the builder for a chart
			ChartBuilder builder = _htmlHelper.DevExtreme().Chart();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the context, so that we can pass it to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ChartContext chartContext = GetOrCreateContext<ChartContext>(context);

			// Process children of the diagram tag
			IHtmlContent content = await output.GetChildContentAsync();

			// Set the data source
			if (sourceContext.Datasource != null)
			{
				// First preference, process the (child) data source
				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}
			else
			{
				// Second preference, process the datasource from my own properties
				builder = builder.DataSource(d => BuildDatasource(d));
			}

			// Chart axes and series
			builder = ProcessArgumentAxis(builder, chartContext);
			builder = ProcessValueAxes(builder, chartContext);
			builder = ProcessSeries(builder, chartContext);

			if (IsTooltip)
			{
				builder = builder.Tooltip(t =>
				{
					t = t.Enabled(true);

					if (TooltipFormat.HasValue)
						t = t.Format(TooltipFormat.Value);

					t = t.Location(TooltipLocation);

					if (TooltipBorder > 0)
					{
						t = t.Border((b) =>
						{
							b = b.Visible(true);
							b = b.Width(TooltipBorder);
							if (!string.IsNullOrEmpty(TooltipBorderColor))
								b = b.Color(TooltipBorderColor);
						});
					}

					t = t.Font((f) =>
					{
						if (TooltipFontSize > 0)
							f = f.Size(TooltipFontSize);
						if (TooltipFontWeight > 0)
							f = f.Weight(TooltipFontWeight);
						if (!string.IsNullOrEmpty(TooltipFontColor))
							f = f.Color(TooltipFontColor);
					});
				});
			}

			// Appearance
			builder = builder.CommonSeriesSettings((s) =>
			{
				//s = s.ArgumentField("State");
				s = s.Type(SeriesType.Bar);
			});

			if (Palette.HasValue)
				builder = builder.Palette(Palette.Value);

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private ChartBuilder ProcessCommon(ChartBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			return builder;
		}

		private ChartBuilder ProcessTitle(ChartBuilder builder)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder.Title(title);
			}

			return builder;
		}

		private ChartBuilder ProcessAttributes(ChartBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		private ChartBuilder ProcessArgumentAxis(ChartBuilder builder, ChartContext chartContext)
		{
			if (chartContext.ArgumentAxis != null)
			{
				builder = builder.ArgumentAxis((a) =>
				{
					var axis = chartContext.ArgumentAxis;

					if (!string.IsNullOrEmpty(axis.Name))
						a = a.Title(axis.Name);

					// Axis type
					if (axis.AxisScaleType.HasValue)
						a = a.Type(axis.AxisScaleType.Value);

					if (!string.IsNullOrEmpty(axis.Color))
						a = a.Color(axis.Color);

					// TODO
					// a = a.WholeRange();
					a = a.WorkdaysOnly(axis.IsWorkdays);

					// Axis label
					a = a.Label((l) =>
					{
						if (axis.LabelFormat.HasValue)
							l = l.Format(axis.LabelFormat.Value);
						if (axis.LabelOverlappingBehavior.HasValue)
							l = l.OverlappingBehavior(axis.LabelOverlappingBehavior.Value);
					});

					// Axis range
					if (axis.Min.HasValue || axis.Max.HasValue)
					{
						a = a.VisualRange(vr =>
						{
							if (axis.Min.HasValue)
								vr = vr.StartValue(axis.Min.Value);
							if (axis.Max.HasValue)
								vr = vr.EndValue(axis.Max.Value);
						});
					}
				});
			}

			return builder;
		}

		private ChartBuilder ProcessValueAxes(ChartBuilder builder, ChartContext chartContext)
		{
			if (chartContext.ValueAxes.Any())
			{
				builder = builder.ValueAxis(a =>
				{
					foreach (var axis in chartContext.ValueAxes)
					{
						var axisBuilder = a.Add();

						if (!string.IsNullOrEmpty(axis.Name))
							axisBuilder = axisBuilder.Name(axis.Name);
						if (axis.Position.HasValue)
							axisBuilder = axisBuilder.Position(axis.Position.Value);
						if (axis.TickInterval.HasValue)
							axisBuilder = axisBuilder.TickInterval(axis.TickInterval.Value);
						if (axis.LabelFormat.HasValue)
							axisBuilder = axisBuilder.Label(l => l.Format(axis.LabelFormat.Value));

						if (!string.IsNullOrEmpty(axis.Color))
							axisBuilder = axisBuilder.Color(axis.Color);

						if (axis.Offset.HasValue)
							axisBuilder = axisBuilder.Offset(axis.Offset.Value);

						axisBuilder = axisBuilder.ShowZero(axis.IsShowZero);

						axisBuilder = axisBuilder.ShowZero(axis.IsShowZero);

						// Axis range
						if (axis.Min.HasValue || axis.Max.HasValue)
						{
							axisBuilder = axisBuilder.VisualRange(vr =>
							{
								if (axis.Min.HasValue)
									vr = vr.StartValue(axis.Min.Value);
								if (axis.Max.HasValue)
									vr = vr.EndValue(axis.Max.Value);
							});
						}
					}
				});
			}
			return builder;
		}
		
		private ChartBuilder ProcessSeries(ChartBuilder builder, ChartContext chartContext)
		{
			if (chartContext.Series.Any())
			{
				builder = builder.Series((s) =>
				{
					foreach (var serie in chartContext.Series)
					{
						ChartSeriesBuilder series = s.Add();

						if (!string.IsNullOrEmpty(serie.Name))
							series = series.Name(serie.Name);
						if (serie.Type.HasValue)
							series = series.Type(serie.Type.Value);
						if (!string.IsNullOrEmpty(serie.ValueField))
							series = series.ValueField(serie.ValueField);
						if (!string.IsNullOrEmpty(serie.ArgumentField))
							series = series.ArgumentField(serie.ArgumentField);
						if (!string.IsNullOrEmpty(serie.Color))
							series = series.Color(serie.Color);
						if (!string.IsNullOrEmpty(serie.Axis))
							series = series.Axis(serie.Axis);
					}
				});
			}

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("palette")]
		public VizPalette? Palette { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartTagHelper properties: tooltip
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set an indicator if a tooltip is to be displayed for values in the chart.
		/// </summary>
		[HtmlAttributeName("tooltip")]
		public bool IsTooltip { get; set; }

		/// <summary>
		/// Get or set the format to be used for the values tooltip in the chart.
		/// </summary>
		[HtmlAttributeName("tooltip-format")]
		public Format? TooltipFormat { get; set; }

		/// <summary>
		/// Get or set the location of the values tooltip in the chart.
		/// </summary>
		[HtmlAttributeName("tooltip-location")]
		public ChartTooltipLocation TooltipLocation { get; set; } = ChartTooltipLocation.Center;

		/// <summary>
		/// Get or set the width of the border of the values tooltip in the chart.
		/// </summary>
		[HtmlAttributeName("tooltip-border")]
		public double TooltipBorder { get; set; }

		/// <summary>
		/// Get or set the color of the border of the values tooltip in the chart.
		/// </summary>
		[HtmlAttributeName("tooltip-border-color")]
		public string TooltipBorderColor { get; set; }

		/// <summary>
		/// Get or set the size of the font of the values tooltip in the chart.
		/// </summary>
		[HtmlAttributeName("tooltip-font-size")]
		public double TooltipFontSize { get; set; }

		/// <summary>
		/// Get or set the weight of the font of the values tooltip in the chart.
		/// </summary>
		[HtmlAttributeName("tooltip-font-weight")]
		public double TooltipFontWeight { get; set; }

		/// <summary>
		/// Get or set the color of the font of the values tooltip in the chart.
		/// </summary>
		[HtmlAttributeName("tooltip-font-color")]
		public string TooltipFontColor { get; set; }

		#endregion
	}
}
