using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.Esri.JTToolbarCommon.Views;

namespace Silverlight.UI.Esri.JTToolbarCommon.ViewModels
{
	[Export(typeof(LocationInputViewModel))]
	public class LocationInputViewModel : NotificationObject, INavigationAware
	{
		[Import]
		public IGisOperations gisOperations { get; set; }

		[Import]
		public IGisCommonTasks gisCommonTasks { get; set; }

		[Import]
		public IGeolocator geolocator { get; set; }

		[Import]
		public IRegionManager regionManager;

		[Import]
		public IMessageBoxCustom messageBoxCustom;

		private readonly CompositionContainer _container;
		private readonly ILoggerFacade _loggerFacade;
		private readonly IEventAggregator eventAggregator;
		private readonly CompositePresentationEvent<MainTabInfo> mainTabEvent;
		private readonly InteractionRequest<Notification> _notificationErrorInteraction;

		public IInteractionRequest NotificationErrorInteraction
		{
			get
			{
				return _notificationErrorInteraction;
			}
		}

		private ObservableCollection<GeoLocatorDetail> _locationResults;
		public ObservableCollection<GeoLocatorDetail> LocationResults
		{
			get
			{
				return _locationResults;
			}
			set
			{
				_locationResults = value;
				this.RaisePropertyChanged(() => this.LocationResults);
			}
		}

		private ObservableCollection<GeoLocatorDetail> _locationsSelected;
		public ObservableCollection<GeoLocatorDetail> LocationsSelected
		{
			get
			{
				return _locationsSelected;
			}
			set
			{
				_locationsSelected = value;
				this.RaisePropertyChanged(() => this.LocationSelectedCommand);
			}
		}

		private ObservableCollection<string> _routeDirections;
		public ObservableCollection<string> RouteDirections
		{
			get
			{
				return _routeDirections;
			}
			set
			{
				_routeDirections = value;
				this.RaisePropertyChanged(() => this.RouteDirections);
			}
		}

		private int _selectedStopIndex = -1;
		public int SelectedStopIndex
		{
			get
			{
				return _selectedStopIndex;
			}
			set
			{
				_selectedStopIndex = value;
				this.RaisePropertyChanged(() => this.SelectedStopIndex);
			}
		}

		private GeoLocatorDetail _selectedStop;
		public GeoLocatorDetail SelectedStop
		{
			get
			{
				return _selectedStop;
			}
			set
			{
				_selectedStop = value;
				this.RaisePropertyChanged(() => this.SelectedStop);
			}
		}

		#region visbility properties
		private Visibility _selectionVisibility;
		public Visibility SelectionVisibility
		{
			get
			{
				return _selectionVisibility;
			}
			set
			{
				_selectionVisibility = value;
				this.RaisePropertyChanged(() => this.SelectionVisibility);
			}
		}

		private Visibility _stopsVisibility;
		public Visibility StopsVisibility
		{
			get
			{
				return _stopsVisibility;
			}
			set
			{
				_stopsVisibility = value;
				this.RaisePropertyChanged(() => this.StopsVisibility);
			}
		}

		private Visibility _routeDirectionsVisibility;
		public Visibility RouteDirectionsVisibility
		{
			get
			{
				return _routeDirectionsVisibility;
			}
			set
			{
				_routeDirectionsVisibility = value;
				this.RaisePropertyChanged(() => this.RouteDirectionsVisibility);
			}
		}


		#endregion

		#region command properties
		private ICommand _moveUpStopCommand;
		public ICommand MoveUpStopCommand
		{
			get
			{
				return _moveUpStopCommand;
			}
			set
			{
				_moveUpStopCommand = value;
				this.RaisePropertyChanged(() => this.MoveUpStopCommand);
			}
		}

		private ICommand _moveDownStopCommand;

		public ICommand MoveDownStopCommand
		{
			get
			{
				return _moveDownStopCommand;
			}
			set
			{
				_moveDownStopCommand = value;
				this.RaisePropertyChanged(() => this.MoveDownStopCommand);
			}
		}

		private ICommand _deleteStopCommand;

		public ICommand DeleteStopCommand
		{
			get
			{
				return _deleteStopCommand;
			}
			set
			{
				_deleteStopCommand = value;
				this.RaisePropertyChanged(() => this.DeleteStopCommand);
			}
		}

		private ICommand _locationSelectedCommand;
		public ICommand LocationSelectedCommand
		{
			get
			{
				return _locationSelectedCommand;
			}
			set
			{
				_locationSelectedCommand = value;
				this.RaisePropertyChanged(() => this.LocationSelectedCommand);
			}
		}

		private ICommand _stopSelectedCommand;

