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

		/// <summary>
		/// Get or set the argument axis for the chart.
		/// </summary>
		public ChartArgumentAxisModel ArgumentAxis { get; set; }

		/// <summary>
		/// Get or set one or more value axes for the chart.
		/// </summary>
		public IList<ChartValueAxisModel> ValueAxes { get; } = new List<ChartValueAxisModel>();

		/// <summary>
		/// Get or set one or more value series for the chart.
		/// </summary>
		public IList<ChartSerieModel> Series { get; } = new List<ChartSerieModel>();

		#endregion
	}
}
