using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.Esri.JTToolbarCommon.Models;
using Silverlight.UI.Esri.JTToolbarCommon.Views;
using System.Windows.Media;

namespace Silverlight.UI.Esri.JTToolbarCommon.ViewModels
{
	[Export(typeof(ToolbarViewModel))]
	public class ToolbarViewModel : NotificationObject
	{
		private ICommand _queryCommand;
		private readonly IRegionManager regionManager;
		private readonly CompositionContainer container;
		private readonly ILoggerFacade loggerFacade;
		private readonly CompositePresentationEvent<List<SearchResult>> resultEvent;
		private readonly CompositePresentationEvent<MainTabInfo> mainTabEvent;
		private bool isMapLoaded;
		private readonly InfoWindow window = new InfoWindow();
		private Point currentPosition;
		private readonly IList<MapPoint> stopPoints = new List<MapPoint>();
		private readonly ListLayers listLayers = new ListLayers();
		private readonly Bookmarks bookmarks = new Bookmarks();
		private LayerListView listView;
		private BookmarkListView bookmarkListView;

		[Import]
		public IGisRouting gisRouting { get; set; }

		[Import]
		public IGeolocator geoLocator { get; set; }

		[Import]
		public IGisOperations gisOperations { get; set; }

		[Import]
		public IGisCommonTasks gisCommonTasks { get; set; }

		[Import]
		public LocationInputViewModel locationInputViewModel;

		[Import]
		public MenuToolbar menuToolbar { get; set; }

		[Import]
		public IConfiguration configuration { get; set; }

		[Import]
		public IModalDialogService modalDialogService { get; set; }

		private Attributes _attributes = new Attributes();

		private string _applicationName;
		public string ApplicationName
		{
			get
			{
				return _applicationName;
			}
			set
			{
				_applicationName = value;
				this.RaisePropertyChanged(() => this.ApplicationName);
			}
		}

		private ImageSource _applicationLogo;

		public ImageSource ApplicationLogo
		{
			get
			{
				return _applicationLogo;
			}
			set
			{
				_applicationLogo = value;
				this.RaisePropertyChanged(() => this.ApplicationLogo);
			}
		}

		public Attributes Attributes
		{
			get
			{
				return _attributes;
			}
			set
			{
				_attributes = value;
			}
		}

		private Map _mapControl;

		public Map MapControl
		{
			get
			{
				return _mapControl;
			}
			set
			{
				_mapControl = value;
				this.RaisePropertyChanged(() => this.MapControl);
			}
		}

		private string _extentInfo;
		public string ExtentInfo
		{
			get
			{
				return _extentInfo;
			}
			set
			{
				_extentInfo = value;
				this.RaisePropertyChanged(() => this.ExtentInfo);
			}
		}

		public InteractionRequest<Notification> ShowMessagebox { get; set; }

		public InteractionRequest<Notification> ShowErrorMessagebox { get; set; }


		#region Command declarations
		public ICommand QueryCommand
		{
			get
			{
				return _queryCommand;
			}
			set
			{
				_queryCommand = value;
				this.RaisePropertyChanged(() => this.QueryCommand);
			}
		}

		private ICommand _zoomOutCommand;
		public ICommand ZoomOutCommand
		{
			get
			{
				return _zoomOutCommand;
			}
			set
			{
				_zoomOutCommand = value;
				this.RaisePropertyChanged(() => this.ZoomOutCommand);
			}
		}

		private ICommand _zoomInCommand;
		public ICommand ZoomInCommand
		{
			get
			{
				return _zoomInCommand;
			}
			set
			{
				_zoomInCommand = value;
				this.RaisePropertyChanged(() => this.ZoomInCommand);
			}
		}

		public ICommand _infoCommand;
		public ICommand InfoCommand
		{
			get
			{
				return _infoCommand;
			}
			set
			{
				_infoCommand = value;
				this.RaisePropertyChanged(() => this.InfoCommand);
			}
		}

		private ICommand _addressLocatorCommand;
		public ICommand AddressLocatorCommand
		{
			get
			{
				return _addressLocatorCommand;
			}
			set
			{
				_addressLocatorCommand = value;
				this.RaisePropertyChanged(() => this.AddressLocatorCommand);
			}
		}

