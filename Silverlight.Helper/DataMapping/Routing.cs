using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace Silverlight.Helper.DataMapping
{
	public class RoutingData
	{
		public Polyline RouteLine { get; set; }
		public List<string> Directions { get; set; }
		public Graphic RouteGraphic { get; set; }
		public DirectionsFeatureSet RouteDirections { get; set; }
		public string RouteName { get; set; }
	}
}
