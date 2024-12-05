using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="PopupTagHelper"/> type implements a popup.
	/// </summary>
	[HtmlTargetElement("dx-popup")]
	public class PopupTagHelper : RouteTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public PopupTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
			_urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
			_htmlEncoder = htmlEncoder;
			_viewComponentHelper = viewComponentHelper;
			_generator = generator;
		}

		private readonly IViewComponentHelper _viewComponentHelper;
		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;
		private readonly IUrlHelper _urlHelper;
		private readonly HtmlEncoder _htmlEncoder;
		private readonly IHtmlGenerator _generator;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Asynchronously executes the tag with the given <paramref name="context"/> and <paramref name="output"/>.
		/// </summary>
		/// <param name="context"> Contains information associated with the current HTML tag. </param>
		/// <param name="output"> A stateful HTML element used to generate an HTML tag. </param>
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// To avoid: InvalidOperationException: Must call 'Contextualize' method before using this HtmlHelper instance.
			_htmlHelper.Contextualize(ViewContext);

			// Suppress myself as output, using only the translated UI text.
			output.SuppressOutput();

			string idValue = Guid.NewGuid().ToString();
			string titleValue = TranslateToProp(Title, ViewContext);
			string textValue = TranslateToProp(Text, ViewContext);

			// Create the builder for a popup
			PopupBuilder builder = _htmlHelper.DevExtreme().Popup();
			builder = builder.ID(idValue);

			if (!string.IsNullOrEmpty(Title))
			{
				// Show the title if we have one
				builder = builder.ShowTitle(true);
				builder = builder.Title(titleValue);
			}

			builder = builder.Content(textValue);

			// Apply default values
			builder = builder.Width(250);
			builder = builder.Height("auto");
			// builder = builder.Style("padding=less");
			builder = builder.DragEnabled(true);
			builder = builder.HideOnOutsideClick(true);
			builder = builder.ShowCloseButton(false);

			// Apply position
			builder = builder.Position(pc => pc
				.At(HorizontalAlignment.Left, VerticalAlignment.Bottom)
				.My(HorizontalAlignment.Left, VerticalAlignment.Top)
				.Collision(PositionResolveCollision.Fit, PositionResolveCollision.Fit));

			// Build the URL (the area part does not work!)
			string url = BuildUrl(_urlHelper);

			// Add the YES and NO buttons
			string yesValue = TranslateToProp(YesText, ViewContext);
			string noValue = TranslateToProp(NoText, ViewContext);

			builder = builder.ToolbarItems(barItems =>
			{
				barItems.Add()
					.Toolbar(Toolbar.Bottom)
					.Location(ToolbarItemLocation.Before)
					.Widget(widget => widget.Button()
						.Icon("check")
						.Text(yesValue)
						.StylingMode(ButtonStylingMode.Contained)
						.Type(ButtonType.Success)
						.OnClick($"function (data) {{ dx_popup_doPopup(data, '{url}'); }}"));
				barItems.Add()
					.Toolbar(Toolbar.Bottom)
					.Location(ToolbarItemLocation.After)
					.Widget(widget => widget.Button()
						.Icon("close")
						.Text(noValue)
						.StylingMode(ButtonStylingMode.Contained)
						.Type(ButtonType.Danger)
						.ElementAttr("data-popup", idValue)
						.OnClick("function (data) { dx_popup_closePopup(data); }"));
			});

			// Render the builder (into the POST content)
			IHtmlContent result = builder;
			output.Content.SetHtmlContent(result);

			// Render myself (into the content)
			IHtmlContent controlContent = await GenerateAnchorControl(context, output, idValue);
			output.Content.AppendHtml(controlContent);
		}

		private async Task<IHtmlContent> GenerateAnchorControl(TagHelperContext context, TagHelperOutput output, string popupID)
		{
			// Get the inner content of the tag helper
			IHtmlContent content = await output.GetChildContentAsync();

			// Create the anchor
			var anchorBuilder = new TagBuilder("a");

			// The anchor does not have a href, the popup does the action for us
			anchorBuilder.Attributes.Add("href", "javascript:;");

			// Set the popover class 
			anchorBuilder.AddCssClass("dx-confirmation");
			anchorBuilder.Attributes.Add("data-popup", popupID);
			anchorBuilder.InnerHtml.SetHtmlContent(content);
			return anchorBuilder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the text message to be used for this popup.
		/// </summary>
		[HtmlAttributeName("text")]
		public string Text { get; set; }

		/// <summary>
		/// Get or set the title to be used for this popup.
		/// </summary>
		[HtmlAttributeName("title")]
		public string Title { get; set; }

		/// <summary>
		/// Get or set the internal ID to be used for this popup, so that multiple anchors can use the same popup.
		/// If left empty, a popup dedicated to this anchor is created with a generated random name.
		/// Cant have this, every popup needs its own IDs, DX does not have a sharing option.
		/// </summary>
		/// [HtmlAttributeName("popup")]
		public string PopupID_ { get; set; }

		/// <summary>
		/// Get or set the text for the YES button. Defaults to the UI text 'YesButton'
		/// </summary>
		[HtmlAttributeName("yes")]
		public string YesText { get; set; } = "#YesButton";

		/// <summary>
		/// Get or set the text for the NO button. Defaults to the UI text 'NoButton'
		/// </summary>
		[HtmlAttributeName("no")]
		public string NoText { get; set; } = "#NoButton";

		#endregion
	}
}
