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
	public class ItemContentTagHelperBase : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemContentTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		/// <summary>
		/// Get or set the ID of this item. Not used anywhere currently.
		/// </summary>
		[HtmlAttributeName("id")]
		public string ID { get; set; }

		#endregion
	}

	/// <summary>
	/// The <see cref="ItemContentTagHelper"/> tag helper implements an item content with a value, text, and other properties.
	/// It can be universally used, but is currently only used for radio buttons within a <see cref="DropdownButtonTagHelper"/>.
	/// TODO align with ContainedItemTagHelper.
	/// </summary>
	[HtmlTargetElement("item-content", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ItemContentTagHelper : ItemContentTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemContentTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			ItemsChildrenContext itemsContext = GetContextSafe<ItemsChildrenContext>(context);

			MenuItemModel item = new MenuItemModel
			{
				ID = ID,
				Value = TranslateToProp(Value, ViewContext),
				Text = TranslateToProp(Text, ViewContext),
				icon = TranslateToProp(icon, ViewContext),
				badge = badge,
				href = href,
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


	[HtmlTargetElement("item-separator", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ItemSeparatorTagHelper : ItemContentTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemSeparatorTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			ItemsChildrenContext itemsContext = GetContextSafe<ItemsChildrenContext>(context);

			MenuSeparatorModel item = new MenuSeparatorModel
			{
				ID = ID,
				template = "<hr style='margin: unset' />",
			};

			itemsContext.Items.Add(item);

			output.SuppressOutput();
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemSeparatorTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public bool sep { get; set; } = true;
		public bool disabled { get; set; } = true;
		public string template { get; set; }

		#endregion
	}

	public class ItemsChildrenContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemsChildrenContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IHtmlContent TemplateContent { get; set; }
		public IHtmlContent ItemTemplateContent { get; set; }
		public IList<ItemModelBase> Items { get; } = new List<ItemModelBase>();

		#endregion
	}

	public class ItemModelBase
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

	public class MenuItemModel : ItemModelBase
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

	public class MenuSeparatorModel : ItemModelBase
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