		public ICommand StopSelectedCommand
		{
			get
			{
				return _stopSelectedCommand;
			}
			set
			{
				_stopSelectedCommand = value;
				this.RaisePropertyChanged(() => this.StopSelectedCommand);
			}
		}


		private ICommand _okCommand;
		public ICommand OkCommand
		{
			get
			{
				return _okCommand;
			}
			set
			{
				_okCommand = value;
				this.RaisePropertyChanged(() => this.OkCommand);
			}
		}

		private ICommand _cancelCommand;
		public ICommand CancelCommand
		{
			get
			{
				return _cancelCommand;
			}
			set
			{
				_cancelCommand = value;
				this.RaisePropertyChanged(() => this.CancelCommand);
			}
		}

		#endregion

		#region input text properties
		private string _street;
		public string Street
		{
			get
			{
				return _street;
			}
			set
			{
				_street = value;
				this.RaisePropertyChanged(() => this.Street);
			}
		}
		private string _number;
		public string Number
		{
			get
			{
				return _number;
			}
			set
			{
				_number = value;
				this.RaisePropertyChanged(() => this.Number);
			}
		}
		private string _city;
		public string City
		{
			get
			{
				return _city;
			}
			set
			{
				_city = value;
				this.RaisePropertyChanged(() => this.City);
			}
		}
		private string _postCode;
		public string PostCode
		{
			get
			{
				return _postCode;
			}
			set
			{
				_postCode = value;
				this.RaisePropertyChanged(() => this.PostCode);
			}
		}


		#endregion

		[ImportingConstructor]
		public LocationInputViewModel(CompositionContainer container, ILoggerFacade loggerFacade, IEventAggregator eventAggregator)
		{
			this.Street = string.Empty;
			this.Number = string.Empty;
			this.City = string.Empty;
			this.PostCode = string.Empty;
			this.OkCommand = new DelegateCommand<object>(
			this.OnOKClicked, this.CanOKClicked);
			this.CancelCommand = new DelegateCommand<object>(
			this.OnCancelClicked, this.CanCancelClicked);
			this.MoveDownStopCommand = new DelegateCommand<object>(
			this.OnMoveDownCommand, this.CanMoveDownCommand);
			this.MoveUpStopCommand = new DelegateCommand<object>(
			this.OnMoveUpCommand, this.CanMoveUpCommand);
			this.DeleteStopCommand = new DelegateCommand<object>(
			this.OnDeleteStopCommand, this.CanDeleteStopCommand);
			this._container = container;
			this._loggerFacade = loggerFacade;
			this.eventAggregator = eventAggregator;
			mainTabEvent = eventAggregator.GetEvent<CompositePresentationEvent<MainTabInfo>>();
			_notificationErrorInteraction = new InteractionRequest<Notification>();
			this.LocationSelectedCommand = new DelegateCommand<object>(
			this.OnLocationSelected, this.CanLocationSelected);
			this.StopSelectedCommand = new DelegateCommand<object>(
			this.OnStopSelected, this.CanStopSelected);
			this.LocationsSelected = new ObservableCollection<GeoLocatorDetail>();
			this.LocationResults = new ObservableCollection<GeoLocatorDetail>();
			this.RouteDirections = new ObservableCollection<string>();
			this.SelectionVisibility = Visibility.Visible;
			this.StopsVisibility = Visibility.Collapsed;
			this.RouteDirectionsVisibility = Visibility.Collapsed;
		}

		private void HandleGeolocatorResults(object sender, ResultsGeoLocatorEventArgs e)
		{
			if (e.result != null)
			{
				this.LocationResults.Clear();
				foreach (var item in e.result)
					this.LocationResults.Add(item);
			}
		}

		private void OnStopSelected(object arg)
		{
		}

		private bool CanStopSelected(object arg)
		{
			return true;
		}

		private void OnLocationSelected(object arg)
		{
			// selected items
			GeoLocatorDetail selectedLocation = (GeoLocatorDetail)arg;
			if (selectedLocation != null)
			{
				this.SelectionVisibility = Visibility.Collapsed;
				this.StopsVisibility = Visibility.Visible;

				// set a pushpin at the location
				gisOperations.GetSelectLayer().Graphics.Add(new Graphic() { Geometry = selectedLocation.Location,
				Symbol = geolocator.GetLocationMarker(),
				Selected = true });
				this.LocationsSelected.Add(selectedLocation);
				this.Street = string.Empty;
				this.Number = string.Empty;
				this.City = string.Empty;
				this.PostCode = string.Empty;
				Zoom2Stops();
				this.RaisePropertyChanged(() => this.LocationSelectedCommand);
			}
		}

