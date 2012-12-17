using ESRI.ArcGIS.Client.Symbols;

namespace Silverlight.Helper.DataMapping
{
	/// <summary>
	/// Symbol information class
	/// </summary>
	public class SymbolMarkerInfo
	{
		public Symbol SymbolMarker { get; set; }
		public string Name { get; set; }
		public object ObjectFeatureType { get; set; }
		public string LayerId { get; set; }
	}
}
