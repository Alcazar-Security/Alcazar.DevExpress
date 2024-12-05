using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="TemplateTagHelper"/> type implements a tag helper for DX controls which allow templates.
	/// Templates are custom HTML content of the control. They can contain embedded Ruby (erb) placeholders.
	/// </summary>
	[HtmlTargetElement("template", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class TemplateTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TemplateTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			ItemsChildrenContext itemContext = GetContextSafe<ItemsChildrenContext>(context);

			itemContext.TemplateContent = await output.GetChildContentAsync();
			output.SuppressOutput();
		}

		#endregion
	}

	/// <summary>
	/// The <see cref="ItemTemplateTagHelper"/> implements a tag helper for DX item controls which allow templates for their items.
	/// Templates are custom HTML content of the control. They can contain embedded Ruby (erb) placeholders.
	/// </summary>
	[HtmlTargetElement("item-template", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ItemTemplateTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemTemplateTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			ItemsChildrenContext itemContext = GetContextSafe<ItemsChildrenContext>(context);
			itemContext.ItemTemplateContent = await output.GetChildContentAsync();
			output.SuppressOutput();
		}

		#endregion
	}
}