		private ICommand _routeStartCommand;
		public ICommand RouteStartCommand
		{
			get
			{
				return _routeStartCommand;
			}
			set
			{
				_routeStartCommand = value;
				this.RaisePropertyChanged(() => this.RouteStartCommand);
			}
		}

		private ICommand _routeClearCommand;
		public ICommand RouteClearCommand
		{
			get
			{
				return _routeClearCommand;
			}
			set
			{
				_routeClearCommand = value;
				this.RaisePropertyChanged(() => this.RouteClearCommand);
			}
		}

		private ICommand _addLayerCommand;

		public ICommand AddLayerCommand
		{
			get
			{
				return _addLayerCommand;
			}
			set
			{
				_addLayerCommand = value;
				this.RaisePropertyChanged(() => this.AddLayerCommand);
			}
		}

		private ICommand _removeLayerCommand;

		public ICommand RemoveLayerCommand
		{
			get
			{
				return _removeLayerCommand;
			}
			set
			{
				_removeLayerCommand = value;
				this.RaisePropertyChanged(() => this.RemoveLayerCommand);
			}
		}

		private ICommand _panCommand;

		public ICommand PanCommand
		{
			get
			{
				return
					_panCommand;
			}
			set
			{
				_panCommand = value;
				this.RaisePropertyChanged(() => this.PanCommand);
			}
		}

		private ICommand _bookmarkCommand;

		public ICommand BookmarkCommand
		{
			get
			{
				return _bookmarkCommand;
			}
			set
			{
				_bookmarkCommand = value;
				this.RaisePropertyChanged(() => this.BookmarkCommand);
			}
		}

		#endregion

		[ImportingConstructor]
		public ToolbarViewModel(IRegionManager regionManager, CompositionContainer container,
		ILoggerFacade loggerFacade, IEventAggregator eventAggregator)
		{
			this.QueryCommand = new DelegateCommand<object>(
			this.OnQueryCommandClicked, this.CanQueryCommandClicked);
			this.ZoomInCommand = new DelegateCommand<object>(
			this.OnZoomInCommandClicked, this.CanZoomInCommandClicked);
			this.ZoomOutCommand = new DelegateCommand<object>(
			this.OnZoomOutCommandClicked, this.CanZoomOutCommandClicked);
			this.InfoCommand = new DelegateCommand<object>(
			this.OnInfoCommandClicked, this.CanInfoCommandClicked);
			this.AddressLocatorCommand = new DelegateCommand<object>(
			this.OnAddressLocatorCommandClicked, this.CanAddressLocatorCommandClicked);
			this.RouteStartCommand = new DelegateCommand<object>(
			this.OnRouteStartCommandClicked, this.CanRouteStartCommandClicked);
			this.RouteClearCommand = new DelegateCommand<object>(
			this.OnRouteClearCommandClicked, this.CanRouteClearCommandClicked);
			this.AddLayerCommand = new DelegateCommand<object>(
			this.OnAddLayerCommandClicked, this.CanAddLayerCommandClicked);
			this.RemoveLayerCommand = new DelegateCommand<object>(
			this.OnRemoveLayerCommandClicked, this.CanRemoveLayerCommandClicked);
			this.PanCommand = new DelegateCommand<object>(
			this.OnPanCommandClicked, this.CanPanCommandClicked);
			this.BookmarkCommand = new DelegateCommand<object>(
				this.OnBookmarkCommandClicked, this.CanBookmarkCommandClicked);
			//
			this.regionManager = regionManager;
			this.container = container;
			this.loggerFacade = loggerFacade;
			resultEvent = eventAggregator.GetEvent<CompositePresentationEvent<List<SearchResult>>>();
			mainTabEvent = eventAggregator.GetEvent<CompositePresentationEvent<MainTabInfo>>();
			if (eventAggregator != null)
				eventAggregator.GetEvent<CompositePresentationEvent<MapLoaded>>().Subscribe(OnMapLoadedChanged);
			if (eventAggregator != null)
				eventAggregator.GetEvent<CompositePresentationEvent<MapExtent>>().Subscribe(OnMapExtentChanged);
			ShowMessagebox = new InteractionRequest<Notification>();
			ShowErrorMessagebox = new InteractionRequest<Notification>();
		}

