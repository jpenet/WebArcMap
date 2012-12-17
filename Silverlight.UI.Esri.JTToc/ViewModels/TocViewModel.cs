using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.Esri.JTToc.Views;


namespace Silverlight.UI.Esri.JTToc
{
	[Export(typeof(TocViewModel))]
	public class TocViewModel : NotificationObject
	{
		private readonly ILoggerFacade loggerFacade;
		private readonly IConfiguration configuration;
		private string layerNameSelected;
		private Popup popup;

		public InteractionRequest<Notification> ShowMessagebox { get; set; }

		public InteractionRequest<Notification> ShowErrorMessagebox { get; set; }

		private ICommand _layerClicked;
		public ICommand LayerClicked
		{
			get
			{
				return _layerClicked;
			}
			set
			{
				_layerClicked = value;
				this.RaisePropertyChanged(() => this.LayerClicked);
			}
		}

		#region Context menu - commands
		private ICommand _attributeListCommand;
		private ICommand _zoom2LayerCommand;
		private ICommand _zoom2SelectedCommand;
		private ICommand _selectAllCommand;

		public ICommand AttributeListCommand
		{
			get
			{
				return _attributeListCommand;
			}
			set
			{
				_attributeListCommand = value;
				this.RaisePropertyChanged(() => this.AttributeListCommand);
			}
		}

		public ICommand Zoom2LayerCommand
		{
			get
			{
				return _zoom2LayerCommand;
			}
			set
			{
				_zoom2LayerCommand = value;
				this.RaisePropertyChanged(() => this.Zoom2LayerCommand);
			}
		}

		public ICommand Zoom2SelectedCommand
		{
			get
			{
				return _zoom2SelectedCommand;
			}
			set
			{
				_zoom2SelectedCommand = value;
				this.RaisePropertyChanged(() => this.Zoom2SelectedCommand);
			}
		}

		public ICommand SelectAllCommand
		{
			get
			{
				return _selectAllCommand;
			}
			set
			{
				_selectAllCommand = value;
				this.RaisePropertyChanged(() => this.SelectAllCommand);
			}
		}

		#endregion

		[Import]
		public TocView tocView;

		[Import]
		public IGisOperations gisOperations;

		[ImportingConstructor]
		public TocViewModel(IEventAggregator eventAggregator, ILoggerFacade loggerFacade, IConfiguration configuration)
		{
			if (eventAggregator != null)
				eventAggregator.GetEvent<CompositePresentationEvent<MapData>>().Subscribe(OnMapChanged);
			this.LayerClicked = new DelegateCommand<object>(
			this.OnLayerClicked, this.CanLayerClicked);
			this.AttributeListCommand = new DelegateCommand<object>(
			this.OnAttributeListCommand, this.CanAttributeListCommand);
			this.Zoom2LayerCommand = new DelegateCommand<object>(
			this.OnZoom2LayerCommand, this.CanZoom2LayerCommand);
			this.Zoom2SelectedCommand = new DelegateCommand<object>(
			this.OnZoom2SelectedCommand, this.CanZoom2SelectedCommand);
			this.SelectAllCommand = new DelegateCommand<object>(
			this.OnSelectAllCommand, this.CanSelectAllCommand);
			this.loggerFacade = loggerFacade;
			this.configuration = configuration;
			ShowMessagebox = new InteractionRequest<Notification>();
			ShowErrorMessagebox = new InteractionRequest<Notification>();
		}

