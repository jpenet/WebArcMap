using System;
using System.Xml.Serialization;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;

namespace Silverlight.Helper.DataMapping
{
	public class MapConfig
	{
		[XmlElement(typeof(Extent), ElementName = "FullExtent")]
		public Extent FullExtent { get; set; }

		[XmlElement(typeof(Extent), ElementName = "InitialExtent")]
		public Extent InitialExtent { get; set; }

		[XmlElement(typeof(bool), ElementName = "DisplayExtent")]
		public bool DisplayExtent  { get; set; } 

		[XmlArray(ElementName = "BaseMaps")]
		[XmlArrayItem(typeof(ArcGISBaseMapLayer), ElementName = "ArcGISMap")]
		public ArcGISBaseMapLayer[] BaseMapLayers { get; set; }

		[XmlArray(ElementName = "FeatureLayers")]
		[XmlArrayItem(typeof(FeatureMapLayer), ElementName = "Layer")]
		public FeatureMapLayer[] FeatureLayers { get; set; }
	}

	public class Extent
	{
		[XmlAttribute(DataType = "double", AttributeName = "xmin")]
		public double xmin { get; set; }

		[XmlAttribute(DataType = "double", AttributeName = "ymin")]
		public double ymin { get; set; }

		[XmlAttribute(DataType = "double", AttributeName = "xmax")]
		public double xmax { get; set; }

		[XmlAttribute(DataType = "double", AttributeName = "ymax")]
		public double ymax { get; set; }

		[XmlAttribute(DataType = "int", AttributeName = "spatialReference")]
		public int spatialReference { get; set; }

		public Envelope ToEnvelope()
		{
			Envelope envelope = new Envelope(this.xmin, this.ymin, this.xmax, this.ymax) 
			{ SpatialReference = new SpatialReference(this.spatialReference) 
			};

			return envelope;
		}

		public Envelope ToEnvelope(int outSRWKID)
		{
			Envelope envelope = null;
			if (IsWebMercatorSR(outSRWKID) && IsGeographicSR(this.spatialReference))
			{
				MapPoint mPoint1 = new MapPoint(this.xmin, this.ymin, new SpatialReference(4326));
				MapPoint mPoint2 = new MapPoint(this.xmax, this.ymax, new SpatialReference(4326));
				envelope = new Envelope(mPoint1.GeographicToWebMercator(), mPoint2.GeographicToWebMercator());
			}
			else if (IsWebMercatorSR(this.spatialReference) && IsGeographicSR(outSRWKID))
			{
				MapPoint mPoint1 = new MapPoint(this.xmin, this.ymin, new SpatialReference(this.spatialReference));
				MapPoint mPoint2 = new MapPoint(this.xmax, this.ymax, new SpatialReference(this.spatialReference));
				envelope = new Envelope(mPoint1.WebMercatorToGeographic(), mPoint2.WebMercatorToGeographic());
			}
			else // this.spatialReference == outSRWKID
			{
				envelope = new Envelope(this.xmin, this.ymin, this.xmax, this.ymax);
				envelope.SpatialReference = new SpatialReference(this.spatialReference);
			}
			return envelope;
		}

		/// <summary>
		/// Check if the Spatial Reference equals the Mercator Spatial Reference (WKID 102100)
		/// </summary>
		public static bool IsWebMercatorSR(int wkid)
		{
			//return (wkid == 102100 || wkid == 102113 || wkid == 3857 || wkid == 3785);
			return SpatialReference.AreEqual(new SpatialReference(102100), new SpatialReference(wkid), false);
		}

		/// <summary>
		/// Check if the Spatial Reference equals the Mercator Spatial Reference (WKID 102100)
		/// </summary>
		public static bool IsWebMercatorSR(SpatialReference sr)
		{
			//return (wkid == 102100 || wkid == 102113 || wkid == 3857 || wkid == 3785);
			return SpatialReference.AreEqual(new SpatialReference(102100), sr, false);
		}

		/// <summary>
		/// Check if the Spatial Reference equals Geographic Spatial Reference (WKID 4326)
		/// </summary>
		public static bool IsGeographicSR(int wkid)
		{
			return SpatialReference.AreEqual(new SpatialReference(4326), new SpatialReference(wkid), false);
		}

