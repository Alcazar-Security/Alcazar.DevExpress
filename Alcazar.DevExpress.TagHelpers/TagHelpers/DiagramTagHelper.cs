using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="DiagramTagHelper"/> type implements a diagram.
	/// </summary>
	[HtmlTargetElement("dx-diagram")]
	public class DiagramTagHelper : DataSourceTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region RadioGroupTagHelper construction
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public DiagramTagHelper(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper as Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
		}

		private readonly Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper _htmlHelper;

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DiagramTagHelper overrides
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

			// Create the builder for a radio group
			DiagramBuilder builder = _htmlHelper.DevExtreme().Diagram();

			// Process common functionality for editors
			builder = ProcessCommon(builder);

			// Process non-tag attriubtes
			builder = ProcessAttributes(builder, output.Attributes);

			// Create the context, so that we can pass it to child tag helpers
			DataSourceContext sourceContext = GetOrCreateContext<DataSourceContext>(context);

			// Process children of the diagram tag
			IHtmlContent content = await output.GetChildContentAsync();

			builder = builder.Nodes(ns =>
			{
				// Set the data source
				if (sourceContext.Datasource != null)
				{
					// First preference, process the (child) data source
					DiagramNodesBuilder builder = ns.DataSource(d => sourceContext.Datasource.BuildDatasource(d));

					builder = builder.KeyExpr("ID")
						.TextExpr("Title")
						//.ParentKeyExpr("PreviousID")
						//.ContainerKeyExpr("BranchID")
						.AutoLayout(al => al.Type(DiagramDataLayoutType.Auto));
				}
				else
				{
					// Second preference, process the datasource from my own properties
					DiagramNodesBuilder builder = ns.DataSource(d => BuildDatasource(d));
				}
				
				//DiagramNodesBuilder builderXx = ns.DataSource(d => d.Mvc()
					//.Controller("DiagramEmployees")
					//.LoadAction("Employees")
					//.InsertAction("InsertEmployee")
					//.UpdateAction("UpdateEmployee")
					//.DeleteAction("DeleteEmployee")
					//.Key("ID")
					//.OnInserting("onInserting")
				//);
			});

			// Render the builder (into the content)
			output.Content.SetHtmlContent(builder);
		}

		private DiagramBuilder ProcessCommon(DiagramBuilder builder)
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

		private DiagramBuilder ProcessAttributes(DiagramBuilder builder, TagHelperAttributeList attributes)
		{
			// We are choosing to place the attributes on the element, not the imput
			foreach (var attr in attributes)
				builder = builder.ElementAttr(attr.Name, attr.Value?.ToString());

			// No option for attributes on the input field here
			return builder;
		}

		#endregion
	}
}
