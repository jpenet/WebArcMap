using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Dialogs;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.Esri.JTMap.View;

namespace Silverlight.UI.Esri.JTMap.ViewModels
{
	[Export(typeof(MapViewModel))]
	public class MapViewModel : NotificationObject, INotifyDataErrorInfo
	{
		private readonly CompositePresentationEvent<MapData> mapDataEvent;
		private readonly CompositePresentationEvent<MapLoaded> mapLoadedEvent;
		private readonly CompositePresentationEvent<MapExtent> mapExtentEvent;
		private readonly ILoggerFacade _loggerFacade;
		private readonly IConfiguration configuration;
		private readonly InteractionRequest<Notification> _notificationErrorInteraction;
		private readonly IRegionManager regionManager;

		public ICommand HandleRightClick { get; set; }

		
		public IInteractionRequest NotificationErrorInteraction
		{
			get
			{
				return _notificationErrorInteraction;
			}
		}

		[Import]
		public IGisRouting gisRouting { get; set; }

		[Import]
		public IGisEditing gisEditing { get; set; }

		[Import]
		public IGisOperations gisOperations { get; set; }

		[Import]
		public IGeolocator geoLocator { get; set; }

		[Import]
		public IMessageBoxCustom messageBoxCustom;

		private Visibility _contextMenuVisibility;
		public Visibility ContextMenuVisibility
		{
			get
			{
				return _contextMenuVisibility;
			}
			set
			{
				_contextMenuVisibility = value;
				this.RaisePropertyChanged(() => this.ContextMenuVisibility);
			}
		}

		private ICommand _menuItemCommand;
		public ICommand MenuItemCommand
		{
			get
			{
				return _menuItemCommand;
			}
			set
			{
				_menuItemCommand = value;
				this.RaisePropertyChanged(() => this.MenuItemCommand);
			}
		}

