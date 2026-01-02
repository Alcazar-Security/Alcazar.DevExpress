using Markdig.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DxLabelControlHelper"/> tag helper implements a stand-alone label for a dx-control.
	/// This tag helper wraps a standard label () and displays it as if it was part of a greater dx-field.
	/// THis is useful when fine-grained control is required over a label.
	/// </summary>
	[HtmlTargetElement("dx-label")]
	public class DxLabelTagHelper : FieldTagHelperBase2
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DxFieldTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DxLabelTagHelper(IHtmlHelper htmlHelper, IHtmlGenerator generator)
			: base(htmlHelper, generator)
		{
		}

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
			_controlContext = GetOrCreateContext<ControlContext>(context);

			// Before processing an inner control, pass in any properties from the dx-field to the editor. 
			// This is used for properties which are used by more than one of the label, control, and validation, so that the dx-field declares them once and the field parts share them (where an inner control is declared)
			_controlContext.For = For;
			_controlContext.Name = Name;

			output.TagName = "div";
			output.TagMode = TagMode.StartTagAndEndTag;

			// Any attributes this standard-field tag might bear, are for its children, clear them, and add only the group-class
			output.Attributes.Clear();
			output.Attributes.Add("class", GroupClass);

			// Generate the label, unless we are a checkbox, and add to the output
			// After the control, it might pass us For/Name information
			IHtmlContent labelContent = null;
			if (InputTypeName != ControlTypes.Checkbox)
			{
				labelContent = await GenerateLabel(context, "dx-field-label-full");
				output.Content.SetHtmlContent(labelContent);
			}
		}

		#endregion
	}
}
