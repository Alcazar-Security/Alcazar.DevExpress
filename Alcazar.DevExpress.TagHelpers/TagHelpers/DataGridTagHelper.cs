using Alcazar.DevExpress.Utilities;
using Amaqele.Common.Base;
using Amaqele.Common.Types;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NLog.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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

			// Filter
			builder = builder.FilterRow(f => f.Visible(IsFilterVisible));
			builder = builder.HeaderFilter(f => f.Visible(IsHeaderFilterVisible));
			//builder = builder.FilterBuilder(fb => fb.Option("", ""));

			// Sorting
			builder = builder.Sorting(s => s
				.Mode(SortingMode)
				.ShowSortIndexes(true));

			// Paging
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
				editing = editing
					.Mode(EditMode)
					.UseIcons(UseIcons)
					.NewRowPosition(NewRowPosition);

				if (!string.IsNullOrEmpty(InsertAction) || !string.IsNullOrEmpty(OnInserting) || !string.IsNullOrEmpty(OnInserted))
					editing.AllowAdding(true);
				if (!string.IsNullOrEmpty(UpdateAction) || !string.IsNullOrEmpty(OnUpdating) || !string.IsNullOrEmpty(OnUpdated))
					editing.AllowUpdating(true);
				if (!string.IsNullOrEmpty(DeleteAction) || !string.IsNullOrEmpty(OnRemoving) || !string.IsNullOrEmpty(OnRemoved))
					editing.AllowDeleting(true);
			});

			// Master/detail
			bool isDetail = !string.IsNullOrEmpty(DetailTemplate) || !string.IsNullOrEmpty(DetailTemplateJS) || !string.IsNullOrEmpty(DetailTemplateNT) || DetailTemplateRZ != null;
			if (isDetail)
			{
				builder = builder.MasterDetail(md =>
				{
					md = md.Enabled(IsDetailEnabled);

					if (!string.IsNullOrEmpty(DetailTemplate))
						md = md.Template(DetailTemplate);
					else if (!string.IsNullOrEmpty(DetailTemplateJS))
						md = md.Template(new JS(DetailTemplateJS));
					else if (DetailTemplateRZ != null)
						md = md.Template(DetailTemplateRZ);
					else if (!string.IsNullOrEmpty(DetailTemplateNT))
						md = md.Template(new TemplateName(DetailTemplateNT));

					md = md.AutoExpandAll(IsAutoExpandAll);
				});
			}

			// Process exporting functionality
			builder = ProcessExport(builder);

			// Data grid events
			builder = ProcessEvents(builder);

			// Create the context, so that we can pass it to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ColumnsContext columnContext = GetOrCreateContext<ColumnsContext>(context);

			// Process children of the data grid tag
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
					builder = builder.RemoteOperations(c =>
					{
						c.Filtering(true);              // MVC always does remote filtering
						c.Grouping(true);               // MVC always does remote grouping
						c.Paging(IsRemotePaging);       // MVC might do remote paging
						c.Sorting(false);               // MVC never does remote sorting
					});

					break;
			}

			if (columnContext.Columns.Any())
			{
				// Add columns, if we have some
				builder = builder.Columns(async c => await ProcessColumnsAsync<T>(c, context, output, columnContext.Columns));

				// Add summaries for columns
				builder = builder.Summary(s => ProcessSummaries<T>(s, context, output, columnContext.Columns));
			}

			// Event handlers
			if (!string.IsNullOrEmpty(OnInitialised))
				builder = builder.OnInitialized(OnInitialised);

			if (!string.IsNullOrEmpty(OnEditorPreparing))
				builder = builder.OnEditorPreparing(OnEditorPreparing);

			// Build the toolbar
			// The location of the toolbar is not changable, DX says:
			// The data grid does not provide an option for the toolbar position.
			// You might want to add a separate toolbar widget under your grid and populate it with desired controls. Samples are available in our Toolbar documentation.

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
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		private DataGridBuilder<T> ProcessExport<T>(DataGridBuilder<T> builder)
		{
			if (!string.IsNullOrEmpty(OnExporting))
			{
				builder = builder.Export(e => e
					.Enabled(true)
					.AllowExportSelectedData(AllowExportSelectedData)
					.Formats(ExportFormats));

				builder = builder.OnExporting(OnExporting);
			}

			return builder;
		}

		private DataGridBuilder<T> ProcessEvents<T>(DataGridBuilder<T> builder)
		{
			// Content events
			if (!string.IsNullOrEmpty(OnContentReady))
				builder = builder.OnContentReady(OnContentReady);

			// CRUD events
			if (!string.IsNullOrEmpty(OnInitNewRow))
				builder = builder.OnInitNewRow(OnInitNewRow);
			if (!string.IsNullOrEmpty(OnRowInserting))
				builder = builder.OnRowInserting(OnRowInserting);
			if (!string.IsNullOrEmpty(OnRowInserted))
				builder = builder.OnRowInserted(OnRowInserted);

			if (!string.IsNullOrEmpty(OnRowUpdating))
				builder = builder.OnRowUpdating(OnRowUpdating);
			if (!string.IsNullOrEmpty(OnRowUpdated))
				builder = builder.OnRowUpdated(OnRowUpdated);

			if (!string.IsNullOrEmpty(OnRowRemoving))
				builder = builder.OnRowRemoving(OnRowRemoving);
			if (!string.IsNullOrEmpty(OnRowRemoved))
				builder = builder.OnRowRemoved(OnRowRemoved);

			// CRUD events
			if (!string.IsNullOrEmpty(OnEditorPrepared))
				builder = builder.OnEditorPrepared(OnEditorPrepared);
			if (!string.IsNullOrEmpty(OnEditingStart))
				builder = builder.OnEditingStart(OnEditingStart);

			if (!string.IsNullOrEmpty(OnSaving))
				builder = builder.OnSaving(OnSaving);
			if (!string.IsNullOrEmpty(OnSaved))
				builder = builder.OnSaved(OnSaved);

			// Exporting - hendled by ProcessExporting

			// Copilot added events
			if (!string.IsNullOrEmpty(OnCellClick))
				builder = builder.OnCellClick(OnCellClick);
			if (!string.IsNullOrEmpty(OnCellDblClick))
				builder = builder.OnCellDblClick(OnCellDblClick);
			if (!string.IsNullOrEmpty(OnCellPrepared))
				builder = builder.OnCellPrepared(OnCellPrepared);

			if (!string.IsNullOrEmpty(OnRowClick))
				builder = builder.OnRowClick(OnRowClick);
			if (!string.IsNullOrEmpty(OnRowDblClick))
				builder = builder.OnRowDblClick(OnRowDblClick);
			if (!string.IsNullOrEmpty(OnRowPrepared))
				builder = builder.OnRowPrepared(OnRowPrepared);

			if (!string.IsNullOrEmpty(OnFocusedCellChanged))
				builder = builder.OnFocusedCellChanged(OnFocusedCellChanged);
			if (!string.IsNullOrEmpty(OnFocusedRowChanged))
				builder = builder.OnFocusedRowChanged(OnFocusedRowChanged);

			if (!string.IsNullOrEmpty(OnOptionChanged))
				builder = builder.OnOptionChanged(OnOptionChanged);
			if (!string.IsNullOrEmpty(OnToolbarPreparing))
				builder = builder.OnToolbarPreparing(OnToolbarPreparing);

			if (!string.IsNullOrEmpty(OnDataErrorOccurred))
				builder = builder.OnDataErrorOccurred(OnDataErrorOccurred);

			if (!string.IsNullOrEmpty(OnContextMenuPreparing))
				builder = builder.OnContextMenuPreparing(OnContextMenuPreparing);

			return builder;
		}

		private string DetermineColumnFormat(ColumnModel col)
		{
			// Parameters which influence formatting
			GridColumnDataType? columnDataType = col.DataType;
			Format? columnFormat = col.Format;
			string columnCustomFormat = col.CustomFormat;

			if(!string.IsNullOrEmpty(columnCustomFormat))
			{
				// 1. Specified custom format
				return columnCustomFormat;
			}
			else
			{
				Format? format = null;
				if (columnFormat.HasValue)
				{
					// 2. Specified format
					format = columnFormat.Value;
				}
				else if (columnDataType.HasValue)
				{
					// 3. Format based on the data type
					format = DxCultureUtilities.ToFormat(columnDataType.Value);
				}

				if (format.HasValue)
				{
					// TODO - when the thread culture on startup differs from the broswer culture, then we can an unmodified culture here, and culture options do not kick in. this needs some work.
					// We have determined the format of the content, translate it into the current culture
					return DxCultureUtilities.GetRequestCultureFormat(ViewContext, format.Value);
				}
			}

			return null;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper methods: columns
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		private async Task ProcessColumnsAsync<T>(CollectionFactory<DataGridColumnBuilder<T>> factory, TagHelperContext context, TagHelperOutput output, IEnumerable<ColumnModel> columns)
		{
			foreach (ColumnModel column in columns)
			{
				switch (column.Type)
				{
					// A data column displays a property of the model
					default:
					case "data":
						ProcessDataColumn<T>(factory, column);
						break;

					// A command column displays command buttons which act on the model which is displayed in this row
					case "command":
						await ProcessCommandColumnAsync<T>(context, output, factory, column);
						break;
				}
			}
		}

		private CollectionFactory<DataGridColumnBuilder<T>> ProcessDataColumn<T>(CollectionFactory<DataGridColumnBuilder<T>> columns, ColumnModel column)
		{
			// Column header
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

				// Set the data type to the default value for the model type, if it has not been explicitely set
				if (!column.DataType.HasValue)
					column.DataType = ToDataType(column.For.Metadata.ModelType);
			}

			// DataGridColumnBuilder<T> builder2 = columns.AddFor(column.For);

			DataGridColumnBuilder<T> builder = columns.Add()
				//.Name(column.Name)
				.DataField(column.Name)
				.Caption(column.Label)
				.Alignment(column.Alignment)
				.AllowEditing(!column.IsReadonly);

			// Data type
			if (column.DataType.HasValue)
				builder = builder.DataType(column.DataType.Value);

			// Visibility
			if (!string.IsNullOrEmpty(column.IsVisibleAction))
				builder = builder.Visible(new JS(column.IsVisibleAction));
			else
				builder = builder.Visible(column.IsVisible);

			// Fixed
			if (column.FixedPosition.HasValue)
				builder = builder.FixedPosition(column.FixedPosition.Value);

			// Filtering
			if (column.FilterType.HasValue)
			{
				builder = builder
					.AllowFiltering(true)
					.FilterType(column.FilterType.Value);

				if (column.FilterOperation.HasValue)
				{
					builder = builder
						.SelectedFilterOperation(column.FilterOperation.Value)
						.FilterValue(column.FilterValue);
				}
			}
			else
			{
				builder = builder
					.AllowFiltering(false);
			}

			// Sorting
			builder = builder.AllowSorting(column.IsSorting);
			if (column.IsSorting)
			{
				if (column.SortIndex > 0)
				{
					builder = builder.SortIndex(column.SortIndex);

					// If we have a sort index, we must have a sort order
					if (column.SortOrder == null)
						column.SortOrder = SortOrder.Asc;
				}

				if (column.SortOrder.HasValue)
					builder = builder.SortOrder(column.SortOrder.Value);

				if (!string.IsNullOrEmpty(column.SortingMethod))
					builder = builder.SortingMethod(column.SortingMethod);

				if (!string.IsNullOrEmpty(column.SortValue))
					builder = builder.CalculateSortValue(column.SortValue);
			}

			// Column and grid layout
			if (!string.IsNullOrEmpty(column.Width))
				builder = builder.Width(column.Width);

			// Editing of a column
			if (column.LookupDatasource != null)
			{
				//builder.EditorOptions(?);
				//builder.ShowEditorAlways(true);
				builder = builder.Lookup(lookup =>
				{
					// Process the (column) data source
					lookup = lookup.DataSource(d => column.LookupDatasource.BuildDatasource(d));

					// Not supported for trees? lookup = lookup.Grouped(true);
					lookup = lookup.DataSourceOptions(o => o.Group(column.GroupExpression).Sort(config => config.AddSorting(column.DisplayExpression)));
					lookup = lookup.ValueExpr(column.ValueExpression);
					lookup = lookup.DisplayExpr(column.DisplayExpression);
				});
			}

			// Set the cell value
			// Thought we need that for cascading editing, but it didnt work
			if (!string.IsNullOrEmpty(column.SetCellValue))
				builder = builder.SetCellValue(column.SetCellValue);

			// Column formatting
			// 1. Specified custom format
			// 2. Specified format
			// 3. Format based on the data type
			column.CustomFormat = DetermineColumnFormat(column);
			if (!string.IsNullOrEmpty(column.CustomFormat))
				builder = builder.Format(column.CustomFormat);

			// Column styling
			if (!string.IsNullOrEmpty(column.CssClass))
				builder = builder.CssClass(column.CssClass);

			// Set the edit templates
			if (!string.IsNullOrEmpty(column.EditTemplate))
				builder = builder.EditCellTemplate(column.EditTemplate);
			else if (!string.IsNullOrEmpty(column.EditTemplateJS))
				builder = builder.EditCellTemplate(new JS(column.EditTemplateJS));
			else if (column.EditTemplateRZ != null)
				builder = builder.EditCellTemplate(column.EditTemplateRZ);
			else if (!string.IsNullOrEmpty(column.EditTemplateNT))
				builder = builder.EditCellTemplate(new TemplateName(column.EditTemplateNT));

			// Set the child content or templates
			if (!string.IsNullOrWhiteSpace(column.Content))
				builder = builder.CellTemplate(column.Content);
			else if (!string.IsNullOrWhiteSpace(column.ContentJS))
				builder = builder.CellTemplate(new JS(column.ContentJS));
			else if (column.ContentRZ != null)
				builder = builder.CellTemplate(column.ContentRZ);
			else if (!string.IsNullOrEmpty(column.ContentNT))
				builder = builder.CellTemplate(new TemplateName(column.ContentNT));

			// Column events
			// There are no events on the column
			return columns;
		}

		private async Task<CollectionFactory<DataGridColumnBuilder<T>>> ProcessCommandColumnAsync<T>(TagHelperContext context, TagHelperOutput output, CollectionFactory<DataGridColumnBuilder<T>> columns, ColumnModel column)
		{
			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			DataGridColumnBuilder<T> builder = columns.Add()
				.Type(column.CommandType)
				.Caption(column.Label);

			if (column.Index.HasValue)
				builder = builder.VisibleIndex(column.Index.Value);

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
					// Add the edit button, if there is an update action
					if (!string.IsNullOrEmpty(UpdateAction))
						buttons.Add().Name("edit");

					// Add the delete button, if there is a delete action
					if (!string.IsNullOrEmpty(DeleteAction))
						buttons.Add().Name("delete");

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
			else if (!string.IsNullOrWhiteSpace(column.ContentNT))
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

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper methods: totals
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		private DataGridSummaryBuilder<T> ProcessSummaries<T>(DataGridSummaryBuilder<T> builder, TagHelperContext context, TagHelperOutput output, IEnumerable<ColumnModel> columns)
		{
			// Recalculate while editing
			builder = builder
				.RecalculateWhileEditing(IsRecalculateWhileEditing)
				.SkipEmptyValues(true);

			if (!string.IsNullOrEmpty(CalculateCustomSummary))
				builder = builder.CalculateCustomSummary(CalculateCustomSummary);

			// Grand totals
			builder = builder.TotalItems(c => ProcessSummaryColumns(c, context, output, columns));

			// Group totals TODO
			// builder = builder.GroupItems(c => ProcessGroupColumns(c, context, output, columns));

			return builder;
		}

		private void ProcessSummaryColumns<T>(CollectionFactory<DataGridSummaryTotalItemBuilder<T>> factory, TagHelperContext context, TagHelperOutput output, IEnumerable<ColumnModel> columns)
		{
			foreach (ColumnModel column in columns)
			{
				switch (column.Type)
				{
					// A data column displays a property of the model
					default:
					case "data":
						ProcessSummaryColumn<T>(factory, column);
						break;

					// A command column displays command buttons which act on the model which is displayed in this row
					case "command":
						break;
				}
			}
		}

		private DataGridSummaryTotalItemBuilder<T> ProcessSummaryColumn<T>(CollectionFactory<DataGridSummaryTotalItemBuilder<T>> factory, ColumnModel column)
		{
			if (column.SummaryType != null)
			{
				DataGridSummaryTotalItemBuilder<T> builder = factory.Add();

				// The name is mandatory, it links the total definition to the column
				// if (column.For != null)
				//	factory.AddFor(column.For);

				// Associate with the column
				builder = builder
					.Name(column.Name)
					.ShowInColumn(column.Name);

				builder = builder.SummaryType(column.SummaryType.Value);

				if (!string.IsNullOrEmpty(column.CustomizeText))
					builder = builder.CustomizeText(column.CustomizeText);

				// The column builder has already set the custom format
				// column.CustomFormat = DetermineColumnFormat(column);
				if (!string.IsNullOrEmpty(column.CustomFormat))
					builder = builder.ValueFormat(column.CustomFormat);

				return builder;
			}

			return null;
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
		/// An expression to be evaluated against the current model.
		/// </summary>
		[HtmlAttributeName("asp-for")]
		public ModelExpression For { get; set; }

		/// <summary>
		/// Get or set a sequence of allowed export formats.
		/// </summary>
		[HtmlAttributeName("export-formats")]
		public IEnumerable<DataGridExportFormat> ExportFormats { get; set; }

		/// <summary>
		/// Get or set an indicator if exporting selected data is allowed.
		/// </summary>
		[HtmlAttributeName("export-selected")]
		public bool AllowExportSelectedData { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: editing
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the edit mode to be used for this control.
		/// Defaults to <see cref="GridEditMode.Row"/>
		/// </summary>
		[HtmlAttributeName("edit-mode")]
		public GridEditMode EditMode { get; set; } = GridEditMode.Row;

		/// <summary>
		/// Get or set an indicator if icons should be used for the edit mode for this control.
		/// </summary>
		[HtmlAttributeName("edit-icons")]
		public bool UseIcons { get; set; } = true;

		/// <summary>
		/// Get or set the position of a new row in edit mode for this control.
		/// </summary>
		[HtmlAttributeName("edit-row-pos")]
		public GridNewRowPosition NewRowPosition { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: filter and sorting
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set an indicator the filter should be visible.
		/// </summary>
		[HtmlAttributeName("filter")]
		public bool IsFilterVisible { get; set; } = true;

		/// <summary>
		/// Get or set an indicator the header filter should be visible.
		/// </summary>
		[HtmlAttributeName("header-filter")]
		public bool IsHeaderFilterVisible { get; set; } = true;

		/// <summary>
		/// Get or set the sorting mode for this control. Defaults to <see cref="GridSortingMode.Multiple"/>.
		/// </summary>
		[HtmlAttributeName("sorting")]
		public GridSortingMode SortingMode { get; set; } = GridSortingMode.Multiple;

		/// <summary>
		/// Get or set an indicator if totals hould be recalculated while editing
		/// </summary>
		[HtmlAttributeName("recalc")]
		public bool IsRecalculateWhileEditing { get; set; }

		/// <summary>
		/// Get or set the JS method to use to calculate a summary in a custom way.
		/// </summary>
		[HtmlAttributeName("calc")]
		public string CalculateCustomSummary { get; set; }
		
		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: master/detail
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set an indicator if master/detail mode of this data grid is enabled.
		/// Set this to false when using the Master/Detail API.
		/// </summary>
		[HtmlAttributeName("detail-enabled")]
		public bool IsDetailEnabled { get; set; } = true;

		/// <summary>
		/// Get or set the (string) master/detail template of this data grid.
		/// </summary>
		[HtmlAttributeName("detail-template")]
		public string DetailTemplate { get; set; }

		/// <summary>
		/// Get or set the JS master/detail template of this data grid.
		/// </summary>
		[HtmlAttributeName("detail-template-js")]
		public string DetailTemplateJS { get; set; }

		/// <summary>
		/// Get or set the RazorBlock master/detail template of this data grid.
		/// </summary>
		[HtmlAttributeName("detail-template-rz")]
		public RazorBlock DetailTemplateRZ { get; set; }

		/// <summary>
		/// Get or set the named master/detail template of this data grid.
		/// </summary>
		[HtmlAttributeName("detail-template-nt")]
		public string DetailTemplateNT { get; set; }

		/// <summary>
		/// Get or set an indicator if all master/details should be expanded.
		/// </summary>
		[HtmlAttributeName("auto-expand")]
		public bool IsAutoExpandAll { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: tag helper events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the action to be executed when the content is ready.
		/// </summary>
		[HtmlAttributeName("content-ready")]
		public string OnContentReady { get; set; }

		/// <summary>
		/// Get or set the action to be executed when the selection changes in selection mode.
		/// </summary>
		[HtmlAttributeName("selection-changed")]
		public string OnSelectionChanged { get; set; }

		/// <summary>
		/// Get or set the action to be executed when an editor is preparing.
		/// </summary>
		[HtmlAttributeName("editor-preparing")]
		public string OnEditorPreparing { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a new row is initialised.
		/// </summary>
		[HtmlAttributeName("row-init")]
		public string OnInitNewRow { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a row is being inserted.
		/// This method is also called when <see cref="DataSourceTagHelperBase.OnInserting"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("row-inserting")]
		public string OnRowInserting { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a row has been inserted.
		/// This method is also called when <see cref="DataSourceTagHelperBase.OnInserted"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("row-inserted")]
		public string OnRowInserted { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a row is being updated.
		/// This method is also called when <see cref="DataSourceTagHelperBase.OnUpdating"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("row-updating")]
		public string OnRowUpdating { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a row has been updated.
		/// This method is also called when <see cref="DataSourceTagHelperBase.OnUpdated"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("row-updated")]
		public string OnRowUpdated { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a row is being removed.
		/// This method is also called when <see cref="DataSourceTagHelperBase.Onemoving"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("row-removing")]
		public string OnRowRemoving { get; set; }

		/// <summary>
		/// Get or set the action to be executed when a row has been removed.
		/// This method is also called when <see cref="DataSourceTagHelperBase.Onemoved"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("row-removed")]
		public string OnRowRemoved { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when the datagrid has been initialised
		/// </summary>
		[HtmlAttributeName("initialised")]
		public string OnInitialised { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a data grid editor has been prepared.
		/// </summary>
		[HtmlAttributeName("editor-prepared")]
		public string OnEditorPrepared { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when editing starts.
		/// </summary>
		[HtmlAttributeName("editing-start")]
		public string OnEditingStart { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when saving.
		/// </summary>
		[HtmlAttributeName("saving")]
		public string OnSaving { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when saving.
		/// </summary>
		[HtmlAttributeName("saved")]
		public string OnSaved { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when exportinf to PDF or XLSX.
		/// </summary>
		[HtmlAttributeName("exporting")]
		public string OnExporting { get; set; }

		// Add these properties to the "DataGridTagHelper properties: tag helper events" region

		/// <summary>
		/// Get or set the JS method to be executed when a cell is clicked.
		/// </summary>
		[HtmlAttributeName("cell-click")]
		public string OnCellClick { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a cell is double-clicked.
		/// </summary>
		[HtmlAttributeName("cell-dbl-click")]
		public string OnCellDblClick { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a cell is prepared.
		/// </summary>
		[HtmlAttributeName("cell-prepared")]
		public string OnCellPrepared { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a row is clicked.
		/// </summary>
		[HtmlAttributeName("row-click")]
		public string OnRowClick { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a row is double-clicked.
		/// </summary>
		[HtmlAttributeName("row-dbl-click")]
		public string OnRowDblClick { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a row is prepared.
		/// </summary>
		[HtmlAttributeName("row-prepared")]
		public string OnRowPrepared { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when the focused cell changes.
		/// </summary>
		[HtmlAttributeName("focused-cell-changed")]
		public string OnFocusedCellChanged { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when the focused row changes.
		/// </summary>
		[HtmlAttributeName("focused-row-changed")]
		public string OnFocusedRowChanged { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when an option is changed.
		/// </summary>
		[HtmlAttributeName("option-changed")]
		public string OnOptionChanged { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when the toolbar is being prepared.
		/// </summary>
		[HtmlAttributeName("toolbar-preparing")]
		public string OnToolbarPreparing { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a data error occurs.
		/// </summary>
		[HtmlAttributeName("error-occurred")]
		public string OnDataErrorOccurred { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when the context menu is being prepared.
		/// </summary>
		[HtmlAttributeName("context-menu-preparing")]
		public string OnContextMenuPreparing { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: data source
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set if remote paging should be used. Not all data source types will adhere to thi parameter.
		/// </summary>
		[HtmlAttributeName("remote-paging")]
		public bool IsRemotePaging { get; set; }

		/// <summary>
		/// Get or set the record type used in this data grid.
		/// </summary>
		[HtmlAttributeName("record-type")]
		public Type RecordType { get; set; }

		#endregion
	}
}
