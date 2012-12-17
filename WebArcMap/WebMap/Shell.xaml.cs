using System.ComponentModel.Composition;
using System.Windows.Controls;
using WebArcMap.ViewModels;

namespace WebArcMap
{
	[Export(typeof(Shell))]
	public partial class Shell : UserControl
	{
		public Shell()
		{
			InitializeComponent();
		}

		[Import]
		public ShellViewModel shellViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}
	}
}