		/// <summary>
		/// Every time a change is done on the extent, by means of a subscription the value of the extent is refreshed at
		/// the toolbar.
		/// </summary>
		/// <param name="mapExtent"></param>
		public void OnMapExtentChanged(MapExtent mapExtent)
		{
			if (gisOperations.GetMap() != null)
				this.ExtentInfo = "1:" + gisOperations.Resolution2Scale(gisOperations.GetMap().Resolution).ToString("########");
			//this.ExtentInfo = string.Format("{0};{1};{2};{3}", mapExtent.Extent.XMin, mapExtent.Extent.YMin
			//, mapExtent.Extent.XMax, mapExtent.Extent.YMax);
		}

		public void OnMapLoadedChanged(MapLoaded mapLoaded)
		{
			this.MapControl = gisOperations.GetMap();
			isMapLoaded = true;
			this.ApplicationName = configuration.GetApplication();
			this.ApplicationLogo = 
				new BitmapImage(new Uri(configuration.GetApplicationConfig().ApplicationLogo, UriKind.Absolute));
			DelegateCommand<object> delegateCommand = this._queryCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._zoomInCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._zoomOutCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._infoCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._routeClearCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._addressLocatorCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._routeStartCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._infoCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._addLayerCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._removeLayerCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._panCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._bookmarkCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			this.ExtentInfo =
			"1:" + gisOperations.Resolution2Scale(gisOperations.GetMap().Resolution).ToString("########");

		}

		public ToolbarViewModel()
		{
			if (DesignerProperties.IsInDesignTool)
			{
			}
		}

		private void OnAddressLocatorCommandClicked(object arg)
		{
			DeleteInfoWindow();
			ActivateLocationWindow();
			gisCommonTasks.SetFinishedInfoEvent(HandleAddressPoint);
			gisCommonTasks.InfoQuery(false);
		}

		private void HandleAddressPoint(object sender, ResultInfoEventArgs e)
		{
			// Geocoding request
			geoLocator.SetResultsGeoLocatorHandler(HandleGeolocatorResults);
			currentPosition = e.ScreenPoint;
			geoLocator.FindAddressByLocation(e.InfoPoint);
		}

		private bool CanAddressLocatorCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		private void OnRouteStartCommandClicked(object arg)
		{
			DeleteInfoWindow();
			// Verify a number of selections has been done
			if (locationInputViewModel.LocationsSelected.Count < 2)
			{
				ShowMessagebox.Raise(new Notification
				{
					Content = Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.SelectMin2Locations,
					Title = Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.Warning
				});
				return;
			}
			gisRouting.SetFinishedEvent(RouteCompleted);
			IList<Graphic> stopPoints = new List<Graphic>();
			foreach (var item in locationInputViewModel.LocationsSelected)
			{
				stopPoints.Add(new Graphic() { Geometry = item.Location });
			}
			RouteParameters routeParameters = new RouteParameters() { Stops = stopPoints };
			const string routeName = "Route";
			gisRouting.StartRouteCalculation(routeParameters, routeName);
		}

		private bool CanRouteStartCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		private void OnRouteClearCommandClicked(object arg)
		{
			DeleteInfoWindow();
			gisRouting.ClearRouting();
			gisOperations.GetRoutingLayer().ClearGraphics();
			if (locationInputViewModel != null)
			{
				locationInputViewModel.ClearRoute();
				locationInputViewModel.RouteDirections.Clear();
			}
		}

		private bool CanRouteClearCommandClicked(object arg)
		{
			return isMapLoaded;
		}


