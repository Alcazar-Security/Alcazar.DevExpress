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

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DxFieldTagHelper"/> type implements a standard field using DX.
	/// </summary>
	[HtmlTargetElement("dx-field")]
	public class DxFieldTagHelper : FieldTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DxFieldTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DxFieldTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator, IModelMetadataProvider modelMetadataProvider)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
			//_urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
			//_htmlEncoder = htmlEncoder;
			//_viewComponentHelper = viewComponentHelper;
			_generator = generator;
			// _modelMetadataProvider = modelMetadataProvider;
		}

		//private readonly IViewComponentHelper _viewComponentHelper;
		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;
		//private readonly IUrlHelper _urlHelper;
		//private readonly HtmlEncoder _htmlEncoder;
		private readonly IHtmlGenerator _generator;
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
			// Determine the input type (done later)
			// SetInputType();

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

		private ControlContext _controlContext;

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

			// 2. Determine the input type
			// After the control, it might pass us For/Name information
			// If we have a control context (from an inner dx-control tag), see if we need its information
			if (For == null)
				For = _controlContext.For;
			if (Name == null)
				Name = _controlContext.Name;

			SetInputType();

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

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DxFieldTagHelper helper methods
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

			DxControlTagHelper childHelper = new DxControlTagHelper(_htmlHelper, null, null);

			childHelper.For = For;
			childHelper.Name = Name;
			childHelper.Value = Value;
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
			// Select box (and other) specific - no longer, we must use an embedded control within a dx-field (or dx-control) if any of these are used
			// childHelper.ValueExpression = ValueExpression;
			// childHelper.DisplayExpression = DisplayExpression;
			// childHelper.MinSearchLength = MinSearchLength;
			// childHelper.SearchTimeout = SearchTimeout;
			// if (_searchMode.HasValue)
			//	childHelper.SearchMode = _searchMode.Value;

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
		#region DxFieldTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		/// <summary> 
		/// Get or set the ID and name of the input element. <see cref="Name"/> and <see cref="For"/> are mutually exclusive.
		/// </summary> 
		/// <remarks> 
		/// Passed through to the generated HTML in all cases. Also used to determine whether <see cref="For"/> is valid with an empty <see cref="ModelExpression.Name"/>. 
		/// </remarks> 
		[HtmlAttributeName("name")]
		public string Name { get; set; }

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
		/// The value displayed in the control. 
		/// </summary> 
		/// <remarks> 
		/// Passed through to the generated HTML in all cases. Also used to determine the generated "checked" attribute 
		/// if <see cref="InputTypeName"/> is "radio". Must not be <c>null</c> in that case. 
		/// </remarks> 
		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public string Value { get; set; }

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
		/// Get or set the generated content of the input control.
		/// </summary>
		public IHtmlContent ControlContent { get; set; }

		#endregion
	}

}
