using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;
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

			// Defaults to true for password mode, false otherwise. If ShowPasswordToggle is set, it overrides the default.
			bool showPasswordToggle = ShowPasswordToggle ?? (Mode == TextBoxMode.Password);

			// Add buttons, but only if we have some OR if this is a password field with toggle
			bool needsButtons = buttonContext.Buttons.Any() || !string.IsNullOrEmpty(HelpText) || showPasswordToggle;
			if (needsButtons)
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Buttons(b =>
				{
					// Add password toggle button for password mode
					AddPasswordToggleButton(b, showPasswordToggle);

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

			// Process events
			builder = ProcessEvents(builder);

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
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// And we are allowing attributes on the input field also
			foreach (var attr in InputAttributes)
				builder = builder.InputAttr(attr.Key, attr.Value?.ToString());

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
			else if (!string.IsNullOrEmpty(ValueJS))
                builder = builder.Value(new JS(ValueJS));

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

		private TextBoxBuilder ProcessEvents(TextBoxBuilder builder)
		{
			if (!string.IsNullOrEmpty(OnContentReady))
				builder = builder.OnContentReady(OnContentReady);
			if (!string.IsNullOrEmpty(OnChange))
				builder = builder.OnChange(OnChange);
			if (!string.IsNullOrEmpty(OnInitialized))
				builder = builder.OnInitialized(OnInitialized);
			if (!string.IsNullOrEmpty(OnOptionChanged))
				builder = builder.OnOptionChanged(OnOptionChanged);
			if (!string.IsNullOrEmpty(OnValueChanged))
				builder = builder.OnValueChanged(OnValueChanged);
			if (!string.IsNullOrEmpty(OnEnterKey))
				builder = builder.OnEnterKey(OnEnterKey);
			if (!string.IsNullOrEmpty(OnFocusOut))
				builder = builder.OnFocusOut(OnFocusOut);
			if (!string.IsNullOrEmpty(OnInput))
				builder = builder.OnInput(OnInput);

			return builder;
		}

		private void AddPasswordToggleButton(CollectionFactory<TextEditorButtonBuilder> builder, bool showPasswordToggle)
		{
			if (showPasswordToggle)
			{
				string toggleHint = TranslateToProp("#ShowPasswordToggleHint", ViewContext);
				builder.Add()
					.Name("password-toggle")
					.Location(TextEditorButtonLocation.After)
					.Widget(w => w.Button()
						.Icon(HidePasswordIcon)
						.Hint(toggleHint)
						.StylingMode(ButtonStylingMode.Text)
						.OnClick("onPasswordToggle"));
			}
		}

		public const string ShowPasswordIcon = "bi bi-eye";
		public const string HidePasswordIcon = "bi bi-eye-slash";

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
		/// Get or set mode of this text box. The mode sets behaviour for common use cases, such as passwords and email. Defaults to a standard text box.
		/// </summary>
		[HtmlAttributeName("mode")]
		public TextBoxMode Mode { get; set; } = TextBoxMode.Text;

		/// <summary>
		/// Get or set whether to show the password toggle button for password mode.
		/// Defaults to true for password mode, false otherwise.
		/// </summary>
		[HtmlAttributeName("password-toggle")]
		public bool? ShowPasswordToggle { get; set; }

		#endregion
	}
}
