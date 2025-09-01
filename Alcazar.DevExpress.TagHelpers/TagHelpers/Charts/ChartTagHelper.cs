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

			builder = builder.ValueAxis((a) =>
			{
				a.Add()
					.Name("frequency")
					.Position(Position.Left)
					.TickInterval(300);
			});

			builder = builder.Series((s) =>
			{
				s.Add()
					.ArgumentField("State")
					.ValueField("Amount")
					.Type(SeriesType.Bar)
					.Color("#3498db")
					.Name("Amount");
			});

			builder = builder.Tooltip(t =>
			{
				t = t.Enabled(true);
				t = t.Format("currency");
				t = t.Location(ChartTooltipLocation.Center);
				t = t.Border((b) =>
				{
					b = b.Color("#000");
					b = b.Width(2);
					b = b.Visible(true);
				});
				t = t.Font((f) =>
				{
					f = f.Color("#000");
					f = f.Size(20);
					f = f.Weight(400);
				});
			});

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
			if (chartContext.ChartAxis != null)
			{
				builder = builder.ArgumentAxis((a) =>
				{
					// Axis type
					if (chartContext.ChartAxis.AxisScaleType.HasValue)
						a = a.Type(chartContext.ChartAxis.AxisScaleType.Value);

					// Axis label
					a = a.Label((l) =>
					{
						if (chartContext.ChartAxis.LabelFormat.HasValue)
							l = l.Format(chartContext.ChartAxis.LabelFormat.Value);
						if (chartContext.ChartAxis.LabelOverlappingBehavior.HasValue)
							l = l.OverlappingBehavior(chartContext.ChartAxis.LabelOverlappingBehavior.Value);
					});
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
		#region AutocompleteTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("palette")]
		public VizPalette? Palette { get; set; }

		#endregion
	}
}
