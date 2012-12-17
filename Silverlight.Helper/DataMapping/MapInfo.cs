using ESRI.ArcGIS.Client;

namespace Silverlight.Helper.DataMapping
{
	public struct MyExtent
	{
		public double XMin;
		public double YMin;
		public double XMax;
		public double YMax;
		public int WKID;
	}
	public class MapData
	{
		public bool IsInitialized { get; set; }
		//public string BaseMap { get; set; }
		//public Map MapView { get; set; }
		//public Editor EditorTool { get; set; }
	}
	public class MapExtent
	{
		public MyExtent Extent { get; set; }
	}
}
