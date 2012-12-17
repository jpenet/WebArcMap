using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;
namespace Silverlight.UI.Esri.JTToolbarCommon.ViewModels
{
	public class LayerListViewModel:NotificationObject
	{
		public ArcGISMapLayer LayerSelected { get; set; }

		private ICommand _layerSelectedCommand;
		public ICommand LayerSelectedCommand
		{
			get
			{
				return _layerSelectedCommand;
			}
			set
			{
				_layerSelectedCommand = value;
				this.RaisePropertyChanged(() => this.LayerSelectedCommand);
			}
		}


		private ObservableCollection<ArcGISMapLayer> _layerList = new ObservableCollection<ArcGISMapLayer>();

		public ObservableCollection<ArcGISMapLayer> LayerList
		{
			get 
			{ 
				return _layerList; 
			}
			set 
			{ 
				_layerList = value;
				this.RaisePropertyChanged(() => this.LayerList);
			}
		}

		public LayerListViewModel(IList<ArcGISMapLayer> layerList)
		{
			foreach (var item in layerList)
				this._layerList.Add(item);
			this.LayerSelectedCommand = new DelegateCommand<object>(
				this.OnLayerSelectedCommand, this.CanLayerSelectedCommand);
			this.RaisePropertyChanged(() => this.LayerList);
			this.LayerSelected = null;
		}
		private void OnLayerSelectedCommand(object arg)
		{
			if (arg != null)
				LayerSelected = (ArcGISMapLayer)arg;
		}

		private bool CanLayerSelectedCommand(object arg)
		{
			return true;
		}
	}
}
