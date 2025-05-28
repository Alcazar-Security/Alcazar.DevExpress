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
	/// The <see cref="CheckboxTagHelper"/> type implements a check box.
	/// </summary>
	[HtmlTargetElement("dx-checkbox")]
	public class CheckboxTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region CheckboxTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public CheckboxTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
			//_urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
			//_htmlEncoder = htmlEncoder;
			//_viewComponentHelper = viewComponentHelper;
			//_generator = generator;
		}

		//private readonly IViewComponentHelper _viewComponentHelper;
		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;
		//private readonly IUrlHelper _urlHelper;
		//private readonly HtmlEncoder _htmlEncoder;
		//private readonly IHtmlGenerator _generator;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region CheckboxTagHelper overrides
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
			CheckBoxBuilder builder = _htmlHelper.DevExtreme().CheckBox();

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

			// Process thedisabled state
			if (IsDisabled)
			{
				builder = builder.Disabled(true);
			}

			// Process text-area specific properties
			builder = builder.EnableThreeStateBehavior(ThreeState);

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

			builder = builder.Text(labelText);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private CheckBoxBuilder ProcessCommon(CheckBoxBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private CheckBoxBuilder ProcessAttributes(CheckBoxBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		private CheckBoxBuilder ApplyFor(CheckBoxBuilder builder, object value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			if (For == null)
				value = Value;

			if (value == null)
			{
				// Setting the value even if it is null (we could be in tri-state)
				if (ThreeState)
					builder = builder.Value(null);
			}
			else if (value is bool boolValue1)
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

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region CheckboxTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public bool? Value { get; set; }

		/// <summary>
		/// Get or set the custom text for the label. Defaults to the equivalent of 'DisplayNameFor', woth a fallback to <see cref="base.Name"/>.
		/// The check box control has this property, as the control can include the label.
		/// </summary>
		[HtmlAttributeName("label-text")]
		public string LabelText { get; set; }

		/// <summary>
		/// Get or set an indicator if check box should have three-state behaviour.
		/// </summary>
		[HtmlAttributeName("three-state")]
		public bool ThreeState { get; set; }

		#endregion
	}
}
