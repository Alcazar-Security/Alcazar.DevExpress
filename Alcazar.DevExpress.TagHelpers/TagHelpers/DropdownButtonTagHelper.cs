using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public DropdownButtonTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

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

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

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
			ItemContext itemsContext = GetOrCreateContext<ItemContext>(context);

			// Process children of the card tag, the header, footer, and my body will need them 
			IHtmlContent content = await output.GetChildContentAsync();

			builder = ItemClick(builder);

			// Set the item template
			if (!string.IsNullOrWhiteSpace(ItemTemplate))
				builder = builder.ItemTemplate(ItemTemplate);
			else if (!string.IsNullOrWhiteSpace(ItemTemplateJS))
				builder = builder.ItemTemplate(new JS(ItemTemplateJS));
			else if (ItemTemplateRZ != null)
				builder = builder.ItemTemplate(ItemTemplateRZ);
			else if (ItemTemplateNT != null)
				builder = builder.ItemTemplate(new TemplateName(ItemTemplateNT));
			else if (itemsContext.ItemTemplateContent != null)
				builder = builder.ItemTemplate(ToString(itemsContext.ItemTemplateContent));

			// Set the template
			if (!string.IsNullOrWhiteSpace(Template))
				builder = builder.Template(Template);
			else if (!string.IsNullOrWhiteSpace(TemplateJS))
				builder = builder.Template(new JS(TemplateJS));
			else if (TemplateRZ != null)
				builder = builder.Template(TemplateRZ);
			else if (TemplateNT != null)
				builder = builder.Template(new TemplateName(TemplateNT));
			else if (itemsContext.TemplateContent != null)
				builder = builder.Template(ToString(itemsContext.TemplateContent));

			// Set the dropdown content template
			if (!string.IsNullOrWhiteSpace(DropDownTemplate))
				builder = builder.DropDownContentTemplate(DropDownTemplate);
			else if (!string.IsNullOrWhiteSpace(DropDownTemplateJS))
				builder = builder.DropDownContentTemplate(new JS(DropDownTemplateJS));
			else if (DropDownTemplateRZ != null)
				builder = builder.DropDownContentTemplate(DropDownTemplateRZ);
			else if (DropDownTemplateNT != null)
				builder = builder.DropDownContentTemplate(new TemplateName(DropDownTemplateNT));
			else if (itemsContext.DropdownTemplateContent != null)
				builder = builder.Template(ToString(itemsContext.DropdownTemplateContent));

			// Apply items, if any
			if (itemsContext.Items != null && itemsContext.Items.Any())
				builder = builder.DataSource(itemsContext.Items);
			else if (Items != null)
				builder = builder.DataSource(Items);

			// Process dropdown-button specific properties
			if (!string.IsNullOrEmpty(KeyExpression))
				builder = builder.KeyExpr(KeyExpression);
			if (!string.IsNullOrEmpty(DisplayExpression))
				builder = builder.DisplayExpr(DisplayExpression);

			// Options
			builder = builder.DropDownOptions(p =>
			{
				if (!string.IsNullOrEmpty(DropDownWidth))
					p = p.Width(DropDownWidth);

				p = p.Position(p =>
				{
					// MY is the dropdown's anchor point.
					if (DropDownMyHorizontalAlignment.HasValue && DropDownMyVerticalAlignment.HasValue)
						p = p.My(DropDownMyHorizontalAlignment.Value, DropDownMyVerticalAlignment.Value);

					// AT is the button's anchor point.
					if (DropDownAtHorizontalAlignment.HasValue && DropDownAtVerticalAlignment.HasValue)
					{
						// If the AT location is to be set, we MUST have an ID
						p = p.At(DropDownAtHorizontalAlignment.Value, DropDownAtVerticalAlignment.Value)
							 .Of($"#{ID}");
					}

					if (DropDownHorizontalCollision.HasValue && DropDownVerticalCollision.HasValue)
						p = p.Collision(DropDownHorizontalCollision.Value, DropDownVerticalCollision.Value);
				});
			});

			// Event handlers
			builder = ItemClick(builder);

			// TODO Text works best for topline dropdown buttons
			builder = builder.StylingMode(ButtonStylingMode.Text);

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
		/// Process attributes.
		/// </summary>
		/// <remarks>
		/// <para>
		///						On outer div						On element
		/// Any attribute		attr on dx-lineargauge				-
		/// Class attribute		class on dx-lineargauge				elem-class on dx-lineargauge
		/// </para>
		/// </remarks>
		private DropDownButtonBuilder ProcessAttributes(DropDownButtonBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the input
			foreach (var attr in attributes)
			{
				// Currently not used, all attributes remain only on the outer div
				// Class is not an attribute we want to pass on here (elem-class does that)
				//if (attr.Name != "class")
				//	builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());
			}

			// Class on element
			if (!string.IsNullOrEmpty(ElementClass))
				builder = builder.ElementAttr("class", ElementClass);

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
				case _clickRb:
					// builder.OnItemClick(RazorBlock(null));
					break;

				// href: the item includes a href, use it by calling a default method
				case _clickHref:
					builder.OnItemClick("dx_dropdown_itemclick");
					break;
			}

			return builder;
		}

		protected const string _clickRb = "rb";
		protected const string _clickHref = "href";

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownButtonTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

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
		/// Get or set the class attribute for the control element.
		/// </summary>
		[HtmlAttributeName("elem-class")]
		public string ElementClass { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownButtonTagHelper properties: dropdown options
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the dropdown width of this control. Defaults to teh width of the dropdown button.
		/// </summary>
		[HtmlAttributeName("dropdown-width")]
		public string DropDownWidth { get; set; }

		/// <summary>
		/// Get or set the horizontal anchor MY point of the dropdown.
		/// </summary>
		[HtmlAttributeName("dropdown-my-horz")]
		public HorizontalAlignment? DropDownMyHorizontalAlignment { get; set; }

		/// <summary>
		/// Get or set the vertical anchor MY point of the dropdown.
		/// </summary>
		[HtmlAttributeName("dropdown-my-vert")]
		public VerticalAlignment? DropDownMyVerticalAlignment { get; set; }

		/// <summary>
		/// Get or set the horizontal anchor AT point of the button.
		/// </summary>
		[HtmlAttributeName("dropdown-at-horz")]
		public HorizontalAlignment? DropDownAtHorizontalAlignment { get; set; }

		/// <summary>
		/// Get or set the vertical anchor AT point of the button.
		/// </summary>
		[HtmlAttributeName("dropdown-at-vert")]
		public VerticalAlignment? DropDownAtVerticalAlignment { get; set; }

		/// <summary>
		/// Get or set the horizontal collision resolution strategy for dropdown positioning.
		/// </summary>
		[HtmlAttributeName("dropdown-horz-collision")]
		public PositionResolveCollision? DropDownHorizontalCollision { get; set; }

		/// <summary>
		/// Get or set the vertical collision resolution strategy for dropdown positioning.
		/// </summary>
		[HtmlAttributeName("dropdown-vert-collision")]
		public PositionResolveCollision? DropDownVerticalCollision { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownButtonTagHelper properties: tag helper templates
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the (string) template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("template")]
		public string Template { get; set; }

		/// <summary>
		/// Get or set the JS template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("template-js")]
		public string TemplateJS { get; set; }

		/// <summary>
		/// Get or set the RazorBlock template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("template-rz")]
		public RazorBlock TemplateRZ { get; set; }

		/// <summary>
		/// Get or set the named template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("template-nt")]
		public string TemplateNT { get; set; }

		/// <summary>
		/// Get or set the (string) item template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("item-template")]
		public string ItemTemplate { get; set; }

		/// <summary>
		/// Get or set the JS item template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("item-template-js")]
		public string ItemTemplateJS { get; set; }

		/// <summary>
		/// Get or set the RazorBlock item template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("item-template-rz")]
		public RazorBlock ItemTemplateRZ { get; set; }

		/// <summary>
		/// Get or set the named item template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("item-template-nt")]
		public string ItemTemplateNT { get; set; }

		/// <summary>
		/// Get or set the (string) dropdown template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("dropdown-template")]
		public string DropDownTemplate { get; set; }

		/// <summary>
		/// Get or set the JS dropdown template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("dropdown-template-js")]
		public string DropDownTemplateJS { get; set; }

		/// <summary>
		/// Get or set the RazorBlock dropdown template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("dropdown-template-rz")]
		public RazorBlock DropDownTemplateRZ { get; set; }

		/// <summary>
		/// Get or set the named dropdown template of this dropdown button control.
		/// </summary>
		[HtmlAttributeName("dropdown-template-nt")]
		public string DropDownTemplateNT { get; set; }


		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownButtonTagHelper properties: tag helper events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the action to be executed when a dropdown item is clicked.
		/// </summary>
		[HtmlAttributeName("item-click")]
		public string OnItemClick { get; set; }

		#endregion
	}
}