		private void RouteCompleted(object sender, RoutingEventArgs e)
		{
			gisRouting.ResetFinishedEvent(RouteCompleted);
			// Present the results
			gisOperations.GetSelectLayer().ClearGraphics();
			RoutingData routingData = e.RoutingResult;
			gisOperations.GetRoutingLayer().ClearGraphics();
			gisOperations.GetRoutingLayer().Graphics.Add(routingData.RouteGraphic);
			gisOperations.ZoomTo(routingData.RouteGraphic.Geometry);
			int i = 0;
			foreach (var item in routingData.RouteDirections.Features)
			{
				System.Text.StringBuilder infoText = new System.Text.StringBuilder();
				infoText.AppendFormat("{0}. {1}", ++i, item.Attributes["text"]);
				if (i > 0 && i < routingData.RouteDirections.Features.Count - 1)
				{
					string distance = FormatDistance(Convert.ToDouble(item.Attributes["length"]), "km");
					string time = null;
					if (item.Attributes.ContainsKey("time"))
					{
						time = FormatTime(Convert.ToDouble(item.Attributes["time"]));
					}
					if (!string.IsNullOrEmpty(distance) || !string.IsNullOrEmpty(time))
						infoText.Append(" (");
					infoText.Append(distance);
					if (!string.IsNullOrEmpty(distance) && !string.IsNullOrEmpty(time))
						infoText.Append(", ");
					infoText.Append(time);
					if (!string.IsNullOrEmpty(distance) || !string.IsNullOrEmpty(time))
						infoText.Append(")");
				}
				// Set rendering symbol at the location
				Polyline polyline = item.Geometry as Polyline;
				ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection> pointCollections = polyline.Paths;
				// Process all segments
				foreach (var path in pointCollections)
				{
					// Skip first point for every segment
					MapPoint point = path.LastOrDefault();
					if (point != null && !IsStopPoint(point))
					{
						point.SpatialReference = routingData.RouteGraphic.Geometry.SpatialReference;
						Graphic graphic = new Graphic() { Geometry = point, Symbol = gisRouting.GetWayPointRouteSymbol() };
						gisOperations.GetRoutingLayer().Graphics.Add(graphic);
					}
				}
				if (locationInputViewModel != null)
				{
					locationInputViewModel.RouteDirections.Add(infoText.ToString());
				}
			}
			foreach (var item in stopPoints)
			{
				Graphic graphic = new Graphic()
				{
					Geometry = new MapPoint(item.X, item.Y, item.SpatialReference),
					Symbol = gisRouting.GetStopRouteSymbol()
				};
				gisOperations.GetRoutingLayer().Graphics.Add(graphic);
			}
			locationInputViewModel.RefreshRoute();
		}

		private bool IsStopPoint(MapPoint point)
		{
			bool stopPoint = false;
			foreach (var item in locationInputViewModel.LocationsSelected)
			{
				if (Math.Abs(item.Location.X - point.X) + Math.Abs(item.Location.Y - point.Y) < 1)
				{
					stopPoint = true;
					break;
				}
			}
			return stopPoint;
		}

		private static string FormatDistance(double dist, string units)
		{
			string result;
			double formatDistance = Math.Round(dist, 2);
			if (formatDistance != 0)
				result = String.Format("{0} {1}", formatDistance, units);
			else
				result = "";
			return result;
		}

		private static string FormatTime(double minutes)
		{
			TimeSpan time = TimeSpan.FromMinutes(minutes);
			string result = "";
			int hours = (int)Math.Floor(time.TotalHours);
			if (hours > 1)
				result = string.Format(Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.Hours, hours);
			else
				if (hours == 1)
					result = string.Format(Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.Hour, hours);
			if (time.Minutes > 1)
				result += string.Format(Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.Minutes, time.Minutes);
			else
				if (time.Minutes == 1)
					result += string.Format(Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.Minute, time.Minutes);
			return result;
		}

		private void OnQueryCommandClicked(object arg)
		{
			DeleteInfoWindow();
			ActivateLocationWindow();
		}

		private void ActivateLocationWindow()
		{
			IRegion region = regionManager.Regions[Constants.RegionZoek];
			if (region.Views.Count() == 0)
			{
				LocationInput locationInput = this.container.GetExportedValueOrDefault<LocationInput>();
				if (locationInput != null)
				{
					region.Add(locationInput);
					region.Activate(locationInput);
				}
				region.RequestNavigate(new Uri("LocationInput", UriKind.Relative),
					(NavigationResult r) =>
					{
						var error = r.Error;
						var result = r.Result;
					});
				MainTabInfo mainTabInfo = new MainTabInfo() { TabIndex = 2 };
				mainTabEvent.Publish(mainTabInfo);
			}
		}

