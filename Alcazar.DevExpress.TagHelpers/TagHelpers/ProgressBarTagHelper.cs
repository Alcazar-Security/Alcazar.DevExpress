using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="ProgressBarTagHelper"/> type implements a progress bar.
	/// </summary>
	[HtmlTargetElement("dx-progressbar")]
	public class ProgressBarTagHelper : ControlTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TabsTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public ProgressBarTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ProgressBarTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Asynchronously executes the tag with the given <paramref name="context"/> and <paramref name="output"/>.
		/// </summary>
		/// <param name="context"> Contains information associated with the current HTML tag. </param>
		/// <param name="output"> A stateful HTML element used to generate an HTML tag. </param>
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			_htmlHelper.Contextualize(ViewContext);

			// Suppress myself as output, using only the translated UI text.
			output.SuppressOutput();

			// Create the builder for the progress bar
			ProgressBarBuilder builder = _htmlHelper.DevExtreme().ProgressBar();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process the title/hint, if it is set
			builder = ProcessTitle(builder);

			builder = builder.Min(Min);
			builder = builder.Max(Max);
			builder = builder.ShowStatus(ShowStatus);

			// Process events
			builder = ProcessEvents(builder);

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private ProgressBarBuilder ProcessCommon(ProgressBarBuilder builder)
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
			if (!string.IsNullOrEmpty(Height))
				builder = builder.Height(Height);

			return builder;
		}

		private ProgressBarBuilder ProcessTitle(ProgressBarBuilder builder)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				string title = TranslateToProp(Title, ViewContext);
				builder.Hint(Title);
			}

			return builder;
		}

		private ProgressBarBuilder ProcessEvents(ProgressBarBuilder builder)
		{
			if (!string.IsNullOrEmpty(OnComplete))
				builder = builder.OnComplete(OnComplete);

			if (!string.IsNullOrEmpty(OnValueChanged))
				builder = builder.OnValueChanged(OnValueChanged);

			if (!string.IsNullOrEmpty(OnInitialized))
				builder = builder.OnInitialized(OnInitialized);

			if (!string.IsNullOrEmpty(OnContentReady))
				builder = builder.OnContentReady(OnContentReady);

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ProgressBarTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the current value of the progress bar.
		/// </summary>
		[HtmlAttributeName("value")]
		public double? Value { get; set; }

		/// <summary>
		/// Get or set the minimum value. Defaults to 0.
		/// </summary>
		[HtmlAttributeName("min")]
		public double Min { get; set; } = 0;

		/// <summary>
		/// Get or set the maximum value. Defaults to 100.
		/// </summary>
		[HtmlAttributeName("max")]
		public double Max { get; set; } = 100;

		/// <summary>
		/// Get or set an indicator if the progress status label is shown.
		/// </summary>
		[HtmlAttributeName("status")]
		public bool ShowStatus { get; set; } = true;

		/// <summary>
		/// Get or set a JS function to format the status label. Receives (ratio, value) and returns a string.
		/// </summary>
		[HtmlAttributeName("status-format")]
		public string StatusFormat { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ProgressBarTagHelper properties: events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//


		/// <summary>
		/// Get or set the JS function called when the value reaches the maximum.
		/// </summary>
		[HtmlAttributeName("complete")]
		public string OnComplete { get; set; }

		/// <summary>
		/// Get or set the JS function called when the value changes.
		/// </summary>
		[HtmlAttributeName("value-changed")]
		public string OnValueChanged { get; set; }

		/// <summary>
		/// Get or set the JS function called when the widget is initialised.
		/// </summary>
		[HtmlAttributeName("initialised")]
		public string OnInitialized { get; set; }

		/// <summary>
		/// Get or set the JS function called when the content is ready.
		/// </summary>
		[HtmlAttributeName("content-ready")]
		public string OnContentReady { get; set; }

		#endregion    
	}
}