		/// <summary>
		/// Constructor following the PRISM pattern
		/// </summary>
		/// <param name="eventAggregator"></param>
		/// <param name="loggerFacade"></param>
		/// <param name="configuration"></param>
		[ImportingConstructor]
		public MapViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ILoggerFacade loggerFacade, IConfiguration configuration)
		{
			_container = new ErrorsContainer<string>(OnErrorsChanged);
			this.regionManager = regionManager;
			mapDataEvent = eventAggregator.GetEvent<CompositePresentationEvent<MapData>>();
			mapLoadedEvent = eventAggregator.GetEvent<CompositePresentationEvent<MapLoaded>>();
			mapExtentEvent = eventAggregator.GetEvent<CompositePresentationEvent<MapExtent>>();
			this._loggerFacade = loggerFacade;
			this.configuration = configuration;
			_notificationErrorInteraction = new InteractionRequest<Notification>();
			this.NotificationErrorInteraction.Raised += (o, e) =>
			{
				// Do some logging
				_loggerFacade.Log(e.Context.Content as string, Category.Exception, Priority.High);
				var result = messageBoxCustom.Show(e.Context.Content as string, Silverlight.UI.Esri.JTMap.Resources.Map.ErrorMessage,
				MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			};
			HandleRightClick = new DelegateCommand<object>(this.OnRightMouseClicked, this.CanRightMouseClicked);
			this.ContextMenuVisibility = Visibility.Collapsed;
			this.MenuItemCommand = new DelegateCommand<object>(
				this.OnMenuItemClicked, this.CanMenuItemClicked);
			string tradeNote = "J&T Software BVBA - Custom Silverlight map editor - Version 1.0";
			HelpContents.DisplayHelp(tradeNote, regionManager,false);
		}

		private void OnMenuItemClicked(object arg)
		{
			this.ContextMenuVisibility = Visibility.Collapsed;
		}

		private bool CanMenuItemClicked(object arg)
		{
			return true;
		}

		private void OnRightMouseClicked(object arg)
		{
			this.ContextMenuVisibility = Visibility.Visible;
			return;
		}
		private bool CanRightMouseClicked(object arg)
		{
			return true;
		}

		[Import]
		public MapView mapView;

		public Envelope InitialExtent { get; set; }
		public int Wkid { get; set; }

		public Envelope Extent { get; set; }
		//public LayerCollection Layers { get; set; }
		public ObservableCollection<string> LayerIds;

		private void SetSymbols()
		{
			try
			{
				// Initialize symbols
				ResourceDictionary dictionary = new ResourceDictionary();
				Uri uri =
					new Uri("/Silverlight.Styles.Dictionaries;component/EsriMarkers.xaml", UriKind.Relative);
				dictionary.Source = uri;
				Application.Current.Resources.MergedDictionaries.Add(dictionary);
				gisOperations.SetSelectionLineSymbol(Application.Current.Resources["SelectLineSymbol"] as LineSymbol);
				gisOperations.SetSelectionMarkerSymbol(Application.Current.Resources["SelectMarkerSymbol"] as MarkerSymbol);
				gisOperations.SetSelectionFillSymbol(Application.Current.Resources["SelectFillSymbol"] as FillSymbol);
				gisRouting.SetLineRouteSymbol(Application.Current.Resources["LineRoute"] as SimpleLineSymbol);
				gisRouting.SetStopRouteSymbol(Application.Current.Resources["StopRouteSymbol"] as SimpleMarkerSymbol);
				gisRouting.SetWayPointRouteSymbol(Application.Current.Resources["WayPointSymbol"] as SimpleMarkerSymbol);
				geoLocator.SetLocationMarker(Application.Current.Resources["LocationMarkerSymbol"] as PictureMarkerSymbol);
			}
			catch (Exception ex)
			{
				var result = messageBoxCustom.Show(ex.Message, "Error message",
				MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}
		public void SetExtent(double xMin, double yMin, double xMax, double yMax)
		{
			SpatialReference sref = new SpatialReference(this.Wkid);
			this.Extent = new Envelope(xMin, yMin, xMax, yMax);
			this.Extent.SpatialReference = sref;
			this.RaisePropertyChanged(() => this.InitialExtent);
		}

		public void SetInitialExtent()
		{
			this.InitialExtent = 
				configuration.GetApplicationConfig().MapConfig.InitialExtent.ToEnvelope();
			PublishMapChange();
		}

		private void PublishMapChange()
		{
			SetSymbols();
			gisOperations.Initialize(configuration.GetApplicationConfig().UrlGeometryService);
			gisOperations.SetMapInitialiseCompleteEvent(MapInitialised);
			gisOperations.SetLayers(mapView.map);
			// Trigger map loaded
			mapView.map.Loaded += Map_loaded;
		}

		private void MapInitialised()
		{
			if (gisOperations.GetFeatureLayerInfos().Count > 0)
			{
				gisOperations.SetMapSize(0);
				// Initialize routing service
				gisRouting.Initialize(configuration.GetApplicationConfig().UrlRoutingService);
				// Initialize Edit service
				gisEditing.Initialize();
				// Initialize geo-locator
				geoLocator.Initialize(configuration.GetApplicationConfig().UrlGeolocatorService);
				geoLocator.SetSpatialReference(mapView.map.SpatialReference);
				// Broadcast changes
				MapData mapData = new MapData() {  IsInitialized=true };
				mapDataEvent.Publish(mapData);
				mapView.map.ExtentChanged += Extend_Changed;
			}
		}

		private void Extend_Changed(object sender, ExtentEventArgs e)
		{
			Envelope extent = gisOperations.GetNormalizedExtent( e.NewExtent);
			MapExtent mapExtent = new MapExtent()
			{ Extent = new MyExtent() { XMin = extent.XMin, YMin = extent.YMin, XMax = extent.XMax, YMax = extent.YMax } };
			mapExtentEvent.Publish(mapExtent);
			if (configuration.GetApplicationConfig().MapConfig.DisplayExtent)
			{
				string extentText = String.Format("x:{0:0} y:{1:0} X:{2:0} Y:{3:0}", extent.XMin, extent.YMin, extent.XMax, extent.YMax);
				HelpContents.DisplayHelp(extentText, regionManager,true);
			}
		}

		private void Map_loaded(object sender, RoutedEventArgs e)
		{
			mapView.map.Loaded -= Map_loaded;
			// Send information that the map is initilized
			MapLoaded mapLoaded = new MapLoaded() { IsLoaded = true };
			mapLoadedEvent.Publish(mapLoaded);
			this.RaisePropertyChanged(() => this.InitialExtent);
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public System.Collections.IEnumerable GetErrors(string propertyName)
		{
			return _container.GetErrors(propertyName);
		}

		public bool HasErrors
		{
			get
			{
				return _container.HasErrors;
			}
		}
		private readonly ErrorsContainer<string> _container;

		protected virtual void OnErrorsChanged(string propertyName)
		{
			EventHandler<DataErrorsChangedEventArgs> eventHandler = ErrorsChanged;
			if (eventHandler != null)
				eventHandler(this, new DataErrorsChangedEventArgs(propertyName));
		}
	}
}
