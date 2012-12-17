using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;

namespace Silverlight.Helper.DataMapping
{
	public class GeoLocatorDetail
	{
		public string Title { get; set; }
		public double Match { get; set; }
		public MapPoint Location { get; set; }
		public Envelope Extent { get; set; }
		public Dictionary<string, object> Attributes { get; set; }
	}
}
