using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="MultiviewTagHelper"/> type implements a multi view.
	/// </summary>
	[HtmlTargetElement("dx-multiview")]
	public class MultiviewTagHelper : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region MultiviewTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public MultiviewTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region MultiviewTagHelper overrides
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

			// Create the builder for the multi view
			MultiViewBuilder builder = _htmlHelper.DevExtreme().MultiView();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the contexts, so that we can pass them to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ViewItemContext viewContext = GetOrCreateContext<ViewItemContext>(context);

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
				// Build the data source from the child tag
				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}

			// Add tabs, but only if we have some
			if (viewContext.ViewItems.Any())
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Items(b =>
				{
					int n = 0;
					foreach (ViewItemModel item in viewContext.ViewItems)
					{
						b.Add()
							.Option("index", n++)
							.Option("name", item.Name)
							.Template(ToString(item.Content))
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
				.OnSelectionChanged("selectionChanged")
				.OnInitialized("onMultiviewInitialized");

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private MultiViewBuilder ProcessCommon(MultiViewBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private MultiViewBuilder ProcessAttributes(MultiViewBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value.ToString());

			return builder;
		}

		private MultiViewBuilder ProcessTitle(MultiViewBuilder builder)
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
		#region MultiviewTagHelper properties: tag helper
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

	[HtmlTargetElement("view", ParentTag = "dx-tabpanel", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("view", ParentTag = "dx-multiview", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ViewTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ViewTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the HTML of this tag, it is used for DX button generation only
			output.SuppressOutput();

			// Process the buttons tag and remember the content, so that the parent can process it
			ViewItemContext viewsContext = GetContextSafe<ViewItemContext>(context);

			string name = TranslateToProp(Name, ViewContext);

			ViewItemModel item = new ViewItemModel
			{
				Name = name,
				Icon = Icon,
				Text = Text,
				IsDisabled = IsDisabled,
				IsVisible = IsVisible,
				Content = await output.GetChildContentAsync(),
			};

			viewsContext.ViewItems.Add(item);
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ViewTagHelper properties
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

	public class ViewItemContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ViewItemContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IList<ViewItemModel> ViewItems { get; } = new List<ViewItemModel>();

		#endregion
	}

	public class ViewItemModel
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ViewItemModel properties
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