		public void Zoom2Stops()
		{
			if (LocationsSelected.Count > 1)
			{
				Envelope extent = new Envelope(LocationsSelected[0].Location, LocationsSelected[1].Location) { 
					SpatialReference = gisOperations.GetMap().SpatialReference };
				for (int i = 2; i < LocationsSelected.Count - 2; i++)
				{
					MapPoint point = LocationsSelected[i].Location;
					extent.XMin = point.X < extent.XMin ? point.X : extent.XMin;
					extent.YMin = point.Y < extent.YMin ? point.Y : extent.YMin;
					extent.XMax = point.X > extent.XMax ? point.X : extent.XMax;
					extent.YMax = point.Y > extent.YMax ? point.Y : extent.YMax;
				}
				gisOperations.ZoomTo(extent);
			}
			else
			{
				MapPoint point = new MapPoint(LocationsSelected[0].Location.X+500,LocationsSelected[0].Location.Y +500) ;
				Envelope extent = new Envelope(LocationsSelected[0].Location, point) { 
					SpatialReference = gisOperations.GetMap().SpatialReference };
				gisOperations.ZoomTo(extent);
			}
		}

		private bool CanLocationSelected(object arg)
		{
			return true;
		}

		private void OnOKClicked(object arg)
		{
			this.SelectionVisibility = Visibility.Visible;
			this.RouteDirections.Clear();
			geolocator.SetResultsGeoLocatorHandler(HandleGeolocatorResults);
			geolocator.FindAddress(this.Street, this.Number, this.PostCode, this.City);
			//geolocator.FindLocation(this.Street, this.Number);
		}
		private bool CanOKClicked(object arg)
		{
			//if (this.Street.Length > 0 && (this.City.Length > 0 || this.PostCode.Length > 0))
			//  return true;
			//else
			return true;
		}
		private void OnCancelClicked(object arg)
		{
			this.SelectionVisibility = Visibility.Visible;
			this.RouteDirections.Clear();
			this.LocationResults.Clear();
			this.LocationsSelected.Clear();
			this.Street = string.Empty;
			this.Number = string.Empty;
			this.PostCode = string.Empty;
			this.City = string.Empty;
			GraphicsLayer graphicsLayer = gisOperations.GetSelectLayer();
			if (graphicsLayer != null)
				graphicsLayer.ClearGraphics();
			// Remove window
			var region4 = (from r in regionManager.Regions
			               where r.Name.Equals(Constants.RegionZoek)
			               select r).FirstOrDefault();
			if (region4 != null)
			{
				var inputView = _container.GetExportedValue<LocationInput>();
				region4.Remove(inputView);
			}
			regionManager.RequestNavigate(Constants.RegionZoek, new Uri("LocationInput", UriKind.Relative));
			MainTabInfo mainTabInfo = new MainTabInfo() { TabIndex = 0 };
			mainTabEvent.Publish(mainTabInfo);
		}

		private void OnMoveUpCommand(object arg)
		{
			GeoLocatorDetail details = arg as GeoLocatorDetail;
			// [
			int position = LocationsSelected.IndexOf(details);
			if (position < 1)
				return;
			GeoLocatorDetail tempDetail = LocationsSelected[position - 1];
			LocationsSelected[position - 1] = details;
			LocationsSelected[position] = tempDetail;
			this.RaisePropertyChanged(() => this.LocationsSelected);
		}

		private bool CanMoveUpCommand(object arg)
		{
			return true;
		}

		private void OnMoveDownCommand(object arg)
		{
			GeoLocatorDetail details = arg as GeoLocatorDetail;
			int position = LocationsSelected.IndexOf(details);
			if (position == LocationsSelected.Count)
				return;
			if (position > -1)
			{
				GeoLocatorDetail tempDetail = LocationsSelected[position + 1];
				LocationsSelected[position + 1] = details;
				LocationsSelected[position] = tempDetail;
				this.RaisePropertyChanged(() => this.LocationsSelected);
			}
		}

		private bool CanMoveDownCommand(object arg)
		{
			return true;
		}


		private void OnDeleteStopCommand(object arg)
		{
			LocationsSelected.Remove(arg as GeoLocatorDetail);
			this.RaisePropertyChanged(() => this.LocationsSelected);
		}

		private bool CanDeleteStopCommand(object arg)
		{
			return true;
		}


		public void RefreshRoute()
		{
			this.SelectionVisibility = Visibility.Collapsed;
			this.StopsVisibility = Visibility.Collapsed;
			this.RouteDirectionsVisibility = Visibility.Visible;
			this.LocationResults.Clear();
			this.RaisePropertyChanged(() => this.RouteDirections);
		}

		public void RefreshStops()
		{
			this.RaisePropertyChanged(() => this.LocationsSelected);
		}

		public void ClearRoute()
		{
			this.LocationResults.Clear();
			this.LocationsSelected.Clear();
		}

		private bool CanCancelClicked(object arg)
		{
			return true;
		}

		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return false;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
		}
	}
}