		private bool CanQueryCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		private void OnZoomInCommandClicked(object arg)
		{
			DeleteInfoWindow();
			gisCommonTasks.ZoomInTask();
		}
		private bool CanZoomInCommandClicked(object arg)
		{
			return isMapLoaded;
		}
		private void OnZoomOutCommandClicked(object arg)
		{
			DeleteInfoWindow();
			gisCommonTasks.ZoomOutTask();
		}
		private bool CanZoomOutCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		private void OnInfoCommandClicked(object arg)
		{
			if (window.IsOpen)
				DeleteInfoWindow();
			if (gisOperations.GetLayersData().Where(l => l.Selection).Count() > 0)
			{
				gisCommonTasks.SetFinishedInfoEvent(HandleResultQuery);
				gisCommonTasks.InfoQuery(true);
			}
		}

		private bool CanInfoCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		/// <summary>
		/// Event handling for the info query
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HandleResultQuery(object sender, ResultInfoEventArgs e)
		{
			if (e != null && !e.Error)
			{
				DisplayInfoResults(e);
			}
		}

		private void DisplayInfoResults(ResultInfoEventArgs e)
		{
			// Create InfoWindow to be used
			if (e.Attributes == null)
				return;
			this.Attributes.AttributeValueList.Clear();
			string objectId = gisOperations.GetFeatureLayerInfo(e.LayerId).ObjectId;
			foreach (var item in e.Attributes)
			{
				if (e.LayerId.Length == 0 || Utilities.IsValidField(item.Key, objectId))
				{
					AttributeFeature attributeFeature = new AttributeFeature() { FieldName = item.Key };
					if (item.Value != null)
						attributeFeature.Value = Utilities.FormatField(item.Value);
					else
						attributeFeature.Value = "";
					this.Attributes.AttributeValueList.Add(attributeFeature);
				}
			}
			DataTemplate dataTemplate = this.menuToolbar.LayoutRoot.Resources["AttributesTemplate"] as DataTemplate;
			DataGrid dataGrid = dataTemplate.LoadContent() as DataGrid;
			dataGrid.ItemsSource = this.Attributes.AttributeValueList;
			this.RaisePropertyChanged(() => this.Attributes);
			Grid layoutRoot = gisOperations.GetMap().Parent as Grid;
			window.Anchor = e.InfoPoint;
			window.Map = gisOperations.GetMap();
			window.IsOpen = true;
			window.Content = dataGrid;
			window.Name = "InfoWindow";
			layoutRoot.Children.Add(window);
		}

		private void HandleGeolocatorResults(object sender, ResultsGeoLocatorEventArgs geoLocatorResults)
		{
			//
			if (geoLocatorResults.result != null && geoLocatorResults.result.Count > 0)
			{
				ResultInfoEventArgs e = new ResultInfoEventArgs(currentPosition, geoLocatorResults.result[0].Location, false)
				{
					Attributes = geoLocatorResults.result[0].Attributes,
					LayerId = string.Empty
				};
				if (locationInputViewModel == null || locationInputViewModel.LocationsSelected.Count == 0)
					DisplayInfoResults(e);
				else
				{
					// Transfer the contents of the result to the stops list
					locationInputViewModel.StopsVisibility = Visibility.Visible;
					locationInputViewModel.RouteDirectionsVisibility = Visibility.Collapsed;
					locationInputViewModel.SelectionVisibility = Visibility.Collapsed;
					locationInputViewModel.LocationsSelected.Add(new GeoLocatorDetail()
					{
						Attributes = geoLocatorResults.result[0].Attributes,
						Location = geoLocatorResults.result[0].Location,
						Match = geoLocatorResults.result[0].Match,
						Title = geoLocatorResults.result[0].Title
					});
					gisOperations.GetSelectLayer().Graphics.Add(new Graphic()
					{
						Geometry = geoLocatorResults.result[0].Location,
						Symbol = geoLocator.GetLocationMarker(),
						Selected = true
					});
					locationInputViewModel.RefreshStops();
					locationInputViewModel.Zoom2Stops();
				}
			}
		}

		private void OnAddLayerCommandClicked(object arg)
		{
			listLayers.layersListCompleted += GetListCompleted;
			listLayers.GetLayerList(configuration.GetApplicationId());
		}

