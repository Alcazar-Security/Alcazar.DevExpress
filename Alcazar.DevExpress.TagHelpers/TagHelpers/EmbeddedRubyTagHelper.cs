using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="EmbeddedRubyTagHelper"/> type implements a tag helper for embedded Ruby (erb) placeholders.
	/// Embedded Ruby (erb) placeholders are used in DX templates to insert model content, such as <%- text %>.
	/// In many cases it should be possible to insert an ERB directly into the template without this tag, but there are cases where Razor seeming cannot handle that.
	/// Such as: <span><%- text %></span>
	/// </summary>
	[HtmlTargetElement("erb")]
	public class EmbeddedRubyTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region EmbeddedRubyTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Suppress the actual tag <erb />, changing it to a <span>
			output.TagName = "span";

			// Add the before value to the content
			if (!string.IsNullOrEmpty(ValueBefore))
			{
				output.Content.AppendHtml(ValueBefore);
				output.Content.AppendHtml("&nbsp;");
			}

			// Add the value to the content
			output.Content.AppendHtml(Value);

			// Add the after value to the content
			if (!string.IsNullOrEmpty(ValueAfter))
			{
				output.Content.AppendHtml("&nbsp;");
				output.Content.AppendHtml(ValueAfter);
			}
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region EmbeddedRubyTagHelper attributes
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		[HtmlAttributeName("before")]
		public string ValueBefore { get; set; }

		[HtmlAttributeName("value")]
		public string Value { get; set; }

		[HtmlAttributeName("after")]
		public string ValueAfter { get; set; }

		#endregion
	}
}
