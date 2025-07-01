using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="ItemContentTagHelper"/> tag helper implements an item content with a value, text, and other properties.
	/// It can be universally used, but is currently only used for menu items within a <see cref="DropdownButtonTagHelper"/>.
	/// OBsolete, aligned with ContainedItemTagHelper.
	/// </summary>
	[HtmlTargetElement("item-content_", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ItemContentTagHelper_ : ContainedItemTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemContentTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			ItemContext itemsContext = GetContextSafe<ItemContext>(context);

			ItemModel item = new ItemModel
            {
				ID = ID,
				Value = TranslateToProp(Value, ViewContext),
				Text = TranslateToProp(Text, ViewContext),
				Icon = TranslateToProp(icon, ViewContext),
				Badge = badge,
				Href = href,
			};

			itemsContext.Items.Add(item);

			output.SuppressOutput();
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemContentTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the key value of this item. Typically used to set a value for the item by ValueExpr or KeyExpr.
		/// </summary>
		[HtmlAttributeName("value")]
		public string Value { get; set; }

		/// <summary>
		/// Get or set the display text of this item. Typically used to set a text for the item by DisplayeExpr.
		/// </summary>
		[HtmlAttributeName("text")]
		public string Text { get; set; }

		/// <summary>
		/// Get or set an icon for this item. This is a DX convention property, and sets an icon for the item.
		/// </summary>
		[HtmlAttributeName("icon")]
		public string icon { get; set; }

		/// <summary>
		/// Get or set a badge for this item. This is a DX convention property, and sets a badge for the item.
		/// </summary>
		[HtmlAttributeName("badge")]
		public string badge { get; set; }

		/// <summary>
		/// Get or set a HREF for this item. This sets the href for a page redirect, where the item is used like an <![CDATA[ <a> ]]> HTML anchor.
		/// </summary>
		[HtmlAttributeName("href")]
		public string href { get; set; }

		#endregion
	}


	public class ItemsChildrenContext_
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemsChildrenContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IHtmlContent TemplateContent { get; set; }
		public IHtmlContent ItemTemplateContent { get; set; }
		public IList<ItemModelBase_> Items { get; } = new List<ItemModelBase_>();

		#endregion
	}

	public class ItemModelBase_
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemModelBase properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the ID of this item. Not used anywhere currently.
		/// </summary>
		public string ID { get; set; }

		#endregion
	}

	public class MenuItemModel_ : ItemModelBase_
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region MenuItemModel properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the key value of this item. Typically used to set a value for the item by ValueExpr or KeyExpr.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Get or set the display text of this item. Typically used to set a text for the item by DisplayeExpr.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Get or set an icon for this item. This is a DX convention property, and sets an icon for the item.
		/// </summary>
		public string icon { get; set; }

		/// <summary>
		/// Get or set a badge for this item. This is a DX convention property, and sets a badge for the item.
		/// </summary>
		public string badge { get; set; }

		/// <summary>
		/// Get or set a HREF for this item. This sets the href for a page redirect, where the item is used like an <![CDATA[ <a> ]]> HTML anchor.
		/// </summary>
		public string href { get; set; }

		#endregion
	}

	public class MenuSeparatorModel_ : ItemModelBase_
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region MenuSeparatorModel properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public bool sep { get; set; } = true;
		public bool disabled { get; set; } = true;
		public string template { get; set; }

		#endregion
	}
}
