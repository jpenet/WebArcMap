using System;
using System.Collections.Generic;

namespace Silverlight.Helper.DataMapping
{
	public class ResultsGeoLocatorEventArgs : EventArgs
	{
		public IList<GeoLocatorDetail> result { get; set; }
		public ResultsGeoLocatorEventArgs(IList<GeoLocatorDetail> result)
		{
			this.result = result;
		}
	}
}
