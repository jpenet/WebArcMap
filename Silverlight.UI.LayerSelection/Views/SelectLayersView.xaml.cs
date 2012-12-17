using System.ComponentModel.Composition;
using System.Windows.Controls;
using Silverlight.UI.LayerSelection.ViewModels;

namespace Silverlight.UI.LayerSelection.Views
{
	[Export(typeof(SelectLayersView))]
	public partial class SelectLayersView : UserControl
	{
		public SelectLayersView()
		{
			InitializeComponent();
		}

		[Import]
		public SelectViewModel selectViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}
	}
}