		private void GetListCompleted(object sender, CompleteEvent e)
		{
			listLayers.layersListCompleted -= GetListCompleted;
			if (e.LayerList != null)
			{
				if (e.LayerList.Count > 0)
				{
					// Show window with the possible layers
					listView = new LayerListView();
					LayerListViewModel listViewModel = new LayerListViewModel(e.LayerList);
					modalDialogService.ShowDialog(listView, listViewModel, EndOfLayerSelection);
				}
				else
				{
					ShowMessagebox.Raise(new Notification
					{
						Content = Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.NoLayersAvailable,
						Title = Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.Warning
					});
				}
			}
			else
			{
				// Error occurred
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("GetListCompleted-{0}[{1}]", e.ErrorMessage, ""),
					Title = "System error"
				});
			}
		}

		private void EndOfLayerSelection(LayerListViewModel listViewModel)
		{
			if (listView.DialogResult.HasValue && listView.DialogResult.Value)
			{
				ArcGISMapLayer layer = listViewModel.LayerSelected;
				if (layer != null)
				{
					// Control layer is not on the map
					Layer mapLayer = gisOperations.GetMap().Layers.FirstOrDefault(l => l.ID.Equals(layer.Title));
					if (mapLayer == null)
						gisOperations.AddNewDynamicLayer(layer.RESTURL, layer.VisibleInitial, layer.Title, gisOperations.GetLayerTocSelected(),layer.ServiceType);
				}
			}
			listView = null;
		}

		private bool CanAddLayerCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		private void OnRemoveLayerCommandClicked(object arg)
		{
			Layer layer = gisOperations.GetMap().Layers[gisOperations.GetLayerTocSelected()];
			if (gisOperations.GetLayerTocSelected() > -1 && layer.GetType() != typeof(FeatureLayer))
				gisOperations.RemoveDynamicLayer(gisOperations.GetMap().Layers[gisOperations.GetLayerTocSelected()].ID);
		}

		private bool CanRemoveLayerCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		private void OnPanCommandClicked(object arg)
		{
			gisOperations.SetDrawMode(DrawMode.None);
		}

		private bool CanPanCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		private void OnBookmarkCommandClicked(object arg)
		{
			gisOperations.SetDrawMode(DrawMode.None);
			bookmarks.bookmarksListCompleted += GetListBookmarksCompleted;
			bookmarks.GetBookmarkList(configuration.GetApplicationId());
		}

		private void GetListBookmarksCompleted(object sender, CompleteBookmarksEvent e)
		{
			bookmarks.bookmarksListCompleted -= GetListBookmarksCompleted;
			if (e.BookmarkList != null)
			{
				if (e.BookmarkList.Count > 0)
				{
					// Show window with the possible layers
					bookmarkListView = new BookmarkListView();
					BookmarkListViewModel listViewModel = new BookmarkListViewModel(e.BookmarkList);
					modalDialogService.ShowDialog(bookmarkListView, listViewModel, EndOfBookmarkSelection);
				}
				else
				{
					ShowMessagebox.Raise(new Notification
					{
						Content = Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.NoBookmarksAvailable,
						Title = Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.Warning
					});
				}
			}
			else
			{
				// Error occurred
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("GetListBookmarksCompleted-{0}[{1}]", e.ErrorMessage, ""),
					Title = "System error"
				});
			}
		}

		private void EndOfBookmarkSelection(BookmarkListViewModel listViewModel)
		{
			if (bookmarkListView.DialogResult.HasValue && bookmarkListView.DialogResult.Value)
			{
				BookmarkElement bookmark = listViewModel.BookmarkSelected;
				if (bookmark != null)
				{
					// Zoom to the extent
					Envelope extent = bookmark.Extent.ToEnvelope();
					gisOperations.ZoomTo(extent);
				}
			}
			bookmarkListView = null;
		}

		private bool CanBookmarkCommandClicked(object arg)
		{
			return isMapLoaded;
		}

		/// <summary>
		/// Clear active action on the buttons
		/// </summary>
		private void DeleteInfoWindow()
		{
			gisOperations.DisableDrawMode();
			window.IsOpen = false;
			RemoveInfoWindow(window);
		}

		private void RemoveInfoWindow(InfoWindow infoWindow)
		{
			Grid layoutRoot = gisOperations.GetMap().Parent as Grid;
			layoutRoot.Children.Remove(infoWindow as UIElement);
		}
	}
}
