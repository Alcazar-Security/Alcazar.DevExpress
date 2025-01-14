using Alcazar.Web.Extensibility;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alcazar.DevExpress.TagHelpers.TagHelpers.Contained
{
    [HtmlTargetElement("button", ParentTag = "dx-autocomplete", TagStructure = TagStructure.NormalOrSelfClosing)]
    [HtmlTargetElement("button", ParentTag = "column", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ContainedButtonTagHelper : TagHelperBase
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ContainedButtonTagHelper overrides
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Suppress the HTML of this tag, it is used for DX button generation only
            output.SuppressOutput();

            // Process the buttons tag and remember the content, so that the parent can process it
            ButtonContext buttonContext = GetContextSafe<ButtonContext>(context);

            string name = TranslateToProp(Name, ViewContext);
            string icon = TranslateToProp(Icon, ViewContext);
            string text = TranslateToProp(Text, ViewContext);

            IHtmlContent content = await output.GetChildContentAsync();

            ButtonModel button = new ButtonModel
            {
                // Buttons
                Name = name,
                Icon = icon,
                Text = text,
                OnClickAction = OnClickAction,

                // Text editor buttons
                Location = Location,
                Styling = Styling,

                // Data grid buttons
                Title = Title,
                Content = content,
            };

            buttonContext.Buttons.Add(button);
        }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ContainedButtonTagHelper properties
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
        /// Get or set the action to be executed when the button is clicked.
        /// </summary>
        [HtmlAttributeName("onclick")]
        public string OnClickAction { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ContainedButtonTagHelper properties: TextEditorButton
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the location of the button.
        /// </summary>
        [HtmlAttributeName("location")]
        public TextEditorButtonLocation Location { get; set; }
        /// <summary>
        /// Get or set the styling of the button.
        /// </summary>
        [HtmlAttributeName("styling")]
        public ButtonStylingMode Styling { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ContainedButtonTagHelper properties: DataGridColumnButton
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the title or hint to be displayed for the button.
        /// </summary>
        [HtmlAttributeName("title")]
        public string Title { get; set; }

        #endregion
    }

    /// <summary>
    /// TextEditorButtonContext, DataGridColumnButtonContext
    /// </summary>
    public class ButtonContext
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ButtonContext properties
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        public IList<ButtonModel> Buttons { get; } = new List<ButtonModel>();

        #endregion
    }

    public class ButtonModel
    {
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region TextEditorButtonModel properties
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
        /// Get or set the action to be executed when the button is clicked.
        /// </summary>
        public string OnClickAction { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ButtonModel properties: TextEditorButton
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the location of the button.
        /// </summary>
        public TextEditorButtonLocation Location { get; set; }

        /// <summary>
        /// Get or set the styling of the button.
        /// </summary>
        public ButtonStylingMode Styling { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region ButtonModel properties: DataGridColumnButton
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

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
