using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	[HtmlTargetElement("dx-dropdown-button")]
	public class DropdownButtonTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownButtonTagHelper costruction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DropdownButtonTagHelper(IHtmlHelper htmlHelper, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
			_htmlEncoder = htmlEncoder;
			_viewComponentHelper = viewComponentHelper;
		}

		private readonly IViewComponentHelper _viewComponentHelper;
		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;
		private readonly HtmlEncoder _htmlEncoder;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownButtonTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// To avoid: InvalidOperationException: Must call 'Contextualize' method before using this HtmlHelper instance.
			_htmlHelper.Contextualize(ViewContext);

			// This tag helper wants to remove the impact of the Props tag helper (a-text)
			output.PreContent.Clear();
			output.PostContent.Clear();

			// <div class="alert alert-primary d-flex align-items-center p-5">
			output.TagName = "div";
			output.TagMode = TagMode.StartTagAndEndTag;

			// Create the builder for a drop down button
			DropDownButtonBuilder builder = _htmlHelper.DevExtreme().DropDownButton();

			// Make it a split button (where the button itself can be clicked too)
			if (IsSplit)
				builder = builder.SplitButton(true);

			// Apply the text, if any
			string text = TranslateToProp(Text, ViewContext);
			if (!string.IsNullOrEmpty(text))
				builder = builder.Text(text);

			// Apply the icon, if any
			string icon = TranslateToProp(Icon, ViewContext);
			if (!string.IsNullOrEmpty(icon))
				builder = builder.Icon(icon);

			// Create the context, so that we can pass it to child tag helpers
			ItemsChildrenContext itemsContext = GetOrCreateContext<ItemsChildrenContext>(context);

			// Process children of the card tag, the header, footer, and my body will need them 
			IHtmlContent content = await output.GetChildContentAsync();

			// Set the item template
			if (itemsContext.ItemTemplateContent != null)
				builder = builder.ItemTemplate(ToString(itemsContext.ItemTemplateContent));

			// Set the template
			if (itemsContext.TemplateContent != null)
				builder = builder.Template(ToString(itemsContext.TemplateContent));

			// Apply items, if any
			if (itemsContext.Items != null)
				builder = builder.DataSource(itemsContext.Items);
			else if (Items != null)
				builder = builder.DataSource(Items);

			// Set layout options
			// builder = builder.DropDownOptions(options => options.Width(230))
			builder = builder.Width(200);

			// Render the builder
			IHtmlContent result = builder;
			output.Content.SetHtmlContent(result);
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownButtonTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		/// <summary>
		/// Get or set an indicator if the  text of the button.
		/// </summary>
		[HtmlAttributeName("split")]
		public bool IsSplit { get; set; }

		/// <summary>
		/// Get or set the text of the button.
		/// </summary>
		[HtmlAttributeName("text")]
		public string Text { get; set; }

		/// <summary>
		/// Get or set the icon of the button.
		/// </summary>
		[HtmlAttributeName("icon")]
		public string Icon { get; set; }

		/// <summary>
		/// Get or set the data source items of the button.
		/// </summary>
		[HtmlAttributeName("items")]
		public System.Collections.IEnumerable Items { get; set; }

		#endregion
	}


	[HtmlTargetElement("item-content", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ItemContentTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemContentTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			ItemsChildrenContext itemsContext = GetContextSafe<ItemsChildrenContext>(context);

			string hrefValue = GetAttrValue(output.Attributes["href"]);
			string textValue = TranslateToProp(output.Attributes["text"], ViewContext);
			string beforeValue = TranslateToProp(output.Attributes["before"], ViewContext);

			dynamic item = new
			{
				href = hrefValue,
				text = textValue,
				before = beforeValue,
			};

			itemsContext.Items.Add(item);

			output.SuppressOutput();
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemContentTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		#endregion
	}


	[HtmlTargetElement("item-separator", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ItemSeparatorTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemSeparatorTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			ItemsChildrenContext itemsContext = GetContextSafe<ItemsChildrenContext>(context);

			dynamic item = new
			{
				sep = true,
				template = "<hr style='margin: unset' />",
				disabled = true,
			};

			itemsContext.Items.Add(item);

			output.SuppressOutput();
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemSeparatorTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		#endregion
	}

	public class ItemsChildrenContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemsChildrenContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IHtmlContent TemplateContent { get; set; }
		public IHtmlContent ItemTemplateContent { get; set; }
		public IList<dynamic> Items { get; } = new List<dynamic>();

		#endregion
	}

	public class ItemWrapper
	{
		public string ID { get; set; }
		public object Value { get; set; }
		public string href { get; set; }
		public string icon { get; set; }
		public string text_before { get; set; }
		public string text { get; set; }
	}
}
