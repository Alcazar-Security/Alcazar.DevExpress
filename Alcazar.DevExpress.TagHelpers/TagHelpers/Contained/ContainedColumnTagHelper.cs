using DevExtreme.AspNet.Mvc;
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
	/// The <see cref="ColumnTagHelper"/> tag helper defines columns of a data grid and other users of data sources.
	/// </summary>
	[HtmlTargetElement("column", ParentTag = "dx-datagrid", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class ColumnTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the HTML of this tag, it is used for DX button generation only
			output.SuppressOutput();

			// Create the context, so that we can pass them to child tag helpers
			ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);

			// Obtain the context, so that we can use it here
			ColumnContext columnContext = GetContextSafe<ColumnContext>(context);

			string label = TranslateToProp(Label, ViewContext);
			string name = TranslateToProp(Name, ViewContext);

			// Process children of the column tag, which becomes the column template
			IHtmlContent content = await output.GetChildContentAsync();
			string text = ToString(content);

			// Construct the column model
			ColumnModel button = new ColumnModel
			{
				// Column type
				Type = Type,

				// Data columns
				Label = label,
				Name = name,
				Alignment = Alignment,
				DataType = DataType,
				Format = Format,
				For = For,
				Content = text,
				ContentJS = ContentJS,
				ContentRZ = ContentRZ,
				ContentNT = ContentNT,
				IsVisible = IsVisible,
				IsReadonly = IsReadonly,
				FilterType = FilterType,
				FilterOperation = FilterOperation,
				FilterValue = FilterValue,

				// Command columns
				CommandType = CommandType,
				Buttons = buttonContext.Buttons,
			};

			columnContext.Columns.Add(button);
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
		/// Get or set the format of the column.
		/// </summary>
		[HtmlAttributeName("format")]
		public Format? Format { get; set; }

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
	/// The <see cref="ColumnContext"/> context type confers contained columns to the parent control, such as a data grid.
	/// </summary>
	public class ColumnContext
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

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnModel properties: data column
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public GridColumnDataType? DataType { get; set; }
		public Format? Format { get; set; }
		public HorizontalAlignment Alignment { get; set; }
		public ModelExpression For { get; set; }
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

		public bool IsReadonly { get; set; }

		public FilterType? FilterType { get; set; }
		public FilterOperations? FilterOperation { get; set; }
		public object FilterValue { get; set; }
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
