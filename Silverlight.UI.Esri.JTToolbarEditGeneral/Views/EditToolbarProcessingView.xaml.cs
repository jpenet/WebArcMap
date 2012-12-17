using System.ComponentModel.Composition;
using System.Windows.Controls;
using Silverlight.UI.Esri.JTToolbarEditGeneral.ViewModels;

namespace Silverlight.UI.Esri.JTToolbarEditGeneral.Views
{
	[Export(typeof(EditToolbarProcessingView))]
	public partial class EditToolbarProcessingView : UserControl
	{
		public EditToolbarProcessingView()
		{
			InitializeComponent();
		}

		[Import]
		public EditToolBarGeneralViewModel editToolBarGeneralViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}
	}
}
