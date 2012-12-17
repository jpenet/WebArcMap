using System;

namespace Silverlight.Helper.DataMapping
{
	public class RoutingEventArgs : EventArgs
	{
		public RoutingEventArgs()
		{
			this.RoutingResult = new RoutingData();
		}
		public RoutingData RoutingResult { get; set; }
	}
}
