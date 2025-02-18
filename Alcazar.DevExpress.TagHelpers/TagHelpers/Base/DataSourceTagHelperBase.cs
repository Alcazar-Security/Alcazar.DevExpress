using Amaqele.Common.Collections;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Builders.DataSources;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Alcazar.Web.Extensibility
{
	public class DataSourceTagHelperBase : RouteTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region RouteTagHelperBase methods: data source builder
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Build the data source. THis method will typically be called by the parent tag.
		/// </summary>
		/// <param name="factory"> The data source factory. </param>
		/// <returns> The data source builder.</returns>
		public OptionsOwnerBuilder BuildDatasource(DataSourceFactory factory)
		{
			if (DatasourceType == DataSourceTypes.None)
			{
				// The datasource type is not set, try to determine it from properties
				if (Items != null)
				{
					// Our datasource comes from the model
					DatasourceType = DataSourceTypes.Array;
				}
				else if (!string.IsNullOrEmpty(Action))
				{
					// We have a (load) action, we interpret this to mean load from MVC
					DatasourceType = DataSourceTypes.Mvc;
				}
			}

			// Hopefully now we have a data source, apply it
			switch (DatasourceType)
			{
				default:
				case DataSourceTypes.None:
					throw new NotSupportedException($"Cannot apply data source, {DatasourceType} is not implemented.");

				case DataSourceTypes.StaticJson: return BuildStaticJson(factory);
				case DataSourceTypes.Array: return BuildArray(factory);
				case DataSourceTypes.Mvc: return BuildMvc(factory);
				case DataSourceTypes.OData: return BuildOData(factory);
				case DataSourceTypes.RemoteController: return BuildRemoteController(factory);
			}
		}

		private OptionsOwnerBuilder BuildStaticJson(DataSourceFactory factory)
		{
			var options = factory.StaticJson();

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key);

			return options;
		}

		private OptionsOwnerBuilder BuildArray(DataSourceFactory factory)
		{
			ArrayDataSourceBuilder options = factory.Array();

			options = options.Data(Items);

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key);

			// Set editing actions
			if (!string.IsNullOrEmpty(OnInserting))
				options = options.OnInserting(OnInserting);
			if (!string.IsNullOrEmpty(OnInserted))
				options = options.OnInserted(OnInserted);
			if (!string.IsNullOrEmpty(OnUpdating))
				options = options.OnUpdating(OnUpdating);
			if (!string.IsNullOrEmpty(OnUpdated))
				options = options.OnUpdated(OnUpdated);
			if (!string.IsNullOrEmpty(OnRemoving))
				options = options.OnRemoving(OnRemoving);
			if (!string.IsNullOrEmpty(OnRemoved))
				options = options.OnRemoved(OnRemoved);

			return options;
		}

		private OptionsOwnerBuilder BuildMvc(DataSourceFactory factory)
		{
			// Create options and its method 
			// Set load action
			ControllerDataSourceOptionsBuilder options = factory.Mvc().LoadMethod(Method);

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

			// Set load action
			options = options.LoadAction(Action);

			// Set editing actions
			if (!string.IsNullOrEmpty(InsertAction))
				options = options.InsertAction(InsertAction);
			if (!string.IsNullOrEmpty(UpdateAction))
				options = options.UpdateAction(UpdateAction);
			if (!string.IsNullOrEmpty(DeleteAction))
				options = options.DeleteAction(DeleteAction);

			// Set editing actions
			if (!string.IsNullOrEmpty(OnInserting))
				options = options.OnInserting(OnInserting);
			if (!string.IsNullOrEmpty(OnInserted))
				options = options.OnInserted(OnInserted);
			if (!string.IsNullOrEmpty(OnUpdating))
				options = options.OnUpdating(OnUpdating);
			if (!string.IsNullOrEmpty(OnUpdated))
				options = options.OnUpdated(OnUpdated);
			if (!string.IsNullOrEmpty(OnRemoving))
				options = options.OnRemoving(OnRemoving);
			if (!string.IsNullOrEmpty(OnRemoved))
				options = options.OnRemoved(OnRemoved);

			return options;
		}

		private OptionsOwnerBuilder BuildOData(DataSourceFactory factory)
		{
			var options = factory.OData();
			return options;
		}

		private OptionsOwnerBuilder BuildRemoteController(DataSourceFactory factory)
		{
			var options = factory.RemoteController();
			return options;
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the text message to be used for this popup.
		/// </summary>
		[HtmlAttributeName("type")]
		public DataSourceTypes DatasourceType { get; set; }

		/// <summary>
		/// Get or set the name of the key property of datasource items.
		/// </summary>
		[HtmlAttributeName("key")]
		public string Key { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper properties: Array
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the sequence of items to be displayed in this control.
		/// </summary>
		[HtmlAttributeName("asp-items")]
		public System.Collections.IEnumerable Items { get; set; }

		/// <summary>
		/// Get or set the JS function to call when a new item is being inserted.
		/// </summary>
		[HtmlAttributeName("oninserting")]
		public string OnInserting { get; set; }

		/// <summary>
		/// Get or set the JS function to call when a new item has been inserted.
		/// </summary>
		[HtmlAttributeName("oninserted")]
		public string OnInserted { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an item is being updated.
		/// </summary>
		[HtmlAttributeName("onupdating")]
		public string OnUpdating { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an item has been updated.
		/// </summary>
		[HtmlAttributeName("onupdated")]
		public string OnUpdated { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an item is being removed.
		/// </summary>
		[HtmlAttributeName("onremoving")]
		public string OnRemoving { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an item has been removed.
		/// </summary>
		[HtmlAttributeName("onremoved")]
		public string OnRemoved { get; set; }

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper properties: MVC
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set parameters used for loading of data records from the data source.
		/// </summary>
		[HtmlAttributeName(DictionaryAttributePrefix = "load-param-")]
		public IDictionary<string, object> LoadParams { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Get or set the HTTP method to be used for the load and CRUD actions.
		/// Defaults to POST.
		/// </summary>
		[HtmlAttributeName("method")]
		public string Method { get; set; } = HttpMethod.Post.ToString();

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

		#endregion
	}
}
