using Amaqele.Common.Base;
using Amaqele.Common.Collections;
using Amaqele.Common.Types;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Builders.DataSources;
using DevExtreme.AspNet.Mvc.Factories;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;

namespace Alcazar.Web.Extensibility
{
	public class DataSourceTagHelperBase : RouteTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelperBase methods: data source builder
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Build the data source. THis method will typically be called by the parent tag.
		/// </summary>
		/// <param name="factory"> The data source factory. </param>
		/// <returns> The data source builder.</returns>
		public StoreDataSourceBuilder BuildDatasource(DataSourceFactory factory)
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

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelperBase methods: generic builders
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		private StoreDataSourceBuilder BuildStaticJson(DataSourceFactory factory)
		{
			var options = factory.StaticJson();

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key.SplitSafe());

			return options;
		}

		private StoreDataSourceBuilder BuildArray(DataSourceFactory factory)
		{
			ArrayDataSourceBuilder options = factory.Array();

			options = options.Data(Items);

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key.SplitSafe());

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

			if (!string.IsNullOrEmpty(OnLoading))
				options = options.OnLoading(OnLoading);
			if (!string.IsNullOrEmpty(OnLoaded))
				options = options.OnLoaded(OnLoaded);
			if (!string.IsNullOrEmpty(OnPush))
				options = options.OnPush(OnPush);

			return options;
		}

		/// <summary>
		/// Build an MVC datasource, which must reside in the current application.
		/// </summary>
		private StoreDataSourceBuilder BuildMvc(DataSourceFactory factory)
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
				options = options.Key(Key.SplitSafe());

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

			if (!string.IsNullOrEmpty(OnBeforeSend))
				options = options.OnBeforeSend(OnBeforeSend);

			if (!string.IsNullOrEmpty(OnLoading))
				options = options.OnLoading(OnLoading);
			if (!string.IsNullOrEmpty(OnLoaded))
				options = options.OnLoaded(OnLoaded);
			if (!string.IsNullOrEmpty(OnPush))
				options = options.OnPush(OnPush);

			return options;
		}

		private StoreDataSourceBuilder BuildOData(DataSourceFactory factory)
		{
			ODataSourceBuilder options = factory.OData();

			// TODO NOT here, Add load parameters
			if (LoadParams.Any())
			{
				//ExpandoObject loadParams = new ExpandoObject();
				//loadParams.AddRange(LoadParams);
				//options = options.LoadParams(loadParams);
			}

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key.SplitSafe());

			// No Area/Controller set here, it is all included in the ODate URL
			options = options.Url(BaseUrl);

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

			if (!string.IsNullOrEmpty(OnLoading))
				options = options.OnLoading(OnLoading);
			if (!string.IsNullOrEmpty(OnLoaded))
				options = options.OnLoaded(OnLoaded);
			if (!string.IsNullOrEmpty(OnPush))
				options = options.OnPush(OnPush);

			return options;
		}

		/// <summary>
		/// Build a remote datasource, which may reside outside the current application.
		/// </summary>
		private StoreDataSourceBuilder BuildRemoteController(DataSourceFactory factory)
		{
			// Create options and its method 
			// Set load action
			RemoteControllerDataSourceOptionsBuilder options = factory.RemoteController().LoadMethod(Method);

			// Add load parameters
			if (LoadParams.Any())
			{
				ExpandoObject loadParams = new ExpandoObject();
				loadParams.AddRange(LoadParams);
				options = options.LoadParams(loadParams);
			}

			if (!string.IsNullOrEmpty(Key))
				options = options.Key(Key.SplitSafe());

			// No Area/Controller set here, it is all included in the URL, but we may have a BaseUrl
			// Set load action
			options = options.LoadUrl(CombineBaseUrl(BaseUrl, Action));

			// Set editing actions
			if (!string.IsNullOrEmpty(InsertAction))
				options = options.InsertUrl(CombineBaseUrl(BaseUrl, InsertAction));
			if (!string.IsNullOrEmpty(UpdateAction))
				options = options.UpdateUrl(CombineBaseUrl(BaseUrl, UpdateAction));
			if (!string.IsNullOrEmpty(DeleteAction))
				options = options.DeleteUrl(CombineBaseUrl(BaseUrl, DeleteAction));

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

			if (!string.IsNullOrEmpty(OnBeforeSend))
				options = options.OnBeforeSend(OnBeforeSend);

			if (!string.IsNullOrEmpty(OnLoading))
				options = options.OnLoading(OnLoading);
			if (!string.IsNullOrEmpty(OnLoaded))
				options = options.OnLoaded(OnLoaded);
			if (!string.IsNullOrEmpty(OnPush))
				options = options.OnPush(OnPush);

			return options;
		}

		private string CombineBaseUrl(string baseUrl, string action)
		{
			// No base URL, just return the action
			if (string.IsNullOrEmpty(baseUrl))
				return action;

			// Combine the base URL with the action
			return baseUrl.EndsWith("/") ? $"{baseUrl}{action}" : $"{baseUrl}/{action}";
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
		/// Get or set the name or names of the key properties of datasource items.
		/// Multiple names are space separated.
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

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region DataSourceTagHelper properties: events
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		/// <summary>
		/// Get or set the JS function to call when the datasource is loading.
		/// </summary>
		[HtmlAttributeName("loading")]
		public string OnLoading { get; set; }

		/// <summary>
		/// Get or set the JS function to call when the datasource has finished loading.
		/// </summary>
		[HtmlAttributeName("loaded")]
		public string OnLoaded { get; set; }

		/// <summary>
		/// Get or set the JS function to call when a data operation fails.
		/// </summary>
		[HtmlAttributeName("error")]
		public string OnError { get; set; }

		/// <summary>
		/// Get or set the JS function to call when data is pushed from the server.
		/// </summary>
		[HtmlAttributeName("push")]
		public string OnPush { get; set; }

		/// <summary>
		/// Get or set the JS function to customize store load options before sending to the server.
		/// </summary>
		[HtmlAttributeName("customize")]
		public string OnCustomizeStoreLoadOptions { get; set; }

		/// <summary>
		/// Get or set the JS function to call when the data in the store changes.
		/// </summary>
		[HtmlAttributeName("datachanged")]
		public string OnDataChanged { get; set; }

		/// <summary>                   
		/// Get or set the JS function to call when a new item is being inserted.
		/// </summary>
		[HtmlAttributeName("inserting")]
		public string OnInserting { get; set; }

		/// <summary>
		/// Get or set the JS function to call when a new item has been inserted.
		/// </summary>
		[HtmlAttributeName("inserted")]
		public string OnInserted { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an item is being updated.
		/// </summary>
		[HtmlAttributeName("updating")]
		public string OnUpdating { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an item has been updated.
		/// </summary>
		[HtmlAttributeName("updated")]
		public string OnUpdated { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an item is being removed.
		/// </summary>
		[HtmlAttributeName("removing")]
		public string OnRemoving { get; set; }

		/// <summary>
		/// Get or set the JS function to call when an item has been removed.
		/// </summary>
		[HtmlAttributeName("removed")]
		public string OnRemoved { get; set; }

		/// <summary>
		/// Get or set the JS function to call before a data call is sent.
		/// </summary>
		[HtmlAttributeName("before-send")]
		public string OnBeforeSend { get; set; }

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

	public class ListControlTagHelperBase : DataSourceTagHelperBase
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ListControlTagHelperBase protected methods
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		protected string RemoveNoname(string value)
		{
			// This is a horrible hack for no.QualifiedName, when we try to obtain the asp-for prop from an IEnumerable model
			string[] parts = value.Split('.');

			// If the string starts with noType.Name, remove that first part
			if (parts.Length > 1 && parts[0].StartsWith("no"))
				return string.Join('.', parts.Skip(1));

			return value;
		}

		protected GridColumnDataType? ToDataType(Type type)
		{
			PrimitiveTypeCode code = PrimitiveType.FromNullableType(type);
			switch (code)
			{
				default:
				case PrimitiveTypeCode.None: return null;

				case PrimitiveTypeCode.Int8:
				case PrimitiveTypeCode.UInt8:
				case PrimitiveTypeCode.Int16:
				case PrimitiveTypeCode.UInt16:
				case PrimitiveTypeCode.Int32:
				case PrimitiveTypeCode.UInt32:
				case PrimitiveTypeCode.Int64:
				case PrimitiveTypeCode.UInt64:
				case PrimitiveTypeCode.Float:
				case PrimitiveTypeCode.Double:
				case PrimitiveTypeCode.Decimal: return GridColumnDataType.Number;
				case PrimitiveTypeCode.Enumeration: return GridColumnDataType.String;

				case PrimitiveTypeCode.Bool: return GridColumnDataType.Boolean;

				case PrimitiveTypeCode.Char: return GridColumnDataType.String;
				case PrimitiveTypeCode.String: return GridColumnDataType.String;
				case PrimitiveTypeCode.Binary: return GridColumnDataType.String;
				case PrimitiveTypeCode.Base64Binary: return GridColumnDataType.String;
				case PrimitiveTypeCode.HexBinary: return GridColumnDataType.String;
				case PrimitiveTypeCode.Guid: return GridColumnDataType.String;

				case PrimitiveTypeCode.Date: return GridColumnDataType.Date;

				case PrimitiveTypeCode.Time:
				case PrimitiveTypeCode.Timestamp:
				case PrimitiveTypeCode.Timespan: return GridColumnDataType.DateTime;

				case PrimitiveTypeCode.Object: return GridColumnDataType.Object;
			}
		}

		#endregion
	}
}
