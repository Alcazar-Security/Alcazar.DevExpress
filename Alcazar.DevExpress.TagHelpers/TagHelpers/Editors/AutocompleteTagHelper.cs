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
	/// The <see cref="AutocompleteTagHelper"/> type implements a autocomplete typeahead dropdown.
	/// </summary>
	[HtmlTargetElement("dx-autocomplete")]
	public class AutocompleteTagHelper : DropdownTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region AutocompleteTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public AutocompleteTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
			//_urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
			//_htmlEncoder = htmlEncoder;
			//_viewComponentHelper = viewComponentHelper;
			//_generator = generator;
		}

		//private readonly IViewComponentHelper _viewComponentHelper;
		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;
		//private readonly IUrlHelper _urlHelper;
		//private readonly HtmlEncoder _htmlEncoder;
		//private readonly IHtmlGenerator _generator;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region AutocompleteTagHelper overrides
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
			AutocompleteBuilder builder = _htmlHelper.DevExtreme().Autocomplete();

			// Apply the control context, which is values which an outer dx-field or dx-control tag might want to pass into me, the editor
			ApplyControlContext(context);

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the For attribute, if it is set
			object value = ProcessFor();

			// Apply the For attribute, or the corresponding direct values
			builder = ApplyFor(builder, value);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the contexts, so that we can pass them to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			// Process the (child) data source
			if (sourceContext.Datasource != null)
			{
				// Build the data source from the child tag
				builder = builder.MinSearchLength(sourceContext.Datasource.MinSearchLength);
				builder = builder.SearchTimeout(sourceContext.Datasource.SearchTimeout);

				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}

			// Add buttons, but only if we have some
			if (buttonContext.Buttons.Any() || !string.IsNullOrEmpty(HelpText))
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Buttons(buttons =>
				{
					// Add the clear button, but only if there are other buttons
					if (AllowClear)
						buttons.Add().Name("clear");

					if (ShowDropDown)
						buttons.Add().Name("dropdown");

					// Add the custom help button, but only if we have a help text
					AddHelpButton(buttons);

					foreach (ButtonModel button in buttonContext.Buttons)
					{
						buttons.Add()
							.Name(button.Name)
							.Location(button.Location)
							.Widget(w => w.Button()
								.Icon(button.Icon)
								.Text(button.Text)
								.StylingMode(button.Styling));
					}
				});
			}
			else
			{
				// We have NO buttons, but check for the clear button
				builder = builder.ShowClearButton(AllowClear);
				builder = builder.ShowDropDownButton(ShowDropDown);
			}

			// Process text-box specific properties
			//                     .Mask("+1 (X00) 000-0000")

			// Set the value and search expressions
			// Autocomplete does NOT have a DisplayExpr, we are keying in something, there is no value/text pair only a value
			if (!string.IsNullOrEmpty(ValueExpression))
				builder = builder.ValueExpr(ValueExpression);
			if (!string.IsNullOrEmpty(SearchExpression))
				builder = builder.SearchExpr(SearchExpression);

			// Search options
			builder = builder.SearchMode(SearchMode);

			// Item counts
			builder = builder.MaxItemCount(MaxItemCount);

			// This setValue thing is really wierd.
			// The function is never called, but it must exist. It is set as an option into the editor, but it somehow updates the grid from the editor
			if (!string.IsNullOrEmpty(SetValueJS))
				builder = builder.Option("setValue", new JS(SetValueJS));

			// Event handlers
			if (!string.IsNullOrEmpty(OnValueChanged))
				builder = builder.OnValueChanged(OnValueChanged);
			if (!string.IsNullOrEmpty(OnSelectionChanged))
				builder = builder.OnSelectionChanged(OnSelectionChanged);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private AutocompleteBuilder ProcessCommon(AutocompleteBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			// Seemingly can only add one attribute using this method
			builder = builder.InputAttr("dx-field-name", Name);

			return builder;
		}

		private AutocompleteBuilder ProcessAttributes(AutocompleteBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// And we are allowing attributes on the input field also
			foreach (var attr in InputAttributes)
				builder = builder.InputAttr(attr.Key, attr.Value?.ToString());

			return builder;
		}

		private AutocompleteBuilder ApplyFor(AutocompleteBuilder builder, object value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			if (For == null)
			{
				if (Value != null)
					builder = builder.Value(Value.ToString());
				else if (ValueJS != null)
					builder = builder.Value(new JS(ValueJS));
			}

			if (value != null)
				builder = builder.Value(value.ToString());

			if (!string.IsNullOrEmpty(Placeholder))
			{
				string placeholder = TranslateToProp(Placeholder, ViewContext);
				builder = builder.Placeholder(placeholder);
			}

			return builder;
		}

		private AutocompleteBuilder ProcessTitle(AutocompleteBuilder builder)
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
		#region AutocompleteTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public string Value { get; set; }

		/// <summary>
		/// Get or set an indicator if the clear button should be shown.
		/// </summary>
		[HtmlAttributeName("clear")]
		public bool AllowClear { get; set; }

		/// <summary>
		/// Get or set an indicator if the dropdown button should be shown.
		/// </summary>
		[HtmlAttributeName("dropdown")]
		public bool ShowDropDown { get; set; }

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
		/// Get or set the maximum number of items to be displayed in the dropdown. Defaults to 10, which is also the DX default.
		/// Zero means unlimited.
		/// </summary>
		[HtmlAttributeName("max")]
		public int MaxItemCount { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region AutocompleteTagHelper properties: not inherited
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		// If any of these properties are required on a control, it must be declared as an embedded control with in a dx-field or dx-control

		/// <summary>
		/// Get or set the name of the item property to be used as autocomplete item value.
		/// </summary>
		[HtmlAttributeName("value-expr")]
		public string ValueExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used when searching for items.
		/// </summary>
		[HtmlAttributeName("search-expr")]
		public string SearchExpression { get; set; }

		#endregion
	}
}
