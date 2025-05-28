using Amaqele.Common.Types;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="EditorTagHelperBase"/> base type implements properties and methods commonly required by editor controls.
	/// </summary>
	public class EditorTagHelperBase : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region EditorTagHelperBase protected methods
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Before processing an inner control, pass in any properties from the dx-field or dx-control to the editor. 
		/// This is used for properties which are used by more than one of the label, control, and validation, so that the dx-field declares them once and the field parts share them (where an inner control is declared)
		/// </summary>
		protected void ApplyControlContext(TagHelperContext context)
		{
			ControlContext controlContext = GetContextSafe<ControlContext>(context);
			if (controlContext != null)
				ApplyControlContext(controlContext);
		}

		/// <summary>
		/// Before processing an inner control, pass in any properties from the dx-field or dx-control to the editor. 
		/// This is used for properties which are used by more than one of the label, control, and validation, so that the dx-field declares them once and the field parts share them (where an inner control is declared)
		/// </summary>
		protected void ApplyControlContext(ControlContext controlContext)
		{
			// Apply values which the control context might want to pass into me, the editor
			if (Name == null)
				Name = controlContext.Name;
			if (For == null)
				For = controlContext.For;
		}

		protected object ProcessFor()
		{
			// If the asp-for is not set, return null
			// We might need the value, but we can only set the value property for the specific editor, not here in the base class
			if (For == null)
				return null;

			// Set the name
			Name = For.Name;

			// Set the place holder, if we dont have an explicit one
			if (string.IsNullOrEmpty(Placeholder))
				Placeholder = For.Metadata.DisplayName;

			// Set the type if we dont have one yet
			if (ModelType == null)
				ModelType = For.Metadata.ModelType;

			try
			{
				object value = For.ModelExplorer.Container.ModelType.InvokeMember(Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, For.ModelExplorer.Container.Model, null);
				return value;
			}
			catch
			{
				return null;
			}
		}

		protected object ProcessForEnums(object value)
		{
			// Converting the return value to string. This is needed for enum values in a select-box, as the value would otherwise be translated to the int representation and then not set the inital value
			// Lets see if that works for other use cases
			if (For != null && For.ModelExplorer.ModelType.IsEnum)
			{
				// Our bound property is an enum
				// Convert to a string
				return value?.ToString();
			}

			return value;
		}

		protected System.Collections.IEnumerable ProcessForMultiEnums(object value)
		{
			if (value != null)
			{
				// Converting the return value to string. This is needed for enum values in a select-box, as the value would otherwise be translated to the int representation and then not set the inital value
				// Lets see if that works for other use cases
				if (For.ModelExplorer.ModelType.IsEnum)
				{
					// Our bound property is an enum
					if (value is Enum enumValue)
					{
						return enumValue.GetFlags(false)
							.Select((o) => o?.ToString())
							.ToArray();
					}

					return null;
				}

				else if (For.ModelExplorer.ModelType.IsArray)
				{
					// Our bound property is an array
					Type elementType = For.ModelExplorer.ModelType.GetElementType();
					if (elementType.IsEnum)
					{
						// Our bound property is an array of enums
						// Convert to an array of strings
						Array values = value as Array;
						return values.OfType<Enum>()
							.Select((o) => o?.ToString())
							.ToArray();
					}
				}

				return null;
			}

			return null;
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

			// Render the builder (into the HTML content)
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

		/// <summary>
		/// Get or set parameters used for loading of data records from the data source.
		/// </summary>
		[HtmlAttributeName(DictionaryAttributePrefix = "input-attr-")]
		public IDictionary<string, object> InputAttributes { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region EditorTagHelperBase properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		protected const string ValueString = "Value";
		protected const string TextString = "Text";

		#endregion
	}
}
