using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Actions;

namespace Silverlight.UI.Esri.JTToolbarCommon.Helper
{
	public class MapContent
	{
		public static readonly DependencyProperty MapMeasureProperty =
		DependencyProperty.RegisterAttached("MapMeasure", typeof(Map),
		typeof(MapContent), new PropertyMetadata(OnMapMeasureChanged));

		public static Map GetMapMeasure(DependencyObject depObject)
		{
			return (Map)depObject.GetValue(MapMeasureProperty);
		}

		public static void SetMapMeasure(DependencyObject depObject, Map value)
		{
			depObject.SetValue(MapMeasureProperty, value);
		}

		private static void OnMapMeasureChanged(DependencyObject depObject,
		DependencyPropertyChangedEventArgs e)
		{
			MeasureAction measureAction = depObject as MeasureAction;
			measureAction.TargetObject = GetMapMeasure(measureAction);
		}
	}
}
