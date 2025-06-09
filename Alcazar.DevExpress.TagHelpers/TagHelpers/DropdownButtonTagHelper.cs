using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DropdownButtonTagHelper"/> implements a dropdown button.
	/// A dropdown button is a DX control, which consists of a button and a dropdown menu with menu items.
	/// </summary>
	[HtmlTargetElement("dx-dropdown-button")]
	public class DropdownButtonTagHelper : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownButtonTagHelper construction
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

			// Process common functionality for editors
			builder = ProcessCommon(builder);

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

			builder = ItemClick(builder);

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

			// Process dropdown-button specific properties
			if (!string.IsNullOrEmpty(KeyExpression))
				builder = builder.KeyExpr(KeyExpression);
			if (!string.IsNullOrEmpty(DisplayExpression))
				builder = builder.DisplayExpr(DisplayExpression);

			// Event handlers
			builder = ItemClick(builder);

			// Render the builder
			IHtmlContent result = builder;
			output.Content.SetHtmlContent(result);
		}

		private DropDownButtonBuilder ProcessCommon(DropDownButtonBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		/// <summary>
		/// Set the action to be executed when a dropdown item is clicked.
		/// </summary>
		/// <param name="builder"></param>
		private DropDownButtonBuilder ItemClick(DropDownButtonBuilder builder)
		{
			switch (OnItemClick)
			{
				// An empty action indicates no action, do not set OnItemClick
				case null:
				case "":
					break;

				// Any other value, use the value as JS method name 
				default:
					builder.OnItemClick(OnItemClick);
					break;

				// rb: TODO
				case "rb":
					// builder.OnItemClick(RazorBlock(null));
					break;

				// href: the item includes a href, use it by calling a default method
				case "href":
					builder.OnItemClick("dx_dropdown_itemclick");
					break;
			}

			return builder;
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

		/// <summary>
		/// Get or set the name of the item property to be used as dropdown item key.
		/// </summary>
		[HtmlAttributeName("key-expr")]
		public string KeyExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as dropdown item display text.
		/// </summary>
		[HtmlAttributeName("display-expr")]
		public string DisplayExpression { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a dropdown item is clicked.
		/// </summary>
		[HtmlAttributeName("item-click")]
		public string OnItemClick { get; set; }

		#endregion
	}
}
