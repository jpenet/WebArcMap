using System.Windows;
using Silverlight.Helper.Interfaces;

namespace Silverlight.Helper.Dialogs
{
	public class MessageBoxCustom : IMessageBoxCustom
	{
		public MessageBoxCustomEnum.MessageBoxResultCustom Show(string message, string caption, MessageBoxCustomEnum.MessageBoxButtonCustom buttons)
		{
			var slButtons = buttons == MessageBoxCustomEnum.MessageBoxButtonCustom.Ok
			? MessageBoxButton.OK
			: MessageBoxButton.OKCancel;

			var result = MessageBox.Show(message, caption, slButtons);
			return result == MessageBoxResult.OK ? MessageBoxCustomEnum.MessageBoxResultCustom.Ok : MessageBoxCustomEnum.MessageBoxResultCustom.Cancel;
		}

		public void Show(string message, string caption)
		{
			MessageBox.Show(message, caption, MessageBoxButton.OK);
		}
	}
}
