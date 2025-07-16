using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="ColumnTagHelper"/> tag helper defines columns of a data grid, tree lists, and other users of data sources.
	/// </summary>
	[HtmlTargetElement("column", ParentTag = "dx-datagrid", TagStructure = TagStructure.NormalOrSelfClosing)]
	[HtmlTargetElement("column", ParentTag = "dx-treelist", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ColumnTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the HTML of this tag, it is used for DX button generation only
			output.SuppressOutput();

			// Obtain the context, so that we can use it here
			ColumnsContext columnContext = GetContextSafe<ColumnsContext>(context);

			// Create the contexts, so that we can pass them to child tag helpers
			ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			sourceContext.DataSourceKey = "column-lookup";

			// Process children of the column tag, which becomes the column template
			IHtmlContent content = await output.GetChildContentAsync();

			DataSourceTagHelper datasource = null;
			if (sourceContext.Datasources.ContainsKey("column-lookup"))
				datasource = sourceContext.Datasources["column-lookup"];

			sourceContext.DataSourceKey = null;
			sourceContext.Datasources.Remove("column-lookup");

			string text = ToString(content);

			string label = TranslateToProp(Label, ViewContext);
			string name = TranslateToProp(Name, ViewContext);

			// Construct the column model
			ColumnModel column = new ColumnModel
			{
				// Column type
				Type = Type,
				Index = Index,

				// Data columns
				Label = label,
				Name = name,
				Alignment = Alignment,
				DataType = DataType,
				Width = Width,
				Format = Format,
				CustomFormat = CustomFormat,
				For = For,
				SetCellValue = SetCellValue,

				Content = text,
				ContentJS = ContentJS,
				ContentRZ = ContentRZ,
				ContentNT = ContentNT,
				EditTemplate = EditTemplate,
				EditTemplateJS = EditTemplateJS,
				EditTemplateRZ = EditTemplateRZ,
				EditTemplateNT = EditTemplateNT,

				IsVisible = IsVisible,
				IsVisibleAction = IsVisibleAction,
				IsReadonly = IsReadonly,

				// Filtering and sorting
				FilterType = FilterType,
				FilterOperation = FilterOperation,
				FilterValue = FilterValue,
				IsSorting = IsSorting,
				SortIndex = SortIndex,
				SortOrder = SortOrder,
				SortingMethod = SortingMethod,
				SortValue = SortValue,

				// Styling
				CssClass = CssClass,

				LookupDatasource = datasource,
				ValueExpression = ValueExpression,
				DisplayExpression = DisplayExpression,
				GroupExpression = GroupExpression,

				// Command columns
				CommandType = CommandType,
				Buttons = buttonContext.Buttons,
			};

			columnContext.Columns.Add(column);
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnTagHelper properties: column type
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the type of the column: data (default), command
		/// </summary>
		[HtmlAttributeName("type")]
		public string Type { get; set; }

		/// <summary>
		/// Get or set the index of the column. Zero denotes the first column.
		/// </summary>
		[HtmlAttributeName("index")]
		public int? Index { get; set; }

		/// <summary>
		/// Get or set the field name of the column.
		/// </summary>
		[HtmlAttributeName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Get or set the header label of the column.
		/// </summary>
		[HtmlAttributeName("label")]
		public string Label { get; set; }

		/// <summary>
		/// Get or set an indicator if this column should be visible. Defaults to <see langword="true"/>.
		/// </summary>
		[HtmlAttributeName("visible")]
		public bool IsVisible { get; set; } = true;

		/// <summary>
		/// Get or set an JS method to determine if this column should be visible.
		/// </summary>
		[HtmlAttributeName("visible-js")]
		public string IsVisibleAction { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnTagHelper properties: data column
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the data type of the column.
		/// </summary>
		[HtmlAttributeName("datatype")]
		public GridColumnDataType? DataType { get; set; }

		/// <summary>
		/// Get or set the width of this column.
		/// </summary>
		[HtmlAttributeName("width")]
		public string Width { get; set; }

		/// <summary>
		/// Get or set the format of the column.
		/// </summary>
		[HtmlAttributeName("format")]
		public Format? Format { get; set; }

		/// <summary>
		/// Get or set the custom format of this column.
		/// </summary>
		[HtmlAttributeName("custom-format")]
		public string CustomFormat { get; set; }

		/// <summary>
		/// Get or set the alignment of the column.
		/// </summary>
		[HtmlAttributeName("align")]
		public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;

		/// <summary>
		/// Get or set the FOR expression of the column.
		/// </summary>
		[HtmlAttributeName("asp-for")]
		public ModelExpression For { get; set; }

		/// <summary>
		/// Get or set an indicator if this column should be readonly, and not editable when in edit mode.
		/// </summary>
		[HtmlAttributeName("readonly")]
		public bool IsReadonly { get; set; }

		/// <summary>
		/// Get or set the name of the JS function which sets the cell value after editing this column.
		/// </summary>
		[HtmlAttributeName("set-value")]
		public string SetCellValue { get; set; }

		/// <summary>
		/// Get or set the JS cell template of this column.
		/// </summary>
		[HtmlAttributeName("content-js")]
		public string ContentJS { get; set; }

		/// <summary>
		/// Get or set the RazorBlock cell template of this column.
		/// </summary>
		[HtmlAttributeName("content-rz")]
		public RazorBlock ContentRZ { get; set; }

		/// <summary>
		/// Get or set the named cell template of this column.
		/// </summary>
		[HtmlAttributeName("content-nt")]
		public string ContentNT { get; set; }

		/// <summary>
		/// Get or set the (string) edit cell template of this column.
		/// </summary>
		[HtmlAttributeName("edit-template")]
		public string EditTemplate { get; set; }

		/// <summary>
		/// Get or set the JS edit cell template of this column.
		/// </summary>
		[HtmlAttributeName("edit-template-js")]
		public string EditTemplateJS { get; set; }

		/// <summary>
		/// Get or set the RazorBlock edit cell template of this column.
		/// </summary>
		[HtmlAttributeName("edit-template-rz")]
		public RazorBlock EditTemplateRZ { get; set; }

		/// <summary>
		/// Get or set the named edit cell template of this column.
		/// </summary>
		[HtmlAttributeName("edit-template-nt")]
		public string EditTemplateNT { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as lookup dropdown item value.
		/// </summary>
		[HtmlAttributeName("value-expr")]
		public string ValueExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as lookup dropdown item display text.
		/// </summary>
		[HtmlAttributeName("display-expr")]
		public string DisplayExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as lookup dropdown item grouping selector.
		/// </summary>
		[HtmlAttributeName("group-expr")]
		public string GroupExpression { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnTagHelper properties: filtering and sorting
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the filter type of this column.
		/// </summary>
		[HtmlAttributeName("filter")]
		public FilterType? FilterType { get; set; }

		/// <summary>
		/// Get or set the initial filter operation of this column.
		/// </summary>
		[HtmlAttributeName("filter-op")]
		public FilterOperations? FilterOperation { get; set; }

		/// <summary>
		/// Get or set the initial filter value of this column.
		/// </summary>
		[HtmlAttributeName("filter-value")]
		public object FilterValue { get; set; }

		/// <summary>
		/// Get an indicator if this column can be sorting. Defaults to <see langword="true"/>.
		/// </summary>
		[HtmlAttributeName("sorting")]
		public bool IsSorting { get; set; } = true;

		/// <summary>
		/// Get or set a sort index of this column.
		/// </summary>
		[HtmlAttributeName("sort-index")]
		public int SortIndex { get; set; }

		/// <summary>
		/// Get or set a sort order of this column.
		/// </summary>
		[HtmlAttributeName("sort-order")]
		public SortOrder? SortOrder { get; set; }

		/// <summary>
		/// Get or set the JS method which sorts this column.
		/// </summary>
		[HtmlAttributeName("sort-method")]
		public string SortingMethod { get; set; }

		/// <summary>
		/// Get or set the calculating sort value.
		/// </summary>
		[HtmlAttributeName("sort-value")]
		public string SortValue { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnModel properties: data column styling
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the CSS class to be applied to grid cells of this column.
		/// </summary>
		[HtmlAttributeName("cell-class")]
		public string CssClass { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnTagHelper properties: command column
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the command column type of this column. Applicable only to command columns.
		/// </summary>
		[HtmlAttributeName("command-type")]
		public GridCommandColumnType CommandType { get; set; }

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

		#endregion
	}

	/// <summary>
	/// The <see cref="ColumnsContext"/> context type confers contained columns to the parent control, such as a data grid.
	/// </summary>
	public class ColumnsContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IList<ColumnModel> Columns { get; } = new List<ColumnModel>();

		#endregion
	}

	/// <summary>
	/// The <see cref="ColumnModel"/> model type represents a contained column, which is confered to the parent control, such as a data grid.
	/// </summary>
	public class ColumnModel
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnModel properties: column type
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the type of the column: data (default), command
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Get or set the field name of the column.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Get or set the header label of the column.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Get or set an indicator if this column should be visible. Defaults to <see langword="true"/>.
		/// </summary>
		public bool IsVisible { get; set; }

		/// <summary>
		/// Get or set an JS method to determine if this column should be visible.
		/// </summary>
		public string IsVisibleAction { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnModel properties: data column
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the width of this column.
		/// </summary>
		public string Width { get; set; }

		/// <summary>
		/// Get or set the index of the column. Zero denotes the first column.
		/// </summary>
		public int? Index { get; set; }

		public GridColumnDataType? DataType { get; set; }

		/// <summary>
		/// Get or set the format of this column.
		/// </summary>
		public Format? Format { get; set; }

		/// <summary>
		/// Get or set the custom format of this column.
		/// </summary>
		public string CustomFormat { get; set; }

		public HorizontalAlignment Alignment { get; set; }
		public ModelExpression For { get; set; }

		/// <summary>
		/// Get or set the name of the JS function which sets the cell value after editing this column.
		/// </summary>
		public string SetCellValue { get; set; }

		/// <summary>
		/// Get or set the (string) cell template of this column.
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// Get or set the JS cell template of this column.
		/// </summary>
		public string ContentJS { get; set; }

		/// <summary>
		/// Get or set the RazorBlock cell template of this column.
		/// </summary>
		public RazorBlock ContentRZ { get; set; }

		/// <summary>
		/// Get or set the named cell template of this column.
		/// </summary>
		public string ContentNT { get; set; }

		/// <summary>
		/// Get or set the (string) edit cell template of this column.
		/// </summary>
		public string EditTemplate { get; set; }

		/// <summary>
		/// Get or set the JS edit cell template of this column.
		/// </summary>
		public string EditTemplateJS { get; set; }

		/// <summary>
		/// Get or set the RazorBlock edit cell template of this column.
		/// </summary>
		public RazorBlock EditTemplateRZ { get; set; }

		/// <summary>
		/// Get or set the named edit cell template of this column.
		/// </summary>
		public string EditTemplateNT { get; set; }

		public bool IsReadonly { get; set; }

		/// <summary>
		/// Get or set the tag helper which represents a data source for column lookup.
		/// </summary>
		public DataSourceTagHelper LookupDatasource { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as lookup dropdown item value.
		/// </summary>
		public string ValueExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as lookup dropdown item display text.
		/// </summary>
		public string DisplayExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used as lookup dropdown item grouping selector.
		/// </summary>
		public string GroupExpression { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnModel properties: data column filtering and sorting
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public FilterType? FilterType { get; set; }
		public FilterOperations? FilterOperation { get; set; }
		public object FilterValue { get; set; }

		/// <summary>
		/// Get an indicator if this column can be sorting.
		/// </summary>
		public bool IsSorting { get; set; }

		/// <summary>
		/// Get or set a sort index of this column.
		/// </summary>
		public int SortIndex { get; set; }

		/// <summary>
		/// Get or set a sort order of this column.
		/// </summary>
		public SortOrder? SortOrder { get; set; }

		/// <summary>
		/// Get or set the JS method which sorts this column.
		/// </summary>
		public string SortingMethod { get; set; }

		/// <summary>
		/// Get or set the calculating sort value.
		/// </summary>
		public string SortValue { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnModel properties: data column styling
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the CSS class to be applied to grid cells of this column.
		/// </summary>
		public string CssClass { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnModel properties: command column
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the command column type of this column. Applicable only to command columns.
		/// </summary>
		public GridCommandColumnType CommandType { get; set; }

		/// <summary>
		/// Get or set the icon of the column.
		/// </summary>
		public string Icon { get; set; }

		/// <summary>
		/// Get or set the text of the column.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Get or set a sequence of buttons of the column.
		/// </summary>
		public IEnumerable<ButtonModel> Buttons { get; internal set; }

		#endregion
	}
}
