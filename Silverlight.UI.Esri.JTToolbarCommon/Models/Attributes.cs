using System.Collections.ObjectModel;

namespace Silverlight.UI.Esri.JTToolbarCommon.Models
{
	public struct AttributeFeature
	{
		public string FieldName { get; set; }
		public string Value { get; set; }
	}
	public class Attributes
	{
		public ObservableCollection<AttributeFeature> AttributeValueList { get; set; }
		public Attributes()
		{
			AttributeValueList = new ObservableCollection<AttributeFeature>();
		}
	}
}
