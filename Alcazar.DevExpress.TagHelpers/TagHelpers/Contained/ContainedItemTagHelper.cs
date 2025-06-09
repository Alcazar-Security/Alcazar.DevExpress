using Alcazar.Web.Extensibility;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="ContainedItemTagHelper"/> tag helper implements an item content with a value, text, and other properties.
	/// It can be universally used, but is currently only used for radio buttons within a <see cref="RadioGroupTagHelper"/>.
	/// TODO align with ItemContentTagHelper.
	/// </summary>
	[HtmlTargetElement("radio", ParentTag = "dx-radiogroup", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ContainedItemTagHelper : TagHelperBase
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
            string title = TranslateToProp(Title, ViewContext);

            IHtmlContent content = await output.GetChildContentAsync();

            ItemModel item = new ItemModel
            {
                // Items
                Name = name,
                Value = Value,
                Text = text,
                Icon = icon,
                Title = title,

                // Actions
                OnClickAction = OnClickAction,

                // Item template (not implemented)
                Content = null,
            };

            itemContext.Items.Add(item);
        }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ContainedItemTagHelper properties
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Get or set the name of the button.
        /// </summary>
        [HtmlAttributeName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Get or set the icon of the button.
        /// </summary>
        [HtmlAttributeName("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Get or set the text of the button.
        /// </summary>
        [HtmlAttributeName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Get or set the value of the button.
        /// </summary>
        [HtmlAttributeName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Get or set the action to be executed when the button is clicked.
        /// </summary>
        [HtmlAttributeName("onclick")]
        public string OnClickAction { get; set; }

        /// <summary>
        /// Get or set the title or hint to be displayed for the button.
        /// </summary>
        [HtmlAttributeName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Get or set the content or template of this button.
        /// </summary>
        public IHtmlContent Content { get; set; }

        #endregion
    }

    /// <summary>
    /// </summary>
    public class ItemContext
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ItemContext properties
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public IList<ItemModel> Items { get; } = new List<ItemModel>();

        #endregion
    }

    public class ItemModel
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ItemModel properties
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

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
        /// Get or set the action to be executed when the button is clicked.
        /// </summary>
        public string OnClickAction { get; set; }

        /// <summary>
        /// Get or set the title or hint to be displayed for the button.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Get or set the content or template of this button.
        /// </summary>
        public IHtmlContent Content { get; set; }

        #endregion
    }
}
