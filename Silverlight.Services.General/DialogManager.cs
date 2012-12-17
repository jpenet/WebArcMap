using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Interfaces;
using Silverlight.Services.General.ViewModels;
using Silverlight.Services.General.Views;

namespace Silverlight.Services.General
{
	[Export(typeof(IDialogManager))]
	public class DialogManager : IDialogManager
	{
		public IEventAggregator MyEventAggregator { get; set; }

		//public SilverFlow.Controls.FloatingWindowHost WindowHost { get; set; }

		public DialogManager()
		{
			this.MyEventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
			if (this.MyEventAggregator != null)
			{
				this.MyEventAggregator.GetEvent<CompositePresentationEvent<List<SearchResult>>>().Subscribe(ShowResultsDialog);
			}
		}

		/// <summary>
		/// Create a dialog box
		/// </summary>
		/// <param name="results"></param>
		public void ShowResultsDialog(List<SearchResult> results)
		{
			ResultsView resultsView = new ResultsView();
			ResultsViewModel resultsViewModel = new ResultsViewModel();
			resultsViewModel.SetResults(results);
			resultsView.DataContext = resultsViewModel;
			resultsView.OverlayOpacity = 0;
			resultsView.Show();
		}
	}
}
