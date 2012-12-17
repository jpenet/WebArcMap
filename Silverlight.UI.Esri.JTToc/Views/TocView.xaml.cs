using System.Windows.Controls;
using System.ComponentModel.Composition;


namespace Silverlight.UI.Esri.JTToc.Views
{
	[Export(typeof(TocView))]
	public partial class TocView : UserControl
	{
		public TocView()
		{
			InitializeComponent();
		}

		[Import]
		public TocViewModel tocViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}
	}
}
