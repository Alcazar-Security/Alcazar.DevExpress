using System.Collections.Generic;

namespace Alcazar.Web.Extensibility
{
	/// <summary>
	/// The <see cref="ChartContext"/> context type confers contained chart helpers to the parent chart control.
	/// </summary>
	public class ChartContext
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region ChartContext properties
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		public ChartAxisModel ChartAxis { get; set; }

		public IList<ChartValueAxisModel> ValueAxes { get; } = new List<ChartValueAxisModel>();

		public IList<ChartSerieModel> Series { get; } = new List<ChartSerieModel>();

		#endregion
	}
}
