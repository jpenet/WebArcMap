using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Interfaces;

namespace WebArcMap.ViewModels
{
	[Export(typeof(ShellViewModel))]
	public class ShellViewModel : NotificationObject
	{
		[Import]
		public IGisOperations gisOperation { get; set; }

		[Import]
		public IConfiguration configuration { get; set; }

		private ICommand _expandedCommand;
		public ICommand ExpandedCommand
		{
			get
			{
				return _expandedCommand;
			}
			set
			{
				_expandedCommand = value;
				this.RaisePropertyChanged(() => this.ExpandedCommand);
			}
		}

		private bool tabExpanded = true;
		// Selected tab index
		private int _tabIndex = 0;
		public int TabIndex
		{
			get
			{
				return _tabIndex;
			}
			set
			{
				_tabIndex = value;
				this.RaisePropertyChanged(() => this.TabIndex);
			}
		}

		private readonly IRegionManager regionManager;
		private readonly IEventAggregator eventAggregator;
		[ImportingConstructor]
		public ShellViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
		{
			this.regionManager = regionManager;
			this.eventAggregator = eventAggregator;
			this.eventAggregator.GetEvent<CompositePresentationEvent<MainTabInfo>>().Subscribe(TabIndexChange);
			this.ExpandedCommand = new DelegateCommand<object>(OnExpandedExecute, CanExpandedExecute);
			App.Current.Host.Content.Resized += ContentResized;
		}

		private void ContentResized(object sender, EventArgs e)
		{
			gisOperation.SetMapSize(0);
		}

		/// <summary>
		/// Based on the tab, resize the map
		/// </summary>
		/// <param name="arg"></param>
		private void OnExpandedExecute(object arg)
		{
			string action = arg as string;
			tabExpanded = action.Equals("1") ? true : false;
			int offset = tabExpanded ? 300 : 0;
			gisOperation.SetMapSize(offset);
		}

		private bool CanExpandedExecute(object arg)
		{
			return true;
		}
		/// <summary>
		/// The event mainTabInfo can be used to set the tab index for the left pane
		/// </summary>
		/// <param name="tabInfo"></param>
		public void TabIndexChange(MainTabInfo tabInfo)
		{
			this.TabIndex = tabInfo.TabIndex;
		}
	}
}
