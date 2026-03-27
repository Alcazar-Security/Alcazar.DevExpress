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
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="SchedulerTagHelper"/> type implements a scheduler calendat.
	/// </summary>
	[HtmlTargetElement("dx-scheduler")]
	public class SchedulerTagHelper : DataSourceTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public SchedulerTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

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

			// Create the builder for a popup
			SchedulerBuilder builder = _htmlHelper.DevExtreme().Scheduler();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Create the context, so that we can pass it to child tag helpers
			ItemContext itemsContext = GetOrCreateContext<ItemContext>(context);
			SchedulerTemplateContext templateContext = GetOrCreateContext<SchedulerTemplateContext>(context);
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);

			// Process child content
			IHtmlContent content = await output.GetChildContentAsync();

			// Process scheduler-specific configuration
			builder = ProcessSchedulerConfiguration(builder, templateContext, itemsContext);

			// Process the popup appointment editor
			builder = ProcessEditing(builder);

			// Process data source
			builder = ProcessDataSource(builder, sourceContext, context);

			// Process views
			builder = ProcessViews(builder);
			
			// Data grid events
			builder = ProcessEvents(builder);

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private SchedulerBuilder ProcessCommon(SchedulerBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);
			if (!string.IsNullOrEmpty(Height))
				builder = builder.Height(Height);

			return builder;
		}

		private SchedulerBuilder ProcessAttributes(SchedulerBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		private SchedulerBuilder ProcessSchedulerConfiguration(SchedulerBuilder builder, SchedulerTemplateContext templateContext, ItemContext itemsContext)
		{
			// Date and time configuration
			if (CurrentDate.HasValue)
				builder = builder.CurrentDate(CurrentDate.Value);
			if (FirstDayOfWeek.HasValue)
				builder = builder.FirstDayOfWeek(FirstDayOfWeek.Value);
			if (!string.IsNullOrEmpty(TimeZone))
				builder = builder.TimeZone(TimeZone);

			// View configuration
			if (CurrentView.HasValue)
				builder = builder.CurrentView(CurrentView.Value);
			if (StartDayHour.HasValue)
				builder = builder.StartDayHour(StartDayHour.Value);
			if (EndDayHour.HasValue)
				builder = builder.EndDayHour(EndDayHour.Value);
			if (CellDuration.HasValue)
				builder = builder.CellDuration(CellDuration.Value);

			// Data field mapping
			if (!string.IsNullOrEmpty(TextExpr))
				builder = builder.TextExpr(TextExpr);
			if (!string.IsNullOrEmpty(StartDateExpr))
				builder = builder.StartDateExpr(StartDateExpr);
			if (!string.IsNullOrEmpty(EndDateExpr))
				builder = builder.EndDateExpr(EndDateExpr);
			if (!string.IsNullOrEmpty(DescriptionExpr))
				builder = builder.DescriptionExpr(DescriptionExpr);
			if (!string.IsNullOrEmpty(AllDayExpr))
				builder = builder.AllDayExpr(AllDayExpr);
			if (!string.IsNullOrEmpty(RecurrenceRuleExpr))
				builder = builder.RecurrenceRuleExpr(RecurrenceRuleExpr);
			if (!string.IsNullOrEmpty(RecurrenceExceptionExpr))
				builder = builder.RecurrenceExceptionExpr(RecurrenceExceptionExpr);

			// Display and behavior options
			builder = builder.ShowAllDayPanel(ShowAllDayPanel);
			builder = builder.ShowCurrentTimeIndicator(ShowCurrentTimeIndicator);
			builder = builder.ShadeUntilCurrentTime(ShadeUntilCurrentTime);
			builder = builder.UseDropDownViewSwitcher(UseDropDownViewSwitcher);
			builder = builder.CrossScrollingEnabled(IsCrossScrollingEnabled);

			// Appointment template
			if (!string.IsNullOrEmpty(AppointmentTemplate))
				builder = builder.AppointmentTemplate(AppointmentTemplate);
			else if (!string.IsNullOrEmpty(AppointmentTemplateJS))
				builder = builder.AppointmentTemplate(new JS(AppointmentTemplateJS));
			else if (AppointmentTemplateNT != null)
				builder = builder.AppointmentTemplate(new TemplateName(AppointmentTemplateNT));
			else if (templateContext.AppointmentContent != null)
				builder = builder.AppointmentTemplate(ToString(templateContext.AppointmentContent));

			// Appointment form template
			// This is different, we use the form template for JavaScript to create the template in 
			if (!string.IsNullOrEmpty(AppointmentFormTemplate))
				builder = builder.Option("customFormTemplate", AppointmentFormTemplate);
			else if (!string.IsNullOrEmpty(AppointmentFormTemplateJS))
				builder = builder.Option("customFormTemplate", new JS(AppointmentFormTemplateJS));
			else if (!string.IsNullOrEmpty(AppointmentFormTemplateNT))
				builder = builder.Option("customFormTemplate", new TemplateName(AppointmentFormTemplateNT));
			else if (itemsContext.FormTemplateContent != null)
				builder = builder.Option("customFormTemplate", ToString(itemsContext.FormTemplateContent));            
	
			// Appointment collector template
			// TODO

			// Appointment tooltip
			if (!string.IsNullOrEmpty(AppointmentTooltipTemplate))
				builder = builder.AppointmentTooltipTemplate(AppointmentTooltipTemplate);
			else if (!string.IsNullOrEmpty(AppointmentTooltipTemplateJS))
				builder = builder.AppointmentTooltipTemplate(new JS(AppointmentTooltipTemplateJS));
			else if (!string.IsNullOrEmpty(AppointmentTooltipTemplateNT))
				builder = builder.AppointmentTooltipTemplate(new TemplateName(AppointmentTooltipTemplateNT));
			else if (templateContext.AppointmentTooltipContent != null)
				builder = builder.AppointmentTemplate(ToString(templateContext.AppointmentTooltipContent));

			// Data cell template
			if (!string.IsNullOrEmpty(DataCellTemplate))
				builder = builder.DataCellTemplate(DataCellTemplate);
			else if (!string.IsNullOrEmpty(DataCellTemplateJS))
				builder = builder.DataCellTemplate(new JS(DataCellTemplateJS));
			else if (DataCellTemplateNT != null)
				builder = builder.DataCellTemplate(new TemplateName(DataCellTemplateNT));
			else if (itemsContext.CellTemplateContent != null)
				builder = builder.DataCellTemplate(ToString(itemsContext.CellTemplateContent));

			// Date header template
			// if (!string.IsNullOrEmpty(DateHeaderTemplate))
			//	builder = builder.DateHeaderTemplate(DateHeaderTemplate);
			// else if (!string.IsNullOrEmpty(DateHeaderTemplateJS))
			//	builder = builder.DateHeaderTemplate(new JS(DateHeaderTemplateJS));

			// Time cell template
			if (!string.IsNullOrEmpty(TimeCellTemplate))
				builder = builder.TimeCellTemplate(TimeCellTemplate);
			else if (!string.IsNullOrEmpty(TimeCellTemplateJS))
				builder = builder.TimeCellTemplate(new JS(TimeCellTemplateJS));
			else if (TimeCellTemplateNT != null)
				builder = builder.TimeCellTemplate(new TemplateName(TimeCellTemplateNT));
			else if (itemsContext.ItemTemplateContent != null)
				builder = builder.TimeCellTemplate(ToString(itemsContext.ItemTemplateContent));

			return builder;
		}

		private SchedulerBuilder ProcessDataSource(SchedulerBuilder builder, DataSourceContext sourceContext, TagHelperContext context)
		{
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

			return builder;
		}

		private SchedulerBuilder ProcessViews(SchedulerBuilder builder)
		{
			if (Views != null && Views.Any())
			{
				builder = builder.Views(Views);
			}
			else
			{
				// Set default views if none specified
				builder = builder.Views(new[] {
					SchedulerViewType.Day,
					SchedulerViewType.Week,
					SchedulerViewType.Month
				});
			}

			return builder;
		}

		private SchedulerBuilder ProcessEditing(SchedulerBuilder builder)
		{
			// Editing configuration
			if (AllowAdding.HasValue || AllowUpdating.HasValue || AllowDeleting.HasValue || AllowDragging.HasValue || AllowResizing.HasValue)
			{
				builder = builder.Editing(editing =>
				{
					if (AllowAdding.HasValue)
						editing = editing.AllowAdding(AllowAdding.Value);
					if (AllowUpdating.HasValue)
						editing = editing.AllowUpdating(AllowUpdating.Value);
					if (AllowDeleting.HasValue)
						editing = editing.AllowDeleting(AllowDeleting.Value);
					if (AllowDragging.HasValue)
						editing = editing.AllowDragging(AllowDragging.Value);
					if (AllowResizing.HasValue)
						editing = editing.AllowResizing(AllowResizing.Value);
				});
			}

			return builder;
		}

	private SchedulerBuilder ProcessEvents(SchedulerBuilder builder)
		{
			// Core events
			if (!string.IsNullOrEmpty(OnContentReady))
				builder = builder.OnContentReady(OnContentReady);
			if (!string.IsNullOrEmpty(OnInitialized))
				builder = builder.OnInitialized(OnInitialized);
			if (!string.IsNullOrEmpty(OnOptionChanged))
				builder = builder.OnOptionChanged(OnOptionChanged);

			// Appointment events
			if (!string.IsNullOrEmpty(OnAppointmentAdding))
				builder = builder.OnAppointmentAdding(OnAppointmentAdding);
			if (!string.IsNullOrEmpty(OnAppointmentAdded))
				builder = builder.OnAppointmentAdded(OnAppointmentAdded);
			if (!string.IsNullOrEmpty(OnAppointmentUpdating))
				builder = builder.OnAppointmentUpdating(OnAppointmentUpdating);
			if (!string.IsNullOrEmpty(OnAppointmentUpdated))
				builder = builder.OnAppointmentUpdated(OnAppointmentUpdated);
			if (!string.IsNullOrEmpty(OnAppointmentDeleting))
				builder = builder.OnAppointmentDeleting(OnAppointmentDeleting);
			if (!string.IsNullOrEmpty(OnAppointmentDeleted))
				builder = builder.OnAppointmentDeleted(OnAppointmentDeleted);

			// Interaction events
			if (!string.IsNullOrEmpty(OnAppointmentClick))
				builder = builder.OnAppointmentClick(OnAppointmentClick);
			if (!string.IsNullOrEmpty(OnAppointmentDblClick))
				builder = builder.OnAppointmentDblClick(OnAppointmentDblClick);
			if (!string.IsNullOrEmpty(OnCellClick))
				builder = builder.OnCellClick(OnCellClick);
			if (!string.IsNullOrEmpty(OnCellContextMenu))
				builder = builder.OnCellContextMenu(OnCellContextMenu);

			// Form events
			if (!string.IsNullOrEmpty(OnAppointmentFormOpening))
				builder = builder.OnAppointmentFormOpening(OnAppointmentFormOpening);
			// if (!string.IsNullOrEmpty(OnAppointmentFormClosed))
			//	builder = builder.OnAppointmentFormClosed(OnAppointmentFormClosed);

			// Rendering events
			if (!string.IsNullOrEmpty(OnAppointmentRendered))
				builder = builder.OnAppointmentRendered(OnAppointmentRendered);
			if (!string.IsNullOrEmpty(OnAppointmentTooltipShowing))
				builder = builder.OnAppointmentTooltipShowing(OnAppointmentTooltipShowing);

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTagHelper properties: core configuration
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the current date of the scheduler.
		/// </summary>
		[HtmlAttributeName("date")]
		public DateTime? CurrentDate { get; set; }

		/// <summary>
		/// Get or set the current view of the scheduler.
		/// </summary>
		[HtmlAttributeName("view")]
		public SchedulerViewType? CurrentView { get; set; }

		/// <summary>
		/// Get or set the available views for the scheduler.
		/// </summary>
		[HtmlAttributeName("views")]
		public IEnumerable<SchedulerViewType> Views { get; set; }

		/// <summary>
		/// Get or set the first day of the week.
		/// </summary>
		[HtmlAttributeName("first")]
		public FirstDayOfWeek? FirstDayOfWeek { get; set; }

		/// <summary>
		/// Get or set the time zone for the scheduler.
		/// </summary>
		[HtmlAttributeName("timezone")]
		public string TimeZone { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTagHelper properties: time configuration
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the start hour of the day (0-23).
		/// </summary>
		[HtmlAttributeName("start-hour")]
		public int? StartDayHour { get; set; }

		/// <summary>
		/// Get or set the end hour of the day (0-23).
		/// </summary>
		[HtmlAttributeName("end-hour")]
		public int? EndDayHour { get; set; }

		/// <summary>
		/// Get or set the cell duration in minutes.
		/// </summary>
		[HtmlAttributeName("duration")]
		public int? CellDuration { get; set; }

		/// <summary>
		/// Get or set whether to show the all-day panel.
		/// </summary>
		[HtmlAttributeName("show-all")]
		public bool ShowAllDayPanel { get; set; } = true;

		/// <summary>
		/// Get or set whether to show the current time indicator.
		/// </summary>
		[HtmlAttributeName("show-current")]
		public bool ShowCurrentTimeIndicator { get; set; } = true;

		/// <summary>
		/// Get or set whether to shade time until current time.
		/// </summary>
		[HtmlAttributeName("shade-until-current")]
		public bool ShadeUntilCurrentTime { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTagHelper properties: data field mapping
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the name of the data field containing appointment text.
		/// </summary>
		[HtmlAttributeName("text-expr")]
		public string TextExpr { get; set; } = "text";

		/// <summary>
		/// Get or set the name of the data field containing start date.
		/// </summary>
		[HtmlAttributeName("start-expr")]
		public string StartDateExpr { get; set; } = "startDate";

		/// <summary>
		/// Get or set the name of the data field containing end date.
		/// </summary>
		[HtmlAttributeName("end-expr")]
		public string EndDateExpr { get; set; } = "endDate";

		/// <summary>
		/// Get or set the name of the data field containing description.
		/// </summary>
		[HtmlAttributeName("description-expr")]
		public string DescriptionExpr { get; set; } = "description";

		/// <summary>
		/// Get or set the name of the data field containing all-day flag.
		/// </summary>
		[HtmlAttributeName("all-day-expr")]
		public string AllDayExpr { get; set; } = "allDay";

		/// <summary>
		/// Get or set the name of the data field containing recurrence rule.
		/// </summary>
		[HtmlAttributeName("recurrence-rule-expr")]
		public string RecurrenceRuleExpr { get; set; } = "recurrenceRule";

		/// <summary>
		/// Get or set the name of the data field containing recurrence exception.
		/// </summary>
		[HtmlAttributeName("recurrence-exception-expr")]
		public string RecurrenceExceptionExpr { get; set; } = "recurrenceException";

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTagHelper properties: editing
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set whether adding appointments is allowed.
		/// </summary>
		[HtmlAttributeName("allow-adding")]
		public bool? AllowAdding { get; set; }

		/// <summary>
		/// Get or set whether updating appointments is allowed.
		/// </summary>
		[HtmlAttributeName("allow-updating")]
		public bool? AllowUpdating { get; set; }

		/// <summary>
		/// Get or set whether deleting appointments is allowed.
		/// </summary>
		[HtmlAttributeName("allow-deleting")]
		public bool? AllowDeleting { get; set; }

		/// <summary>
		/// Get or set whether dragging appointments is allowed.
		/// </summary>
		[HtmlAttributeName("allow-dragging")]
		public bool? AllowDragging { get; set; }

		/// <summary>
		/// Get or set whether resizing appointments is allowed.
		/// </summary>
		[HtmlAttributeName("allow-resizing")]
		public bool? AllowResizing { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTagHelper properties: appearance
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set whether to use dropdown view switcher.
		/// </summary>
		[HtmlAttributeName("dropdown-view-switcher")]
		public bool UseDropDownViewSwitcher { get; set; } = true;

		/// <summary>
		/// Get or set whether cross-scrolling is enabled.
		/// </summary>
		[HtmlAttributeName("cross-scrolling")]
		public bool IsCrossScrollingEnabled { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTagHelper properties: templates
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the appointment template.
		/// </summary>
		[HtmlAttributeName("appointment-template")]
		public string AppointmentTemplate { get; set; }

		/// <summary>
		/// Get or set the appointment template as JavaScript.
		/// </summary>
		[HtmlAttributeName("appointment-template-js")]
		public string AppointmentTemplateJS { get; set; }

		/// <summary>
		/// Get or set the appointment template as named template.
		/// </summary>
		[HtmlAttributeName("appointment-template-nt")]
		public string AppointmentTemplateNT { get; set; }

		/// <summary>
		/// Get or set the appointment form template.
		/// </summary>
		[HtmlAttributeName("form-template")]
		public string AppointmentFormTemplate { get; set; }

		/// <summary>
		/// Get or set the appointment form template as JavaScript.
		/// </summary>
		[HtmlAttributeName("form-template-js")]
		public string AppointmentFormTemplateJS { get; set; }

		/// <summary>
		/// Get or set the appointment form template as named template.
		/// </summary>
		[HtmlAttributeName("form-template-nt")]
		public string AppointmentFormTemplateNT { get; set; }    
		
		/// <summary>
																/// Get or set the appointment tooltip template.
																/// </summary>
		[HtmlAttributeName("appointment-tooltip-template")]
		public string AppointmentTooltipTemplate { get; set; }

		/// <summary>
		/// Get or set the appointment tooltip template as JavaScript.
		/// </summary>
		[HtmlAttributeName("appointment-tooltip-template-js")]
		public string AppointmentTooltipTemplateJS { get; set; }

		/// <summary>
		/// Get or set the appointment tooltip template as named template.
		/// </summary>
		[HtmlAttributeName("appointment-tooltip-template-nt")]
		public string AppointmentTooltipTemplateNT { get; set; }

		/// <summary>
		/// Get or set the data cell template.
		/// </summary>
		[HtmlAttributeName("cell-template")]
		public string DataCellTemplate { get; set; }

		/// <summary>
		/// Get or set the data cell template as JavaScript.
		/// </summary>
		[HtmlAttributeName("cell-template-js")]
		public string DataCellTemplateJS { get; set; }

		/// <summary>
		/// Get or set the data cell template as named template.
		/// </summary>
		[HtmlAttributeName("cell-template-nt")]
		public string DataCellTemplateNT { get; set; }

		/// <summary>
		/// Get or set the date header template.
		/// </summary>
		[HtmlAttributeName("date-header-template")]
		public string DateHeaderTemplate { get; set; }

		/// <summary>
		/// Get or set the date header template as JavaScript.
		/// </summary>
		[HtmlAttributeName("date-header-template-js")]
		public string DateHeaderTemplateJS { get; set; }

		/// <summary>
		/// Get or set the time cell template.
		/// </summary>
		[HtmlAttributeName("time-cell-template")]
		public string TimeCellTemplate { get; set; }

		/// <summary>
		/// Get or set the time cell template as JavaScript.
		/// </summary>
		[HtmlAttributeName("time-cell-template-js")]
		public string TimeCellTemplateJS { get; set; }

		/// <summary>
		/// Get or set the time cell template as named template.
		/// </summary>
		[HtmlAttributeName("time-template-nt")]
		public string TimeCellTemplateNT { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region SchedulerTagHelper properties: events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the JavaScript method to be called when content is ready.
		/// </summary>
		[HtmlAttributeName("content-ready")]
		public string OnContentReady { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when the scheduler is initialized.
		/// </summary>
		[HtmlAttributeName("initialized")]
		public string OnInitialized { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an option changes.
		/// </summary>
		[HtmlAttributeName("option-changed")]
		public string OnOptionChanged { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment is being added.
		/// </summary>
		[HtmlAttributeName("adding")]
		public string OnAppointmentAdding { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment has been added.
		/// </summary>
		[HtmlAttributeName("added")]
		public string OnAppointmentAdded { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment is being updated.
		/// </summary>
		[HtmlAttributeName("updating")]
		public string OnAppointmentUpdating { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment has been updated.
		/// </summary>
		[HtmlAttributeName("updated")]
		public string OnAppointmentUpdated { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment is being deleted.
		/// </summary>
		[HtmlAttributeName("deleting")]
		public string OnAppointmentDeleting { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment has been deleted.
		/// </summary>
		[HtmlAttributeName("deleted")]
		public string OnAppointmentDeleted { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment is clicked.
		/// </summary>
		[HtmlAttributeName("click")]
		public string OnAppointmentClick { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment is double-clicked.
		/// </summary>
		[HtmlAttributeName("double-click")]
		public string OnAppointmentDblClick { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when a cell is clicked.
		/// </summary>
		[HtmlAttributeName("cell-click")]
		public string OnCellClick { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when a cell context menu is opened.
		/// </summary>
		[HtmlAttributeName("context-menu")]
		public string OnCellContextMenu { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when the appointment form is opening.
		/// </summary>
		[HtmlAttributeName("form-opening")]
		public string OnAppointmentFormOpening { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when the appointment form is closed.
		/// </summary>
		[HtmlAttributeName("form-closed")]
		public string OnAppointmentFormClosed { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment is rendered.
		/// </summary>
		[HtmlAttributeName("rendered")]
		public string OnAppointmentRendered { get; set; }

		/// <summary>
		/// Get or set the JavaScript method to be called when an appointment tooltip is showing.
		/// </summary>
		[HtmlAttributeName("tooltip-showing")]
		public string OnAppointmentTooltipShowing { get; set; }

		#endregion
	}
}
