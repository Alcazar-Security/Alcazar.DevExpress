using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
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

                IsVisible = IsVisible,
                IsVisibleAction = IsVisibleAction,

                OnClickAction = OnClickAction,

                // Text editor buttons
                Location = Location,
                Styling = Styling,

                // Data grid buttons
                Title = Title,
                Content = content,
            };

            // Set Class and Attributes
            button.SetAttributes(output.Attributes);

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
		/// Get or set an indicator if this button should be visible. Defaults to <see langword="true"/>.
		/// </summary>
		[HtmlAttributeName("visible")]
		public bool IsVisible { get; set; } = true;

		/// <summary>
		/// Get or set an JS method to determine if this button should be visible.
		/// </summary>
		[HtmlAttributeName("visible-js")]
		public string IsVisibleAction { get; set; }
		
        /// <summary>
		/// Get or set the JS method to be executed when the button is clicked.
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

	public class ContainedModelBase
    {
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ContainedModelBase properties: attributes
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Set the class and attributes of the base model.
        /// </summary>
        /// <param name="attributes"></param>
		public void SetAttributes(TagHelperAttributeList attributes)
		{
            Attributes = attributes;

			TagHelperAttribute @class = attributes.SingleOrDefault((a) => a.Name == "class");
			if (@class != null)
			{
				Class = @class.Value.ToString();
				Attributes = attributes.Where((a) => a.Name != "class").ToArray();
			}
		}

		/// <summary>
		/// Get or set the custom class of this button.
		/// </summary>
		public string Class { get; set; }

		/// <summary>
		/// Get or set a list of non-taghelper attributes of this button.
		/// </summary>
		public IList<TagHelperAttribute> Attributes { get; internal set; }

		#endregion
	}


	public class ButtonModel : ContainedModelBase
    {
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ButtonModel properties
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
		/// Get or set an indicator if this column should be visible. Defaults to <see langword="true"/>.
		/// </summary>
		public bool IsVisible { get; set; } = true;

		/// <summary>
		/// Get or set an JS method to determine if this column should be visible.
		/// </summary>
		public string IsVisibleAction { get; set; }
		
        /// <summary>
		/// Get or set the content or template of this button.
		/// </summary>
		public IHtmlContent Content { get; set; }
	}

	#endregion
}

