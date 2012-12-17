using System.ComponentModel.Composition;
using System.Windows.Controls;
using Silverlight.UI.Esri.JTMap.ViewModels;

namespace Silverlight.UI.Esri.JTMap.View
{
	[Export(typeof(MapView))]
	public partial class MapView : UserControl
	{
		public MapView()
		{
			InitializeComponent();
		}
		[Import]
		public MapViewModel mapViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}
	}
}
