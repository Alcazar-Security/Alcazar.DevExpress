using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Alcazar.Web.Extensibility
{
	public class EditorTagHelperBase : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TextboxTagHelper protected methods
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		protected object ProcessFor()
		{
			// If the asp-for is not set, return null
			// We might need the value, but we can only set the value property for the specific editor, not here in the base class
			if (For == null)
				return null;

			// Set the name
			Name = For.Name;
			Placeholder = For.Metadata.DisplayName;

			// Set the type if we dont have one yet
			if (ModelType == null)
				ModelType = For.Metadata.ModelType;

			try
			{
				// Converting the return value to string. This is needed for enum values in a select-box, as the value would otherwise be translated to the int representation and then not set the inital value
				// Lsts see if that works for other use cases
				object value = For.ModelExplorer.Container.ModelType.InvokeMember(Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, For.ModelExplorer.Container.Model, null);
				return value?.ToString();
			}
			catch
			{
				return null;
			}
		}

		protected void AddHelpButton(CollectionFactory<TextEditorButtonBuilder> button)
		{
			if (!string.IsNullOrEmpty(HelpText))
			{
				string helpText = TranslateToProp(HelpText, ViewContext);
				button.Add()
					.Name("help")
					.Location(TextEditorButtonLocation.After)
					.Widget(w => w.Button()
						.Icon("help")
						.Hint(helpText)
						.StylingMode(ButtonStylingMode.Contained));
			}
		}

		protected void AddCustomButtons(CollectionFactory<TextEditorButtonBuilder> builder, IEnumerable<ButtonModel> buttons)
		{
			foreach (ButtonModel button in buttons)
			{
				builder.Add()
					.Name(button.Name)
					.Location(button.Location)
					.Widget(w => w.Button()
						.Icon(button.Icon)
						.Text(button.Text)
						.StylingMode(button.Styling));
			}
		}

		protected void Render(TagHelperContext context, TagHelperContent content, IHtmlContent result)
		{
			// This control could be embedded in a dx-field tag, in which case we pass the control content back in the context
			ControlContext controlContext = GetContextSafe<ControlContext>(context);
			if (controlContext != null)
			{
				// We are embedded in an outer control container (a dx-field), therefore pass the output to the container
				controlContext.ControlContent = result;
				controlContext.For = For;
				controlContext.Name = Name;
			}

			// Render the builder (into the POST content)
			content.SetHtmlContent(result);
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region EditorTagHelperBase properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary> 
		/// Get or set the ID and name of the input element. <see cref="Name"/> and <see cref="For"/> are mutually exclusive.
		/// </summary> 
		/// <remarks> 
		/// Passed through to the generated HTML in all cases. Also used to determine whether <see cref="For"/> is valid with an empty <see cref="ModelExpression.Name"/>. 
		/// </remarks> 
		[HtmlAttributeName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Get or set the Model property for which this input element is for. <see cref="Name"/> and <see cref="For"/> are mutually exclusive.
		/// </summary>
		[HtmlAttributeName("asp-for")]
		public ModelExpression For { get; set; }

		/// <summary>
		/// Get or set an placeholder to be shown in the control.
		/// </summary>
		[HtmlAttributeName("placeholder")]
		public string Placeholder { get; set; }

		/// <summary>
		/// Get or set the title to be used for this control. The title is displayed over the control itself, while the help text is displayed over a [?] button.
		/// </summary>
		[HtmlAttributeName("title")]
		public string Title { get; set; }

		/// <summary>
		/// Get or set the help text to be used for this control. The help text is displayed over a [?] button, while the title is displayed over the control itself.
		/// </summary>
		[HtmlAttributeName("help")]
		public string HelpText { get; set; }

		/// <summary>
		/// Get or set an indicator if this control is readonly.
		/// </summary>
		[HtmlAttributeName("readonly")]
		public bool IsReadonly { get; set; }

		/// <summary>
		/// Get or set the model type. This property is not set by an attribute.
		/// </summary>
		public Type ModelType { get; set; }

		#endregion
	}
}
