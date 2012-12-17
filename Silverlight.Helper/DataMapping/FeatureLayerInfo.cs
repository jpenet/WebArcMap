using System.Collections.Generic;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Tasks;

namespace Silverlight.Helper.DataMapping
{
	public struct FeatureLayerInfo
	{
		public string Url { get; set; }
		public IDictionary<string, FeatureTemplate> FeatureTemplates { get; set; }
		public IDictionary<object, FeatureType> FeatureTypes { get; set; }
		public string Name { get; set; }
		public string Id { get; set; }
		public GeometryType LayerGeometryType { get; set; }
		public string ObjectId { get; set; }
	}
}
