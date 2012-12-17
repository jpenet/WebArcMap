using System.Windows.Controls;
using System.ComponentModel.Composition;
using Silverlight.UI.Esri.JTToolbarEditGeneral.ViewModels;

namespace Silverlight.UI.Esri.JTToolbarEditGeneral.Views
{
	[Export(typeof(EditToolBarGeneralView))]
	public partial class EditToolBarGeneralView : UserControl
	{
		public EditToolBarGeneralView()
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
