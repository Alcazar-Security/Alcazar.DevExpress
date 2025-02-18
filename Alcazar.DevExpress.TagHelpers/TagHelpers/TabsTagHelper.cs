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
	/// The <see cref="TabsTagHelper"/> type implements a line of tab headers.
	/// </summary>
	[HtmlTargetElement("dx-tabs")]
	public class TabsTagHelper : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabsTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public TabsTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabsTagHelper overrides
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
			TabsBuilder builder = _htmlHelper.DevExtreme().Tabs();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the contexts, so that we can pass them to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			TabItemContext tabsContext = GetOrCreateContext<TabItemContext>(context);

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
			if (tabsContext.TabItems.Any())
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Items(b =>
				{
					int n = 0;
					foreach (TabItemModel item in tabsContext.TabItems)
					{
						b.Add()
							.Option("index", n++)
							.Option("name", item.Name)
							.Option("advanced", !item.IsVisible)
							.Icon(item.Icon)
							.Text(item.Text)
							.Visible(item.IsVisible)
							.Disabled(item.IsDisabled);
					}
				});
			}

			builder = builder.SelectedIndex(SelectedIndex);

			if (IsDisabled)
			{
				builder = builder.Disabled(true);
			}

			// Process text-box specific properties
			//                     .Mask("+1 (X00) 000-0000")
			builder = builder
				.OnSelectionChanged("onTabsChanged")
				.OnInitialized("onTabsInitialized");

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private TabsBuilder ProcessCommon(TabsBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private TabsBuilder ProcessAttributes(TabsBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value.ToString());

			return builder;
		}

		private TabsBuilder ProcessTitle(TabsBuilder builder)
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

	[HtmlTargetElement("tab", ParentTag = "dx-tabs", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class TabTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the HTML of this tag, it is used for DX button generation only
			output.SuppressOutput();

			// Process the buttons tag and remember the content, so that the parent can process it
			TabItemContext tabsContext = GetContextSafe<TabItemContext>(context);

			string name = TranslateToProp(Name, ViewContext);
			string icon = TranslateToProp(Icon, ViewContext);
			string text = TranslateToProp(Text, ViewContext);

			TabItemModel item = new TabItemModel
			{
				Name = name,
				Icon = icon,
				Text = text,
				IsDisabled = IsDisabled,
				IsVisible = IsVisible,
				//Content = await output.GetChildContentAsync(),
			};

			tabsContext.TabItems.Add(item);
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabTagHelper properties
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
		/// Get or set an indicator if this tab is disabled.
		/// </summary>
		[HtmlAttributeName("disabled")]
		public bool IsDisabled { get; set; }

		/// <summary>
		/// Get or set an indicator if this tab is visible. Defaults to <see langword="true"/>.
		/// </summary>
		[HtmlAttributeName("visible")]
		public bool IsVisible { get; set; } = true;

		#endregion
	}

	public class TabItemContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabItemContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IList<TabItemModel> TabItems { get; } = new List<TabItemModel>();

		#endregion
	}

	public class TabItemModel
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabItemModel properties
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
		/// Get or set an indicator if this tab is disabled.
		/// </summary>
		public bool IsDisabled { get; set; }

		/// <summary>
		/// Get or set an indicator if this tab is visible. Defaults to <see langword="true"/>.
		/// </summary>
		public bool IsVisible { get; set; } = true;

		public IHtmlContent Content { get; set; }
		#endregion
	}
}
