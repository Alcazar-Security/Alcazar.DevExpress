using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="AppointmentTemplateTagHelper"/> implements a tag helper for the DX scheduler control which allow appointment templates.
	/// Templates are custom HTML content of the control. They can contain embedded Ruby (erb) placeholders.
	/// </summary>
	[HtmlTargetElement("appointment-template", ParentTag = "dx-scheduler", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class AppointmentTemplateTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region AppointmentTemplateTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			SchedulerTemplateContext templateContext = GetContextSafe<SchedulerTemplateContext>(context);
			templateContext.AppointmentContent = await output.GetChildContentAsync();
			output.SuppressOutput();
		}

		#endregion
	}

	/// <summary>
	/// The <see cref="AppointmentTooltipTemplateTagHelper"/> implements a tag helper for the DX scheduler control which allow appointment tooltip templates.
	/// Templates are custom HTML content of the control. They can contain embedded Ruby (erb) placeholders.
	/// </summary>
	[HtmlTargetElement("tooltip-template", ParentTag = "dx-scheduler", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class AppointmentTooltipTemplateTagHelper : TagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region AppointmentTooltipTemplateTagHelper overrides
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			// Process the card-actions tag and remember the content, so that the parent can inject it into the header
			SchedulerTemplateContext templateContext = GetContextSafe<SchedulerTemplateContext>(context);
			templateContext.AppointmentTooltipContent = await output.GetChildContentAsync();
			output.SuppressOutput();
		}

		#endregion
	}

	/// <summary>
	/// The <see cref="SchedulerTemplateContext"/> handles template types specific to the scheduler control.
	/// </summary>
	public class SchedulerTemplateContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTemplateContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the content or template for appointments shown by the scheduler control.
		/// </summary>
		public IHtmlContent AppointmentContent { get; set; }

		/// <summary>
		/// Get or set the content or template for tooltips of appointments shown by the scheduler control.
		/// </summary>
		public IHtmlContent AppointmentTooltipContent { get; set; }

		#endregion
	}
}
