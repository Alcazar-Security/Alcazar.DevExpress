using Alcazar.Common.Config;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alcazar.DevExpress.Utilities
{
	internal class DxCultureUtilities
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region TagHelperBase helper methods: culture, formatting
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		static public string GetRequestCultureFormat(ViewContext viewContext, Format format)
		{
			// The feature is not available from the constructor, therefore request it here.
			IRequestCultureFeature requestCultureFeature = viewContext.HttpContext.Features.Get<IRequestCultureFeature>();
			if (requestCultureFeature == null)
				return null;

			CultureInfo cultureInfo = requestCultureFeature?.RequestCulture?.Culture ?? Thread.CurrentThread.CurrentCulture;

			return DxCultureUtilities.ToCustomFormat(format, cultureInfo);
		}

		static public Format? ToFormat(DateBoxType dataType)
		{
			switch (dataType)
			{
				default:
				case DateBoxType.Date:
					return Format.ShortDate;
				case DateBoxType.DateTime:
					return Format.ShortDateShortTime;
				case DateBoxType.Time:
					return Format.ShortTime;
			}
		}

		static public Format? ToFormat(GridColumnDataType dataType)
		{
			switch (dataType)
			{
				default:
				case GridColumnDataType.String:
				case GridColumnDataType.Boolean:
				case GridColumnDataType.Object: return null;

				case GridColumnDataType.Number: return Format.Decimal;

				case GridColumnDataType.Date: return Format.ShortDate;
				case GridColumnDataType.DateTime: return Format.ShortDateShortTime;
			}
		}

		protected string ToCustomFormat(GridColumnDataType dataType)
		{
			switch (dataType)
			{
				default:
				case GridColumnDataType.String:
				case GridColumnDataType.Boolean:
				case GridColumnDataType.Object: return null;

				case GridColumnDataType.Number: return "##0.00";

				case GridColumnDataType.Date: return "yyyy-MM-dd";
				case GridColumnDataType.DateTime: return "yyyy-MM-dd HH:mm";

			}
		}

		static public string ToCustomFormat(Format format, CultureInfo culture)
		{
			switch (format)
			{
				// NOT supported
				default:
				case Format.Billions:
				case Format.Currency:
				case Format.Day:
				case Format.Millions:
				case Format.Millisecond:
				case Format.Month:
				case Format.MonthAndDay:
				case Format.MonthAndYear:
				case Format.Quarter:
				case Format.QuarterAndYear:
				case Format.Thousands:
				case Format.Trillions:
				case Format.Year:
				case Format.DayOfWeek:
				case Format.Hour:
				case Format.Minute:
				case Format.Second: return null;

				// Numbers TODO
				case Format.Decimal: //var x = culture.GetFormat(typeof(int)); return x.ToString();//.NumberFormat.NumberNegativePattern.ToString();
				case Format.Exponential: //return culture.NumberFormat.NumberNegativePattern.ToString();
				case Format.FixedPoint: //return culture.NumberFormat.NumberNegativePattern.ToString();
				case Format.LargeNumber: //return culture.NumberFormat.NumberNegativePattern.ToString();
				case Format.Percent: return null; //return culture.NumberFormat.PercentPositivePattern.ToString();

				// Date and time
				case Format.LongDate: return culture.DateTimeFormat.LongDatePattern;
				case Format.LongTime: return culture.DateTimeFormat.LongTimePattern;
				case Format.ShortDate: return culture.DateTimeFormat.ShortDatePattern;
				case Format.ShortTime: return culture.DateTimeFormat.ShortTimePattern;
				case Format.LongDateLongTime: return $"{culture.DateTimeFormat.LongDatePattern} {culture.DateTimeFormat.LongTimePattern}";
				case Format.ShortDateShortTime: return $"{culture.DateTimeFormat.ShortDatePattern} {culture.DateTimeFormat.ShortTimePattern}";
			}
		}

		#endregion
	}
}
