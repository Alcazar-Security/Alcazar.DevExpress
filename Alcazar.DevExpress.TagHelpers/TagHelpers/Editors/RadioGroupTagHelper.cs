using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="RadioGroupTagHelper"/> type implements a set of radio buttons.
	/// </summary>
	[HtmlTargetElement("dx-radiogroup")]
	public class RadioGroupTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region RadioGroupTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public RadioGroupTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region RadioGroupTagHelper overrides
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

			// Create the builder for a radio group
			RadioGroupBuilder builder = _htmlHelper.DevExtreme().RadioGroup();

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
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder.Hint(title);
			}

			// Process the read-only state
			if (IsReadonly)
			{
				builder = builder
					.ReadOnly(true)
					.HoverStateEnabled(true);
			}

			// Process thedisabled state
			if (IsDisabled)
			{
				builder = builder.Disabled(true);
			}

			// Process radio-group specific properties

			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);
			ItemContext itemContext = GetOrCreateContext<ItemContext>(context);

			// Process children of the tag, we will need them
			IHtmlContent content = await output.GetChildContentAsync();

			if (Items != null)
			{
				// TODO maybe convert to datasource a la DxGrid
				// Process server-side supplied string items
				builder = builder.DataSource(Items);
			}
			else if (itemContext.Items.Any())
			{
				// Process server-side supplied content items
				builder = builder.Items(c =>
				{
					foreach (var item in itemContext.Items)
					{
						c.Add()
							.Option("name", item.Name)
							.Option("value", item.Value)
							.Text(item.Text);
					}
				});
			}
			else if (sourceContext.Datasource != null)
			{
				// Process the (child) data source
				builder = builder.DataSource(d => sourceContext.Datasource.BuildDatasource(d));
			}

			builder = builder.Layout(Orientation);

			// Event handlers
			if (!string.IsNullOrEmpty(OnValueChanged))
				builder = builder.OnValueChanged(OnValueChanged);
			if (!string.IsNullOrEmpty(OnOptionChanged))
				builder = builder.OnOptionChanged(OnOptionChanged);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private RadioGroupBuilder ProcessCommon(RadioGroupBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private RadioGroupBuilder ProcessAttributes(RadioGroupBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		private RadioGroupBuilder ApplyFor(RadioGroupBuilder builder, object value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			if (For == null)
				value = Value;

			builder = builder.Value(value);
			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region RadioGroupTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the items of this radio group.
		/// Items are supplied server side, and are an alternative to a data source.
		/// </summary>
		[HtmlAttributeName("asp-items")]
		public IEnumerable<string> Items { get; set; }

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public string Value { get; set; }

		/// <summary>
		/// Get or set the custom text for the label. Defaults to the equivalent of 'DisplayNameFor', woth a fallback to <see cref="base.Name"/>.
		/// The check box control has this property, as the control can include the label.
		/// </summary>
		[HtmlAttributeName("label-text")]
		public string LabelText { get; set; }

		/// <summary>
		/// Get or set the layout orientation of the radio group.
		/// </summary>
		[HtmlAttributeName("orientation")]
		public Orientation Orientation { get; set; }

		/// <summary>
		/// Get or set the action to be executed when the value of the radio group is changed.
		/// </summary>
		[HtmlAttributeName("value-changed")]
		public string OnValueChanged { get; set; }

		/// <summary>
		/// Get or set the action to be executed when the option of the radio group is changed.
		/// </summary>
		[HtmlAttributeName("option-changed")]
		public string OnOptionChanged { get; set; }

		#endregion
	}
}
