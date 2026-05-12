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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="LoadPanelTagHelper"/> type implements a load panel.
	/// </summary>
	[HtmlTargetElement("dx-loadpanel")]
	public class LoadPanelTagHelper : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabsTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public LoadPanelTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LoadPanelTagHelper overrides
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

			// Create the builder for the load panel
			LoadPanelBuilder builder = _htmlHelper.DevExtreme().LoadPanel();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag WRAPPER attriubtes
			// builder = ProcessAttributes(builder, output.Attributes);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the contexts, so that we can pass them to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			TabItemContext tabsContext = GetOrCreateContext<TabItemContext>(context);

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			// Process load panel specific properties
			if (!string.IsNullOrEmpty(ShadingColor))
			{
				builder = builder.Shading(true);
				builder = builder.ShadingColor(ShadingColor);
			}

			//builder = builder.Container();
			//builder = builder.Delay();
			//builder = builder.HideOnOutsideClick();
			//builder = builder.HideOnParentScroll();
			//builder = builder.IndicatorSrc();

			builder = builder.Position(p =>
			{
				// MY is the dropdown's anchor point.
				if (PositionMyHorizontalAlignment.HasValue && PositionMyVerticalAlignment.HasValue)
					p = p.My(PositionMyHorizontalAlignment.Value, PositionMyVerticalAlignment.Value);

				// OF is the panels anchor element.
				if (!string.IsNullOrEmpty(AnchorID))
				{
					p = p.Of($"#{AnchorID}");

					if (PositionAtHorizontalAlignment.HasValue && PositionAtVerticalAlignment.HasValue)
					{
						// If the AT location is to be set, we MUST have an ID
						p = p.At(PositionAtHorizontalAlignment.Value, PositionAtVerticalAlignment.Value);
					}
				}

				// AT is the button's anchor point.
				if (PositionAtHorizontalAlignment.HasValue && PositionAtVerticalAlignment.HasValue)
				{
					// If the AT location is to be set, we MUST have an ID
					p = p.At(PositionAtHorizontalAlignment.Value, PositionAtVerticalAlignment.Value)
						 .Of($"#{AnchorID}");
				}

				if (PositionHorizontalCollision.HasValue && PositionVerticalCollision.HasValue)
					p = p.Collision(PositionHorizontalCollision.Value, PositionVerticalCollision.Value);
			});

			if (!string.IsNullOrEmpty(OnInitialized))
				builder = builder.OnInitialized(OnInitialized);

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private LoadPanelBuilder ProcessCommon(LoadPanelBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private LoadPanelBuilder ProcessAttributes(LoadPanelBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.WrapperAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		private LoadPanelBuilder ProcessTitle(LoadPanelBuilder builder)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder.Hint(Title);
			}
			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LoadPanelTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the JS method to be executed when the panel is initialised.
		/// </summary>
		[HtmlAttributeName("initialized")]
		public string OnInitialized { get; set; }

		/// <summary>
		/// Get or set the shading color of the panel.
		/// </summary>
		[HtmlAttributeName("shading")]
		public string ShadingColor { get; set; }

		/// <summary>
		/// Get or set an indicator if panel indicator of the panel is shown.
		/// Defaults to <see langword="true"/>.
		/// </summary>
		[HtmlAttributeName("indicator")]
		public bool ShowIndicator { get; set; } = true;

		/// <summary>
		/// Get or set an indicator if panel pane of the panel is shown.
		/// Defaults to <see langword="true"/>.
		/// </summary>
		[HtmlAttributeName("pane")]
		public bool ShowPane { get; set; } = true;

		/// <summary>
		/// Get or set the message of the panel.
		/// Defaults to "Loading ..." but can be translated to the current culture.
		/// </summary>
		[HtmlAttributeName("message")]
		public string Message { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region LoadPanelTagHelper properties: position options
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the ID of the anchor element
		/// </summary>
		[HtmlAttributeName("pos-at")]
		public string AnchorID { get; set; }

		/// <summary>
		/// Get or set the horizontal anchor MY point of the panel.
		/// </summary>
		[HtmlAttributeName("pos-my-horz")]
		public HorizontalAlignment? PositionMyHorizontalAlignment { get; set; }

		/// <summary>
		/// Get or set the vertical anchor MY point of the panel.
		/// </summary>
		[HtmlAttributeName("pos-my-vert")]
		public VerticalAlignment? PositionMyVerticalAlignment { get; set; }

		/// <summary>
		/// Get or set the horizontal anchor AT point of the anchor element.
		/// </summary>
		[HtmlAttributeName("pos-at-horz")]
		public HorizontalAlignment? PositionAtHorizontalAlignment { get; set; }

		/// <summary>
		/// Get or set the vertical anchor AT point of the anchor element.
		/// </summary>
		[HtmlAttributeName("pos-at-vert")]
		public VerticalAlignment? PositionAtVerticalAlignment { get; set; }

		/// <summary>
		/// Get or set the horizontal collision resolution strategy for panel positioning.
		/// </summary>
		[HtmlAttributeName("pos-horz-collision")]
		public PositionResolveCollision? PositionHorizontalCollision { get; set; }

		/// <summary>
		/// Get or set the vertical collision resolution strategy for panel positioning.
		/// </summary>
		[HtmlAttributeName("pos-vert-collision")]
		public PositionResolveCollision? PositionVerticalCollision { get; set; }

		#endregion
	}
}
