using System.Xml.Serialization;
namespace Silverlight.Helper.DataMapping
{
	[XmlRoot(ElementName = "BookmarkConfig")]
	public class BookmarkConfig
	{
		[XmlArray(ElementName = "Bookmarks")]
		[XmlArrayItem(typeof(BookmarkElement), ElementName = "Bookmark")]
		public BookmarkElement[] Bookmarks { get; set; }

		public static BookmarkConfig Deserialize(string configXml)
		{
			BookmarkConfig bookmarkConfig = null;
			XmlSerializer serializer = new XmlSerializer(typeof(BookmarkConfig));
			using (System.IO.TextReader textReader = new System.IO.StringReader(configXml))
			{
				bookmarkConfig = (BookmarkConfig)serializer.Deserialize(textReader);
			}
			return bookmarkConfig;
		}
	}

	public class BookmarkElement
	{
		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		[XmlElement(typeof(Extent), ElementName = "Extent")]
		public Extent Extent { get; set; }
	}
}
