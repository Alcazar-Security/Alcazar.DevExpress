using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DxControlTagHelper"/> tag helper implements the control (input, etc) for a standard form field <see cref="DxFieldTagHelper"/>.
	/// </summary>
	[HtmlTargetElement("dx-control", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class DxControlTagHelper : FieldTagHelperBase
	{
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxControlTagHelper construction
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary> 
        /// Creates a new <see cref="DxControlTagHelper"/>. 
        /// </summary> 
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param> 
        public DxControlTagHelper(IHtmlHelper htmlHelper, IHtmlGenerator generator, IModelMetadataProvider modelMetadataProvider)
		//	: base(generator)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
			//_modelMetadataProvider = modelMetadataProvider;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;
		//private IModelMetadataProvider _modelMetadataProvider;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DxControlTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Determine the input type (this would have already been done by the dx-field helper, if called from there, and not instantiated separately).
			SetInputType();

			// Set class properties, depending on the chosen style. 
			SetStyleProperties(output);

			// Create the contexts, so that we can pass them to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);
			_controlContext = GetOrCreateContext<ControlContext>(context);

			// Before processing an inner control, pass in any properties from the dx-field to the editor. 
			// This is used for properties which are used by more than one of the label, control, and validation, so that the dx-field declares them once and the field parts share them (where an inner control is declared)
			_controlContext.For = For;
			_controlContext.Name = Name;

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			// Generate the control
			IHtmlContent controlContent;
			if (_controlContext.ControlContent != null)
			{
				// We have an inner dx-control, use it
				controlContent = _controlContext.ControlContent;
			}
			else
			{
				// The dx-field has all control information irself, generate the control 
				// This method is only called if there is no embedded control directly inside the 'dx-field'
				controlContent = await GenerateControl(context, output);
			}
			// Suppress my own output, just use the output from the child label
			output.SuppressOutput();
			output.Content.SetHtmlContent(controlContent);
		}

		private ControlContext _controlContext;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region StandardControlTagHelper methods: generate controls
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		private async Task<IHtmlContent> GenerateControl(TagHelperContext context, TagHelperOutput output)
		{
			IHtmlContent controlContent;

            // Dropdown and select (with select being the default if there are items)
            if (InputTypeName == ControlTypes.MultiSelect)
            {
                controlContent = await GenerateMultiselect(context, output);
            }
            else if (InputTypeName == ControlTypes.Autocomplete)
            {
                controlContent = await GenerateAutocomplete(context, output);
            }
            else if (InputTypeName == ControlTypes.Select || Items != null)
			{
				controlContent = await GenerateSelect(context, output);
			}

			// Standard texts and controls
			else if (InputTypeName == ControlTypes.Textarea)
			{
				// <input asp-for="Name" class="form-control" />
				controlContent = await GenerateTextarea(context, output);
			}
			else if (InputTypeName == ControlTypes.Date)
			{
				controlContent = await GenerateDate(context, output, DateBoxType.Date);
				output.Content.SetHtmlContent(controlContent);
			}
			else if (InputTypeName == ControlTypes.Datetime)
			{
				controlContent = await GenerateDate(context, output, DateBoxType.DateTime);
				output.Content.SetHtmlContent(controlContent);
			}
			else if (InputTypeName == ControlTypes.Time)
			{
				controlContent = await GenerateDate(context, output, DateBoxType.Time);
				output.Content.SetHtmlContent(controlContent);
			}

			// Special-use texts and controls
			else if (InputTypeName == ControlTypes.Password)
			{
				// <input asp-for="Name" class="form-control" />
				controlContent = await GenerateInput(context, output, TextBoxMode.Password);
			}
			else if (InputTypeName == ControlTypes.Email)
			{
				controlContent = await GenerateInput(context, output, TextBoxMode.Email);
			}
			else if (InputTypeName == ControlTypes.Search)
			{
				controlContent = await GenerateInput(context, output, TextBoxMode.Search);
			}

			// Checkbox and radio
			else if (InputTypeName == ControlTypes.Checkbox)
			{
				controlContent = await GenerateCheck(context, output);
				output.Content.SetHtmlContent(controlContent);
			}
			else if (InputTypeName == ControlTypes.Radio)
			{
				controlContent = await GenerateRadio(context, output);
				output.Content.SetHtmlContent(controlContent);
			}
			else
			{
				// <input asp-for="pkEntityID" class="form-control" placeholder="name@example.com" />
				controlContent = await GenerateInput(context, output, TextBoxMode.Text);
			}

			return controlContent;
		}

		/// <summary>
		/// Generate a select (dropdown) control.
		/// The select box is suitable for: single-select 
		/// </summary>
		protected async Task<IHtmlContent> GenerateSelect(TagHelperContext context, TagHelperOutput output)
		{
			// Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
			List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();
			contextAttributes.Add(new TagHelperAttribute("type", InputTypeName));
			if (For != null)
			{
				contextAttributes.Add(new TagHelperAttribute("asp-for", For.Name));
			}

			//if (!string.IsNullOrEmpty(Value))
			//	contextAttributes.Add(new TagHelperAttribute("value", Value));

			// Output attributes are all non-helper attributes on the output tag
			List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);
			outputAttributes.Add(new TagHelperAttribute("placeholder", Placeholder));

			TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
			TagHelperOutput helperOutput = new TagHelperOutput("input", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

			SelectBoxTagHelper childHelper = new SelectBoxTagHelper(_htmlHelper);
			childHelper.Name = Name;
			childHelper.For = For;
			childHelper.Value = Value;
			childHelper.Items = Items;
			childHelper.Title = Title;
			childHelper.HelpText = HelpText;
			childHelper.Placeholder = Placeholder;
			childHelper.AllowClear = AllowClear;
			childHelper.IsReadonly = IsReadonly;
			childHelper.IsDisabled = IsDisabled;

			// Select box specific - no longer, we must use an embedded control within a dx-field (or dx-control) if any of these are used
			// childHelper.ValueExpression = ValueExpression;
			// childHelper.DisplayExpression = DisplayExpression;
			// childHelper.MinSearchLength = MinSearchLength;
			// childHelper.SearchTimeout = SearchTimeout;
			// if (_searchMode.HasValue)
			//	childHelper.SearchMode = _searchMode.Value;

			childHelper.ViewContext = ViewContext;
			childHelper.Init(helperContext);

			await childHelper.ProcessAsync(helperContext, helperOutput);
			return helperOutput;
		}

        /// <summary>
        /// Generate a select (dropdown) control.
        /// The autocomplete box is suitable for: single-select type-ahead
        /// </summary>
        protected async Task<IHtmlContent> GenerateAutocomplete(TagHelperContext context, TagHelperOutput output)
		{
			// Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
			List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();
			contextAttributes.Add(new TagHelperAttribute("type", InputTypeName));
			if (For != null)
			{
				contextAttributes.Add(new TagHelperAttribute("asp-for", For.Name));
			}

			//if (!string.IsNullOrEmpty(Value))
			//	contextAttributes.Add(new TagHelperAttribute("value", Value));

			// Output attributes are all non-helper attributes on the output tag
			List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);
			outputAttributes.Add(new TagHelperAttribute("placeholder", Placeholder));

			TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
			TagHelperOutput helperOutput = new TagHelperOutput("input", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

			AutocompleteTagHelper childHelper = new AutocompleteTagHelper(_htmlHelper, null, null, null, null, null);
			childHelper.Name = Name;
			//childHelper.For = For;
			//childHelper.Value = Value;
			childHelper.Title = Title;
			childHelper.HelpText = HelpText;
			childHelper.Placeholder = Placeholder;
			childHelper.AllowClear = AllowClear;
			childHelper.IsReadonly = IsReadonly;
			childHelper.IsDisabled = IsDisabled;
			//childHelper.Format = Format;
			//childHelper.InputTypeName = InputTypeName;

			// Autocomplete specific - no longer, we must use an embedded control within a dx-field (or dx-control) if any of these are used
			// childHelper.ValueExpression = ValueExpression;

			childHelper.ViewContext = ViewContext;
			childHelper.Init(helperContext);

			await childHelper.ProcessAsync(helperContext, helperOutput);
			return helperOutput;
		}

        /// <summary>
        /// Generate a multi-select (dropdown) control.
        /// The tag box is suitable for: single-select type-ahead
        /// </summary>
        protected async Task<IHtmlContent> GenerateMultiselect(TagHelperContext context, TagHelperOutput output)
        {
            // Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
            List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();
            contextAttributes.Add(new TagHelperAttribute("type", InputTypeName));
            if (For != null)
            {
                contextAttributes.Add(new TagHelperAttribute("asp-for", For.Name));
            }

            //if (!string.IsNullOrEmpty(Value))
            //	contextAttributes.Add(new TagHelperAttribute("value", Value));

            // Output attributes are all non-helper attributes on the output tag
            List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);
            outputAttributes.Add(new TagHelperAttribute("placeholder", Placeholder));

            TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
            TagHelperOutput helperOutput = new TagHelperOutput("input", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

            TagboxTagHelper childHelper = new TagboxTagHelper(_htmlHelper);
            childHelper.Name = Name;
            //childHelper.For = For;
            childHelper.Value = Value;
			childHelper.Items = Items;
            childHelper.Title = Title;
            childHelper.HelpText = HelpText;
            childHelper.Placeholder = Placeholder;
            childHelper.AllowClear = AllowClear;
            childHelper.IsReadonly = IsReadonly;
            childHelper.IsDisabled = IsDisabled;

            // TagBox specific - no longer, we must use an embedded control within a dx-field (or dx-control) if any of these are used
            // childHelper.ValueExpression = ValueExpression;

            childHelper.ViewContext = ViewContext;
            childHelper.Init(helperContext);

            await childHelper.ProcessAsync(helperContext, helperOutput);
            return helperOutput;
        }
        
		/// <summary>
        /// Generate a default input field ('text')
        /// </summary>
        protected async Task<IHtmlContent> GenerateInput(TagHelperContext context, TagHelperOutput output, TextBoxMode mode)
		{
			// <input asp-for="pkEntityID" class="form-control" placeholder="name@example.com" />

			// Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
			List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();
			contextAttributes.Add(new TagHelperAttribute("type", InputTypeName));
			if (For != null)
			{
				contextAttributes.Add(new TagHelperAttribute("asp-for", For.Name));
			}

			//if (!string.IsNullOrEmpty(Value))
			//	contextAttributes.Add(new TagHelperAttribute("value", Value));

			// Output attributes are all non-helper attributes on the output tag
			List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);

			TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
			TagHelperOutput helperOutput = new TagHelperOutput("input", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

			return await GenerateInputHelper(helperContext, helperOutput, mode);
		}

		/// <summary>
		/// Generate a 'textarea' input field
		/// </summary>
		protected async Task<IHtmlContent> GenerateTextarea(TagHelperContext context, TagHelperOutput output)
		{
			// Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
			List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();

			// Output attributes are all non-helper attributes on the output tag
			List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);

			TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
			TagHelperOutput helperOutput = new TagHelperOutput("textarea", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

			TextareaTagHelper childHelper = new TextareaTagHelper(_htmlHelper, null, null, null, null, null);
			childHelper.Name = Name;
			childHelper.For = For;
			childHelper.Value = Value?.ToString();
			childHelper.Title = Title;
			childHelper.HelpText = HelpText;
			childHelper.Placeholder = Placeholder;
			childHelper.IsReadonly = IsReadonly;
			childHelper.IsDisabled = IsDisabled;
			childHelper.Width = Width;
			childHelper.Height = Height;
			//childHelper.Format = Format;
			//childHelper.InputTypeName = InputTypeName;
			childHelper.ViewContext = ViewContext;
			childHelper.Init(helperContext);

			await childHelper.ProcessAsync(helperContext, helperOutput);
			return helperOutput;
		}

		/// <summary>
		/// Generate a 'checkbox' input field. UNlike the other input generators, this method generates label and input all in one.
		/// </summary>
		protected async Task<IHtmlContent> GenerateCheck(TagHelperContext context, TagHelperOutput output)
		{
			// Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
			List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();
			contextAttributes.Add(new TagHelperAttribute("type", InputTypeName));           // Needed, setting childHelper.InputTypeName is not enough!
			if (For != null)
			{
				contextAttributes.Add(new TagHelperAttribute("asp-for", For.Name));
			}

			// Output attributes are all non-helper attributes on the output tag
			List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);

			TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
			TagHelperOutput helperOutput = new TagHelperOutput("input", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

			CheckboxTagHelper childHelper = new CheckboxTagHelper(_htmlHelper, null, null, null, null, null);
			childHelper.Name = Name;
			childHelper.For = For;
			childHelper.Title = Title;
			childHelper.HelpText = HelpText;
			childHelper.Placeholder = Placeholder;
			childHelper.IsReadonly = IsReadonly;
			childHelper.IsDisabled = IsDisabled;
            childHelper.LabelText = LabelText;
            childHelper.LabelClass = LabelClass;
            childHelper.ViewContext = ViewContext;

			if (Value is bool boolValue)
			{
				childHelper.Value = boolValue;
			}
			else if (Value is string stringValue)
			{
				bool.TryParse(stringValue, out bool boolValue2);
				childHelper.Value = boolValue2;
			}

			childHelper.Init(helperContext);

			await childHelper.ProcessAsync(helperContext, helperOutput);
			return helperOutput;
		}

		/// <summary>
		/// Generate a 'checkbox' input field. UNlike the other input generators, this method generates label and input all in one.
		/// </summary>
		protected async Task<IHtmlContent> GenerateDate(TagHelperContext context, TagHelperOutput output, DateBoxType type)
		{
			// Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
			List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();
			contextAttributes.Add(new TagHelperAttribute("type", InputTypeName));           // Needed, setting childHelper.InputTypeName is not enough!
			if (For != null)
			{
				contextAttributes.Add(new TagHelperAttribute("asp-for", For.Name));
			}

			// Output attributes are all non-helper attributes on the output tag
			List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);

			TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
			TagHelperOutput helperOutput = new TagHelperOutput("input", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

			DateboxTagHelper childHelper = new DateboxTagHelper(_htmlHelper);
			childHelper.Type = type;
			childHelper.Name = Name;
			childHelper.For = For;
			childHelper.Title = Title;
			childHelper.HelpText = HelpText;
			childHelper.Placeholder = Placeholder;
			childHelper.IsReadonly = IsReadonly;
			childHelper.IsDisabled = IsDisabled;
			//childHelper.Format = Format;
			//childHelper.InputTypeName = InputTypeName;
			childHelper.ViewContext = ViewContext;

			if (Value is DateTime dateValue)
			{
				childHelper.Value = dateValue;
			}
			else if (Value is string stringValue)
			{
				DateTime.TryParse(stringValue, out DateTime dateValue2);
				childHelper.Value = dateValue2;
			}

			childHelper.Init(helperContext);

			await childHelper.ProcessAsync(helperContext, helperOutput);
			return helperOutput;
		}

		/// <summary>
		/// Generate a 'radio' input field. Unlike the other input generators, this method generates label and input all in one.
		/// </summary>
		protected async Task<IHtmlContent> GenerateRadio(TagHelperContext context, TagHelperOutput output)
		{
			// Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
			List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();
			contextAttributes.Add(new TagHelperAttribute("type", InputTypeName));
			if (For != null)
			{
				contextAttributes.Add(new TagHelperAttribute("asp-for", For.Name));
			}

			//if (!string.IsNullOrEmpty(Value))
			//	contextAttributes.Add(new TagHelperAttribute("value", Value));

			// Output attributes are all non-helper attributes on the output tag
			List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);

			TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
			TagHelperOutput helperOutput = new TagHelperOutput("input", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

			return await GenerateInputHelper(helperContext, helperOutput, TextBoxMode.Tel);
		}

		private async Task<IHtmlContent> GenerateInputHelper(TagHelperContext helperContext, TagHelperOutput helperOutput, TextBoxMode mode)
		{
			TextboxTagHelper childHelper = new TextboxTagHelper(_htmlHelper, null, null, null, null, null);
			childHelper.Mode = mode;
			childHelper.Name = Name;
			childHelper.For = For;
            childHelper.Value = Value?.ToString();
            childHelper.Title = Title;
			childHelper.HelpText = HelpText;
			childHelper.Placeholder = Placeholder;
			childHelper.IsReadonly = IsReadonly;
			childHelper.IsDisabled = IsDisabled;
			//childHelper.Format = Format;
			//childHelper.InputTypeName = InputTypeName;
			childHelper.ViewContext = ViewContext;
			childHelper.Init(helperContext);

			await childHelper.ProcessAsync(helperContext, helperOutput);
			return helperOutput;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region StandardControlTagHelper helper methods
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Set class properties, depending on the chosen <see cref="Style"/>. 
		/// </summary>
		/// <returns> Returns an indicator if the label (if any) must be placed after the control. </returns>
		private void SetStyleProperties(TagHelperOutput output)
		{
			// Obtain the class attribute from output attributes
			string classValue = null;
			if (output.Attributes.TryGetAttribute("class", out TagHelperAttribute classAttr))
				_controlClass = classValue = classAttr.Value as string;

			// If there was none, use the built-in class
			//if (string.IsNullOrEmpty(_controlClass))
			//	_controlClass = SetControlClass(Style);

			// Supplement class extras
			if (!string.IsNullOrEmpty(ControlClassExtra))
				_controlClass = $"{_controlClass} {ControlClassExtra}";

			// If there was a change, update the class attribute in the output
			if (_controlClass != classValue)
			{
				output.Attributes.RemoveAll("class");
				output.Attributes.Add(new TagHelperAttribute("class", _controlClass));
			}
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region StandardControlTagHelper properties:
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region StandardControlTagHelper properties: label info
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the custom text for the label. Defaults to the equivalent of 'DisplayNameFor'.
		/// The label text on the control is only required for controls which supply their own label (such as a checkbox).
		/// </summary>
		[HtmlAttributeName("label-text")]
		public string LabelText { get; set; }

        /// <summary>
        /// Get or set the class to be applied to the label.
		/// The label class on the control is only required for controls which supply their own label (such as a checkbox).
        /// </summary>
        [HtmlAttributeName("label-class")]
        public string LabelClass { get; set; }
        
		#endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region StandardControlTagHelper properties: control info
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
		/// Get or set the control placeholder text.
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
		/// Get or set an indicator if the clear button should be shown. Defaults to true.
		/// </summary>
		[HtmlAttributeName("clear")]
		public bool AllowClear { get; set; } = true;

		/// <summary>
		/// Get or set an indicator if the control is read-only. Defaults to <see langword="false"/>.
		/// </summary>
		[HtmlAttributeName("is-readonly")]
		public bool IsReadonly { get; set; }

		/// <summary>
		/// Get or set an indicator if this control is disabled. Defaults to <see langword="false"/>.
		/// </summary>
		[HtmlAttributeName("disabled")]
		public bool IsDisabled { get; set; }

		[HtmlAttributeName("div-class")]
		public string ControlDivClass { get; set; } = "col-md-10";

		[HtmlAttributeName("columns")]
		/// <summary>
		/// Get or set the number of columns to be occupied by the control.
		/// Only used for <see cref="StandardFieldStyles"/> styles which allow column restrictions for the control. Defaults to 9.
		/// If extra space is required, the default or set value may be reduced to allow for the extra space.
		/// </summary>
		public int Columns { get; set; } = 9;

		//[HtmlAttributeName("class")]
		//public string ControlClass { get; set; }

		private string _controlClass;

		private const string _controlClassDefault = "form-control";
		private const string _controlClassReadonly = "form-control form-control-solid";
		private const string _controlClassCheck = "form-check-input";
		private const string _controlClassSelect = "form-select";

		/// <summary>
		/// Required for core?
		/// </summary>
		[HtmlAttributeName("class-extra")]
		public string ControlClassExtra { get; set; } = "";

		[HtmlAttributeName("asp-items")]
		public System.Collections.IEnumerable Items { get; set; }

		/// <summary> 
		/// The format string (see https://msdn.microsoft.com/en-us/library/txafckwd.aspx) used to format the <see cref="For"/> result.
		/// Sets the generated "value" attribute to that formatted string. 
		/// </summary> 
		/// <remarks> 
		/// Not used if the provided (see <see cref="InputTypeName"/>) or calculated "type" attribute value is 
		/// <c>checkbox</c>, <c>password</c>, or <c>radio</c>. That is, <see cref="Format"/> is used when calling 
		/// <see cref="IHtmlGenerator.GenerateTextBox"/>. 
		/// </remarks> 
		[HtmlAttributeName("asp-format")]
		public string Format { get; set; }

		/// <summary> 
		/// The value of the &lt;input&gt; element. 
		/// </summary> 
		/// <remarks> 
		/// Passed through to the generated HTML in all cases. Also used to determine the generated "checked" attribute 
		/// if <see cref="InputTypeName"/> is "radio". Must not be <c>null</c> in that case. 
		/// </remarks> 
		public object Value { get; set; }

		#endregion
	}
}
