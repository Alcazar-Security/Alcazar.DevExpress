using Amaqele.Common.Base;
using Amaqele.Common.Types;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DropdownTagHelperBase"/> base type implements properties and methods commonly required by dropdown controls.
	/// Dropdown controls are editors which can display a dropdown for selection.
	/// </summary>
	public class DropdownTagHelperBase : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownTagHelperBase properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the JS function which sets the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value-js")]
		public string ValueJS { get; set; }

		/// <summary>
		/// Get or set the JS function which updates the value back into the cell ehich is edited by this control.
		/// </summary>
		[HtmlAttributeName("set-value")]
		public string SetValueJS { get; set; }

		/// <summary>
		/// Get or set the JS function to be executed when the value of the dropdown box is changed.
		/// </summary>
		[HtmlAttributeName("value-changed")]
		public string OnValueChanged { get; set; }

		/// <summary>
		/// Get or set the JS function to be executed when the selection in the control changes.
		/// </summary>
		[HtmlAttributeName("selection-changed")]
		public string OnSelectionChanged { get; set; }

		#endregion
	}

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
		/// <returns> The control context, if any. </returns>
		protected ControlContext ApplyControlContext(TagHelperContext context)
		{
			ControlContext controlContext = GetContextSafe<ControlContext>(context);
			if (controlContext != null)
				ApplyControlContext(controlContext);

			return controlContext;
		}

		/// <summary>
		/// Before processing an inner control, pass in any properties from the dx-field or dx-control to the editor. 
		/// This is used for properties which are used by more than one of the label, control, and validation, so that the dx-field declares them once and the field parts share them (where an inner control is declared)
		/// </summary>
		protected void ApplyControlContext(ControlContext controlContext)
		{
			// Apply values which the control context might want to pass into me, the editor
			if (ID == null)
				ID = controlContext.Name ?? controlContext.For?.Name;
			if (Name == null)
				Name = controlContext.Name;
			if (For == null)
				For = controlContext.For;

			// These two are of interest for the DxOptionTagHelper
			// DxOptionTagHelper also wants to set the value, but we cant, Value is defined by the editor itself, not by this base class (it can be a string or an object) 
			// DxOptionTagHelper also wants to set the items, but we cant, Items is defined only for some editors, not by this base class
			if (ModelType == null)
				ModelType = controlContext.ModelType;
			if (HelpText == null)
				HelpText = controlContext.HelpText;
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
				// Option 1. Get the value as text (this can be a problem, it is a string, which is not suitable for e.g. DateBoxes)
				string simple = For.ModelExplorer.GetSimpleDisplayText();

				// Option 2. Get the value as object
				object value2 = For.Model;

				// Option 3. 
				// Doing something really snazzy here, climbing the tree to get to my value, but we already have the simple value.
				Type modelType = For.ModelExplorer.Container.ModelType;
				object value = For.ModelExplorer.Container.Model;

				string[] path = Name.SplitSafe('.');
				foreach (var name in path)
				{
					if (value == null || modelType == null)
						break;

					value = modelType.InvokeMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, value, null);
					modelType = value?.GetType();
				}

				if (simple as string != value as string)
				{ }

				// Use Option 1. simple value, until proven otherwise
				// Works for WorkflowDefinition.InitialState, where the model is an int, and then the control does not display the incoming value
				return value2;
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

		/// <summary>
		/// Convert a value to a suitable <see cref="System.Collections.IEnumerable"/> for editors which require the value to be a sequence (such as the tag box).
		/// Works with typed and untyped enums, with strings, and with sequences of enums and strings.
		/// </summary>
		/// <param name="value"> The value to be displayed in the editor. </param>
		/// <returns> The sequence of values to be displayed in the editor. </returns>
		protected System.Collections.IEnumerable ProcessForMultiEnums(object value)
		{
			if (value == null)
				return null;

			// Converting the return value to string. This is needed for enum values in a select-box, as the value would otherwise be translated to the int representation and then not set the inital value
			// Lets see if that works for other use cases
			if (For.ModelExplorer.ModelType.IsEnum)
			{
				// Our bound property is an enum, gets obtain its flag composution, and convert to a sequence of individual string values
				if (value is Enum enumValue)
				{
					return enumValue.GetFlags(false)
						.Select((o) => o?.ToString())
						.ToArray();
				}

				// No value, cant return a thing
				return null;
			}

			else if (For.ModelExplorer.ModelType == typeof(string))
			{
				// Our bound property is a plain string — break it up into parts using a default separator
				if (value is string stringValue)
				{
					return stringValue.SplitSafe('|')
						.ToArray();
				}

				// No value, cant return a thing
				return null;
			}

			else if (For.ModelExplorer.ModelType.IsArray)
			{
				// Our bound property is an array
				Type elementType = For.ModelExplorer.ModelType.GetElementType();
				if (elementType.IsEnum)
				{
					// Our bound property is an array of enums, convert to an array of strings
					Array values = value as Array;
					return values.OfType<Enum>()
						.Select((o) => o?.ToString())
						.ToArray();
				}

				else if (elementType == typeof(string))
				{
					// Our bound property is an array of strings — return as-is
					return value as Array;
				}
			}

			else if (typeof(IEnumerable<string>).IsAssignableFrom(For.ModelExplorer.ModelType))
			{
				// Our bound property is IEnumerable<string> — return as-is
				return value as System.Collections.IEnumerable;
			}

			return null;
		}

		protected void AddHelpButton(CollectionFactory<TextEditorButtonBuilder> builder)
		{
			if (!string.IsNullOrEmpty(HelpText))
			{
				string helpText = TranslateToProp(HelpText, ViewContext);
				builder.Add()
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
		/// Get or set an indicator if the clear button should be shown. Defaults to true.
		/// </summary>
		[HtmlAttributeName("clear")]
		public bool AllowClear { get; set; } = true;

		/// <summary>
		/// Get or set the model type. This property is not set by an attribute.
		/// </summary>
		public Type ModelType { get; set; }

		/// <summary>
		/// Get or set parameters used for loading of data records from the data source.
		/// </summary>
		[HtmlAttributeName(DictionaryAttributePrefix = "input-attr-")]
		public IDictionary<string, object> InputAttributes { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Get or set the format of this editor control.
		/// </summary>
		public Format? Format { get; set; }

		/// <summary>
		/// Get or set the custom format of this editor control.
		/// </summary>
		public string CustomFormat { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region EditorTagHelperBase properties: events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		// If any of these events are required on a control, it must be declared as an embedded control with in a dx-field or dx-control

		/// <summary>
		/// Get or set the Javascript method to be called when the control is initialised.
		/// </summary>
		[HtmlAttributeName("initialised")]
		public string OnInitialized { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the content of the control is ready.
		/// </summary>
		[HtmlAttributeName("content-ready")]
		public string OnContentReady { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an option of the control has changed.
		/// </summary>
		[HtmlAttributeName("option-changed")]
		public string OnOptionChanged { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the value in the control changes.
		/// </summary>
		[HtmlAttributeName("change")]
		public string OnChange { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the value in the control changes.
		/// </summary>
		[HtmlAttributeName("value-changed")]
		public string OnValueChanged { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the enter key is pressed in the control changes.
		/// </summary>
		[HtmlAttributeName("enter")]
		public string OnEnterKey { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the control obtains focus.
		/// </summary>
		[HtmlAttributeName("focus-in")]
		public string OnFocusIn { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the control looses focus.
		/// </summary>
		[HtmlAttributeName("focus-out")]
		public string OnFocusOut { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the imput TODO  control changes.
		/// </summary>
		[HtmlAttributeName("input")]
		public string OnInput { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region EditorTagHelperBase properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		protected const string ValueString = "Value";
		protected const string TextString = "Text";

		#endregion
	}
}
