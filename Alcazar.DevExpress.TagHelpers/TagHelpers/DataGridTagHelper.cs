using Amaqele.Common.Base;
using Amaqele.Common.Types;
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
using NLog.Config;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DataGridTagHelper"/> type implements a data grid.
	/// </summary>
	[HtmlTargetElement("dx-datagrid")]
	public class DataGridTagHelper : ListControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DataGridTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, HtmlEncoder htmlEncoder, IViewComponentHelper viewComponentHelper, IHtmlGenerator generator)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
			//_urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
			//_htmlEncoder = htmlEncoder;
			//_viewComponentHelper = viewComponentHelper;
			//_generator = generator;

			// Apply defaults (of inherited properties)
			// Width = "100%"; we should do this by CSS
		}

		//private readonly IViewComponentHelper _viewComponentHelper;
		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;
		//private readonly IUrlHelper _urlHelper;
		//private readonly HtmlEncoder _htmlEncoder;
		//private readonly IHtmlGenerator _generator;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper overrides
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

			// Generate a builder mathod using the record type as generic type
			MethodInfo method = typeof(DataGridTagHelper).GetMethod(nameof(BuildDataGridAsync), BindingFlags.Instance | BindingFlags.NonPublic);
			if (method.IsGenericMethod)
				method = method.MakeGenericMethod(RecordType);

			// Configure the builder
			Task<IHtmlContent> result = (Task<IHtmlContent>)method.Invoke(this, new object[] { context, output });
			IHtmlContent content = result.GetAwaiter().GetResult();

			// Render the builder (into the POST content)
			output.Content.SetHtmlContent(content);
		}

		/// <summary>
		/// Build the data grid. 
		/// This method is dynamically generated with a generic type argument and then executed.
		/// </summary>
		private async Task<IHtmlContent> BuildDataGridAsync<T>(TagHelperContext context, TagHelperOutput output)
		{
			// Create the builder for a popup
			DataGridBuilder<T> builder = _htmlHelper.DevExtreme().DataGrid<T>();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			builder = builder.AllowColumnReordering(true);
			builder = builder.ShowBorders(false);
			builder = builder.ShowRowLines(true);
			builder = builder.ShowColumnLines(false);
			builder = builder.ShowColumnHeaders(true);
			builder = builder.ColumnAutoWidth(true);
			//builder = builder.ColumnWidth(Mode.Auto);
			//builder = builder.Scrolling(s => s
			//	.ColumnRenderingMode(GridColumnRenderingMode.Standard)
			//	.Mode(GridScrollingMode.Infinite)
			//	.ShowScrollbar(ShowScrollbarMode.OnHover));
			//builder = builder.RootOnly(true);
			//builder = builder.Filter(true);
			builder = builder.FilterRow(f => f.Visible(true));
			builder = builder.HeaderFilter(f => f.Visible(true));

			builder = builder.Paging(p => p.PageSize(50));
			builder = builder.Pager(p => p
				.DisplayMode(GridPagerDisplayMode.Adaptive)
				.ShowPageSizeSelector(true)
				.ShowNavigationButtons(true)
				.AllowedPageSizes(new[] { 25, 50, 100, 250, 500 }));

			// Process selection options
			if (SelectionMode != SelectionMode.None)
			{
				// Selection always sets hover state
				builder = builder.HoverStateEnabled(true);

				// Set the selection mode
				// TODO there are a few other options to choose
				builder = builder.Selection(s => s.Mode(SelectionMode));

				// Set the JS function to execute when the selection changes
				if (!string.IsNullOrEmpty(OnSelectionChanged))
					builder = builder.OnSelectionChanged(OnSelectionChanged);
			}

			// Process editing options
			builder = builder.Editing(editing =>
			{
				// Allow editing options of the corresponding actions are set
				editing
					.Mode(GridEditMode.Row)
					.UseIcons(true);

				if (!string.IsNullOrEmpty(InsertAction) || !string.IsNullOrEmpty(OnInserting) || !string.IsNullOrEmpty(OnInserted))
					editing.AllowAdding(true);
				if (!string.IsNullOrEmpty(UpdateAction) || !string.IsNullOrEmpty(OnUpdating) || !string.IsNullOrEmpty(OnUpdated))
					editing.AllowUpdating(true);
				if (!string.IsNullOrEmpty(DeleteAction) || !string.IsNullOrEmpty(OnRemoving) || !string.IsNullOrEmpty(OnRemoved))
					editing.AllowDeleting(true);
			});

			if (!string.IsNullOrEmpty(OnRowInserting))
				builder = builder.OnRowInserting(OnRowInserting);
			if (!string.IsNullOrEmpty(OnRowInserted))
				builder = builder.OnRowInserted(OnRowInserted);

			//builder = builder.OnCellClick("onCellClick");
			//builder = builder.OnContentReady("onContentReady");

			// Create the context, so that we can pass it to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ColumnsContext columnContext = GetOrCreateContext<ColumnsContext>(context);

			// Process children of the card tag, the header, footer, and my body will need them 
			IHtmlContent content = await output.GetChildContentAsync();

			// Set the data source
			if (sourceContext.Datasource != null)
			{
				// First preference, process the (child) data source
				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}
			else
			{
				// Second preference, process the datasource from my own properties
				builder = builder.DataSource(d => BuildDatasource(d));
			}

			// Post process, based on the data source
			switch (DatasourceType)
			{
				case DataSourceTypes.Mvc:
					builder = builder.RemoteOperations(c => { c.Filtering(true); });
					break;
			}

			// Add columns, if we have some
			if (columnContext.Columns.Any())
			{
				builder = builder.Columns(async columns =>
				{
					foreach (ColumnModel column in columnContext.Columns)
					{
						switch (column.Type)
						{
							// A data column displays a property of the model
							default:
							case "data":
								ProcessDataColumn<T>(columns, column);
								break;

							// A command column displays command buttons which act on the model which is displayed in this row
							case "command":
								await ProcessCommandColumnAsync<T>(context, output, columns, column);
								break;
						}
					}
				});
			}

			if (!string.IsNullOrEmpty(OnInitializedAction))
				builder = builder.OnInitialized(OnInitializedAction);

			// Build the toolbar
			// The location of the toolbar is not changable, DX says:
			// The data grid does not provide an option for the toolbar position. You might want to add a separate toolbar widget under your grid and populate it with desired controls. Samples are available in our Toolbar documentation.
			//builder = builder.Toolbar(toolbar =>
			//{
			//	toolbar.Items(i =>
			//	{
			//		// If we are inserting, show and customise the ADD toolbar button
			//		if (string.IsNullOrEmpty(InsertAction) || string.IsNullOrEmpty(OnInserted))
			//		{
			//			i.Add()
			//				.Name(DataGridToolbarItem.AddRowButton)
			//				.Location(ToolbarItemLocation.After)
			//				.ShowText(ToolbarItemShowTextMode.InMenu);
			//		}
			//	});
			//});

			return builder;
		}

		private DataGridBuilder<T> ProcessCommon<T>(DataGridBuilder<T> builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private DataGridBuilder<T> ProcessAttributes<T>(DataGridBuilder<T> builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value.ToString());

			return builder;
		}

		private CollectionFactory<DataGridColumnBuilder<T>> ProcessDataColumn<T>(CollectionFactory<DataGridColumnBuilder<T>> columns, ColumnModel column)
		{
			if (column.For != null)
			{
				// If we have both FOR and NAME, keep the existing name, this is an override for the DataField() method, where the JSON field and the property name differ due to a [JsonProperty] attribute
				if (string.IsNullOrEmpty(column.Name))
				{
					// Check for a [JsonProperty] attribute which renames the field in JSON data and confuses DX
					// 2025-05 UNDO this, we use Newtonsoft attributes, but DX uses System.Text.Json, therefore DX serialises with the original property names
					//if (column.For.Metadata is Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata mmd && mmd.Attributes.PropertyAttributes != null)
					//{
					//	// Use the JSON property name
					//	JsonPropertyAttribute attribute = mmd.Attributes.PropertyAttributes.FirstOrDefault((a) => a is JsonPropertyAttribute) as JsonPropertyAttribute;
					//	if (attribute != null)
					//		column.Name = attribute.PropertyName;
					//}

					if (string.IsNullOrEmpty(column.Name))
					{
						// This is a horrible hack for no.QualifiedName, when we try to obtain the asp-for prop from an IEnumerable model
						column.Name = RemoveNoname(column.For.Name);
					}
				}

				// Set the label if it is not explicitely set
				if (string.IsNullOrEmpty(column.Label))
					column.Label = column.For.Metadata.DisplayName;
				if (string.IsNullOrEmpty(column.Label))
					column.Label = column.For.Metadata.Name;

				column.DataType = ToDataType(column.For.Metadata.ModelType);
			}

			DataGridColumnBuilder<T> builder = columns.Add()
				.DataField(column.Name)
				.Caption(column.Label)
				.Alignment(column.Alignment)
				.AllowSorting(true)
				.AllowEditing(!column.IsReadonly);

			if (!string.IsNullOrEmpty(column.IsVisibleAction))
				builder = builder.Visible(new JS(column.IsVisibleAction));
			else
				builder = builder.Visible(column.IsVisible);

			//.SortOrder(SortOrder.Asc);

			if (column.DataType.HasValue)
				builder = builder.DataType(column.DataType.Value);

			if (column.FilterType.HasValue)
			{
				builder = builder
					.AllowFiltering(true)
					.FilterType(FilterType.Include);

				if (column.FilterOperation.HasValue)
				{
					builder = builder
						.SelectedFilterOperation(column.FilterOperation.Value)
						.FilterValue(column.FilterValue)
						.EditorOptions("");
				}
			}
			else
			{
				builder = builder
					.AllowFiltering(false);
			}

			// Set the child content or templates
			if (!string.IsNullOrWhiteSpace(column.Content))
				builder = builder.CellTemplate(column.Content);
			else if (!string.IsNullOrWhiteSpace(column.ContentJS))
				builder = builder.CellTemplate(new JS(column.ContentJS));
			else if (column.ContentRZ != null)
				builder = builder.CellTemplate(column.ContentRZ);
			else if (column.ContentNT != null)
				builder = builder.CellTemplate(new TemplateName(column.ContentNT));

			return columns;
		}

		private async Task<CollectionFactory<DataGridColumnBuilder<T>>> ProcessCommandColumnAsync<T>(TagHelperContext context, TagHelperOutput output, CollectionFactory<DataGridColumnBuilder<T>> columns, ColumnModel column)
		{
			// Get the context, so that we can use it here
			// ButtonContext buttonContext = GetContextSafe<ButtonContext>(context);

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			DataGridColumnBuilder<T> builder = columns.Add()
				.Type(column.CommandType)
				.Caption(column.Label);

			if (!string.IsNullOrEmpty(column.IsVisibleAction))
				builder = builder.Visible(new JS(column.IsVisibleAction));
			else
				builder = builder.Visible(column.IsVisible);

			// Add buttons, but only if we have some
			if (column.Buttons != null)
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Buttons(buttons =>
				{
					// Add the clear button, but only if there are other buttons
					if (!string.IsNullOrEmpty(UpdateAction))
						buttons.Add().Name("edit");

					foreach (ButtonModel button in column.Buttons)
					{
						DataGridColumnButtonBuilder buttonBuilder = buttons.Add();

						BuildButton<T>(buttonBuilder, button);
					}
				});
			}
			else
			{
				// We have NO buttons, but check for the clear button
				// So we should not have a custom column in the first place, nothing to do
			}

			// Set the child content or templates
			// This is challenged here, since the custom template overwrites the button content AND behaviour, eg a delete no longer deletes
			if (!string.IsNullOrWhiteSpace(column.Content))
				builder = builder.CellTemplate(column.Content);
			else if (!string.IsNullOrWhiteSpace(column.ContentJS))
				builder = builder.CellTemplate(new JS(column.ContentJS));
			else if (column.ContentRZ != null)
				builder = builder.CellTemplate(column.ContentRZ);
			else if (column.ContentNT != null)
				builder = builder.CellTemplate(new TemplateName(column.ContentNT));

			return columns;
		}

		private void BuildButton<T>(DataGridColumnButtonBuilder buttonBuilder, ButtonModel button)
		{
			if (!string.IsNullOrEmpty(button.Name))
				buttonBuilder = buttonBuilder.Name(button.Name);

			// Seemingly can only display icon OR text
			if (!string.IsNullOrEmpty(button.Icon))
				buttonBuilder = buttonBuilder.Icon(button.Icon);
			else if (!string.IsNullOrEmpty(button.Text))
			{
				string text = TranslateToProp(button.Text, ViewContext);
				buttonBuilder = buttonBuilder.Text(text);
			}

			if (!string.IsNullOrEmpty(button.Title))
			{
				string title = TranslateToProp(button.Title, ViewContext);
				buttonBuilder = buttonBuilder.Hint(title);
			}

			if (!string.IsNullOrEmpty(button.IsVisibleAction))
				buttonBuilder = buttonBuilder.Visible(new JS(button.IsVisibleAction));
			else
				buttonBuilder = buttonBuilder.Visible(button.IsVisible);

			if (!string.IsNullOrEmpty(button.OnClickAction))
				buttonBuilder = buttonBuilder.OnClick(button.OnClickAction);

			// Set the class
			if (!string.IsNullOrEmpty(button.Class))
				buttonBuilder = buttonBuilder.CssClass(button.Class);

			// Not used currently
			//.Template("<span>xxx</span>")
			// if (button.Content != null)
			//	builder2 = builder2.Template(ToString(button.Content));
		}

		private void todo(DataGridBuilder<object> builder)
		{
			builder = builder.RowAlternationEnabled(true);

			builder = builder.SearchPanel(s => s
				.Visible(true)
				.HighlightCaseSensitive(true));

			//builder = builder.OnContentReady("contentReady");
			builder = builder.GroupPanel(g => g.Visible(true));
			builder = builder.Grouping(g => g.AutoExpandAll(false));

			BulletBuilder builder2 = _htmlHelper.DevExtreme().Bullet()
				.Value(new JS("value * 100"))
				.Size(s => s
					.Height(35)
					.Width(150))
				.Margin(m => m
					.Top(5)
					.Bottom(0)
					.Left(5))
				.ShowTarget(false)
				.ShowZeroLevel(true)
				.StartScaleValue(0)
				.EndScaleValue(100)
				.Tooltip(t => t
					.Enabled(true)
					.Font(f => f.Size(18))
					.PaddingTopBottom(2)
					/*.CustomizeTooltip("customizeTooltip")*/);
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the text message to be used for this popup.
		/// </summary>
		[HtmlAttributeName("text")]
		public string Text { get; set; }

		/// <summary>
		/// Get or set an indicator if the clear button should be shown.
		/// </summary>
		[HtmlAttributeName("clear")]
		public bool AllowClear { get; set; }

		/// <summary>
		/// Get or set an placeholder to be shown in the control.
		/// </summary>
		[HtmlAttributeName("placeholder")]
		public string Placeholder { get; set; }

		/// <summary>
		/// Get or set the number of characters to enter before the lookup starts.
		/// </summary>
		[HtmlAttributeName("min-length")]
		public int MinSearchLength { get; set; }

		/// <summary>
		/// Get or set the search timeout (in ms) for calling the datasource.
		/// </summary>
		[HtmlAttributeName("timeout")]
		public int SearchTimeout { get; set; }

		/// <summary>
		/// Get or set the selection mode of the data grid.
		/// </summary>
		[HtmlAttributeName("select")]
		public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

		/// <summary>
		/// Get or set the action to be executed when the selection changes in selection mode.
		/// </summary>
		[HtmlAttributeName("onselect")]
		public string OnSelectionChanged { get; set; }

		/// <summary>
		/// Get or set the action to be executed when the selection changes.
		/// </summary>
		[HtmlAttributeName("onchange")]
		public string OnChangeAction { get; set; }

		/// <summary>
		/// Get or set the action to be executed when the selection changes.
		/// </summary>
		[HtmlAttributeName("onchange2")]
		public JS OnChangeAction2 { get; set; }

		/// <summary>
		/// Get or set the action to be executed when the selection changes.
		/// </summary>
		[HtmlAttributeName("onchange3")]
		public RazorBlock OnChangeAction3 { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a row is being inserted.
		/// This method is also called when <see cref="DataSourceTagHelperBase.OnInserting"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("onrowinserting")]
		public string OnRowInserting { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a row has been inserted.
		/// This method is also called when <see cref="DataSourceTagHelperBase.OnInserted"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("onrowinserted")]
		public string OnRowInserted { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when the datagrid has been initialised
		/// </summary>
		[HtmlAttributeName("oninitialized")]
		public string OnInitializedAction { get; set; }

		/// <summary>
		/// An expression to be evaluated against the current model.
		/// </summary>
		[HtmlAttributeName("asp-for")]
		public ModelExpression For { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: data source
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the record type used in this data grid.
		/// </summary>
		[HtmlAttributeName("record-type")]
		public Type RecordType { get; set; }

		#endregion
	}
}
