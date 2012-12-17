using System;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace Silverlight.UI.Esri.JTMap.Helper
{
	public static class MapViewHelper
	{
		public static readonly DependencyProperty MapExtentProperty =
		DependencyProperty.RegisterAttached("MapExtent", typeof(Envelope),
		typeof(MapViewHelper), new PropertyMetadata(OnMapExtentChanged));

		public static Envelope GetMapExtent(DependencyObject depObject)
		{
			return (Envelope)depObject.GetValue(MapExtentProperty);
		}

		public static void SetMapExtent(DependencyObject depObject, Envelope value)
		{
			depObject.SetValue(MapExtentProperty, value);
		}

		private static void OnMapExtentChanged(DependencyObject depObject,
		DependencyPropertyChangedEventArgs e)
		{
			Map map = depObject as Map;
			if (map == null)
			{
				throw new ArgumentException(
				"DependencyObject must be of type ESRI.ArcGis.client.Map");
			}
			Envelope newExtent = GetMapExtent(map);
			newExtent.SpatialReference = map.SpatialReference;
			if (newExtent != null)
				map.ZoomTo(newExtent);
		}
	}
}
