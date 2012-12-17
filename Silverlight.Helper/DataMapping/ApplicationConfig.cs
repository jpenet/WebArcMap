using System.Xml.Serialization;

namespace Silverlight.Helper.DataMapping
{
	[XmlRoot(ElementName = "AppConfig")]
	public class ApplicationConfig
	{
		[XmlElement(ElementName = "ApplicationLogo")]
		public string ApplicationLogo { get; set; }

		[XmlElement(ElementName = "ApplicationTitle")]
		public string ApplicationTitle { get; set; }

		[XmlElement(ElementName = "UrlGeometryService")]
		public string UrlGeometryService { get; set; }

		[XmlElement(ElementName = "UrlGeolocatorService")]
		public string UrlGeolocatorService { get; set; }

		[XmlElement(ElementName = "UrlRoutingService")]
		public string UrlRoutingService { get; set; }

		[XmlElement(ElementName = "Map")]
		public MapConfig MapConfig { get; set; }

		public ApplicationConfig()
		{
		}

		public static ApplicationConfig Deserialize(string configXml)
		{
			ApplicationConfig appConfig = null;
			XmlSerializer serializer = new XmlSerializer(typeof(ApplicationConfig));

			using (System.IO.TextReader textReader = new System.IO.StringReader(configXml))
			{
				appConfig = (ApplicationConfig)serializer.Deserialize(textReader);
			}

			return appConfig;
		}

	}
}
