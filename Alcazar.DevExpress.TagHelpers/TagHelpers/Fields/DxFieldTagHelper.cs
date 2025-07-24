using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Xml.Linq;
using System;
using Alcazar.Common.Config;
using Alcazar.Property;
using System.Reflection;
using Amaqele.Common.Base;
using Amaqele.Common.Types;

namespace Alcazar.Web.Extensibility
{
    [HtmlTargetElement("dx-option")]
    public class DxOptionTagHelper : FieldTagHelperBase2
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxOptionTagHelper construction
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public DxOptionTagHelper(IHtmlHelper htmlHelper, IHtmlGenerator generator)
              : base(htmlHelper, generator)
        {
        }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxOptionTagHelper overrides
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Asynchronously executes the tag with the given <paramref name="context"/> and <paramref name="output"/>.
        /// </summary>
        /// <param name="context"> Contains information associated with the current HTML tag. </param>
        /// <param name="output"> A stateful HTML element used to generate an HTML tag. </param>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Create the context, so that we can pass it to child tag helpers
            DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
            ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);
            _controlContext = GetOrCreateContext<ControlContext>(context);

            // Before processing an inner control, pass in any properties from the dx-field to the editor. 
            // This is used for properties which are used by more than one of the label, control, and validation, so that the dx-field declares them once and the field parts share them (where an inner control is declared)
            _controlContext.For = For;
            _controlContext.Name = Name;

			// Process the option, which sets: LabelText, LabelClass, HelpText, ModelType, Value
			ProcessBefore();

			// Of this, the following are of interest to any embedded control
			_controlContext.ModelType = ModelType;
			_controlContext.Value = Value;
			_controlContext.HelpText = HelpText;
			_controlContext.Items = Items;

			// Process children of the standard-field tag
			// Any content would be attributed to the control or the label
			IHtmlContent content = await output.GetChildContentAsync();

            // For now, we only have one style to process
            await ProcessDefaultAsync(context, output);
        }

        /// <summary>
        /// Generate a form field using the default style.
        /// </summary>
        protected async Task ProcessDefaultAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            // Any attributes this standard-field tag might bear, are for its children, clear them, and add only the group-class
            output.Attributes.Clear();
            output.Attributes.Add("class", GroupClass);

            // 1. Generate the control, and add to the output
            IHtmlContent controlDivContent = await GenerateControlDiv(context, output);

 			// 2. Generate the label, unless we are a checkbox, and add to the output
			// After the control, it might pass us For/Name information
			IHtmlContent labelContent = null;
            if (this.InputTypeName != ControlTypes.Checkbox)
            {
                labelContent = await GenerateLabel(context);
                output.Content.SetHtmlContent(labelContent);
            }

            // 3. Add control content to the output
            // After the label, that is the sequence in HTML
            output.Content.AppendHtml(controlDivContent);

            // 3. Generate the validation element (POST)
            if (IsValidation)
            {
                // Add validation output, as POST content
                IHtmlContent validationContent = await GenerateValidation(context);
                output.PostContent.SetHtmlContent(validationContent);
            }
        }

        override protected void SetInputType()
        {
            // Call the base class implementation first
            base.SetInputType();

            // Nothing to do here if we already have a specialised control type
            if (InputTypeName != ControlTypes.Text)
                return;

            if (ModelType.IsEnum)
            {
                // Set the selected item, if any
                // It is possible that an enum property has no default value, and is not set, therefore the value could be null, test for that
                if (Value != null)
                {
                    // Just in case, if the value comes in as int and not as enum (as it nicely happens for Culture.NumberFormat.NumberNegativePattern)
                    //if (Enum.IsDefined(ModelType, Value))
                    //    Value = Enum.ToObject(ModelType, Value).ToString();
                }

				// An enum always leads to a select
				// If a multi-select is desired, the dx-option tag can use a dx-tag embedded control.
				InputTypeName = ControlTypes.Select;
            }
        }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxOptionTagHelper helper methods
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        private void ProcessBefore()
        {
            try
            {
                // var viewData = ViewContext.ViewData as ViewDataDictionary<ConfigOptions>;
                // var modelExpression = _modelExpressionProvider.CreateModelExpression<ConfigOptions, string>(viewData, null);

                // Get the node which we are displaying - can be null, if the property has not been saved before
                ConfigOptions configOptions = ViewContext.ViewData.Model as ConfigOptions;
                if (configOptions == null)
                {
                    // We dont have the right model
                }

                string name = Name;

                // Get the property and descriptor, either one can be null
                PropertyValue2021 propertyDescriptor = configOptions.GetPropertyDescriptor(name);
                PropertyValue2021 propertyValue = configOptions.GetPropertyValue(name);

                // Get metadata to build the HTML output
                object defaultValue = null;

                if (propertyDescriptor != null)
                {
                    if (propertyDescriptor.Metadata != null)
                    {
                        // Get the display name - the nested-path serves as fallback
                        if (!string.IsNullOrEmpty(propertyDescriptor.Metadata.DisplayName) && string.IsNullOrEmpty(LabelText))
                            LabelText = TrimWhitespace(propertyDescriptor.Metadata.DisplayName);

                        // Get the description - the nested-path serves as fallback
                        if (!string.IsNullOrEmpty(propertyDescriptor.Metadata.Description) && string.IsNullOrEmpty(HelpText))
                            HelpText = TrimWhitespace(propertyDescriptor.Metadata.Description);
                    }

                    // Get the property type
                    ModelType = propertyDescriptor.EffectiveType ?? propertyValue?.EffectiveType;

                    // Get the default value
                    defaultValue = propertyDescriptor.Value;
                    if (defaultValue == null && DefaultValue != null)
                        defaultValue = DefaultValue;
                }
                else
                {
                    // No descriptor, alert the user
                    LabelClass = LabelClass == null ? _propertyNotExists : $"{LabelClass} {_propertyNotExists}";
                }

                // There must always be a label text
                if (string.IsNullOrEmpty(LabelText))
                    LabelText = name;

                // Get the current value, or default value
                if (propertyValue != null)
                {
                    Value = propertyValue.GetValue(ModelType);

                    // We have a value, as opposed to the default value, indicate this
                    LabelClass = LabelClass == null ? _propertyWithValueClass : $"{LabelClass} {_propertyWithValueClass}";
                }

                else if (defaultValue != null)
                    Value = defaultValue;

                // Finalise the options
                // The model type is mandatory for option-fields, the input type determination depends on it
                if (ModelType == null)
                    ModelType = typeof(string);

                if (ModelType.IsEnum)
                {
                    // Create our own choices if we dont have them yet
                    if (Items == null)
                        Items = Enum.GetNames(ModelType); //ModelType.ToSelectListItems();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string TrimWhitespace(string text)
        {
            if (text == null)
                return null;

            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            bool wasWhitespace = false;

            List<char> chars = new List<char>();
            foreach (char c in text.Trim())
            {
                if (char.IsWhiteSpace(c))
                {
                    // For whitespace, add the first whitespace character
                    if (wasWhitespace == false)
                    {
                        wasWhitespace = true;
                        chars.Add(c);
                    }
                }
                else
                {
                    // For chars, add them
                    wasWhitespace = false;
                    chars.Add(c);
                }
            }

            return new string(chars.ToArray());
        }

        private const string _propertyWithValueClass = "text-primary";
        private const string _propertyNotExists = "text-danger fw-bold";

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxOptionTagHelper properties: tag helper
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        [HtmlAttributeName("default-value")]
        public object DefaultValue { get; set; }

        #endregion
    }

    public class FieldTagHelperBase2 : FieldTagHelperBase
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region FieldTagHelperBase construction
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public FieldTagHelperBase2(IHtmlHelper htmlHelper, IHtmlGenerator generator)
        {
            _htmlHelper = htmlHelper as HtmlHelper;
            _generator = generator;
        }

        private readonly HtmlHelper _htmlHelper;
        private readonly IHtmlGenerator _generator;

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region FieldTagHelperBase helper methods
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Generate a required, standard, or named label.
        /// </summary>
        protected async Task<IHtmlContent> GenerateLabel(TagHelperContext context)
        {
            // Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
            TagHelperAttribute[] contextAttributes = new TagHelperAttribute[0];

            // Output attributes are all non-helper attributes on the output tag
            List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>();
            if (!string.IsNullOrEmpty(LabelClass))
                outputAttributes.Add(new TagHelperAttribute("class", LabelClass));

            TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
            TagHelperOutput helperOutput = new TagHelperOutput("standard-label", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

            StandardLabelTagHelper childHelper = new StandardLabelTagHelper(_generator);
            childHelper.For = For;
            childHelper.Name = Name;
            childHelper.IsRequired = IsRequired;
            childHelper.IsNotRequired = IsNotRequired;
            childHelper.Columns = LabelColumns;
            childHelper.LabelDivClass = LabelDivClass;
            childHelper.LabelText = LabelText;
            childHelper.ViewContext = ViewContext;

            await childHelper.ProcessAsync(helperContext, helperOutput);

            // Generate the label-div as container for the label
            // <div class="dx-field-label">
            var labelDiv = new TagBuilder("div");
            labelDiv.AddCssClass(LabelDivClass);
            labelDiv.InnerHtml.AppendHtml(helperOutput);

            return labelDiv;
        }

        /// <summary>
        /// Generate the div containing the control, and the control itself.
        /// </summary>
        protected async Task<IHtmlContent> GenerateControlDiv(TagHelperContext context, TagHelperOutput output)
        {
            IHtmlContent controlContent;
            if (_controlContext != null && _controlContext.ControlContent != null)
            {
                // We have an inner dx-control, use it
                controlContent = _controlContext.ControlContent;

				// 2. Determine the input type
				// After the control, it might pass us For/Name information
				// If we have a control context (from an inner dx-control tag), see if we need its information
				if (For == null)
					For = _controlContext.For;
				if (Name == null)
					Name = _controlContext.Name;

				// These four are of interest for the DxOptionTagHelper, but we are trying them out here too
				if (ModelType == null)
					ModelType = _controlContext.ModelType;
				if (HelpText == null)
					HelpText = _controlContext.HelpText;
				if (Value == null)
					Value = _controlContext.Value;
				if (Items == null)
					Items = _controlContext.Items;
			}
			else
            {
				// The dx-field has all control information itself, generate the control 
				// First, let the model type determine the input type
				SetInputType();

				// This method is only called if there is no embedded control directly inside the 'dx-field'
				controlContent = await GenerateControl(context, output);
            }

            // Generate the control-div as container for the control
            var controlDiv = new TagBuilder("div");
            controlDiv.AddCssClass(ControlDivClass);

            // And add the control into its div
            controlDiv.InnerHtml.AppendHtml(controlContent);
            return controlDiv;
        }

        /// <summary>
        /// Generate the control.
        /// This method is only called if there is no embedded control directly inside the 'dx-field'
        /// </summary>
        private async Task<IHtmlContent> GenerateControl(TagHelperContext context, TagHelperOutput output)
        {
            // Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
            List<TagHelperAttribute> contextAttributes = new List<TagHelperAttribute>();

            // Output attributes are all non-helper attributes on the output tag. We want to pass on ALL these attributes to the control, it probably needs several of them.
            List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>(output.Attributes);
            if (!string.IsNullOrEmpty(ControlClass))
                outputAttributes.Add(new TagHelperAttribute("class", ControlClass));

            TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
            TagHelperOutput helperOutput = new TagHelperOutput("standard-control", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

            DxControlTagHelper childHelper = new DxControlTagHelper(_htmlHelper, _generator, null);

            childHelper.For = For;
            childHelper.Name = Name;
            childHelper.Value = Value ?? StringValue;
            childHelper.InputTypeName = InputTypeName;
            childHelper.ModelType = ModelType;
            childHelper.Placeholder = Placeholder;
            childHelper.Title = Title;
            childHelper.HelpText = HelpText;
            childHelper.AllowClear = AllowClear;
            childHelper.IsReadonly = IsReadonly;
            childHelper.IsDisabled = IsDisabled;
            childHelper.Columns = ControlColumns;
            childHelper.ControlClassExtra = ControlClassExtra;
            childHelper.ControlDivClass = ControlDivClass;
            childHelper.Width = Width;
            childHelper.Height = Height;
            childHelper.Items = Items;
            childHelper.LabelText = LabelText;
            childHelper.LabelClass = LabelClass;

            childHelper.ViewContext = ViewContext;

            await childHelper.ProcessAsync(helperContext, helperOutput);

            return helperOutput;
        }

        protected async Task<IHtmlContent> GenerateValidation(TagHelperContext context)
        {
            // <span asp-validation-for="Name" class="text-danger"></span>

            // Context attributes are all attributes on the source tag - we pass on all attributes which this part needs
            TagHelperAttribute[] contextAttributes = new TagHelperAttribute[0];

            // Output attributes are all non-helper attributes on the output tag
            List<TagHelperAttribute> outputAttributes = new List<TagHelperAttribute>();
            if (!string.IsNullOrEmpty(LabelClass))
                outputAttributes.Add(new TagHelperAttribute("class", LabelClass));

            TagHelperContext helperContext = new TagHelperContext(new TagHelperAttributeList(contextAttributes), context.Items, context.UniqueId);
            TagHelperOutput helperOutput = new TagHelperOutput("standard-validation", new TagHelperAttributeList(outputAttributes), GetChildContentAsync);

            StandardValidationTagHelper childHelper = new StandardValidationTagHelper(_generator);
            childHelper.For = For;
            childHelper.Name = Name;
            childHelper.ValidationClass = ValidationClass;
            childHelper.ViewContext = ViewContext;

            await childHelper.ProcessAsync(helperContext, helperOutput);

            return helperOutput;
        }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region FieldTagHelperBase properties: tag helper
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary> 
        /// Get or set the ID and name of the input element.
        /// </summary> 
        [HtmlAttributeName("name")]
        public string Name { get; set; }

        protected ControlContext _controlContext;

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxFieldTagHelper properties: tag helper
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the text message to be used for this control.
        /// </summary>
        [HtmlAttributeName("text")]
        public string Text { get; set; }

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
        /// Get or set an indicator if this control is disabled.
        /// </summary>
        [HtmlAttributeName("disabled")]
        public bool IsDisabled { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxFieldTagHelper properties: group info
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        [HtmlAttributeName("group-class")]
        public string GroupClass { get; set; } = "dx-field";

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxFieldTagHelper properties: label info
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the <c>a-required</c> attribute - for a model element that is NOT [Required] to show as required on the page.
        /// </summary>
        [HtmlAttributeName("required")]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Get or set the <c>a-not-required</c> attribute - for a model element that is [Required] to show as NOT required on the page.
        /// </summary>
        [HtmlAttributeName("not-required")]
        public bool IsNotRequired { get; set; }

        /// <summary>
        /// Get or set the class to be applied to the label.
        /// </summary>
        [HtmlAttributeName("label-class")]
        public string LabelClass { get; set; }

        /// <summary>
        /// Required for core?
        /// </summary>
        [HtmlAttributeName("label-div-class")]
        public string LabelDivClass { get; set; } = "dx-field-label";

        /// <summary>
        /// Get or set the number of columns to be occupied by the label.
        /// Only used for <see cref="StandardFieldStyles"/> styles which allow column restrictions for the label. Defaults to 3.
        /// </summary>
        [HtmlAttributeName("label-columns")]
        public int LabelColumns { get; set; } = 3;

        /// <summary>
        /// Get or set the custom text for the label. Defaults to the equivalent of 'DisplayNameFor'.
        /// </summary>
        [HtmlAttributeName("label-text")]
        public string LabelText { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxFieldTagHelper properties: control info
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set an placeholder to be shown in the control.
        /// </summary>
        [HtmlAttributeName("placeholder")]
        public string Placeholder { get; set; }

        /// <summary>
        /// Get or set an indicator if the control is read-only. Defaults to <see langword="false"/>.
        /// </summary>
        [HtmlAttributeName("readonly")]
        public bool IsReadonly { get; set; }


        [HtmlAttributeName("control-div-class")]
        public string ControlDivClass { get; set; } = "dx-field-value";

        /// <summary>
        /// Get or set the number of columns to be occupied by the control.
        /// Only used for <see cref="StandardFieldStyles"/> styles which allow column restrictions for the control. Defaults to 9.
        /// If extra space is required, the default or set value may be reduced to allow for the extra space.
        /// </summary>
        [HtmlAttributeName("control-columns")]
        public int ControlColumns { get; set; } = 9;

        [HtmlAttributeName("control-class")]
        public string ControlClass { get; set; }

        /// <summary>
        /// Required for core?
        /// </summary>
        [HtmlAttributeName("control-class-extra")]
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
        /// Get or set the value to be displayed in this control.
        /// </summary
        /// <remarks>
        /// <para>
        /// The string value is provided for convenience where a string gets set in cshtml.
        /// </para>
        /// <para>
        /// Passed through to the generated HTML in all cases. Also used to determine the generated "checked" attribute 
        /// if <see cref="InputTypeName"/> is "radio". Must not be <c>null</c> in that case. 
        /// </remarks>
        /// </para>
        [HtmlAttributeName("value")]
        public string StringValue { get; set; }

        /// <summary>
        /// Get or set the value to be displayed in this control.
        /// </summary>
        /// <remarks> 
        /// <para>
        /// The string value is provided by neccessity where a non-string value is passed to a DX control.
        /// </para>
        /// Passed through to the generated HTML in all cases. Also used to determine the generated "checked" attribute 
        /// if <see cref="InputTypeName"/> is "radio". Must not be <c>null</c> in that case. 
        /// </remarks> 
        [HtmlAttributeName("val")]
        public object Value { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxFieldTagHelper properties: validation info
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set an indicator if the control validation tag is to be created. Defaults to <see langword="true"/>.
        /// </summary>
        [HtmlAttributeName("valid")]
        public bool IsValidation { get; set; } = true;

        [HtmlAttributeName("validation-class")]
        public string ValidationClass { get; set; } = "text-danger";

        #endregion
    }

    /// <summary>
    /// The <see cref="DxFieldTagHelper"/> type implements a standard field using DX.
    /// </summary>
    [HtmlTargetElement("dx-field")]
    public class DxFieldTagHelper : FieldTagHelperBase2
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxFieldTagHelper construction
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public DxFieldTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator, IModelMetadataProvider modelMetadataProvider)
            : base(htmlHelper, generator)
        {
        }

        //private readonly IViewComponentHelper _viewComponentHelper;
        //private readonly IUrlHelper _urlHelper;
        //private readonly HtmlEncoder _htmlEncoder;
        // private IModelMetadataProvider _modelMetadataProvider;

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region DxFieldTagHelper overrides
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Asynchronously executes the tag with the given <paramref name="context"/> and <paramref name="output"/>.
        /// </summary>
        /// <param name="context"> Contains information associated with the current HTML tag. </param>
        /// <param name="output"> A stateful HTML element used to generate an HTML tag. </param>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Create the context, so that we can pass it to child tag helpers
            DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
            ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);
            _controlContext = GetOrCreateContext<ControlContext>(context);

            // Before processing an inner control, pass in any properties from the dx-field to the editor. 
            // This is used for properties which are used by more than one of the label, control, and validation, so that the dx-field declares them once and the field parts share them (where an inner control is declared)
            _controlContext.For = For;
            _controlContext.Name = Name;

            // Process children of the standard-field tag
            // Any content would be attributed to the control or the label
            IHtmlContent content = await output.GetChildContentAsync();

            // For now, we only have one style to process
            await ProcessDefaultAsync(context, output);
        }

        /// <summary>
        /// Generate a form field using the default style.
        /// </summary>
        protected async Task ProcessDefaultAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            // Any attributes this standard-field tag might bear, are for its children, clear them, and add only the group-class
            output.Attributes.Clear();
            output.Attributes.Add("class", GroupClass);

            // 1. Generate the control, and add to the output
            IHtmlContent controlDivContent = await GenerateControlDiv(context, output);

            // 2. Generate the label, unless we are a checkbox, and add to the output
            // After the control, it might pass us For/Name information
            IHtmlContent labelContent = null;
            if (this.InputTypeName != ControlTypes.Checkbox)
            {
                labelContent = await GenerateLabel(context);
                output.Content.SetHtmlContent(labelContent);
            }

            // 3. Add control content to the output
            // After the label, that is the sequence in HTML
            output.Content.AppendHtml(controlDivContent);

            // 3. Generate the validation element (POST)
            if (IsValidation)
            {
                // Add validation output, as POST content
                IHtmlContent validationContent = await GenerateValidation(context);
                output.PostContent.SetHtmlContent(validationContent);
            }
        }

        #endregion
    }

    /// <summary>
    /// TextEditorButtonContext, DataGridColumnButtonContext
    /// </summary>
    public class ControlContext
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ControlContext properties
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary> 
        /// Get or set the ID and name of the input element. <see cref="Name"/> and <see cref="For"/> are mutually exclusive.
        /// </summary> 
        public string Name { get; set; }

        /// <summary>
        /// Get or set the Model property for which this input element is for. <see cref="Name"/> and <see cref="For"/> are mutually exclusive.
        /// </summary>
        public ModelExpression For { get; set; }

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		public object Value { get; set; }
		
        /// <summary>
		/// Get or set the model type.
		/// </summary>
		public Type ModelType { get; set; }

		/// <summary>
		/// Get or set the help text to be used for this control. The help text is displayed over a [?] button, while the title is displayed over the control itself.
		/// </summary>
		public string HelpText { get; set; }

		/// <summary>
		/// Get or set the items of this dropdown control.
		/// Items are supplied server side, and are an alternative to a data source.
		/// </summary>
		public System.Collections.IEnumerable Items { get; set; }
		
        /// <summary>
		/// Get or set the generated content of the input control.
		/// </summary>
		public IHtmlContent ControlContent { get; set; }

        #endregion
    }

}
