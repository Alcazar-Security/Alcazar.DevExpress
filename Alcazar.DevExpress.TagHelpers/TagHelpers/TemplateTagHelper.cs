using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
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
			ItemContext itemContext = GetContextSafe<ItemContext>(context);

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
            ItemContext itemContext = GetContextSafe<ItemContext>(context);
			itemContext.ItemTemplateContent = await output.GetChildContentAsync();
			output.SuppressOutput();
		}

		#endregion
	}

	/// <summary>
	/// The <see cref="PopupTemplateTagHelper"/> implements a tag helper for DX item controls which allow templates for a popup.
	/// Templates are custom HTML content of the control. They can contain embedded Ruby (erb) placeholders.
	/// </summary>
	[HtmlTargetElement("popup-template", ParentTag = "dx-scheduler", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class PopupTemplateTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTemplateTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			ItemContext itemContext = GetContextSafe<ItemContext>(context);
			itemContext.PopupTemplateContent = await output.GetChildContentAsync();
			output.SuppressOutput();
		}

		#endregion
	}

	/// <summary>
	/// The <see cref="TemplateTagHelper"/> type implements a tag helper for DX controls which allow templates.
	/// Templates are custom HTML content of the control. They can contain embedded Ruby (erb) placeholders.
	/// </summary>
	[HtmlTargetElement("dx-namedtemplate", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class NamedTemplateTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region NamedTemplateTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public NamedTemplateTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region NamedTemplateTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			_htmlHelper.Contextualize(ViewContext);

			using (IDisposable disposable = _htmlHelper.DevExtreme().NamedTemplate(Name))
			{
				// Cant do, internal!
				// NamedTemplate namedTemplate = disposable as NamedTemplate;

				var templateContent = await output.GetChildContentAsync();

				// Cant do, no way to set the content
				// namedTemplate.Content = templateContent;
			}

			output.SuppressOutput();
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region NamedTemplateTagHelper properties: data source
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the view context.
		/// </summary>
		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }


		/// <summary>
		/// Get or set the name of this named template.
		/// </summary>
		[HtmlAttributeName("name")]
		public string Name { get; set; }

		#endregion
	}
}
