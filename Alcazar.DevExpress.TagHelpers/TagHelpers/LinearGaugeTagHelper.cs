using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="LinearGaugeTagHelper"/> type implements a DevExtreme linear gauge control.
	/// </summary>
	[HtmlTargetElement("dx-lineargauge")]
	public class LinearGaugeTagHelper : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LinearGaugeTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public LinearGaugeTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LinearGaugeTagHelper overrides
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

			// Suppress myself as output, using only the translated UI text.
			output.SuppressOutput();

			// Create the builder for the linear gauge
			LinearGaugeBuilder builder = _htmlHelper.DevExtreme().LinearGauge();

			// Process common functionality for controls
			builder = ProcessCommon(builder);

			// Process non-tag WRAPPER attributes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the contexts, so that we can pass them to child tag helpers
			RangeContext rangeContext = GetOrCreateContext<RangeContext>(context);

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			// Process linear gauge specific properties
			if (Value.HasValue)
				builder = builder.Value(Value.Value);

			//if (!string.IsNullOrEmpty(Subvalues))
			//	builder = builder.Subvalues(Subvalues);

			// Configure the scale
			builder = builder.Scale(scale =>
			{
				if (MinValue.HasValue)
					scale = scale.StartValue(MinValue.Value);

				if (MaxValue.HasValue)
					scale = scale.EndValue(MaxValue.Value);

				if (TickInterval.HasValue)
					scale = scale.TickInterval(TickInterval.Value);

				//if (MinorTickCount.HasValue)
				//	scale = scale.MinorTickCount(MinorTickCount.Value);

				//if (!string.IsNullOrEmpty(ScaleOrientation))
				//{
				//	if (Enum.TryParse<DevExtreme.AspNet.Mvc.Orientation>(ScaleOrientation, true, out var orientation))
				//		scale = scale.Orientation(orientation);
				//}

				// Configure scale labels
				if (ShowScaleLabels.HasValue)
				{
					scale = scale.Label(l => l.Visible(ShowScaleLabels.Value));
				}

				// Configure ticks
				scale = scale.Tick(t =>
				{
					t = t.Visible(ShowTicks);
				});

				scale = scale.MinorTick(t =>
				{
					t = t.Visible(ShowMinorTicks);
				});
			});

			// Configure the geometry
			if (!string.IsNullOrEmpty(Orientation))
			{
				if (Enum.TryParse<DevExtreme.AspNet.Mvc.Orientation>(Orientation, true, out var orientation))
					builder = builder.Geometry(g => g.Orientation(orientation));
			}

			// Configure range bar
			//if (ShowRangeBar.HasValue)
			//{
			//	builder = builder.RangeBar(rb => rb.Visible(ShowRangeBar.Value));
			//}

			// Configure value indicator
			if (!string.IsNullOrEmpty(ValueIndicatorType))
			{
				builder = builder.ValueIndicator(vi =>
				{
					if (Enum.TryParse<GaugeIndicatorType>(ValueIndicatorType, true, out var type))
						vi = vi.Type(type);
				});
			}

			// Configure subvalue indicators
			if (!string.IsNullOrEmpty(SubValueIndicatorType))
			{
				builder = builder.SubvalueIndicator(svi =>
				{
					if (Enum.TryParse<GaugeIndicatorType>(SubValueIndicatorType, true, out var type))
						svi = svi.Type(type);
				});
			}

			// Process ranges from child <range> elements
			if (rangeContext.Ranges.Any())
			{
				builder = builder.RangeContainer(rc =>
				{
					rc = rc.Ranges(ranges =>
					{
						foreach (var rangeModel in rangeContext.Ranges)
						{
							var range = ranges.Add();
							if (rangeModel.StartValue.HasValue)
								range = range.StartValue(rangeModel.StartValue.Value);

							if (rangeModel.EndValue.HasValue)
								range = range.EndValue(rangeModel.EndValue.Value);

							if (!string.IsNullOrEmpty(rangeModel.Color))
								range = range.Color(rangeModel.Color);

							//if (rangeModel.Width.HasValue)
							//	range = range.Width(rangeModel.Width.Value);

							//if (rangeModel.Opacity.HasValue)
							//	range = range.Opacity(rangeModel.Opacity.Value);
						}
					});

					// Configure range container properties if specified
					if (RangeContainerWidth.HasValue)
						rc = rc.Width(RangeContainerWidth.Value);

					if (RangeOffset.HasValue)
						rc = rc.Offset(RangeOffset.Value);

					if (RangeContainerHorizontalOrientation.HasValue)
					{
						rc = rc.HorizontalOrientation(RangeContainerHorizontalOrientation.Value);
					}
				});
			}

			// Event handlers
			if (!string.IsNullOrEmpty(OnInitialized))
				builder = builder.OnInitialized(OnInitialized);

			if (!string.IsNullOrEmpty(OnTooltipShown))
				builder = builder.OnTooltipShown(OnTooltipShown);

			if (!string.IsNullOrEmpty(OnTooltipHidden))
				builder = builder.OnTooltipHidden(OnTooltipHidden);

			// Tooltip configuration
			if (ShowTooltip.HasValue)
			{
				builder = builder.Tooltip(t => t.Enabled(ShowTooltip.Value));
			}

			// Animation settings
			builder = builder.Animation(a => a.Enabled(AnimationEnabled));

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private LinearGaugeBuilder ProcessCommon(LinearGaugeBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			//if (!string.IsNullOrEmpty(Width))
			//	builder = builder.Width(Width);

			//if (!string.IsNullOrEmpty(Height))
			//	builder = builder.Height(Height);

			// Set disabled state
			if (IsDisabled)
				builder = builder.Disabled(true);

			return builder;
		}

		/// <summary>
		/// Process attributes.
		/// </summary>
		/// <remarks>
		/// <para>
		/// </para>
		/// </remarks>
		private LinearGaugeBuilder ProcessAttributes(LinearGaugeBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the input
			foreach (var attr in attributes)
			{
				if (attr.Name != "class")
					builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());
			}

			// Class on element TODO
			//if (!string.IsNullOrEmpty(ElementClass))
			//	builder = builder.ElementAttr("class", ElementClass);

			// No option for attributes on the input field here
			return builder;
		}

		private LinearGaugeBuilder ProcessTitle(LinearGaugeBuilder builder)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder = builder.Title(t => t.Text(title));
			}

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LinearGaugeTagHelper properties: Core values
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the main value displayed on the gauge.
		/// </summary>
		[HtmlAttributeName("value")]
		public double? Value { get; set; }

		/// <summary>
		/// Get or set additional values displayed on the gauge (as a JavaScript array string).
		/// </summary>
		[HtmlAttributeName("subvalues")]
		public IEnumerable<double> Subvalues { get; set; }

		/// <summary>
		/// Get or set the minimum value of the gauge scale.
		/// </summary>
		[HtmlAttributeName("min")]
		public double? MinValue { get; set; }

		/// <summary>
		/// Get or set the maximum value of the gauge scale.
		/// </summary>
		[HtmlAttributeName("max")]
		public double? MaxValue { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LinearGaugeTagHelper properties: Scale configuration
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the interval between major ticks on the scale.
		/// </summary>
		[HtmlAttributeName("interval")]
		public double? TickInterval { get; set; }

		/// <summary>
		/// Get or set the number of minor ticks between major ticks.
		/// </summary>
		[HtmlAttributeName("minor-count")]
		public int? MinorTickCount { get; set; }

		/// <summary>
		/// Get or set the orientation of the scale ('horizontal' or 'vertical').
		/// </summary>
		[HtmlAttributeName("scale-orientation")]
		public string ScaleOrientation { get; set; }

		/// <summary>
		/// Get or set whether scale labels are visible.
		/// </summary>
		[HtmlAttributeName("show-scale-labels")]
		public bool? ShowScaleLabels { get; set; }

		/// <summary>
		/// Get or set whether major ticks are visible.
		/// </summary>
		[HtmlAttributeName("ticks")]
		public bool ShowTicks { get; set; }

		/// <summary>
		/// Get or set whether minor ticks are visible.
		/// </summary>
		[HtmlAttributeName("minor-ticks")]
		public bool ShowMinorTicks { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LinearGaugeTagHelper properties: Range container
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the width of the range container.
		/// </summary>
		[HtmlAttributeName("range-container-width")]
		public double? RangeContainerWidth { get; set; }

		/// <summary>
		/// Get or set the offset of the range container.
		/// </summary>
		[HtmlAttributeName("range-offset")]
		public double? RangeOffset { get; set; }

		/// <summary>
		/// Get or set the orientation of the range container ('horizontal' or 'vertical').
		/// </summary>
		[HtmlAttributeName("range-container-horz-orientation")]
		public HorizontalAlignment? RangeContainerHorizontalOrientation { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LinearGaugeTagHelper properties: Appearance
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the class attribute for the control element.
		/// </summary>
		[HtmlAttributeName("elem-class")]
		public string ElementClass { get; set; }

		/// <summary>
		/// Get or set the orientation of the gauge ('horizontal' or 'vertical').
		/// </summary>
		[HtmlAttributeName("orientation")]
		public string Orientation { get; set; }

		/// <summary>
		/// Get or set whether the range bar is visible.
		/// </summary>
		[HtmlAttributeName("show-range-bar")]
		public bool? ShowRangeBar { get; set; }

		/// <summary>
		/// Get or set the type of the main value indicator ('circle', 'rangeBar', 'rectangle', 'rhombus', 'textCloud', 'triangleMarker').
		/// </summary>
		[HtmlAttributeName("value-indicator-type")]
		public string ValueIndicatorType { get; set; }

		/// <summary>
		/// Get or set the type of the sub-value indicators ('circle', 'rangeBar', 'rectangle', 'rhombus', 'textCloud', 'triangleMarker').
		/// </summary>
		[HtmlAttributeName("subvalue-indicator-type")]
		public string SubValueIndicatorType { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LinearGaugeTagHelper properties: Behavior
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set whether tooltips are shown on hover.
		/// </summary>
		[HtmlAttributeName("show-tooltip")]
		public bool? ShowTooltip { get; set; }

		/// <summary>
		/// Get or set whether animations are enabled.
		/// </summary>
		[HtmlAttributeName("animation")]
		public bool AnimationEnabled { get; set; } = true;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LinearGaugeTagHelper properties: Events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the JS method to be executed when the gauge is initialized.
		/// </summary>
		[HtmlAttributeName("initialized")]
		public string OnInitialized { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a tooltip is shown.
		/// </summary>
		[HtmlAttributeName("tooltip-shown")]
		public string OnTooltipShown { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a tooltip is hidden.
		/// </summary>
		[HtmlAttributeName("tooltip-hidden")]
		public string OnTooltipHidden { get; set; }

		#endregion
	}

	public class ContainedRangeTagHelperBase : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ContainedRangeTagHelperBase properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		/// <summary>
		/// Get or set the ID of this range. Not used anywhere currently.
		/// </summary>
		[HtmlAttributeName("id")]
		public string ID { get; set; }

		#endregion
	}

	/// <summary>
	/// The <see cref="ContainedRangeTagHelper"/> tag helper implements a range element with start value, end value, color, and other properties.
	/// It is currently used for:
	/// * Range indicators within a <see cref="LinearGaugeTagHelper"/>.
	/// * Range bars within gauge components.
	/// </summary>
	[HtmlTargetElement("range", ParentTag = "dx-lineargauge", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ContainedRangeTagHelper : ContainedRangeTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ContainedRangeTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the HTML of this tag, it is used for DX range generation only
			output.SuppressOutput();

			// Process the range tags and remember the content, so that the parent can process it
			RangeContext rangeContext = GetContextSafe<RangeContext>(context);

			string name = TranslateToProp(Name, ViewContext);
			string title = TranslateToProp(Title, ViewContext);

			IHtmlContent content = await output.GetChildContentAsync();

			RangeModel range = new RangeModel
			{
				ID = ID,

				// Range properties
				Name = name,
				StartValue = StartValue,
				EndValue = EndValue,
				Color = Color,
				Title = title,

				// Appearance
				Width = Width,
				Opacity = Opacity,

				// Range template
				Content = content,
			};

			rangeContext.Ranges.Add(range);
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ContainedRangeTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the name for this range.
		/// </summary>
		[HtmlAttributeName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Get or set the start value of this range on the gauge scale.
		/// </summary>
		[HtmlAttributeName("start")]
		public double? StartValue { get; set; }

		/// <summary>
		/// Get or set the end value of this range on the gauge scale.
		/// </summary>
		[HtmlAttributeName("end")]
		public double? EndValue { get; set; }

		/// <summary>
		/// Get or set the color of this range. Can be a hex color, named color, or CSS color value.
		/// </summary>
		[HtmlAttributeName("color")]
		public string Color { get; set; }

		/// <summary>
		/// Get or set the title or hint to be displayed for this range.
		/// </summary>
		[HtmlAttributeName("title")]
		public string Title { get; set; }

		/// <summary>
		/// Get or set the width of this range indicator.
		/// </summary>
		[HtmlAttributeName("width")]
		public double? Width { get; set; }

		/// <summary>
		/// Get or set the opacity of this range (0.0 to 1.0).
		/// </summary>
		[HtmlAttributeName("opacity")]
		public double? Opacity { get; set; }

		/// <summary>
		/// Get or set the content or template for this range.
		/// </summary>
		public IHtmlContent Content { get; set; }

		/// <summary>
		/// Get or set the template for this range.
		/// </summary>
		public string Template { get; set; }

		#endregion
	}

	/// <summary>
	/// The <see cref="RangeContext"/> class holds a collection of ranges for parent controls to process.
	/// </summary>
	public class RangeContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region RangeContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IHtmlContent TemplateContent { get; set; }
		public IHtmlContent RangeTemplateContent { get; set; }

		public IList<RangeModel> Ranges { get; } = new List<RangeModel>();

		#endregion
	}

	/// <summary>
	/// The <see cref="RangeModel"/> class represents a single range element with its properties.
	/// </summary>
	public class RangeModel
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region RangeModel properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the ID of this range. Not used anywhere currently.
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// Get or set the name of the range.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Get or set the start value of this range on the gauge scale.
		/// This property must be lowercase for DevExtreme to recognize it as startValue property.
		/// </summary>
		[JsonPropertyName("startValue")]
		public double? StartValue { get; set; }

		/// <summary>
		/// Get or set the end value of this range on the gauge scale.
		/// This property must be lowercase for DevExtreme to recognize it as endValue property.
		/// </summary>
		[JsonPropertyName("endValue")]
		public double? EndValue { get; set; }

		/// <summary>
		/// Get or set the color of this range.
		/// This property must be lowercase for DevExtreme to recognize it as color property.
		/// </summary>
		[JsonPropertyName("color")]
		public string Color { get; set; }

		/// <summary>
		/// Get or set the title or hint to be displayed for this range.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Get or set the width of this range indicator.
		/// This property must be lowercase for DevExtreme to recognize it as width property.
		/// </summary>
		[JsonPropertyName("width")]
		public double? Width { get; set; }

		/// <summary>
		/// Get or set the opacity of this range (0.0 to 1.0).
		/// This property must be lowercase for DevExtreme to recognize it as opacity property.
		/// </summary>
		[JsonPropertyName("opacity")]
		public double? Opacity { get; set; }

		/// <summary>
		/// Get or set the content or template of this range.
		/// </summary>
		public IHtmlContent Content { get; set; }

		/// <summary>
		/// Get or set the content or template for this range.
		/// </summary>
		public string Template { get; set; }

		#endregion
	}
}
