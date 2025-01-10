using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Html;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DateboxTagHelper"/> type implements a date box (with no time component).
	/// </summary>
	[HtmlTargetElement("dx-date")]
	public class DateboxTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DateboxTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxTagHelper overrides
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
			DateBoxBuilder builder = _htmlHelper.DevExtreme().DateBox();

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

			// Process text-area specific properties
			// builder = builder.DisplayFormat(Format.ShortDate);
			builder = builder.DisplayFormat("yyyy-MM-dd");

			//builder = builder.DateSerializationFormat();

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private DateBoxBuilder ProcessCommon(DateBoxBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private DateBoxBuilder ProcessAttributes(DateBoxBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value.ToString());

			return builder;
		}

		private DateBoxBuilder ApplyFor(DateBoxBuilder builder, object value)
		{
			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Apply the value, but only if the asp-for is not set (else the asp-for drives the value)
			if (For == null)
				value = Value;

			if (value is DateTime dateValue1)
			{
				// Setting the value as direct date
				builder = builder.Value(dateValue1);
			}

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DateboxTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the type of the date box.
		/// </summary>
		[HtmlAttributeName("type")]
		public DateBoxType Type { get; set; } = DateBoxType.Date;

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public DateTime? Value { get; set; }

		#endregion
	}
}
