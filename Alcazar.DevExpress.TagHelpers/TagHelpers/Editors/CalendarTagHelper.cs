using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
    /// <summary>
    /// The <see cref="CalendarTagHelper"/> type implements a calendar control.
    /// </summary>
    [HtmlTargetElement("dx-calendar")]
    public class CalendarTagHelper : EditorTagHelperBase
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region CalendarTagHelper construction
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public CalendarTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
        }

        private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region CalendarTagHelper overrides
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

            // Create the builder for a popup
            CalendarBuilder builder = _htmlHelper.DevExtreme().Calendar();

            // Apply the control context, which is values which an outer dx-field or dx-control tag might want to pass into me, the editor
            ApplyControlContext(context);

            // Process common functionality for editors
            ProcessCommon(builder);

            // Process non-tag attriubtes
            builder = ProcessAttributes(builder, output.Attributes);

            // Process the For attribute, if it is set
            object value = ProcessFor();

            // Apply the For attribute, or the corresponding direct values
            ApplyFor(builder, value);

            // Process the title/hint, if it is set
            ProcessTitle(builder);

            // Create the context, so that we can pass it to child tag helpers
            ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);

            if (IsReadonly)
            {
                builder = builder
                    .ReadOnly(true)
                    .HoverStateEnabled(true);
            }

            if (IsDisabled)
            {
                builder = builder.Disabled(true);
            }

            // Calendar specific properties - layout
            builder = builder.FirstDayOfWeek(FirstDay);
            builder = builder.ZoomLevel(ZoomLevel);
            if (MinZoomLevel.HasValue)
                builder = builder.MinZoomLevel(MinZoomLevel.Value);
            if (MaxZoomLevel.HasValue)
                builder = builder.MaxZoomLevel(MaxZoomLevel.Value);

            // Calendar specific properties - behaviour
            builder = builder.DateSerializationFormat(DateSerializationFormat);
            builder = builder.SelectionMode(SelectionMode);
            builder = builder.SelectWeekOnClick(SelectWeekOnClick);
            builder = builder.ShowTodayButton(ShowTodayButton);

            if (WeekNumberRule.HasValue)
            {
                builder = builder.ShowWeekNumbers(true);
                builder = builder.WeekNumberRule(WeekNumberRule.Value);
            }

            builder = builder.ValidationMessageMode(ValidationMessageMode);

			// Calendar specific properties - events
			builder = builder.OnInitialized(OnInitialized);
			builder = builder.OnValueChanged(OnValueChanged);
			builder = builder.OnOptionChanged(OnOptionChanged);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
        }

        private CalendarBuilder ProcessCommon(CalendarBuilder builder)
        {
            // Set the ID to a random value
            string idValue = ID ?? Guid.NewGuid().ToString();
            builder = builder.ID(idValue);

            // Set the width and height
            if (!string.IsNullOrEmpty(Width))
                builder = builder.Width(Width);

            return builder;
        }

        private CalendarBuilder ProcessAttributes(CalendarBuilder builder, TagHelperAttributeList attributes)
        {
            // We are choosing to place the attributes on the element, not the imput
            foreach (var attr in attributes)
                builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

            return builder;
        }

        private CalendarBuilder ApplyFor(CalendarBuilder builder, object value)
        {
            if (!string.IsNullOrEmpty(Name))
                builder = builder.Name(Name);

            // Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
            if (For == null)
                value = Value;

            if (value != null)
                builder = builder.Value(value.ToString());
            else if (!string.IsNullOrEmpty(ValueJS))
                builder = builder.Value(new JS(ValueJS));

            return builder;
        }

        private CalendarBuilder ProcessTitle(CalendarBuilder builder)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                string title = TranslateToProp(Title, ViewContext);
                builder.Hint(title);
            }

            return builder;
        }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region TextboxTagHelper properties: tag helper
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the string value to be displayed in this control.
        /// </summary>
        [HtmlAttributeName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Get or set the JS function which provides the value to be displayed in this control.
        /// </summary>
        [HtmlAttributeName("value-js")]
        public string ValueJS { get; set; }

         /// <summary>
        /// Get or set which day is to be shown as first day of the week  on the calendar control.
        /// </summary>
        [HtmlAttributeName("first-day")]
        public FirstDayOfWeek FirstDay { get; set; } = FirstDayOfWeek.Monday;

        /// <summary>
        /// Get or set the initial zoom level of the calendar control.
        /// </summary>
        [HtmlAttributeName("zoom")]
        public CalendarZoomLevel ZoomLevel { get; set; } = CalendarZoomLevel.Month;

        /// <summary>
        /// Get or set the minimum zoom level of the calendar control.
        /// </summary>
        [HtmlAttributeName("zoom")]
        public CalendarZoomLevel? MinZoomLevel { get; set; }

        /// <summary>
        /// Get or set the maximum zoom level of the calendar control.
        /// </summary>
        [HtmlAttributeName("zoom")]
        public CalendarZoomLevel? MaxZoomLevel { get; set; }

        /// <summary>
        /// Get or set the format to serialise dates for the calendar control.
        /// </summary>
        [HtmlAttributeName("format")]
        public string DateSerializationFormat { get; set; }

        /// <summary>
        /// Get or set the selection mode for the calendar control.
        /// </summary>
        [HtmlAttributeName("mode")]
        public CalendarSelectionMode SelectionMode { get; set; } = CalendarSelectionMode.Single;

        /// <summary>
        /// Get or set indicator if the entire week should be selected in click for the calendar control.
        /// </summary>
        [HtmlAttributeName("select-week")]
        public bool SelectWeekOnClick { get; set; }

        /// <summary>
        /// Get or set an indicator if the today button should be shown for the calendar control.
        /// </summary>
        [HtmlAttributeName("today")]
        public bool ShowTodayButton { get; set; }

        /// <summary>
        /// Get or set rule for displaying week numbers for the calendar control.
        /// </summary>
        [HtmlAttributeName("week-number")]
        public WeekNumberRule? WeekNumberRule { get; set; }

        /// <summary>
        /// Get or set the mode for validation messages for the calendar control.
        /// </summary>
        [HtmlAttributeName("validation-message")]
        public ValidationMessageMode ValidationMessageMode { get; set; } = ValidationMessageMode.Auto;

		#endregion
	}
}
