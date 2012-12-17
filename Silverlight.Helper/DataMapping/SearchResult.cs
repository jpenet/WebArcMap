using System.Collections.Generic;

namespace Silverlight.Helper.DataMapping
{
	public class SearchResult
	{
		private string _layerName;
		private bool _selected;
		private IDictionary<string, object> _attributeValues;
		private int _id;
		private ESRI.ArcGIS.Client.Geometry.Geometry _geometry;

		public ESRI.ArcGIS.Client.Geometry.Geometry Geometry
		{
			get
			{
				return _geometry;
			}
			set
			{
				_geometry = value;
			}
		}


		public string LayerName
		{
			get
			{
				return _layerName;
			}
			set
			{
				_layerName = value;
			}
		}

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				_selected = value;
			}
		}

		public IDictionary<string, object> AttributeValues
		{
			get
			{
				return _attributeValues;
			}
			set
			{
				_attributeValues = value;
			}
		}

		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}
	}
}
