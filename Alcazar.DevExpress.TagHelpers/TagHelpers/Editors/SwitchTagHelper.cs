using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="SwitchTagHelper"/> type implements an on/off switch.
	/// </summary>
	[HtmlTargetElement("dx-switch")]
	public class SwitchTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SwitchTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public SwitchTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SwitchTagHelper overrides
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
			SwitchBuilder builder = _htmlHelper.DevExtreme().Switch();

			// Apply the control context, which is values which an outer dx-field or dx-control tag might want to pass into me, the editor
			ApplyControlContext(context);

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

			// Process the read-only state
			if (IsReadonly)
			{
				builder = builder
					.ReadOnly(true)
					.HoverStateEnabled(true);
			}

			// Set the label text (From LabelText, For, Name, in that order)
			string labelText = null;
			if (!string.IsNullOrEmpty(LabelText))
				labelText = TranslateToProp(LabelText, ViewContext);

			if (string.IsNullOrEmpty(labelText))
			{
				if (For != null)
					labelText = For.Metadata.GetDisplayName();
			}

			if (string.IsNullOrEmpty(labelText))
				labelText = Name;

			// Process events
			builder = ProcessEvents(builder);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private SwitchBuilder ProcessCommon(SwitchBuilder builder)
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

		private SwitchBuilder ProcessAttributes(SwitchBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
			{
				if (attr.Name != "class")
					builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());
			}

			// Apply the label class
			if (!string.IsNullOrEmpty(LabelClass))
				builder.ElementAttr("class", LabelClass);

			// No option for attributes on the input field here
			return builder;
		}

		private SwitchBuilder ApplyFor(SwitchBuilder builder, object value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			if (For == null)
				value = Value;

			if (value is bool boolValue1)
			{
				// Setting the value as direct bool
				builder = builder.Value(boolValue1);
			}
			else if (value is string stringValue)
			{
				// Setting the value from string
				Boolean.TryParse(stringValue, out bool boolValue2);
				builder = builder.Value(boolValue2);
			}

			return builder;
		}

		private SwitchBuilder ProcessEvents(SwitchBuilder builder)
		{
			if (!string.IsNullOrEmpty(OnContentReady))
				builder = builder.OnContentReady(OnContentReady);
			if (!string.IsNullOrEmpty(OnValueChanged))
				builder = builder.OnValueChanged(OnValueChanged);

			if (!string.IsNullOrEmpty(OnInitialized))
				builder = builder.OnInitialized(OnInitialized);
			if (!string.IsNullOrEmpty(OnOptionChanged))
				builder = builder.OnOptionChanged(OnOptionChanged);
			if (!string.IsNullOrEmpty(OnDisposing))
				builder = builder.OnDisposing(OnDisposing);

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region CheckboxTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public bool Value { get; set; }

		/// <summary>
		/// Get or set the custom text for the label. Defaults to the equivalent of 'DisplayNameFor', woth a fallback to <see cref="base.Name"/>.
		/// The check box control has this property, as the control can include the label.
		/// </summary>
		[HtmlAttributeName("label-text")]
		public string LabelText { get; set; }

        /// <summary>
        /// Get or set the class to be applied to the label.
        /// The check box control has this property, as the control can include the label.
        /// </summary>
        [HtmlAttributeName("label-class")]
        public string LabelClass { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region CheckboxTagHelper properties: tag helper events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the Javascript method to be called when the switch is being disposed.
		/// </summary>
		[HtmlAttributeName("disposing")]
		public string OnDisposing { get; set; }
	
		#endregion
	}
}