		/// <summary>
		/// Check if the Spatial Reference equals Geographic Spatial Reference (WKID 4326)
		/// </summary>
		public static bool IsGeographicSR(SpatialReference sr)
		{
			return SpatialReference.AreEqual(new SpatialReference(4326), sr, false);
		}
	}


	#region Map Layer Config
	public enum ArcGISServiceType
	{
		[XmlEnum(Name = "Unknown")]
		Unknown = 0,
		[XmlEnum(Name = "Cached")]
		Cached = 1,
		[XmlEnum(Name = "Dynamic")]
		Dynamic = 2,
		[XmlEnum(Name = "Image")]
		Image = 3,
		[XmlEnum(Name = "Feature")]
		Feature = 4
	}

	/// <summary>
	/// Basemap consist of multiple layers
	/// </summary>
	public partial class ArcGISBaseMapLayer
	{
		[XmlArray(ElementName = "Layers")]
		[XmlArrayItem(typeof(ArcGISMapLayer), ElementName = "Layer")]
		public ArcGISMapLayer[] Layers { get; set; }
	}

	public partial class ArcGISMapLayer : LayerConfig
	{
		private Guid guid = Guid.NewGuid();

		public override string ID
		{
			get
			{
				return guid.ToString("N");
			}
		}

		[XmlAttribute(AttributeName = "serviceType")]
		public ArcGISServiceType ServiceType { get; set; }

		[XmlAttribute(AttributeName = "restURL")]
		public string RESTURL { get; set; }

		[XmlAttribute(AttributeName = "proxyURL")]
		public string ProxyURL { get; set; }

		[XmlAttribute(AttributeName = "token")]
		public string Token { get; set; }
	}


	public partial class FeatureMapLayer : ArcGISMapLayer
	{
		private Guid guid = Guid.NewGuid();

		public override string ID
		{
			get { return "map_" + guid.ToString("N"); }
		}

		[XmlAttribute(AttributeName = "opacityBar")]
		public bool OpacityBar { get; set; }

		[XmlAttribute(AttributeName = "toggleLayer")]
		public bool ToggleLayer { get; set; }

		[XmlAttribute(AttributeName = "visibleLayers")]
		public string VisibleLayers { get; set; }

		[XmlAttribute(AttributeName = "refreshRate")]
		public double RefreshRate { get; set; }

		[XmlAttribute(AttributeName = "opacity")]
		public double Opacity { get; set; }

		/// <summary>
		/// Feature Layer Configuration Extension
		/// </summary>
		[XmlElement(typeof(FeatureLayerExtension), ElementName = "FeatureLayerExtension")]
		public FeatureLayerExtension FeatureLayerConfig { get; set; }
	}


	// Shared Properties by 
	// ArcGISBaseMapLayer, and 
	public abstract class LayerConfig
	{
		public abstract string ID { get; }

		[XmlAttribute(AttributeName = "title")]
		public string Title { get; set; }

		[XmlAttribute(AttributeName = "icon")]
		public string IconSource { get; set; }

		[XmlAttribute(AttributeName = "visibleInitial")]
		public bool VisibleInitial { get; set; }

		[XmlAttribute(AttributeName = "expandable")]
		public bool Expandable { get; set; }
	}


	public partial class FeatureLayerExtension
	{
		/// <summary>
		/// Determine if cluster point type features using FlareSymbol
		/// </summary>
		[XmlElement(typeof(bool), ElementName = "UseCluster")]
		public bool UseCluster { get; set; }

		/// <summary>
		/// A where clause to filter the features 
		/// </summary>
		[XmlElement(typeof(string), ElementName = "WhereString")]
		public string WhereString { get; set; }

		/// <summary>
		/// An envelope to be used as a spatial filter
		/// </summary>
		[XmlElement(typeof(Extent), ElementName = "EnvelopeFilter")]
		public Extent EnvelopeFilter { get; set; }

		/// <summary>
		/// Symbolize point features with an image. Leave this blank for polygons and ploylines
		/// </summary>
		[XmlElement(typeof(string), ElementName = "SymbolImage")]
		public string SymbolImage { get; set; }

		/// <summary>
		/// A list out fields separated with ','
		/// </summary>
		[XmlElement(typeof(string), ElementName = "OutFields")]
		public string OutFields { get; set; }
	}
	#endregion

}