using System;
using System.Collections.Generic;
using System.Net;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Interfaces;
namespace Silverlight.UI.Esri.JTToolbarCommon.Models
{
	/// <summary>
	/// Event returning bookmarkts
	/// </summary>
	public class CompleteBookmarksEvent : EventArgs
	{
		public string ErrorMessage;
		public IList<BookmarkElement> BookmarkList;
	}

	public delegate void RetrieveBookmarksCompleted(object sender, CompleteBookmarksEvent e);

	/// <summary>
	/// Bookmarks model
	/// </summary>
	public class Bookmarks
	{
		private IList<BookmarkElement> bookmarkList = null;
		public event RetrieveBookmarksCompleted bookmarksListCompleted;

		/// <summary>
		/// Get bookmark list
		/// </summary>
		/// <param name="applicationId">Application ID</param>
		public void GetBookmarkList(string applicationId)
		{
			if (bookmarkList != null)
			{
				CompleteBookmarksEvent e = new CompleteBookmarksEvent() { BookmarkList = bookmarkList, ErrorMessage = string.Empty };
				OnLoaded(e);
			}
			else
			{
				bookmarkList = new List<BookmarkElement>();
				WebClient xmlClient = new WebClient();
				xmlClient.DownloadStringCompleted += DownloadListXMLCompleted;
				xmlClient.DownloadStringAsync(new Uri(String.Format("{0}\\Bookmarks.xml", applicationId), UriKind.RelativeOrAbsolute));
			}
		}

		/// <summary>
		/// Callback of the xml transfer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DownloadListXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			try
			{
				string xmlConfig = e.Result;
				bookmarkList = BookmarkConfig.Deserialize(xmlConfig).Bookmarks;
				CompleteBookmarksEvent completeEvent = new CompleteBookmarksEvent() { BookmarkList = bookmarkList, ErrorMessage = string.Empty };
				OnLoaded(completeEvent);
			}
			catch (Exception ex)
			{
				// Force error 
				CompleteBookmarksEvent completeEvent = new CompleteBookmarksEvent() { BookmarkList = null, ErrorMessage = ex.Message };
				OnLoaded(completeEvent);
			}
		}

		/// <summary>
		/// End processing of the retrieve
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnLoaded(CompleteBookmarksEvent e)
		{
			if (bookmarksListCompleted != null)
				bookmarksListCompleted(this, e);
		}
	}
}
