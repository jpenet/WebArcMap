using System;
using Silverlight.Helper.Interfaces;

namespace Silverlight.Helper.Dialogs
{
	public class ModalDialogService : IModalDialogService
	{
		public void ShowDialog<TDialogViewModel>(IModalWindow view, TDialogViewModel viewModel, Action<TDialogViewModel> onDialogClose)
		{
			view.DataContext = viewModel;
			if (onDialogClose != null)
			{
				view.Closed += (sender, e) => onDialogClose(viewModel);
			}
			view.Show();
		}

		public void ShowDialog<TDialogViewModel>(IModalWindow view, TDialogViewModel viewModel)
		{
			this.ShowDialog(view, viewModel, null);
		}
	}

}
