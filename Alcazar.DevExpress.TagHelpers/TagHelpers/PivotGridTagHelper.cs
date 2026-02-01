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
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;
using System;
using System.Collections;
using System.Collections.Generic;
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
				.Texts(t => t
					.RowFields("Drag fields here xx")
					.AllFields("Search fields xx"))
				.AllowSearch(AllowSearch)
				.ApplyChangesMode(ApplyChangesMode.Instantly)
				.Layout(PivotGridFieldChooserLayout.Layout1));

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

			// Set the data source
			if (sourceContext.Datasource != null)
			{
				// First preference, process the (child) data source
				builder = builder.DataSource(b => b
					.Store(f => sourceContext.Datasource.BuildDatasource(f))
						.Fields(c => ProcessFieldColumns(c, columnContext.Columns))
					)
				    .FieldChooser(c => c.Height(500));
			}
			else
			{
				// Second preference, process the datasource from my own properties
				builder = builder.DataSource(b => b
					.Store(f => BuildDatasource(f))
						.Fields(c => ProcessFieldColumns(c, columnContext.Columns))
					)
					.FieldChooser(c => c.Height(500));
			}

			if (ShowRowFields || ShowColumnFields || ShowDataFields || ShowFilterFields)
			{
				builder = builder.FieldPanel(p => p
					.ShowRowFields(ShowRowFields)
					.ShowColumnFields(ShowColumnFields)
					.ShowDataFields(ShowDataFields)
					.ShowFilterFields(ShowFilterFields)
					.AllowFieldDragging(AllowFieldDragging)
					.Visible(true));
			}

			// Store state
			if (!string.IsNullOrEmpty(State))
			{
				builder = builder.StateStoring(s => s
					.Enabled(true)
					.Type(StateStoringType.LocalStorage)
					.StorageKey("dx-widget-gallery-pivotgrid-storing"));
			}

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

		private void ProcessFieldColumns<T>(CollectionFactory<PivotGridDataSourceFieldBuilder<T>> factory, IEnumerable<ColumnModel> columns)
		{
			foreach (ColumnModel column in columns)
			{
				switch (column.Type)
				{
					// A field column places a field into the pivot table
					default:
					case "field":
					case "filter":
						ProcessFieldColumn<T>(factory, column);
						break;
				}
			}
		}

		private CollectionFactory<PivotGridDataSourceFieldBuilder<T>> ProcessFieldColumn<T>(CollectionFactory<PivotGridDataSourceFieldBuilder<T>> factory, ColumnModel column)
		{
			// Column header
			if (column.For != null)
			{
				ProcessColumnFor(column);
			}

			PivotGridDataSourceFieldBuilder<T> builder = factory.Add()
				.DataField(column.Name)
				.Caption(column.Label);

			if (column.DoubleWidth > 0)
				builder = builder.Width(column.DoubleWidth);

			// Data type
			else if (column.PivotDataType.HasValue)
				builder = builder.DataType(column.PivotDataType.Value);

			// Visibility
			if (!string.IsNullOrEmpty(column.IsVisibleAction))
				builder = builder.Visible(new JS(column.IsVisibleAction));
			else
				builder = builder.Visible(column.IsVisible);

			// Filtering
			builder = ProcessColumnFiltering(column, builder);

			// Sorting
			builder = ProcessColumnSorting(column, builder);

			// Area and expansion
			builder = builder.Area(column.PivotArea);
			if (column.AreaIndex.HasValue)
				builder = builder.AreaIndex(column.AreaIndex.Value);

			// Although DX uses count as default, we do sum as default
			// Somehow this also works for the field-chooser, not sure how
			if (column.PivotArea == PivotGridArea.Data)
				builder.SummaryType(column.SummaryType ?? SummaryType.Sum);

			builder = builder.Expanded(column.IsExpanded);

			// Column formatting
			// 1. Specified custom format
			// 2. Specified format
			// 3. Format based on the data type
			builder = ProcessColumnFormatting(column, builder);

			// Column events
			// There are no events on the column
			return factory;
		}

		private void ProcessColumnFor(ColumnModel column)
		{
			// If we have both FOR and NAME, keep the existing name, this is an override for the DataField() method, where the JSON field and the property name differ due to a [JsonProperty] attribute
			if (string.IsNullOrEmpty(column.Name))
			{
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

			column.PivotDataType = ToPivotDataType(column.For.Metadata.ModelType);
		}

		private PivotGridDataSourceFieldBuilder<T> ProcessColumnFiltering<T>(ColumnModel column, PivotGridDataSourceFieldBuilder<T> builder)
		{
			if (column.FilterType.HasValue)
			{
				builder = builder
					.AllowFiltering(true)
					.FilterType(column.FilterType.Value);
			}
			else
			{
				builder = builder
					.AllowFiltering(false);
			}

			return builder;
		}

		private PivotGridDataSourceFieldBuilder<T> ProcessColumnSorting<T>(ColumnModel column, PivotGridDataSourceFieldBuilder<T> builder)
		{
			builder = builder.AllowSorting(column.IsSorting);
			if (column.IsSorting)
			{
				if (column.SortIndex > 0)
				{
					// If we have a sort index, we must have a sort order
					if (column.SortOrder == null)
						column.SortOrder = SortOrder.Asc;

					builder = builder.SortOrder(column.SortOrder.Value);
				}

				if (!string.IsNullOrEmpty(column.SortingMethod))
				{
					builder = builder.SortingMethod(column.SortingMethod);
				}

				// Sorting by summary
				builder = builder.AllowSortingBySummary(column.AllowSortingBySummary);
				if (!string.IsNullOrEmpty(column.SortBySummaryField))
					builder = builder.SortBySummaryField(column.SortBySummaryField);
				if (column.SortBySummaryPath != null)
					builder = builder.SortBySummaryPath(column.SortBySummaryPath);
			}

			return builder;
		}

		private PivotGridDataSourceFieldBuilder<T> ProcessColumnFormatting<T>(ColumnModel column, PivotGridDataSourceFieldBuilder<T> builder)
		{
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

			return builder;
		}

		protected PivotGridDataType? ToPivotDataType(Type type)
		{
			PrimitiveTypeCode code = PrimitiveType.FromNullableType(type);
			switch (code)
			{
				default:
				case PrimitiveTypeCode.None: return null;

				case PrimitiveTypeCode.Int8:
				case PrimitiveTypeCode.UInt8:
				case PrimitiveTypeCode.Int16:
				case PrimitiveTypeCode.UInt16:
				case PrimitiveTypeCode.Int32:
				case PrimitiveTypeCode.UInt32:
				case PrimitiveTypeCode.Int64:
				case PrimitiveTypeCode.UInt64:
				case PrimitiveTypeCode.Float:
				case PrimitiveTypeCode.Double:
				case PrimitiveTypeCode.Decimal: return PivotGridDataType.Number;
				case PrimitiveTypeCode.Enumeration: return PivotGridDataType.String;

				case PrimitiveTypeCode.Bool: return PivotGridDataType.String;

				case PrimitiveTypeCode.Char: return PivotGridDataType.String;
				case PrimitiveTypeCode.String: return PivotGridDataType.String;
				case PrimitiveTypeCode.Binary: return PivotGridDataType.String;
				case PrimitiveTypeCode.Base64Binary: return PivotGridDataType.String;
				case PrimitiveTypeCode.HexBinary: return PivotGridDataType.String;
				case PrimitiveTypeCode.Guid: return PivotGridDataType.String;

				case PrimitiveTypeCode.Date: return PivotGridDataType.Date;

				case PrimitiveTypeCode.Time:
				case PrimitiveTypeCode.Timestamp:
				case PrimitiveTypeCode.Timespan: return PivotGridDataType.String;

				case PrimitiveTypeCode.Object: return PivotGridDataType.String;
			}
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
		#region DataGridTagHelper properties: pivot
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set an indicator if row fields should be shown in the field panel.
		/// </summary>
		[HtmlAttributeName("row-fields")]
		public bool ShowRowFields { get; set; }

		/// <summary>
		/// Get or set an indicator if column fields should be shown in the field panel.
		/// </summary>
		[HtmlAttributeName("column-fields")]
		public bool ShowColumnFields { get; set; }

		/// <summary>
		/// Get or set an indicator if data (cell) fields should be shown in the field panel.
		/// </summary>
		[HtmlAttributeName("cell-fields")]
		public bool ShowDataFields { get; set; }

		/// <summary>
		/// Get or set an indicator if filter fields should be shown in the field panel.
		/// </summary>
		[HtmlAttributeName("filter-fields")]
		public bool ShowFilterFields { get; set; }

		/// <summary>
		/// Get or set an indicator if fields in the field panel can be dragged.
		/// </summary>
		[HtmlAttributeName("drag-fields")]
		public bool AllowFieldDragging { get; set; }

		/// <summary>
		/// Get or set an indicator if the field chooser allows searching.
		/// </summary>
		[HtmlAttributeName("search")]
		public bool AllowSearch { get; set; }

		/// <summary>
		/// Get or set the storage key of the state store. Defaults to 'dx-pivot-grid-store'.
		/// </summary>
		[HtmlAttributeName("state")]
		public string State { get; set; } = "dx-pivot-grid-store";

		/// <summary>
		/// Get or set the type of store used for storing the state of the pivot grid control. Defaults to <see cref="StateStoringType.LocalStorage"/>.
		/// </summary>
		[HtmlAttributeName("state-type")]
		public StateStoringType StateStoringType { get; set; } = StateStoringType.LocalStorage;

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
