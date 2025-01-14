using Alcazar.DevExpress.TagHelpers.TagHelpers.Contained;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
    /// <summary>
    /// The <see cref="TagboxTagHelper"/> type implements a multiple selection dropdown box.
    /// The tag box supplements the select box implementation, as it can do multiple selection.
    /// </summary>
    [HtmlTargetElement("dx-tag")]
	public class TagboxTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TagboxTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public TagboxTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TagboxTagHelper overrides
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
			TagBoxBuilder builder = _htmlHelper.DevExtreme().TagBox();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the For attribute, if it is set
			object value = ProcessFor();
			System.Collections.IEnumerable values = value as System.Collections.IEnumerable;

			// Apply the For attribute, or the corresponding direct values
			builder = ApplyFor(builder, values);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the contexts, so that we can pass them to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			if (Items != null)
			{
				// Process server-side supplied items
				builder = builder.DataSource(Items, Key);
			}
			else if (sourceContext.Datasource != null)
			{
				// Process the (child) data source
				// Build the data source from the child tag
				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}

			// Add buttons, but only if we have some
			if (buttonContext.Buttons.Any() || !string.IsNullOrEmpty(HelpText))
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Buttons(b =>
				{
					// Add the clear button, but only if there are other buttons
					if (AllowClear)
						b.Add().Name("clear");

					// Add the custom help button, but only if we have a help text
					AddHelpButton(b);

					// Add custom defined buttons, but only if we have some
					AddCustomButtons(b, buttonContext.Buttons);
				});
			}
			else
			{
				// We have NO buttons, but check for the clear button
				if (AllowClear)
					builder = builder.ShowClearButton(AllowClear);
			}

			if (IsReadonly)
			{
				builder = builder
					.ReadOnly(true)
					.HoverStateEnabled(true);
			}

			if (IsDisabled)
			{
				builder = builder.Disabled(true);
			}

			// Process text-box specific properties
			//                     .Mask("+1 (X00) 000-0000")
			if (_searchMode.HasValue)
			{
				// Set the search mode and its properties
				builder = builder.SearchEnabled(true);
				builder = builder.SearchMode(_searchMode.Value);
				//builder = builder.SearchExpr("searchex");
				builder = builder.SearchTimeout(SearchTimeout);
				builder = builder.MinSearchLength(MinSearchLength);
			}

			builder = builder.HideSelectedItems(IsHideSelectedItens);
			builder = builder.Multiline(IsMultiLine);

			if (!string.IsNullOrEmpty(ValueExpression))
				builder = builder.ValueExpr(ValueExpression);
			if (!string.IsNullOrEmpty(DisplayExpression))
				builder = builder.DisplayExpr(DisplayExpression);

			builder = builder.OnSelectionChanged(OnSelectionChanged);
			builder = builder.OnChange(OnChange);
			//builder = builder.OnEnterKey("onMemberAdded");
			//builder = builder.OnItemClick("onMemberAdded");
			//builder = builder.OnOptionChanged("onMemberAdded");
			//builder = builder.OnValueChanged("onMemberAdded");

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private TagBoxBuilder ProcessCommon(TagBoxBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			// Seemingly can only add one attribute using this method
			// When Name is set, and not For, then we have a hidden input field with the name=name attr, and the visible input field with the id=name attr, and then the form post-back works (using value-expr and display-expr)
			// Now we just need to get For (and not Name) to work :)
			// builder = builder.InputAttr("dx-field-name", Name);
			return builder;
		}

		private TagBoxBuilder ProcessAttributes(TagBoxBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value.ToString());

			return builder;
		}

		private TagBoxBuilder ApplyFor(TagBoxBuilder builder, System.Collections.IEnumerable value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			// TODO if (For == null)
			//	value = Value;
			
			if (value != null)
				builder = builder.Value(value);
			
			if (!string.IsNullOrEmpty(Placeholder))
			{
				string placeholder = TranslateToProp(Placeholder, ViewContext);
				builder = builder.Placeholder(placeholder);
			}

			return builder;
		}

		private TagBoxBuilder ProcessTitle(TagBoxBuilder builder)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder.Hint(title);
			}

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SelectBoxTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public object Value { get; set; }

		/// <summary>
		/// Get or set the items of this dropdown box.
		/// Items are supplied server side, and are an alternative to a data source.
		/// </summary>
		[HtmlAttributeName("asp-items")]
		public System.Collections.IEnumerable Items { get; set; }

		/// <summary>
		/// Get or set the name of the key property of datasource items.
		/// </summary>
		[HtmlAttributeName("key")]
		public string Key { get; set; }

		/// <summary>
		/// Get or set an indicator if the clear button should be shown. Defaults to true.
		/// </summary>
		[HtmlAttributeName("clear")]
		public bool AllowClear { get; set; } = true;

		/// <summary>
		/// Get or set an indicator if selected items should be hidden in the dropdown, when opened.
		/// </summary>
		[HtmlAttributeName("hide")]
		public bool IsHideSelectedItens { get; set; }

		/// <summary>
		/// Get or set an indicator if selected items should be shown in multiple lines.
		/// </summary>
		[HtmlAttributeName("multi-line")]
		public bool IsMultiLine { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as dropdown item value.
		/// </summary>
		[HtmlAttributeName("value-expr")]
		public string ValueExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used to display dropdown items.
		/// </summary>
		[HtmlAttributeName("display-expr")]
		public string DisplayExpression { get; set; }

		/// <summary>
		/// Get or set the search mode used by the control. Setting the search mode enables searching.
		/// </summary>
		[HtmlAttributeName("search")]
		public DropDownSearchMode SearchMode
		{
			get { return _searchMode ?? DropDownSearchMode.StartsWith; }
			set { _searchMode = value; }
		}

		// Require the private fields so that we dont have to fully qualify the mode in cshtml (DropDownSearchMode.StartsWith)
		private DropDownSearchMode? _searchMode;

		/// <summary>
		/// Get or set the number of characters to enter before the lookup starts.
		/// This value may also be set in the data source, but the select box needs it to know when it should retrieve data from the data source.
		/// </summary>
		[HtmlAttributeName("min-length")]
		public int MinSearchLength { get; set; }

		/// <summary>
		/// Get or set the search timeout (in ms) for calling the datasource.
		/// This value may also be set in the data source, but the select box needs it to know when it should retrieve data from the data source.
		/// </summary>
		[HtmlAttributeName("timeout")]
		public int SearchTimeout { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the selection in the control changes.
		/// </summary>
		[HtmlAttributeName("selection-changed")]
		public string OnSelectionChanged { get; set; }

		/// <summary>
		/// Get or set the Javascript method to be called when the selection in the control changes.
		/// </summary>
		[HtmlAttributeName("change")]
		public string OnChange { get; set; }

		#endregion
	}
}
