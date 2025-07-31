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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="PivotGridTagHelper"/> type implements a pivot grid.
	/// </summary>
	[HtmlTargetElement("dx-pivotgrid")]
	public class PivotGridTagHelper : ListControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PivotGridTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public PivotGridTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region PivotGridTagHelper overrides
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
			MethodInfo method = typeof(PivotGridTagHelper).GetMethod(nameof(BuildPivotGridAsync), BindingFlags.Instance | BindingFlags.NonPublic);
			if (method.IsGenericMethod)
				method = method.MakeGenericMethod(RecordType);

			// Configure the builder
			Task<IHtmlContent> result = (Task<IHtmlContent>)method.Invoke(this, new object[] { context, output });
			IHtmlContent content = result.GetAwaiter().GetResult();

			// Render the builder (into the POST content)
			output.Content.SetHtmlContent(content);
		}

		/// <summary>
		/// Build the pivot grid. 
		/// This method is dynamically generated with a generic type argument and then executed.
		/// </summary>
		private async Task<IHtmlContent> BuildPivotGridAsync<T>(TagHelperContext context, TagHelperOutput output)
		{
			// Create the builder for a popup
			PivotGridBuilder<T> builder = _htmlHelper.DevExtreme().PivotGrid<T>();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			builder = builder.ShowBorders(false);

			// Filter
			builder = builder.AllowFiltering(AllowFiltering);
			builder = builder.HeaderFilter(f => f.AllowSelectAll(AllowSelectAll));

			// Sorting
			builder = builder
				.AllowSorting(AllowSorting)
				.AllowSortingBySummary(AllowSortingBySummary);

			// Field chooser
			builder = builder.FieldChooser(c => c
				.Enabled(true)
				.Height(400)
				.AllowSearch(false)
				.ApplyChangesMode(ApplyChangesMode.Instantly)
				.Layout(PivotGridFieldChooserLayout.Layout0));

			// Totals
			builder = builder
				.ShowColumnTotals(ShowColumnTotals)
				.ShowColumnGrandTotals(ShowColumnGrandTotals)
				.ShowRowTotals(ShowRowTotals)
				.ShowRowGrandTotals(ShowRowGrandTotals);


			// Process exporting functionality
			builder = ProcessExport(builder);

			// Data grid events
			builder = ProcessEvents(builder);

			// Create the context, so that we can pass it to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ColumnsContext columnContext = GetOrCreateContext<ColumnsContext>(context);

			// Process children of the data grid tag
			IHtmlContent content = await output.GetChildContentAsync();

			builder = builder.DataSource(d => d
				.Store(s => s.Mvc().Controller("PivotGridData").LoadAction("Get")));

			PivotGridStoreFactory d;

			// Set the data source
			if (sourceContext.Datasource != null)
			{
				// First preference, process the (child) data source
				builder = builder.DataSource(b => b.Store(f => sourceContext.Datasource.BuildDatasource(f)));
			}
			else
			{
				// Second preference, process the datasource from my own properties
				builder = builder.DataSource(b => b.Store(f => BuildDatasource(f)));
			}

			// Add columns - seems to be done in the datasource

			// Event handlers
			if (!string.IsNullOrEmpty(OnInitialised))
				builder = builder.OnInitialized(OnInitialised);

			return builder;
		}

		private PivotGridBuilder<T> ProcessCommon<T>(PivotGridBuilder<T> builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private PivotGridBuilder<T> ProcessAttributes<T>(PivotGridBuilder<T> builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		private PivotGridBuilder<T> ProcessExport<T>(PivotGridBuilder<T> builder)
		{
			if (!string.IsNullOrEmpty(OnExporting))
			{
				builder = builder.Export(e => e.Enabled(true));
				builder = builder.OnExporting(OnExporting);
			}

			return builder;
		}

		private PivotGridBuilder<T> ProcessEvents<T>(PivotGridBuilder<T> builder)
		{
			// Content events
			if (!string.IsNullOrEmpty(OnContentReady))
				builder = builder.OnContentReady(OnContentReady);

			// Exporting - hendled by ProcessExporting

			// Copilot added events
			if (!string.IsNullOrEmpty(OnCellClick))
				builder = builder.OnCellClick(OnCellClick);
			if (!string.IsNullOrEmpty(OnCellPrepared))
				builder = builder.OnCellPrepared(OnCellPrepared);

			if (!string.IsNullOrEmpty(OnOptionChanged))
				builder = builder.OnOptionChanged(OnOptionChanged);

			if (!string.IsNullOrEmpty(OnContextMenuPreparing))
				builder = builder.OnContextMenuPreparing(OnContextMenuPreparing);

			return builder;
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

				column.DataType = ToDataType(column.For.Metadata.ModelType);
			}

			DataGridColumnBuilder<T> builder = columns.Add()
				.DataField(column.Name)
				.Caption(column.Label)
				.Alignment(column.Alignment)
				.AllowEditing(!column.IsReadonly);

			if (!string.IsNullOrEmpty(column.IsVisibleAction))
				builder = builder.Visible(new JS(column.IsVisibleAction));
			else
				builder = builder.Visible(column.IsVisible);

			if (column.DataType.HasValue)
				builder = builder.DataType(column.DataType.Value);

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
					//.EditorOptions("");
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
			if (!string.IsNullOrEmpty(column.CustomFormat))
			{
				// 1. Specified custom format
				builder = builder.Format(column.CustomFormat);
			}
			else
			{
				Format? format = null;
				if (column.Format.HasValue)
				{
					// 2. Specified format
					format = column.Format.Value;
				}
				else if (column.DataType.HasValue)
				{
					// 3. Format based on the data type
					format = DxCultureUtilities.ToFormat(column.DataType.Value);
				}

				if (format.HasValue)
				{
					// We have determined the format of the content, translate it into the current culture
					string customFormat = DxCultureUtilities.GetRequestCultureFormat(ViewContext, format.Value);
					if (!string.IsNullOrEmpty(customFormat))
						builder = builder.Format(customFormat);
				}
			}

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
			// Get the context, so that we can use it here
			// ButtonContext buttonContext = GetContextSafe<ButtonContext>(context);

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

		private void todo(PivotGridBuilder<object> builder)
		{
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
		#region PivotGridTagHelper properties: tag helper events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the action to be executed when the content is ready.
		/// </summary>
		[HtmlAttributeName("content-ready")]
		public string OnContentReady { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when the datagrid has been initialised
		/// </summary>
		[HtmlAttributeName("initialised")]
		public string OnInitialised { get; set; }

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
		/// Get or set the JS method to be executed when a cell is prepared.
		/// </summary>
		[HtmlAttributeName("cell-prepared")]
		public string OnCellPrepared { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when an option is changed.
		/// </summary>
		[HtmlAttributeName("option-changed")]
		public string OnOptionChanged { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when the context menu is being prepared.
		/// </summary>
		[HtmlAttributeName("context-menu-preparing")]
		public string OnContextMenuPreparing { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: filter and sorting
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set an indicator if filtering is allowed. Defaults to <see langword="true"/>
		/// </summary>
		[HtmlAttributeName("filter")]
		public bool AllowFiltering { get; set; } = true;

		/// <summary>
		/// Get or set an indicator if selecting all for filtering is allowed. Defaults to <see langword="true"/>
		/// </summary>
		[HtmlAttributeName("select-all")]
		public bool AllowSelectAll { get; set; } = true;

		/// <summary>
		/// Get or set an indicator if sorting is allowed. Defaults to <see langword="true"/>
		/// </summary>
		[HtmlAttributeName("sorting")]
		public bool AllowSorting { get; set; } = true;

		/// <summary>
		/// Get or set an indicator if sorting is allowed.
		/// </summary>
		[HtmlAttributeName("sorting-summary")]
		public bool AllowSortingBySummary { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: filter and sorting
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set an indicator if column totals should be shown.
		/// </summary>
		[HtmlAttributeName("col-totals")]
		public bool ShowColumnTotals { get; set; }

		/// <summary>
		/// Get or set an indicator if column grand totals should be shown.
		/// </summary>
		[HtmlAttributeName("col-grand-totals")]
		public bool ShowColumnGrandTotals { get; set; }

		/// <summary>
		/// Get or set an indicator if row totals should be shown.
		/// </summary>
		[HtmlAttributeName("row-totals")]
		public bool ShowRowTotals { get; set; }

		/// <summary>
		/// Get or set an indicator if row grand totals should be shown.
		/// </summary>
		[HtmlAttributeName("row-grand-totals")]
		public bool ShowRowGrandTotals { get; set; }
		
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
