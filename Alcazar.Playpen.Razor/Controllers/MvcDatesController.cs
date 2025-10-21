using Alcazar.Playpen.Models;
using Alcazar.Web.Utilities;
using Amaqele.Common.Base;
using Amaqele.Common.Types;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Text.Json;

namespace Alcazar.Playpen.Razor.Controllers
{
	/// <summary>
	/// ODATa controller for Dategrid/Dates
	/// Challenged because it serialises dates to local time, which is not what we want.
	/// </summary>
	public class DataGridDatesController : Controller
	{
		public DataGridDatesController()
		{
		}

		public IActionResult Index()
		{
			return Ok();
		}

		[HttpPost]
		public object GetDates(DataSourceLoadOptions loadOptions, DateTime? searchFrom, DateTime? searchUntil, int? max)
		{
			try
			{
				int maxRecords = max ?? 300;

				IEnumerable<DateItem> model = BuildDateItems();

				ToViewTime(model);
				return GetLoadResult(model);
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		private IEnumerable<DateItem> BuildDateItems()
		{
			// Generate sample data
			List<DateItem> dataList = new List<DateItem>();

			// Create a UTC base date, as if we get it from the database, adjusted for what it is (UTC)
			DateTime baseDate = DateTime.SpecifyKind(new DateTime(2000, 2, 14), DateTimeKind.Utc);
			TimeSpan baseTime = TimeSpan.FromHours(12);

			for (int i = 0; i < 5; i++)
			{
				DateTime currentDate = baseDate.AddDays(i);
				DateTime currentDateTime = currentDate.AddHours(12);

				DateItem item = new DateItem
				{
					Id = i + 1,
					LocalDateTime = currentDateTime,
					UniversalDateTime = currentDateTime,

					LocalDate = currentDate,
					UniversalDate = currentDate,

					Day = currentDate,

					LocalTime = currentDateTime,
					UniversalTime = currentDateTime,

					LocalTimespan = baseTime,
					UniversalTimespan = baseTime,

					Date = currentDateTime,
					DateTime = currentDateTime,
				};

				dataList.Add(item);
			}

			return dataList;
		}

		/// <summary>
		/// Generate a DX load result.
		/// Because DX using System.Json and not Newtonsoft, using LoadResults, [Newtonsoft.JsonProperty] attributes are NOT observed and no JSON adjustment is needed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		protected LoadResult GetLoadResult<T>(IEnumerable<T> enumerable)
		{
			return new LoadResult
			{
				data = enumerable,
			};
		}

		/// <summary>
		/// Adjust all date/time values in a graph to 'view' time, which can be local time or UTC depending on property attributes.
		/// </summary>
		/// <param name="graph"> The graph to adjust. </param>
		protected void ToViewTime(object graph)
		{
			GraphToAdjustedTime(graph, (a) => a.ViewAs);
		}

		/// <summary>
		/// Recursively adjust all date/time values in a graph from UTC/local TO local time, based on the <paramref name="adjustingFor"/> parameter.
		/// Date/time values without an attribute remain untouched.
		/// </summary>
		/// <param name="graph"> The graph whose date/time properties get adjusted. </param>
		/// <param name="adjustingFor"> A function which returns a <see cref="DateTimeKind"/> determining if to adjust, no action is taken otherwise. <param>
		protected void GraphToAdjustedTime(object graph, Func<DateTimeUsageAttribute, DateTimeKind> adjustingFor)
		{
			int offset = GetEffectiveTimezoneOffset();
			TimezoneManager.GraphToAdjustedTime(graph, adjustingFor, offset, MaxDateDepth);
		}

		public int GetEffectiveTimezoneOffset()
		{
			// USing UTC-08:00 to make it obvious, and also get a (-) so that the date would be wrong if we cut off the time component
			TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
			//TimeZoneInfo timezone = WebTimezoneManager.GetEffectiveTimezone(HttpContext.Session, User);
			return (int)timezone.BaseUtcOffset.TotalMinutes;
		}

		private const int MaxDateDepth = 5;
	}
}