using Alcazar.Web.Extensibility;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
    public class ContainedItemTagHelperBase : TagHelperBase
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
    /// The <see cref="ContainedItemTagHelper"/> tag helper implements an item content with a value, text, and other properties.
    /// It is currently used for:
    /// * Radio buttons within a <see cref="RadioGroupTagHelper"/>.
    /// * Menu items within a <see cref="DropdownButtonTagHelper"/>.
    /// * Button items within a <see cref="PopupTagHelper"/>.
    /// </summary>
    [HtmlTargetElement("item", ParentTag = "dx-radiogroup", TagStructure = TagStructure.NormalOrSelfClosing)]
    [HtmlTargetElement("item", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
    [HtmlTargetElement("item", ParentTag = "dx-popup", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ContainedItemTagHelper : ContainedItemTagHelperBase
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ContainedItemTagHelper overrides
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Suppress the HTML of this tag, it is used for DX button generation only
            output.SuppressOutput();

            // Process the item tags and remember the content, so that the parent can process it
            ItemContext itemContext = GetContextSafe<ItemContext>(context);

            string name = TranslateToProp(Name, ViewContext);
            string icon = TranslateToProp(Icon, ViewContext);
            string text = TranslateToProp(Text, ViewContext);
            string value = TranslateToProp(Value, ViewContext);
            string title = TranslateToProp(Title, ViewContext);

            IHtmlContent content = await output.GetChildContentAsync();

            ItemModel item = new ItemModel
            {
                ID = ID,

                // Items
                Name = name,
                Value = value,
                Text = text,
                Icon = icon,
                Title = title,
                Badge = Badge,
                Href = Href,

                // Actions
                OnClick = OnClick,

                // Item template
                Content = content,
            };

            itemContext.Items.Add(item);
        }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ContainedItemTagHelper properties
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the name for this item.
        /// </summary>
        [HtmlAttributeName("name")]
        public string Name { get; set; }

        /// <summary>
		/// Get or set the display text of this item. Often used to set a text for the item by DisplayExpr.
        /// </summary>
        [HtmlAttributeName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Get or set the key value of this item. Often used to set a value for the item by ValueExpr or KeyExpr.
        /// </summary>
        [HtmlAttributeName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Get or set an icon for this item. This is a DX convention property, and sets an icon for the item.
        /// </summary>
        [HtmlAttributeName("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Get or set a badge for this item. This is a DX convention property, and sets a badge for the item.
        /// </summary>
        [HtmlAttributeName("badge")]
        public string Badge { get; set; }

        /// <summary>
        /// Get or set a HREF for this item. This sets the href for a page redirect, where the item is used like an <![CDATA[ <a> ]]> HTML anchor.
        /// </summary>
        [HtmlAttributeName("href")]
        public string Href { get; set; }

        /// <summary>
        /// Get or set the title or hint to be displayed ffor this item.
        /// </summary>
        [HtmlAttributeName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Get or set the content or template for this item.
        /// </summary>
        public IHtmlContent Content { get; set; }

        /// <summary>
        /// Get or set the content or template for this item.
        /// </summary>
        public string Template { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ContainedItemTagHelper properties: events
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the action to be executed when the button is clicked.
        /// </summary>
        [HtmlAttributeName("click")]
        public string OnClick { get; set; }

        #endregion
    }

    [HtmlTargetElement("item-separator", ParentTag = "dx-dropdown-button", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ContainedItemSeparatorTagHelper : ContainedItemTagHelperBase
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ItemSeparatorTagHelper overrides
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Process the card-actions tag and remember the content, so that the parent can inject it into the header
            ItemContext itemsContext = GetContextSafe<ItemContext>(context);

            ItemModel item = new ItemModel
            {
                ID = ID,
                Template = "<hr style='margin: unset' />",
            };

            itemsContext.Items.Add(item);

            output.SuppressOutput();
        }

        #endregion
    }

    /// <summary>
    /// </summary>
    public class ItemContext
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ItemContext properties
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public IHtmlContent TemplateContent { get; set; }
        public IHtmlContent ItemTemplateContent { get; set; }

        public IList<ItemModel> Items { get; } = new List<ItemModel>();

        #endregion
    }

    public class ItemModel
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ItemModel properties
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the ID of this item. Not used anywhere currently.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Get or set the name of the button.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or set the icon of the button.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Get or set the text of the button.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Get or set the value of the button.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Get or set the title or hint to be displayed for the button.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Get or set a badge for this item. This is a DX convention property, and sets a badge for the item.
        /// </summary>
        [HtmlAttributeName("badge")]
		[JsonPropertyName("badge")]
		public string Badge { get; set; }

        /// <summary>
        /// Get or set a HREF for this item. This property sets the href for a page redirect, where the item is used like an <![CDATA[ <a> ]]> HTML anchor.
        /// </summary>
        [HtmlAttributeName("href")]
        [JsonPropertyName("href")]
        public string Href { get; set; }

        /// <summary>
        /// Get or set the content or template of this button.
        /// </summary>
        public IHtmlContent Content { get; set; }

        /// <summary>
        /// Get or set the content or template for this item.
        /// </summary>
        public string Template { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemModel properties: popup toolbar items
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the toolbar to place this item in.
		/// </summary>
		[HtmlAttributeName("toolbar")]
		public Toolbar Toolbar { get; set; }

		/// <summary>
		/// Get or set the location of the item in the popup.
		/// </summary>
		[HtmlAttributeName("location")]
		public ToolbarItemLocation Location { get; set; }
		/// <summary>
		/// Get or set the mode in which this item may be placed into the popup menu.Defaults to <see cref="ToolbarItemLocateInMenuMode.Auto"/>.
		/// </summary>
		[HtmlAttributeName("menu")]
        public ToolbarItemLocateInMenuMode MenuMode { get; set; } = ToolbarItemLocateInMenuMode.Auto;

		/// <summary>
		/// Get or set the type of this (button) item. Defaults to <see cref="ButtonType.Normal"/>
		/// </summary>
		[HtmlAttributeName("button-type")]
        public ButtonType ButtonType { get; set; } = ButtonType.Normal;

		/// <summary>
		/// Get or set the styling mode of this (button) item. Defaults to <see cref="ButtonStylingMode.Contained"/>.
		/// </summary>
		[HtmlAttributeName("styling")]
		public ButtonStylingMode ButtonStylingMode { get; set; } = ButtonStylingMode.Contained;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ItemModel properties: events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the JS function to be executed when the button is clicked.
		/// </summary>
		[HtmlAttributeName("click")]
		public string OnClick { get; set; }

        #endregion
    }
}
