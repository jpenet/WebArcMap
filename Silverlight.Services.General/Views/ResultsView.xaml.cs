using System.Windows.Controls;

namespace Silverlight.Services.General.Views
{
	public partial class ResultsView : ChildWindow
	{
		public ResultsView()
		{
			InitializeComponent();
		}

		private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}

