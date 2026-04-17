using Alcazar.Web.Extensibility;
using Alcazar.Web.Utilities;
using DevExpress.Data.Utils;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Policy;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="PopupTagHelper"/> type implements a popup dialog on a web page.
	/// </summary>
	[HtmlTargetElement("dx-popup")]
	public class PopupTagHelper : RouteTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public PopupTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
			_urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);

			// Set Defaults for inherited properties
			Width = "250";
			Height = "auto";
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;
		private readonly IUrlHelper _urlHelper;

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

			// Create the builder for a popup
			PopupBuilder builder = _htmlHelper.DevExtreme().Popup();

			// Process common functionality for editors
			builder = ProcessCommon(builder, out string idValue);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Obtain the context, so that we can pass them to child tag helpers
			ItemContext itemsContext = GetOrCreateContext<ItemContext>(context);

			// Process children of the popup tag, which becomes the column template (or contain button items)
			IHtmlContent content = await output.GetChildContentAsync();

			// Process the content
			builder = ProcessContent(builder, content);

			// Apply default values
			builder = builder.DragEnabled(AllowDrag);
			builder = builder.HideOnOutsideClick(IsHideOnOutsideClick);
			builder = builder.ShowCloseButton(AllowClose);

			//builder = builder.Container("");

			switch (PopupMode)
			{
				default:
					builder = ProcessPopup(builder, itemsContext);
					break;

				case PopupModes.Confirmation:
					builder = ProcessConfirmation(builder, idValue);
					break;
			}

			// Popup events
			builder = builder.OnInitialized(OnInitialized);
			builder = builder.OnContentReady(OnContentReady);
			builder = builder.OnShown(OnShown);
			builder = builder.OnHidden(OnHidden);

			// Render the builder (into the content)
			IHtmlContent result = builder;
			output.Content.SetHtmlContent(result);

			// Render the mode-specific content (into the content)
			switch (PopupMode)
			{
				case PopupModes.Confirmation:
					await RenderConfirmation(context, output, idValue);
					break;
			}
		}

		private PopupBuilder ProcessCommon(PopupBuilder builder, out string idValue)
		{
			// Set the ID to a random value
			idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			if (!string.IsNullOrEmpty(Height))
				builder = builder.Height(Height);

			return builder;
		}

		private PopupBuilder ProcessTitle(PopupBuilder builder)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				builder = builder.ShowTitle(true);

				string title = TranslateToProp(Title, ViewContext);
				builder.Title(title);
			}

			return builder;
		}

		private PopupBuilder ProcessContent(PopupBuilder builder, IHtmlContent content)
		{
			// Obtain the inner content of the popup tag
			string text = ToString(content);

			// If there is no inner content, try the text
			if (string.IsNullOrEmpty(text))
				text = TranslateToProp(Text, ViewContext);

			// Process content
			if (!string.IsNullOrEmpty(text))
				builder = builder.Content(text);

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper default mode
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		private PopupBuilder ProcessPopup(PopupBuilder builder, ItemContext itemsContext)
		{
			builder = builder.ToolbarItems(barItems =>
			{
				foreach (ItemModel item in itemsContext.Items)
				{
					PopupToolbarItemBuilder itemBuilder = barItems.Add();
					itemBuilder = itemBuilder.Toolbar(item.Toolbar);
					itemBuilder = itemBuilder.Location(item.Location);
					itemBuilder = itemBuilder.LocateInMenu(item.MenuMode);
					itemBuilder = itemBuilder.Widget(widget => ProcessWidgetButton(widget, item));
				}
			});

			return builder;
		}

		private WidgetBuilder ProcessWidgetButton(ToolbarItemFactory widget, ItemModel item)
		{
			ButtonBuilder builder = widget.Button();

			if (!string.IsNullOrEmpty(item.ID))
				builder = builder.ID(item.ID);

			if (!string.IsNullOrEmpty(item.Icon))
				builder = builder.Icon(item.Icon);

			if (!string.IsNullOrEmpty(item.Text))
				builder = builder.Text(item.Text);

			builder = builder.StylingMode(item.ButtonStylingMode);
			builder = builder.Type(item.ButtonType);

			if (!string.IsNullOrEmpty(item.OnClick))
				builder = builder.OnClick(item.OnClick);

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper confirmation mode
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		// Confirmation mode creates an anchor <a /> with a JS confirmation popup, with a YES/NO choice

		private PopupBuilder ProcessConfirmation(PopupBuilder builder, string idValue)
		{
			// 2026-04 introduced RouteUtilities
			// Build the URL (the area part does not work!)
			string url = RouteUtilities.BuildUrl(_urlHelper, Url, Action, Controller, Area, RouteValues);

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

			// Apply position relative to the position-of element
			builder = builder.Position(pc => pc
				.At(HorizontalAlignment.Left, VerticalAlignment.Bottom)
				.My(HorizontalAlignment.Left, VerticalAlignment.Top)
				.Collision(PositionResolveCollision.Fit, PositionResolveCollision.Fit));

			return builder;
		}

		private async Task RenderConfirmation(TagHelperContext context, TagHelperOutput output, string idValue)
		{
			// Render the anchor (into the content)
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
		#region PopupTagHelper properties: content
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the text message to be used for this popup.
		/// </summary>
		[HtmlAttributeName("text")]
		public string Text { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper properties: behaviour
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set an indicator if the close button should be shown.
		/// </summary>
		[HtmlAttributeName("close")]
		public bool AllowClose { get; set; }

		/// <summary>
		/// Get or set an indicator if the user can drag the popup. Defaults to <see langword="true"/>.
		/// </summary>
		[HtmlAttributeName("drag")]
		public bool AllowDrag { get; set; } = true;

		/// <summary>
		/// Get or set an indicator if the popup closes (gets hidden) when the user clicks outsode the popup. Defaults to <see langword="true"/>.
		/// </summary>
		[HtmlAttributeName("hide")]
		public bool IsHideOnOutsideClick { get; set; } = true;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper properties: mode specific
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the popup mode.
		/// Modes allow for special, built-in behaviour of popups.
		/// </summary>
		[HtmlAttributeName("mode")]
		public PopupModes PopupMode { get; set; }

		/// <summary>
		/// Get or set the text for the YES button for the <see cref="PopupModes.Confirmation"/> mode. Defaults to the UI text 'YesButton'
		/// </summary>
		[HtmlAttributeName("yes")]
		public string YesText { get; set; } = "#YesButton";

		/// <summary>
		/// Get or set the text for the NO button for the <see cref="PopupModes.Confirmation"/> mode. Defaults to the UI text 'NoButton'
		/// </summary>
		[HtmlAttributeName("no")]
		public string NoText { get; set; } = "#NoButton";

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PopupTagHelper properties: events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the JS function to be executed when the popup is initialised.
		/// </summary>
		[HtmlAttributeName("initialised")]
		public string OnInitialized { get; set; }

		/// <summary>
		/// Get or set the JS function to be executed when the content of the popup is ready.
		/// </summary>
		[HtmlAttributeName("content-ready")]
		public string OnContentReady { get; set; }

		/// <summary>
		/// Get or set the JS function to be executed when the popup is shown.
		/// </summary>
		[HtmlAttributeName("shown")]
		public string OnShown { get; set; }

		/// <summary>
		/// Get or set the JS function to be executed when the popup is hidden.
		/// </summary>
		[HtmlAttributeName("hidden")]
		public string OnHidden { get; set; }

		#endregion
	}

	public enum PopupModes
	{
		None,
		Confirmation,
	}
}
