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
	/// The <see cref="TextboxTagHelper"/> type implements a simple text box.
	/// </summary>
	[HtmlTargetElement("dx-textbox")]
	public class TextboxTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TextboxTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public TextboxTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator)
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
		#region TextboxTagHelper overrides
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
			TextBoxBuilder builder = _htmlHelper.DevExtreme().TextBox();

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

			// Add buttons, but only if we have some
			if (buttonContext.Buttons.Any() || !string.IsNullOrEmpty(HelpText))
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
					AddCustomButtons(b, buttonContext.Buttons);
				});
			}
			else
			{
				// We have NO buttons, but check for the clear button
				builder = builder.ShowClearButton(AllowClear);
			}

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

			// Process text-box specific properties
			//                     .Mask("+1 (X00) 000-0000")
			if (AllowClear)
				builder = builder.ShowClearButton(true);

			builder = builder.Mode(Mode);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private TextBoxBuilder ProcessCommon(TextBoxBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			// Seemingly can only add one attribute using this method
			builder = builder.InputAttr("dx-field-name", Name);

			return builder;
		}

		private TextBoxBuilder ProcessAttributes(TextBoxBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value.ToString());

			return builder;
		}

		private TextBoxBuilder ApplyFor(TextBoxBuilder builder, object value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			if (For == null)
				value = Value;
			
			if (value != null)
				builder = builder.Value(value.ToString());
			
			if (!string.IsNullOrEmpty(Placeholder))
			{
				string placeholder = TranslateToProp(Placeholder, ViewContext);
				builder = builder.Placeholder(placeholder);
			}

			return builder;
		}

		private TextBoxBuilder ProcessTitle(TextBoxBuilder builder)
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
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public string Value { get; set; }

		/// <summary>
		/// Get or set an indicator if the clear button should be shown. Defaults to true.
		/// </summary>
		[HtmlAttributeName("clear")]
		public bool AllowClear { get; set; } = true;

		/// <summary>
		/// Get or set mode of this text box. The mode sets behaviour for common use cases, such as passwords and email. Defaults to a standard text box.
		/// </summary>
		[HtmlAttributeName("mode")]
		public TextBoxMode Mode { get; set; } = TextBoxMode.Text;

		#endregion
	}
}
