using System.Xml.Serialization;
namespace Silverlight.Helper.DataMapping
{
	/// <summary>
	/// List of the basemap layers available for adding to the map
	/// </summary>
	[XmlRoot(ElementName = "LayerLibConfig")]
	public class LayerLibConfig
	{
		[XmlArray(ElementName = "Layers")]
		[XmlArrayItem(typeof(ArcGISMapLayer), ElementName = "Layer")]
		public ArcGISMapLayer[] Layers { get; set; }

		public static LayerLibConfig Deserialize(string configXml)
		{
			LayerLibConfig layerLibConfig = null;
			XmlSerializer serializer = new XmlSerializer(typeof(LayerLibConfig));
			using (System.IO.TextReader textReader = new System.IO.StringReader(configXml))
			{
				layerLibConfig = (LayerLibConfig)serializer.Deserialize(textReader);
			}
			return layerLibConfig;
		}
	}
}
