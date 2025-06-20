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
	/// The <see cref="DropdownTagHelper"/> type implements a single or multiple selection dropdown.
	/// Multiple selection is enabled by the selection mode of the inner content template, the dropdown itself cannot enable multiple selection itself.
	/// </summary>
	[HtmlTargetElement("dx-dropdown")]
	public class DropdownTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DropdownTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownTagHelper overrides
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
			DropDownBoxBuilder builder = _htmlHelper.DevExtreme().DropDownBox();

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

			// Set the data source, for single or multiple selection
			builder = IsMultiple ? ProcessMultiple(builder) : ProcessSingle(builder, sourceContext);

			// Add buttons, but only if we have some
			if (buttonContext.Buttons.Any() || !string.IsNullOrEmpty(HelpText))
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Buttons(b =>
				{
					// Add the clear button, but only if there are other buttons
					if (AllowClear)
						b.Add().Name("clear");

					if (ShowDropDown)
						b.Add().Name("dropdown");

					// Add the custom help button, but only if we have a help text
					AddHelpButton(b);

					foreach (ButtonModel button in buttonContext.Buttons)
					{
						b.Add()
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

			// Process dropdown-box specific properties
			if (!string.IsNullOrEmpty(ValueExpression))
				builder = builder.ValueExpr(ValueExpression);
			if (!string.IsNullOrEmpty(DisplayExpression))
				builder = builder.DisplayExpr(DisplayExpression);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private DropDownBoxBuilder ProcessCommon(DropDownBoxBuilder builder)
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

		private DropDownBoxBuilder ProcessAttributes(DropDownBoxBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// And we are allowing attributes on the input field also
			foreach (var attr in InputAttributes)
				builder = builder.InputAttr(attr.Key, attr.Value?.ToString());

			return builder;
		}

		private DropDownBoxBuilder ApplyFor(DropDownBoxBuilder builder, object value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			if (For == null)
				value = Value;

			if (value != null)
				builder = builder.Value(value.ToString());

			if (!string.IsNullOrEmpty(Placeholder))
			{
				string placeholder = TranslateToProp(Placeholder, ViewContext);
				builder = builder.Placeholder(placeholder);
			}

			return builder;
		}

		private DropDownBoxBuilder ProcessTitle(DropDownBoxBuilder builder)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder.Hint(title);
			}

			return builder;
		}

		private DropDownBoxBuilder ProcessSingle(DropDownBoxBuilder builder, DataSourceContext sourceContext)
		{
			// Process the content of the dropdown
			if (Items != null)
			{
				// Lets see if this works
				builder = builder.Items(c =>
				{
					c.Add().Text("Read");
					c.Add().Text("Write");
				});

				// WIP
				// Convert to an array datasource, a la DxGrid
				// builder = builder.DataSource(d => BuildDatasource(d));
				// 
			}

			// Process the (child) data source
			else if (sourceContext.Datasource != null)
			{
				// Build the data source from the child tag
				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}

			return builder;
		}

		private DropDownBoxBuilder ProcessMultiple(DropDownBoxBuilder builder)
		{
			// Process the content of the dropdown
			if (Items != null)
			{
				// Convert to an array datasource
				// builder = builder.Items(Items);
				//builder = builder.Items(c =>
				//{
				//	foreach(var item in Items)	
				//		c.Add().Text(Items.ToString());
				//});
				builder = builder.DataSource(Items, Key);

				// WIP
				// Build a id/text datagrid template, with multiple selection
				//IHtmlContent simpleTemplate = GetSimpleTemplate();
				//string content = ToString(simpleTemplate);
				//builder = builder.ContentTemplate(new TemplateName("Hannes2"));
			}

			return builder;
		}

		private IHtmlContent GetSimpleTemplate()
		{
			string idValue = Guid.NewGuid().ToString();

			var builder = _htmlHelper.DevExtreme().DataGrid()
				.ID(idValue)
				.DataSource("dx_dropdown_getDataSource")
				.Columns(columns =>
				{
					columns.Add().DataField("Value");
					columns.Add().DataField("Text");
				})
				.HoverStateEnabled(true)
				//.Paging(p => p.PageSize(10))
				//.FilterRow(f => f.Visible(true))
				//.Scrolling(s => s.Mode(GridScrollingMode.Virtual))
				//.Height(345)
				.Selection(s => s.Mode(SelectionMode.Multiple))
				.SelectedRowKeys("dx_dropdown_selectedRowKeys")
				.OnSelectionChanged("dx_dropdown_selected");

			IHtmlContent result = builder;
			return result;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DropdownTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public string Value { get; set; }

		/// <summary>
		/// Get or set the sequence of items to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("asp-items")]
		public System.Collections.IEnumerable Items { get; set; }


		/// <summary>
		/// Get or set the name of the key property of datasource items.
		/// </summary>
		[HtmlAttributeName("key")]
		public string Key { get; set; }

		/// <summary>
		/// Get or set an indicator if the dropdown button should be shown.
		/// </summary>
		[HtmlAttributeName("multiple")]
		public bool IsMultiple { get; set; }

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
		/// Get or set the name of the item property to be used as dropdown item value.
		/// </summary>
		[HtmlAttributeName("value-expr")]
		public string ValueExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as dropdown item display text.
		/// </summary>
		[HtmlAttributeName("display-expr")]
		public string DisplayExpression { get; set; }

		#endregion
	}
}
