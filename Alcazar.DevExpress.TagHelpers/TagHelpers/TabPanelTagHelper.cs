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
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="TabPanelTagHelper"/> type implements a tab panel, which consists of a string of tabs with an associated multi view.
	/// </summary>
	[HtmlTargetElement("dx-tabpanel")]
	public class TabPanelTagHelper : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabPanelTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public TabPanelTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabPanelTagHelper overrides
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

			// Create the builder for the tabs
			TabPanelBuilder builder = _htmlHelper.DevExtreme().TabPanel();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the contexts, so that we can pass them to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ViewItemContext viewsContext = GetOrCreateContext<ViewItemContext>(context);

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			if (Items != null)
			{
				// Process server-side supplied items
				builder = builder.DataSource(Items);
			}
			else if (sourceContext.Datasource != null)
			{
				// Process the (child) data source
				// TODO maybe convert to datasource a la DxGrid
				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}

			// Add tabs, but only if we have some
			if (viewsContext.ViewItems.Any())
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Items(b =>
				{
					int n = 0;
					foreach (ViewItemModel item in viewsContext.ViewItems)
					{
						string text = TranslateToProp(item.Text, ViewContext);
						string icon = TranslateToProp(item.Icon, ViewContext);

						TabPanelItemBuilder itemBuilder = b.Add()
							//.Option("index", n++)
							.Option("name", item.Name)
							.Icon(icon)
							.Text(text)
							// .TabTemplate("<span><i class='<%- item.Icon %>'>XX</i><%- item.Text %></span>")

							//.TabTemplate();
							//.Visible(item.IsVisible)
							.Disabled(item.IsDisabled);

						string content = ToString(item.Content);
						if (!string.IsNullOrWhiteSpace(content))
							itemBuilder = itemBuilder.Template(content);
					}
				});
			}

			builder = builder.SelectedIndex(SelectedIndex);

			// Process text-box specific properties
			//                     .Mask("+1 (X00) 000-0000")
			builder = builder
				.OnSelectionChanged("onPanelChanged");
			//	.OnInitialized("onTabsInitialized");

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private TabPanelBuilder ProcessCommon(TabPanelBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Visible and disabled
			if (!IsVisible)
				builder = builder.Visible(IsVisible);
			if (IsDisabled)
				builder = builder.Disabled(IsDisabled);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);
	
			return builder;
		}

		private TabPanelBuilder ProcessAttributes(TabPanelBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		private TabPanelBuilder ProcessTitle(TabPanelBuilder builder)
		{
			//if (!string.IsNullOrEmpty(Title))
			//{
			//	string title = TranslateToProp(Title, ViewContext);
			//	builder.Hint(title);
			//}
			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabsTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public string Value { get; set; }

		/// <summary>
		/// Get or set the index of the selected item in this control.
		/// </summary>
		[HtmlAttributeName("selected-index")]
		public int SelectedIndex { get; set; }

		/// <summary>
		/// Get or set the items of this dropdown box.
		/// Items are supplied server side, and are an alternative to a data source.
		/// </summary>
		[HtmlAttributeName("asp-items")]
		public System.Collections.IEnumerable Items { get; set; }

		#endregion
	}

	public class TabPanelContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabItemContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IList<TabItemModel> TabItems { get; } = new List<TabItemModel>();

		#endregion
	}
}
