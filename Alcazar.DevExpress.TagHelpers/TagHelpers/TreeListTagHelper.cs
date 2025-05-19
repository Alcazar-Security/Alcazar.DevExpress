using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using System.Reflection;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Factories;
using DevExpress.Data.Helpers;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="TreeListTagHelper"/> type implements a tree control, which displays a tree in list format.
	/// </summary>
	[HtmlTargetElement("dx-treelist")]
	public class TreeListTagHelper : ListControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TreeListTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public TreeListTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TreeListTagHelper overrides
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
			MethodInfo method = typeof(TreeListTagHelper).GetMethod(nameof(BuildTreeListAsync), BindingFlags.Instance | BindingFlags.NonPublic);
			if (method.IsGenericMethod)
				method = method.MakeGenericMethod(RecordType);

			// Configure the builder
			Task<IHtmlContent> result = (Task<IHtmlContent>)method.Invoke(this, new object[] { context, output });
			IHtmlContent content = result.GetAwaiter().GetResult();

			// Render the builder (into the POST content)
			output.Content.SetHtmlContent(content);
		}

		/// <summary>
		/// Build the tree list. 
		/// This method is dynamically generated with a generic type argument and then executed.
		/// </summary>
		private async Task<IHtmlContent> BuildTreeListAsync<T>(TagHelperContext context, TagHelperOutput output)
		{
			// Create the builder for a popup
			TreeListBuilder<T> builder = _htmlHelper.DevExtreme().TreeList<T>();

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
					.Mode(EditMode)
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
			if (!string.IsNullOrEmpty(OnInitNewRow))
				builder = builder.OnInitNewRow(OnInitNewRow);

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
								// TODO LATER
								//case "command":
								//	await ProcessCommandColumnAsync<T>(context, output, columns, column);
								//	break;
						}
					}
				});
			}

			// Set state-storing
			if (!string.IsNullOrEmpty(StorageKey))
			{
				builder.StateStoring(s => s
					.Enabled(true)
					.Type(StateStoringType.SessionStorage)
					.StorageKey(StorageKey)
				);
			}

			// Set tree hierarchy properties
			builder = builder.ParentIdExpr(ParentIDExpr);
			builder = builder.HasItemsExpr(HasItemsExpr);
			builder = builder.RootValue(RootValue);

			return builder;
		}

		private TreeListBuilder<T> ProcessCommon<T>(TreeListBuilder<T> builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private TreeListBuilder<T> ProcessAttributes<T>(TreeListBuilder<T> builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value.ToString());

			return builder;
		}

		private CollectionFactory<TreeListColumnBuilder<T>> ProcessDataColumn<T>(CollectionFactory<TreeListColumnBuilder<T>> columns, ColumnModel column)
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

			TreeListColumnBuilder<T> builder = columns.Add()
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

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TreeListTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the selection mode of the tree list.
		/// </summary>
		[HtmlAttributeName("select")]
		public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

		/// <summary>
		/// Get or set the edit mode to be used for this control.
		/// Defaults to <see cref="GridEditMode.Row"/>
		/// </summary>
		[HtmlAttributeName("edit-mode")]
		public GridEditMode EditMode { get; set; } = GridEditMode.Row;

		/// <summary>
		/// Get or set the expression for the parent ID.
		/// </summary>
		[HtmlAttributeName("parent-expr")]
		public string ParentIDExpr { get; set; }

		/// <summary>
		/// Get or set the expression for the has-items indicator.
		/// </summary>
		[HtmlAttributeName("items-expr")]
		public string HasItemsExpr { get; set; }

		/// <summary>
		/// Get or set the root value for the parent ID.
		/// This is the value of the parent ID when starting at root level.
		/// </summary>
		[HtmlAttributeName("root")]
		public string RootValue { get; set; }

		/// <summary>
		/// Get or set the key for state persistence storage.
		/// IF this value is set, state persistence is enabled and defaults to session storage.
		/// </summary>
		[HtmlAttributeName("storage")]
		public string StorageKey { get; set; }

		/// <summary>
		/// Get or set the action to be executed when the selection changes in selection mode.
		/// </summary>
		[HtmlAttributeName("onselect")]
		public string OnSelectionChanged { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a row is being inserted.
		/// This method is also called when <see cref="DataSourceTagHelperBase.OnInserting"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("onrowinserting")]
		public string OnRowInserting { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a row has been inserted.
		/// This method is also called when <see cref="DataSourceTagHelperBase.OnInserted"/> for an array datasource is called.
		/// </summary>
		[HtmlAttributeName("onrowinserted")]
		public string OnRowInserted { get; set; }

		/// <summary>
		/// Get or set the JS method to be executed when a new row must be initialised.
		/// </summary>
		[HtmlAttributeName("oninitnew")]
		public string OnInitNewRow { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TreeListTagHelper properties: data source
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the record type used in this data grid.
		/// </summary>
		[HtmlAttributeName("record-type")]
		public Type RecordType { get; set; }

		#endregion
	}
}