		/// <summary>
		/// Refresh legend
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Legend_Refreshed(object sender, Legend.RefreshedEventArgs e)
		{
			try
			{
				if (e.LayerItem.Layer.GetType() == typeof(FeatureLayer))
				{
					e.LayerItem.IsExpanded = false;
				}
				else
					if (e.LayerItem.Layer.GetType() == typeof(GraphicsLayer))
					{
						this.tocView.LayerLegend.LayerItems.Remove(e.LayerItem);
					}
					else
					{
						ArcGISMapLayer layer = null;
						foreach (var item in configuration.GetApplicationConfig().MapConfig.BaseMapLayers)
						{
							layer = item.Layers.FirstOrDefault(b => b.Title.Equals(e.LayerItem.Layer.ID));
							if (layer != null)
								break;
						}

						if (layer != null && !layer.Expandable)
						{
							// Remove the details for the base layers if no details are required
							if (e.LayerItem.LayerItems != null && e.LayerItem.LayerItems.Count > 0)
							{
								e.LayerItem.LayerItems.Clear();
							}
						}
						else
						{
							if (e.LayerItem.LayerItems != null && e.LayerItem.LayerItems.Count > 0)
							{
								for (int i = 0; i < e.LayerItem.LayerItems.Count; i++)
								{
									e.LayerItem.LayerItems[i].IsExpanded = false;
								}
							}
						}
					}

			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("Legend_Refreshed-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		public void OnMapChanged(MapData mapData)
		{
			if (mapData != null)
			{
				tocView.LayerLegend.Map = gisOperations.GetMap();
				// Collapse the legend
				tocView.LayoutUpdated += LayerLegend_LayoutUpdated;
			}
		}

		void LayerLegend_LayoutUpdated(object sender, EventArgs e)
		{
			if (tocView.LayerLegend != null)
			{
				tocView.LayoutUpdated -= LayerLegend_LayoutUpdated;
				tocView.LayerLegend.Refreshed += Legend_Refreshed;
				this.RaisePropertyChanged(() => this.AttributeListCommand);
			}
		}

		private void OnLayerClicked(object arg)
		{
			// Future expansion
			layerNameSelected = (string)arg;
			BaseMapLayerInfo layerInfo = 
				gisOperations.GetBaseMapLayerInfos().FirstOrDefault(l => l.BaseMapLayerId.Equals(layerNameSelected));
			// set index
			if (layerInfo != null)
			{
				Layer layer =
					gisOperations.GetMap().Layers.FirstOrDefault(l => l.ID.Equals(layerInfo.BaseMapLayerId));
				int index = gisOperations.GetMap().Layers.IndexOf(layer);
				gisOperations.SetLayerTocSelected(index);
			}
			else
			{
				FeatureLayerInfo featurelayerInfo =
					gisOperations.GetFeatureLayerInfos().FirstOrDefault(l => l.Id.Equals(layerNameSelected));
				Layer layer =
					gisOperations.GetMap().Layers.FirstOrDefault(l => l.ID.Equals(featurelayerInfo.Name));
				int index = gisOperations.GetMap().Layers.IndexOf(layer);
				gisOperations.SetLayerTocSelected(index);
			}
		}

		private bool CanLayerClicked(object arg)
		{
			return true;
		}

		private void OnAttributeListCommand(object arg)
		{
			layerNameSelected = (string)arg;
			CreateAttributeWindow();
		}

		private bool CanAttributeListCommand(object arg)
		{
			return true;
		}

		private void OnZoom2LayerCommand(object arg)
		{
			layerNameSelected = (string)arg;
			FeatureLayer featurelayer = gisOperations.GetFeatureLayer(layerNameSelected);
			Envelope extent = featurelayer.FullExtent;
			gisOperations.ZoomTo(extent);
		}

		private bool CanZoom2LayerCommand(object arg)
		{
			return true;
		}

		private void OnZoom2SelectedCommand(object arg)
		{
			layerNameSelected = (string)arg;
		}

		private bool CanZoom2SelectedCommand(object arg)
		{
			return true;
		}


		private void OnSelectAllCommand(object arg)
		{
			layerNameSelected = (string)arg;
		}

		private bool CanSelectAllCommand(object arg)
		{
			return true;
		}

		private void CreateAttributeWindow()
		{
			try
			{
				AttributeGrid attributeGrid = new AttributeGrid();
				FeatureDataGrid featureDataGrid =
					attributeGrid.LayoutRoot.FindName("AttributeDataGrid") as FeatureDataGrid;
				featureDataGrid.Map = gisOperations.GetMap();
				featureDataGrid.GraphicsLayer =
				gisOperations.GetMap().Layers[layerNameSelected] as GraphicsLayer;
				featureDataGrid.IsReadOnly = true;
				Button commitButton = attributeGrid.btnClose;
				attributeGrid.btnClose.Click += ClosePopup;
				popup = new Popup()
				{
					VerticalOffset = 150,
					HorizontalOffset = 50,
					Child = attributeGrid,
					IsOpen = true
				};

			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("CreateAttributeWindow-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}
		private void ClosePopup(object sender, EventArgs e)
		{
			popup.IsOpen = false;
			popup = null;
		}
	}
}
