using System.Windows;
using Silverlight.Helper.Interfaces;

namespace Silverlight.UI.Esri.JTToolbarCommon.Views
{
	public partial class BookmarkListView : IModalWindow
	{
		public BookmarkListView()
		{
			InitializeComponent();
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}

