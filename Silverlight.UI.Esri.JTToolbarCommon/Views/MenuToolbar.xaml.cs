using System.ComponentModel.Composition;
using System.Windows.Controls;
using Silverlight.UI.Esri.JTToolbarCommon.ViewModels;

namespace Silverlight.UI.Esri.JTToolbarCommon.Views
{
	[Export(typeof(MenuToolbar))]
	public partial class MenuToolbar : UserControl
	{
		[Import]
		public ToolbarViewModel toolbarViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}

		public MenuToolbar()
		{
			InitializeComponent();
		}
	}
}
