using System;

namespace Silverlight.Helper.Interfaces
{
	public interface IModalWindow
	{
		bool? DialogResult { get; set; }
		event EventHandler Closed;
		void Show();
		object DataContext { get; set; }
		void Close();
	}

}
