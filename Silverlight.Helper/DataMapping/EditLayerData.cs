using System;
using ESRI.ArcGIS.Client.Tasks;

namespace Silverlight.Helper.DataMapping
{
	public class EditLayerData : IEquatable<EditLayerData>
	{
		public string LayerName { get; set; }
		public GeometryType LayerGeometryType { get; set; }

		public bool Equals(EditLayerData other)
		{
			if (other == null) return false;
			return (this.LayerName.Equals(other.LayerName));

		}
	}
}
