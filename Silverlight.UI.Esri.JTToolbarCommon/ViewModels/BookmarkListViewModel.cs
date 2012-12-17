using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;

namespace Silverlight.UI.Esri.JTToolbarCommon.ViewModels
{
	public class BookmarkListViewModel : NotificationObject
	{
		public BookmarkElement  BookmarkSelected { get; set; }

		private ICommand _bookmarkSelectedCommand;
		public ICommand BookmarkSelectedCommand
		{
			get
			{
				return _bookmarkSelectedCommand;
			}
			set
			{
				_bookmarkSelectedCommand = value;
				this.RaisePropertyChanged(() => this.BookmarkSelectedCommand);
			}
		}

		private ObservableCollection<BookmarkElement> _bookmarkList = new ObservableCollection<BookmarkElement>();

		public ObservableCollection<BookmarkElement> BookmarkList
		{
			get
			{
				return _bookmarkList;
			}
			set
			{
				_bookmarkList = value;
				this.RaisePropertyChanged(() => this.BookmarkList);
			}
		}

		public BookmarkListViewModel(IList<BookmarkElement> bookmarkList)
		{
			foreach (var item in bookmarkList)
				this._bookmarkList.Add(item);
			this.BookmarkSelectedCommand = new DelegateCommand<object>(
				this.OnBookmarkSelectedCommand, this.CanBookmarkSelectedCommand);
			this.RaisePropertyChanged(() => this.BookmarkList);
			this.BookmarkSelected = null;
		}

		private void OnBookmarkSelectedCommand(object arg)
		{
			if (arg != null)
				BookmarkSelected = (BookmarkElement)arg;
		}

		private bool CanBookmarkSelectedCommand(object arg)
		{
			return true;
		}
	}
}
