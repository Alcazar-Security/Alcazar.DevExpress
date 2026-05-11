using Alcazar.Common.Base;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="FileUploaderTagHelper"/> type implements a file uploader.
	/// </summary>
	[HtmlTargetElement("dx-file")]
	public class FileUploaderTagHelper : EditorTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region FileUploaderTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public FileUploaderTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;

		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region FileUploaderTagHelper overrides
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
			FileUploaderBuilder builder = _htmlHelper.DevExtreme().FileUploader();

			// Apply the control context, which is values which an outer dx-field or dx-control tag might want to pass into me, the editor
			ApplyControlContext(context);

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Process the For attribute, if it is set
			// object value = ProcessFor();

			// Apply the For attribute, or the corresponding direct values
			builder = ApplyFor(builder, Value);

			// Process the title/hint, if it is set
			//if (!string.IsNullOrEmpty(Title))
			//{
			//	string title = TranslateToProp(Title, ViewContext);
			//	builder.Hint(title);
			//}

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

			// Process file uploader specific properties
			builder = builder.UploadMode(Mode);

			// Set the label (select button) text (From LabelText, Name, in that order)
			string labelText = TranslateToProp(LabelText, ViewContext);
			string selectText = TranslateToProp(SelectText, ViewContext);
			string uploadText = TranslateToProp(UploadText, ViewContext);

			if (!string.IsNullOrEmpty(labelText))
				builder = builder.LabelText(labelText);
			if (!string.IsNullOrEmpty(selectText))
				builder = builder.SelectButtonText(selectText);
			if (!string.IsNullOrEmpty(uploadText))
				builder = builder.UploadButtonText(uploadText);

			if (!string.IsNullOrEmpty(Extensions))
			{
				if (Extensions == "#common")
					Extensions = _commonExts;

				builder = builder.AllowedFileExtensions(Extensions.Split(','));
			}
			else
				builder = builder.Accept(MimeTypes);

			// Process events
			builder = ProcessEvents(builder);

			// Render the builder (into the content)
			Render(context, output.Content, builder);
		}

		private FileUploaderBuilder ProcessCommon(FileUploaderBuilder builder)
		{
			// Set the ID to a random value
			string idValue = ID ?? Guid.NewGuid().ToString();
			builder = builder.ID(idValue);

			if (!string.IsNullOrEmpty(Name))
				builder = builder.Name(Name);

			// Set the width and height
			if (!string.IsNullOrEmpty(Width))
				builder = builder.Width(Width);

			return builder;
		}

		private FileUploaderBuilder ProcessAttributes(FileUploaderBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// And we are allowing attributes on the input field also
			foreach (var attr in InputAttributes)
				builder = builder.InputAttr(attr.Key, attr.Value?.ToString());

			return builder;
		}

		private FileUploaderBuilder ProcessEvents(FileUploaderBuilder builder)
		{
			// Process file uploader events
			if (!string.IsNullOrEmpty(OnValueChanged))
				builder = builder.OnValueChanged(OnValueChanged);

			if (!string.IsNullOrEmpty(OnFilesUploaded))
				builder = builder.OnFilesUploaded(OnFilesUploaded);

			if (!string.IsNullOrEmpty(OnUploadFailed))
				builder = builder.OnUploadError(OnUploadFailed);

			if (!string.IsNullOrEmpty(OnProgress))
				builder = builder.OnProgress(OnProgress);

			if (!string.IsNullOrEmpty(OnUploadStarted))
				builder = builder.OnUploadStarted(OnUploadStarted);
	
			return builder;
		}

		private FileUploaderBuilder ApplyFor(FileUploaderBuilder builder, IEnumerable value)
		{
			// Setting the value as direct bool
			if (value != null)
				builder = builder.Value(value);

			return builder;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region FileUploaderTagHelper properties: tag helper
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the value to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("value")]
		public IEnumerable Value { get; set; }

		/// <summary>
		/// Get or set the text for the label if this file uploader.
		/// This label is specific to the file uploader, and does not relate to the label of the label/control pair.
		/// </summary>
		[HtmlAttributeName("label-text")]
		public string LabelText { get; set; }

		/// <summary>
		/// Get or set the text for the select button if this file uploader.
		/// </summary>
		[HtmlAttributeName("select-text")]
		public string SelectText { get; set; }

		/// <summary>
		/// Get or set the text for the upload button if this file uploader.
		/// </summary>
		[HtmlAttributeName("upload-text")]
		public string UploadText { get; set; }

		/// <summary>
		/// Get or set the mime types (s), comma separated, which this file uploader should accept.
		/// </summary>
		[HtmlAttributeName("accept")]
		public string MimeTypes { get; set; } = _commonAccept;

		/// <summary>
		/// Get or set the mime types (s), comma separated, which this file uploader should accept.
		/// </summary>
		[HtmlAttributeName("extensions")]
		public string Extensions { get; set; }

		/// <summary>
		/// Get or set the upload mode of the control. Defaults to <see cref="FileUploadMode.UseForm"/>.
		/// <see cref="FileUploadMode.UseForm"/> uploads the 
		/// </summary>
		[HtmlAttributeName("mode")]
		public FileUploadMode Mode { get; set; } = FileUploadMode.UseForm;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region FileUploaderTagHelper properties: events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the JS function called when all files have been successfully uploaded.
		/// </summary>
		[HtmlAttributeName("uploaded")]
		public string OnFilesUploaded { get; set; }

		/// <summary>
		/// Get or set the JS function called when a file upload fails.
		/// </summary>
		[HtmlAttributeName("upload-failed")]
		public string OnUploadFailed { get; set; }

		/// <summary>
		/// Get or set the JS function called during file upload to report progress.
		/// </summary>
		[HtmlAttributeName("progress")]
		public string OnProgress { get; set; }

		/// <summary>
		/// Get or set the JS function called when a file upload starts.
		/// </summary>
		[HtmlAttributeName("upload-started")]
		public string OnUploadStarted { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region FileUploaderTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		
		const string _commonExts = "txt,csv,dat,bin,pdf,xls,xlsx,xml,xsl,html,cshtml,js,css,jpg,jpeg,png,gif,bmp";
		const string _commonAccept = $"{ContentTypes.PlainText},{ContentTypes.Csv},{ContentTypes.Xls},{ContentTypes.Xlsx},{ContentTypes.Pdf},{ContentTypes.Xml},{ContentTypes.Html},{ContentTypes.XHtml},{ContentTypes.JavaScript},{ContentTypes.Css},{ContentTypes.Cshtml},{ContentTypes.Images}";

		#endregion
	}
}
