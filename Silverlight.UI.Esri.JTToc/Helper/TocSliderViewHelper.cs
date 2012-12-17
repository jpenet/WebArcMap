using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace Silverlight.UI.Esri.JTToc.Helper
{
	public static class TocSliderViewHelper
	{
		public static readonly DependencyProperty VisibleSliderProperty =
		DependencyProperty.RegisterAttached("VisibleSlider", typeof(Layer),
		typeof(TocSliderViewHelper), new PropertyMetadata(OnTocSliderViewChanged));

		public static Layer GetVisibleSlider(DependencyObject depObject)
		{
			return (Layer)depObject.GetValue(VisibleSliderProperty);
		}

		public static void SetVisibleSlider(DependencyObject depObject, Layer value)
		{
			depObject.SetValue(VisibleSliderProperty, value);
		}

		private static void OnTocSliderViewChanged(DependencyObject depObject,
		DependencyPropertyChangedEventArgs e)
		{
			Slider slider = depObject as Slider;
			if (slider == null)
			{
				throw new ArgumentException(
				"DependencyObject must be of type System.Windows.Controls.Slider");
			}
			Layer layer = GetVisibleSlider(slider);
			if (layer.GetType() == typeof(FeatureLayer))
				slider.Visibility = Visibility.Collapsed;
			else
				slider.Visibility = Visibility.Visible;
		}
	}
}
