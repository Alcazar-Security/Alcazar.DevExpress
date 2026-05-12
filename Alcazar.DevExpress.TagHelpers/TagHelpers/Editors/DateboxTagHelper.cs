using Alcazar.DevExpress.Utilities;
using Alcazar.Web.Utilities;
using Amaqele.Common.Types;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DateboxTagHelper"/> type implements a date box (with no time component).
	/// </summary>
	[HtmlTargetElement("dx-date")]
	public class DateboxTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DateboxTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;

			// Contrary to most other controls, dont use the clear button by default
			AllowClear = false;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxTagHelper overrides
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
			DateBoxBuilder builder = _htmlHelper.DevExtreme().DateBox();

			// Apply the control context, which is values which an outer dx-field or dx-control tag might want to pass into me, the editor
			ControlContext controlContext = ApplyControlContext(context);

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the For attribute, if it is set
			object value = ProcessFor();

			// Apply the For attribute, or the corresponding direct values
			builder = ApplyFor(builder, value);

			// Process the title/hint, if it is set
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder.Hint(title);
			}

			// Process the timezone
			builder = ProcessTimezone(builder, value, !string.IsNullOrEmpty(Title), out string timezoneText, out string timezoneTitle);

			// Process the read-only state
			if (IsReadonly)
			{
				builder = builder
					.ReadOnly(true)
					.HoverStateEnabled(true);
			}

			// Enable opening calendar on text field click
			builder = builder.OpenOnFieldClick(IsOpenOnFieldClick);

			// Process text-area specific properties
			builder = ProcessFormat(builder, out string customFormat);

			// Process buttons
			builder = ProcessButtons(builder, timezoneText, timezoneTitle);

			// Controls how the date/time value is serialized to and from the server (i.e., the format used in AJAX requests, form posts, or data binding).
			// We use the default ISO8601, works just fine
			// builder = builder.DateSerializationFormat();

			// Set the type: date, time, datetime
			builder = builder.Type(Type);

			builder = ProcessEvents(builder);

			//builder = builder.ActiveStateEnabled(OnChange);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private DateBoxBuilder ProcessCommon(DateBoxBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Visible and disabled
			if (!IsVisible)
				builder = builder.Visible(IsVisible);
			if (IsDisabled)
				builder = builder.Disabled(IsDisabled);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private DateBoxBuilder ProcessAttributes(DateBoxBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// And we are allowing attributes on the input field also
			foreach (var attr in InputAttributes)
				builder = builder.InputAttr(attr.Key, attr.Value?.ToString());

			return builder;
		}

		private DateBoxBuilder ApplyFor(DateBoxBuilder builder, object value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			if (For == null)
				value = Value;

			if (value is DateTime dateValue1)
			{
				// Setting the value as direct date
				builder = builder.Value(dateValue1);
			}
			else if (value is TimeSpan timespan1)
			{
				// Setting the value as direct date
				DateTime dateValue2 = new DateTime(timespan1.Ticks);
				builder = builder.Value(dateValue2);
			}

			return builder;
		}

		/// <summary>
		/// Process the display format of the control.
		/// </summary>
		private DateBoxBuilder ProcessFormat(DateBoxBuilder builder, out string customFormat)
		{
			if (!string.IsNullOrEmpty(CustomFormat))
			{
				// 1. Specified custom format
				builder = builder.DisplayFormat(customFormat = CustomFormat);
			}
			else
			{
				Format? format = null;
				if (Format.HasValue)
				{
					// 2. Specified format
					format = Format.Value;
				}
				else
				{
					// 3. Format based on the date box type
					format = DxCultureUtilities.ToFormat(Type);
				}

				customFormat = null;
				if (format.HasValue)
				{
					// We have determined the format of the content, translate it into the current culture
					customFormat = DxCultureUtilities.GetRequestCultureFormat(ViewContext, format.Value);
					if (!string.IsNullOrEmpty(customFormat))
						builder = builder.DisplayFormat(customFormat);
				}
			}

			return builder;
		}

		/// <summary>
		/// Process the display format of the control using defaults. Not used.
		/// </summary>
		private DateBoxBuilder ProcessDefaultFormat(DateBoxBuilder builder)
		{
			switch (Type)
			{
				// Default is the fill date/time, so that we always see the time component, even if it is 00:00:00
				default:
				case DateBoxType.DateTime:
					builder = builder.DisplayFormat("yyyy-MM-dd HH:mm:ss");
					break;

				// Expressly requesting date only
				case DateBoxType.Date:
					builder = builder.DisplayFormat("yyyy-MM-dd");
					break;

				// Expressly requesting time only
				case DateBoxType.Time:
					builder = builder.DisplayFormat("HH:mm:ss");
					break;
			}

			return builder;
		}

		private DateBoxBuilder ProcessTimezone(DateBoxBuilder builder, object value, bool hasTitle, out string timezoneText, out string timezoneHint)
		{
			timezoneText = null;
			timezoneHint = null;

			TimeZoneInfo timezone = WebTimezoneManager.GetEffectiveTimezone(ViewContext.HttpContext.Session, ViewContext.HttpContext.User);

			// Display timezone information
			if (value is DateTime dateTime)
			{
				// A DateTime uses the Kind parameter, and we dont know the offset
				switch (dateTime.Kind)
				{
					default:
					case DateTimeKind.Unspecified:
						timezoneText = "?";
						timezoneHint = "Un-specified timezone";
						break;

					case DateTimeKind.Utc:
						timezoneText = "Z";
						timezoneHint = "Universal Coordinated Time (UTC)";
						break;

					case DateTimeKind.Local:
						timezoneText = null;
						timezoneHint = $"Local time ({timezone.Id})";
						break;
				}
			}
			else if (value is DateTimeOffset dateTimeOffset)
			{
				// A DateTimeOffset specifies its offset from UTC, and makes our work here much easier
				if (dateTimeOffset.Offset.TotalSeconds == 0.0)
				{
					timezoneText = "Z";
					timezoneHint = "Universal Coordinated Time (UTC)";
				}
				else
				{
					timezoneText = $"{dateTimeOffset.Offset.TotalHours:N1}";
					timezoneHint = $"Local time ({dateTimeOffset.Offset.ToShortFriendly()})";
				}
			}

			if (!hasTitle && !string.IsNullOrEmpty(timezoneHint))
				builder.Hint(timezoneHint);

			return builder;
		}

		private string FormatValue(object value, string customFormat)
		{
			if (value is DateTime dateTime)
			{
				// A DateTime uses the Kind parameter, and we dont know the offset
				switch (dateTime.Kind)
				{
					default:
					case DateTimeKind.Unspecified: return $"{dateTime.ToString(customFormat)}?";
					case DateTimeKind.Utc: return $"{dateTime.ToString(customFormat)}Z";
					case DateTimeKind.Local: return $"{dateTime.ToString(customFormat)}?";
				}
			}
			else if (value is DateTime dateTimeOffset)
			{
				// A DateTime uses the Kind parameter, and we dont know the offset
				switch (dateTimeOffset.Kind)
				{
					default:
					case DateTimeKind.Unspecified: return $"{dateTimeOffset.ToString(customFormat)}?";
					case DateTimeKind.Utc: return $"{dateTimeOffset.ToString(customFormat)}Z";
					case DateTimeKind.Local: return $"{dateTimeOffset.ToString(customFormat)}?";
				}
			}

			return null;
		}

		private DateBoxBuilder ProcessButtons(DateBoxBuilder builder, string timezoneText, string timezoneTitle)
		{
			// Add buttons, but only if we have some
			if (!string.IsNullOrEmpty(HelpText) || !string.IsNullOrEmpty(timezoneText))
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Buttons(b =>
				{
					// Add the clear button, but only if there are other buttons
					if (AllowClear)
						b.Add().Name("clear");

					// Add the custom help button, but only if we have a help text
					AddHelpButton(b);

					// Add custom defined buttons, but only if we have some
					if(!string.IsNullOrEmpty(timezoneText))
						AddTimezoneButton(b, timezoneText, timezoneTitle);

					// Add the default calendar dropdown button EXPLICITLY, as the last one
					b.Add().Name("dropDown");
				});
			}
			else
			{
				// We have NO buttons, but check for the clear button
				builder = builder.ShowClearButton(AllowClear);
			}

			return builder;
		}

		/// <summary>
		/// Add a TZ button which shows the TZ hint.
		/// This is not really needed, the hint is already shown over the entire control, although a 'Z' or '+2' might be a nice move.
		/// But since the offset is not known here, we dont use the button for local time for now. Maybe some JS could get us the offset...
		/// </summary>
		protected void AddTimezoneButton(CollectionFactory<TextEditorButtonBuilder> button, string timezoneText, string timezoneTitle)
		{
			string helpText = TranslateToProp(HelpText, ViewContext);
			button.Add()
				.Name("timezone")
				.Location(TextEditorButtonLocation.After)
				.Widget(w =>
				{
					var button = w.Button();

					if (string.IsNullOrEmpty(timezoneText))
						button = button.Icon("time");
					else
						button = button.Text(timezoneText);

					button = button
						.Hint(timezoneTitle)
						.StylingMode(ButtonStylingMode.Contained);

					return button;
				});
		}

		private DateBoxBuilder ProcessEvents(DateBoxBuilder builder)
		{
			// Inherited events
			if (!string.IsNullOrEmpty(OnInitialized))
				builder = builder.OnInitialized(OnInitialized);

			if (!string.IsNullOrEmpty(OnContentReady))
				builder = builder.OnContentReady(OnContentReady);

			if (!string.IsNullOrEmpty(OnOptionChanged))
				builder = builder.OnOptionChanged(OnOptionChanged);

			if (!string.IsNullOrEmpty(OnChange))
				builder = builder.OnChange(OnChange);

			if (!string.IsNullOrEmpty(OnValueChanged))
				builder = builder.OnValueChanged(OnValueChanged);

			if (!string.IsNullOrEmpty(OnEnterKey))
				builder = builder.OnEnterKey(OnEnterKey);

			if (!string.IsNullOrEmpty(OnFocusIn))
				builder = builder.OnFocusIn(OnFocusIn);

			if (!string.IsNullOrEmpty(OnFocusOut))
				builder = builder.OnFocusOut(OnFocusOut);

			if (!string.IsNullOrEmpty(OnInput))
				builder = builder.OnInput(OnInput);

			// Calendar/dropdown specific events
			if (!string.IsNullOrEmpty(OnOpened))
				builder = builder.OnOpened(OnOpened);

			if (!string.IsNullOrEmpty(OnClosed))
				builder = builder.OnClosed(OnClosed);

			// Keyboard events
			if (!string.IsNullOrEmpty(OnKeyDown))
				builder = builder.OnKeyDown(OnKeyDown);

			if (!string.IsNullOrEmpty(OnKeyUp))
				builder = builder.OnKeyUp(OnKeyUp);

			// Copy/paste events
			if (!string.IsNullOrEmpty(OnCopy))
				builder = builder.OnCopy(OnCopy);

			if (!string.IsNullOrEmpty(OnCut))
				builder = builder.OnCut(OnCut);

			if (!string.IsNullOrEmpty(OnPaste))
				builder = builder.OnPaste(OnPaste); 
			
			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the type of the date box.
		/// </summary>
		[HtmlAttributeName("type")]
		public DateBoxType Type { get; set; } = DateBoxType.Date;

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public DateTime? Value { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxTagHelper properties: behaviour
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
	
		/// <summary>
		/// Get or set an indicator if this control should open the calender control on clickin anywhere in the text field.
		/// </summary>
		[HtmlAttributeName("open-click")]
		public bool IsOpenOnFieldClick { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxTagHelper properties: events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the Javascript method to be called when the calendar/dropdown is opened.
		/// </summary>
		[HtmlAttributeName("opened")]
		public string OnOpened { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the calendar/dropdown is closed.
		/// </summary>
		[HtmlAttributeName("closed")]
		public string OnClosed { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when a key is pressed down.
		/// </summary>
		[HtmlAttributeName("key-down")]
		public string OnKeyDown { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when a key is released.
		/// </summary>
		[HtmlAttributeName("key-up")]
		public string OnKeyUp { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when text is copied.
		/// </summary>
		[HtmlAttributeName("copy")]
		public string OnCopy { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when text is cut.
		/// </summary>
		[HtmlAttributeName("cut")]
		public string OnCut { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when text is pasted.
		/// </summary>
		[HtmlAttributeName("paste")]
		public string OnPaste { get; set; }
		
		#endregion
	}
}
