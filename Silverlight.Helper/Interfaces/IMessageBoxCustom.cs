using Silverlight.Helper.Dialogs;

namespace Silverlight.Helper.Interfaces
{
	public interface IMessageBoxCustom
	{
		MessageBoxCustomEnum.MessageBoxResultCustom
		Show(string message, string caption, MessageBoxCustomEnum.MessageBoxButtonCustom buttons);

		void Show(string message, string caption);
	}
}
