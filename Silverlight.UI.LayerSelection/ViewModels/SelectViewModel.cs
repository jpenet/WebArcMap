using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Interfaces;

namespace Silverlight.UI.LayerSelection.ViewModels
{
	[Export(typeof(SelectViewModel))]
	public class SelectViewModel : NotificationObject
	{
		private IList<LayerData> _layersData;
		public IList<LayerData> LayersData
		{
			get
			{
				return _layersData;
			}
			set
			{
				_layersData = value;
			}
		}

		[Import]
		public IGisOperations gisOperations;

		private bool _checkedAll;
		public bool CheckedAll
		{
			get
			{
				return _checkedAll;
			}
			set
			{
				_checkedAll = value;
				this.RaisePropertyChanged(() => this.CheckedAll);
			}
		}

		[ImportingConstructor]
		public SelectViewModel(IEventAggregator eventAggregator)
		{
			if (eventAggregator != null)
				eventAggregator.GetEvent<CompositePresentationEvent<MapData>>().Subscribe(OnMapChanged);
		}

		public void OnMapChanged(MapData mapData)
		{
			if (mapData != null)
			{
				LayersData = gisOperations.GetLayersData();
				this.RaisePropertyChanged(() => this.LayersData);
			}
		}
	}
}
