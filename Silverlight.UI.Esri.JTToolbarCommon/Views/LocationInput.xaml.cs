using System.ComponentModel.Composition;
using System.Windows.Controls;
using Silverlight.UI.Esri.JTToolbarCommon.ViewModels;

namespace Silverlight.UI.Esri.JTToolbarCommon.Views
{
	[Export(typeof(LocationInput))]
	public partial class LocationInput : UserControl
	{
		[Import]
		public LocationInputViewModel locationInputViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}

		public LocationInput()
		{
			InitializeComponent();
		}
	}
}
