using Alcazar.DevExpress.TagHelpers.TagHelpers.Contained;
using Amaqele.Common.Authn;
using Amaqele.Common.Collections;
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
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
    /// <summary>
    /// The <see cref="DataGridTagHelper"/> type implements a data grid.
    /// </summary>
    [HtmlTargetElement("dx-datagrid")]
	public class DataGridTagHelper : RouteTagHelperBase
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

			MethodInfo method = typeof(DataGridTagHelper).GetMethod("BuildDateGridAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method.IsGenericMethod)
				method = method.MakeGenericMethod(RecordType);

			// Configure the builder
			Task<IHtmlContent> result = (Task<IHtmlContent>)method.Invoke(this, new object[] { context, output });
			IHtmlContent content = result.GetAwaiter().GetResult();

			// Render the builder (into the POST content)
			output.Content.SetHtmlContent(content);
		}

		private async Task<IHtmlContent> BuildDateGridAsync<T>(TagHelperContext context, TagHelperOutput output)
		{
			// Create the builder for a popup
			DataGridBuilder<T> builder = _htmlHelper.DevExtreme().DataGrid<T>();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

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

			builder = builder.Paging(p => p.PageSize(25));
			builder = builder.Pager(p => p
				.DisplayMode(GridPagerDisplayMode.Adaptive)
				.ShowPageSizeSelector(true)
				.ShowNavigationButtons(true)
				.AllowedPageSizes(new[] { 25, 50, 100, 250, 500 }));

			builder = builder.Editing(editing =>
			{
				// Allow editing options of the corresponding actions are set
				editing
					.Mode(GridEditMode.Row)
					.UseIcons(true);

				if (!string.IsNullOrEmpty(InsertAction))
					editing.AllowAdding(true);
				if (!string.IsNullOrEmpty(UpdateAction))
					editing.AllowUpdating(true);
				if (!string.IsNullOrEmpty(DeleteAction))
					editing.AllowDeleting(true);
			});

			//builder = builder.OnCellPrepared("onCellPrepared");
			//builder = builder.OnCellClick("onCellClick");
			//builder = builder.OnContentReady("onContentReady");

			// Set the data source
			if (Items != null)
			{
				// Our datasource comes from the model
				builder = builder.DataSource(Items);
			}
			else if (!string.IsNullOrEmpty(LoadAction))
			{
				// Our datasource is a web api
				builder = builder.RemoteOperations( c => { c.Filtering(true); });
				builder = builder.DataSource(d =>
				{
					var options = d.Mvc().LoadMethod(HttpMethod.Post.ToString()).LoadAction(LoadAction);

					// Add load parameters
					if (LoadParams.Any())
					{
						ExpandoObject loadParams = new ExpandoObject();
						loadParams.AddRange(LoadParams);
						options = options.LoadParams(loadParams);
					}

					if (!string.IsNullOrEmpty(Controller))
						options = options.Controller(Controller);
					if (!string.IsNullOrEmpty(Area))
						options = options.Area(Area);

					if (!string.IsNullOrEmpty(Key))
						options = options.Key(Key);

					// Set editing actions
					if (!string.IsNullOrEmpty(InsertAction))
						options = options.InsertAction(InsertAction);
					if (!string.IsNullOrEmpty(UpdateAction))
						options = options.UpdateAction(UpdateAction);
					if (!string.IsNullOrEmpty(DeleteAction))
						options = options.DeleteAction(DeleteAction);

					return options;
				});
			}

			// Create the context, so that we can pass it to child tag helpers
			ColumnContext columnContext = GetOrCreateContext<ColumnContext>(context);

			// Process children of the card tag, the header, footer, and my body will need them 
			IHtmlContent content = await output.GetChildContentAsync();

			// Add buttons, but only if we have some
			if (columnContext.Columns.Any())
			{
				builder = builder.Columns(async columns =>
				{
					foreach (ColumnModel column in columnContext.Columns)
					{
						switch (column.Type)
						{
							default:
							case "data":
								ProcessDataColumn<T>(columns, column);
								break;

							case "command":
								await ProcessCommandColumnAsync<T>(context, output, columns, column);
								break;
						}
					}

					//columns.AddFor(m => m.Tenant.Name)
					//	.Lookup(lookup => lookup
					//		.DataSource(d => d.Mvc().Controller("Data").LoadAction("GetTenant").Key("pkTenantID"))
					//		.ValueExpr("fkTenantID")
					//		.DisplayExpr("Name"));
				});
			}

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

		private DataGridBuilder<T> ProcessTitle<T>(DataGridBuilder<T> builder)
		{
			//if (!string.IsNullOrEmpty(Title))
			//{
			//	string title = TranslateToProp(Title, ViewContext);
			//	builder.Hint(title);
			//}
			return builder;
		}

		private CollectionFactory<DataGridColumnBuilder<T>> ProcessDataColumn<T>(CollectionFactory<DataGridColumnBuilder<T>> columns, ColumnModel column)
		{
			if (column.For != null)
			{
				// This is a horrible hack for no.QualifiedName, when we try to obtain the asp-for prop from an IEnumerable model
				column.Name = RemoveNoname(column.For.Name);

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
				.AllowEditing(!column.IsReadonly)
				.Visible(column.IsVisible);

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
				.Caption(column.Label)
				.Visible(column.IsVisible);

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
						var builder2 = buttons.Add()
							.Name(button.Name)
							// Seemingly can only display icon OR text
							//.Text(button.Text)
							.Icon(button.Icon)
							.Hint(button.Title)
							//.Template("<span>xxx</span>")
							.OnClick(button.OnClickAction);

						// Not used currently
						// if (button.Content != null)
						//	builder2 = builder2.Template(ToString(button.Content));
					}
				});
			}
			else
			{
				// We have NO buttons, but check for the clear button
				// So we should not have a custom column in the first place, nothing to do
			}

			return columns;
		}

		private GridColumnDataType? ToDataType(Type type)
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
				case PrimitiveTypeCode.Decimal: return GridColumnDataType.Number;
				case PrimitiveTypeCode.Enumeration: return GridColumnDataType.String;

				case PrimitiveTypeCode.Bool: return GridColumnDataType.Boolean;

				case PrimitiveTypeCode.Char: return GridColumnDataType.String;
				case PrimitiveTypeCode.String: return GridColumnDataType.String;
				case PrimitiveTypeCode.Binary: return GridColumnDataType.String;
				case PrimitiveTypeCode.Base64Binary: return GridColumnDataType.String;
				case PrimitiveTypeCode.HexBinary: return GridColumnDataType.String;
				case PrimitiveTypeCode.Guid: return GridColumnDataType.String;

				case PrimitiveTypeCode.Date: return GridColumnDataType.Date;

				case PrimitiveTypeCode.Time:
				case PrimitiveTypeCode.Timestamp:
				case PrimitiveTypeCode.Timespan: return GridColumnDataType.DateTime;

				case PrimitiveTypeCode.Object: return GridColumnDataType.Object;
			}
		}

		private string RemoveNoname(string value)
		{
			// This is a horrible hack for no.QualifiedName, when we try to obtain the asp-for prop from an IEnumerable model
			string[] parts = value.Split('.');

			// If the string starts with noType.Name, remove that first part
			if (parts.Length > 1 && parts[0].StartsWith("no"))
				return string.Join('.', parts.Skip(1));

			return value;
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

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		/// <summary>
		/// Get or set the text message to be used for this popup.
		/// </summary>
		[HtmlAttributeName("text")]
		public string Text { get; set; }

		/// <summary>
		/// Get or set the title to be used for this popup.
		/// </summary>
		[HtmlAttributeName("title")]
		public string Title { get; set; }

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
		/// An expression to be evaluated against the current model.
		/// </summary>
		[HtmlAttributeName("asp-for")]
		public ModelExpression For { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataGridTagHelper properties: data source
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the items to be displayed in the data grid.
		/// </summary>
		[HtmlAttributeName("asp-items")]
		public System.Collections.IEnumerable Items { get; set; }

		/// <summary>
		/// Get or set the name of the load action for a web api data source.
		/// </summary>
		[HtmlAttributeName("asp-load")]
		public string LoadAction { get; set; }

		/// <summary>
		/// Get or set the name of the insert action for a web api data source.
		/// </summary>
		[HtmlAttributeName("asp-insert")]
		public string InsertAction { get; set; }

		/// <summary>
		/// Get or set the name of the update action for a web api data source.
		/// </summary>
		[HtmlAttributeName("asp-update")]
		public string UpdateAction { get; set; }

		/// <summary>
		/// Get or set the name of the delete action for a web api data source.
		/// </summary>
		[HtmlAttributeName("asp-delete")]
		public string DeleteAction { get; set; }

		/// <summary>
		/// Get or set the name of the key property of the data record returned by this data source.
		/// </summary>
		[HtmlAttributeName("key")]
		public string Key { get; set; }

		/// <summary>
		/// Get or set the record type used in this data grid.
		/// </summary>
		[HtmlAttributeName("record-type")]
		public Type RecordType { get; set; }

		/// <summary>
		/// Get or set parameters used for loading of data records from the data source.
		/// </summary>
		[HtmlAttributeName(DictionaryAttributePrefix = "load-param-")]
		public IDictionary<string, object> LoadParams { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		#endregion
	}

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

	public class ColumnContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ColumnContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public IList<ColumnModel> Columns { get; } = new List<ColumnModel>();

		#endregion
	}

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
