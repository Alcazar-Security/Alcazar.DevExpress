using Amaqele.Common.Types;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="SelectBoxTagHelper"/> type implements a single selection dropdown box.
	/// The select box can do anything except multiple selection, therefore it is chosen over the DropdownBox and Lookup.
	/// The other implemented choices are:
	/// * Autocomplete for lookups
	/// * Tag box for multiple select
	/// </summary>
	[HtmlTargetElement("dx-select")]
	public class SelectBoxTagHelper : DropdownTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SelectBoxTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public SelectBoxTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SelectBoxTagHelper overrides
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
			SelectBoxBuilder builder = _htmlHelper.DevExtreme().SelectBox();

			// Apply the control context, which is values which an outer dx-field or dx-control tag might want to pass into me, the editor
			ControlContext controlContext = ApplyControlContext(context);
			if (controlContext != null)
			{
				// Apply more from the context
				if (Value == null)
					Value = controlContext.Value;
				if (Items == null)
					Items = controlContext.Items;
			}

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the For attribute, if it is set
			object value = ProcessFor();
			value = ProcessForEnums(value);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			// Create the contexts, so that we can pass them to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ButtonContext buttonContext = GetOrCreateContext<ButtonContext>(context);

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			if (Items != null)
			{
				// Process server-side supplied items
				builder = builder.DataSource(Items);

				// Process known item types
				builder = ProcessItemTypes(builder, Items);
			}
			else if (sourceContext.Datasource != null)
			{
				// Process the (child) data source
				// TODO maybe convert to datasource a la DxGrid
				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}

			// Process data source options
			if (!string.IsNullOrEmpty(GroupExpression))
			{
				builder = builder.Grouped(true);
				builder = builder.DataSourceOptions(o => o
					.Group(GroupExpression));
			}

			// AFTER adding items...
			// Apply the For attribute, or the corresponding direct values
			builder = ApplyFor(builder, value);

			// Add buttons, but only if we have some
			if (buttonContext.Buttons.Any() || !string.IsNullOrEmpty(HelpText))
			{
				// We have buttons, add them, also the clear button if needed (otherwise if there are any buttons, the clear button gets lost)
				builder = builder.Buttons(b =>
				{
					// Add the clear button, but only if there are other buttons
					if (AllowClear)
						b.Add().Name("clear");

					// Add the custom help button, but only if we have a help text
					AddHelpButton(b);

					// Add custom defined buttons, but only if we have some
					AddCustomButtons(b, buttonContext.Buttons);
				});
			}
			else
			{
				// We have NO buttons, but check for the clear button
				if (AllowClear)
					builder = builder.ShowClearButton(AllowClear);
			}

			if (IsReadonly)
			{
				builder = builder
					.ReadOnly(true)
					.HoverStateEnabled(true);
			}

			// Process text-box specific properties
			//                     .Mask("+1 (X00) 000-0000")
			if (_searchMode.HasValue)
			{
				// Set the search mode and its properties
				builder = builder.SearchEnabled(true);
				builder = builder.SearchMode(_searchMode.Value);
				builder = builder.SearchTimeout(SearchTimeout);
				builder = builder.MinSearchLength(MinSearchLength);
				if (!string.IsNullOrEmpty(ValueExpression))
					builder = builder.SearchExpr(SearchExpression);
			}

			// Item template
			if (!string.IsNullOrWhiteSpace(ItemTemplate))
				builder = builder.ItemTemplate(ItemTemplate);
			else if (!string.IsNullOrWhiteSpace(ItemTemplateJS))
				builder = builder.ItemTemplate(new JS(ItemTemplateJS));
			else if (ItemTemplateRZ != null)
				builder = builder.ItemTemplate(ItemTemplateRZ);
			else if (ItemTemplateNT != null)
				builder = builder.ItemTemplate(new TemplateName(ItemTemplateNT));

			// Field template
			if (!string.IsNullOrWhiteSpace(FieldTemplate))
				builder = builder.FieldTemplate(FieldTemplate);
			else if (!string.IsNullOrWhiteSpace(FieldTemplateJS))
				builder = builder.FieldTemplate(new JS(FieldTemplateJS));
			else if (FieldTemplateRZ != null)
				builder = builder.FieldTemplate(FieldTemplateRZ);
			else if (FieldTemplateNT != null)
				builder = builder.FieldTemplate(new TemplateName(FieldTemplateNT));

			// This setValue thing is really wierd.
			// The function is never called, but it must exist. It is set as an option into the editor, but it somehow updates the grid from the editor
			if (!string.IsNullOrEmpty(SetValueJS))
				builder = builder.Option("setValue", new JS(SetValueJS));

			if (!string.IsNullOrEmpty(ValueExpression))
				builder = builder.ValueExpr(ValueExpression);
			if (!string.IsNullOrEmpty(DisplayExpression))
				builder = builder.DisplayExpr(DisplayExpression);

			// Process events
			builder = ProcessEvents(builder);

			// The control should be opened on click anywhere in the control.
			builder = builder.OpenOnFieldClick(IsOpenClick);
			//builder = builder.OnEnterKey("onMemberAdded");
			//builder = builder.OnItemClick("onMemberAdded");
			//builder = builder.OnOptionChanged("onMemberAdded");

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private SelectBoxBuilder ProcessCommon(SelectBoxBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Visible and disabled
			if (!IsVisible)
				builder = builder.Visible(IsVisible);
			if (IsDisabled)
				builder = builder.Disabled(IsDisabled);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private SelectBoxBuilder ProcessAttributes(SelectBoxBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the input tag
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// And we are allowing attributes on the input field also
			foreach (var attr in InputAttributes)
				builder = builder.InputAttr(attr.Key, attr.Value?.ToString());

			return builder;
		}

		protected new object ProcessForEnums(object value)
		{
			// Converting the return value to string. This is needed for enum values in a select-box, as the value would otherwise be translated to the int representation and then not set the inital value
			if (Value != null && ModelType?.IsEnum == true)
				return Value = Value.ToString();

			return base.ProcessForEnums(value);
		}

		private SelectBoxBuilder ApplyFor(SelectBoxBuilder builder, object value)
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
				builder = builder.Value(value);
			
			if (!string.IsNullOrEmpty(Placeholder))
			{
				string placeholder = TranslateToProp(Placeholder, ViewContext);
				builder = builder.Placeholder(placeholder);
			}

			return builder;
		}

		private SelectBoxBuilder ProcessItemTypes(SelectBoxBuilder builder, IEnumerable items)
		{
			Type elementType = TypeHelper.GetEnumerableElementType(items);
			if (elementType  == typeof(SelectListItem))
			{
				// For SelectListItems, use 'Value' and 'Text'
				if (string.IsNullOrEmpty(ValueExpression))
					ValueExpression = ValueString;
				if (string.IsNullOrEmpty(DisplayExpression))
					DisplayExpression = TextString;
			}

			return builder;
		}

		private SelectBoxBuilder ProcessTitle(SelectBoxBuilder builder)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder.Hint(title);
			}

			return builder;
		}

		private SelectBoxBuilder ProcessEvents(SelectBoxBuilder builder)
		{
			// Event handlers
			if (!string.IsNullOrEmpty(OnValueChanged))
				builder = builder.OnValueChanged(OnValueChanged);
			if (!string.IsNullOrEmpty(OnSelectionChanged))
				builder = builder.OnSelectionChanged(OnSelectionChanged);
			if (!string.IsNullOrEmpty(OnChange))
				builder = builder.OnChange(OnChange);

			if (!string.IsNullOrEmpty(OnContentReady))
				builder = builder.OnContentReady(OnContentReady);
			if (!string.IsNullOrEmpty(OnInitialized))
				builder = builder.OnInitialized(OnInitialized);
			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SelectBoxTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public object Value { get; set; }

		/// <summary>
		/// Get or set the items of this dropdown box.
		/// Items are supplied server side, and are an alternative to a data source.
		/// </summary>
		[HtmlAttributeName("asp-items")]
		public System.Collections.IEnumerable Items { get; set; }

		/// <summary>
		/// Get or set an indicator if the control should be opened on click anywhere in the control.
		/// </summary>
		[HtmlAttributeName("open-click")]
		public bool IsOpenClick { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SelectBoxTagHelper properties: not inherited
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		// If any of these properties are required on a control, it must be declared as an embedded control with in a dx-field or dx-control

		/// <summary>
		/// Get or set the name of the item property to be used as dropdown item value.
		/// </summary>
		[HtmlAttributeName("value-expr")]
		public string ValueExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used to display dropdown items.
		/// </summary>
		[HtmlAttributeName("display-expr")]
		public string DisplayExpression { get; set; }

		/// <summary>
		/// Get or set the name of the item property to be used for grouping.
		/// </summary>
		[HtmlAttributeName("group-expr")]
		public string GroupExpression { get; set; }

		/// <summary>
		/// Get or set the search mode used by the control. Setting the search mode enables searching.
		/// </summary>
		[HtmlAttributeName("search")]
		public DropDownSearchMode SearchMode
		{
			get { return _searchMode ?? DropDownSearchMode.StartsWith; }
			set { _searchMode = value; }
		}

		/// <summary>
		/// Get or set the name of the item property to be used when searching for items.
		/// </summary>
		[HtmlAttributeName("search-expr")]
		public string SearchExpression { get; set; }

		// Require the private fields so that we dont have to fully qualify the mode in cshtml (DropDownSearchMode.StartsWith)
		private DropDownSearchMode? _searchMode;

		/// <summary>
		/// Get or set the number of characters to enter before the lookup starts.
		/// This value may also be set in the data source, but the select box needs it to know when it should retrieve data from the data source.
		/// </summary>
		[HtmlAttributeName("min-length")]
		public int MinSearchLength { get; set; }

		/// <summary>
		/// Get or set the search timeout (in ms) for calling the datasource.
		/// This value may also be set in the data source, but the select box needs it to know when it should retrieve data from the data source.
		/// </summary>
		[HtmlAttributeName("timeout")]
		public int SearchTimeout { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region SelectBoxTagHelper properties: templates
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

        /// <summary>
        /// Get or set the (string) field template of this select control.
        /// </summary>
        [HtmlAttributeName("field-template")]
        public string FieldTemplate { get; set; }

        /// <summary>
        /// Get or set the JS field template of this select control.
        /// </summary>
        [HtmlAttributeName("field-template-js")]
        public string FieldTemplateJS { get; set; }

        /// <summary>
        /// Get or set the RazorBlock field template of this select control.
        /// </summary>
        [HtmlAttributeName("field-template-rz")]
        public RazorBlock FieldTemplateRZ { get; set; }

        /// <summary>
        /// Get or set the named field template of this select control.
        /// </summary>
        [HtmlAttributeName("field-template-nt")]
        public string FieldTemplateNT { get; set; }

        /// <summary>
        /// Get or set the (string) item template of this select control.
        /// </summary>
        [HtmlAttributeName("item-template")]
        public string ItemTemplate { get; set; }

        /// <summary>
        /// Get or set the JS item template of this select control.
        /// </summary>
        [HtmlAttributeName("item-template-js")]
        public string ItemTemplateJS { get; set; }

        /// <summary>
        /// Get or set the RazorBlock item template of this select control.
        /// </summary>
        [HtmlAttributeName("item-template-rz")]
        public RazorBlock ItemTemplateRZ { get; set; }

        /// <summary>
        /// Get or set the named item template of this select control.
        /// </summary>
        [HtmlAttributeName("item-template-nt")]
        public string ItemTemplateNT { get; set; }

        #endregion

        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        #region SelectBoxTagHelper events: not inherited
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		#endregion
	}
}
