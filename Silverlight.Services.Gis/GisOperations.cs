//===================================================================================
// GisOperation service.
// This contains most of the low level functionality of the ArcGis processing.
// This service is responsible for doing the complete initialize processing of the map
// so that the map module only have to do the final processing for the other modules by communicating
// the final map control contents (publish / subscribe pattern).  
//===================================================================================
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Dialogs;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using Silverlight.Services.Gis.Resources;

namespace Silverlight.Services.Gis
{
	[Export(typeof(IGisOperations))]
	public class GisOperations : IGisOperations
	{
		private Map mapControl;

		private MarkerSymbol markerSymbol;
		private LineSymbol lineSymbol;
		private FillSymbol fillSymbol;
		private bool continuousMode;
		private int layerInitialised;
		private readonly IList<BaseMapLayerInfo> baseMapLayerInfos = new List<BaseMapLayerInfo>();
		private readonly IList<FeatureLayerInfo> featureLayerInfos = new List<FeatureLayerInfo>();
		private readonly IList<LayerData> layersData = new List<LayerData>();
		private bool tabControlVisible = true;
		private readonly IMessageBoxCustom messageBoxCustom;
		private readonly IConfiguration configuration;
		private GraphicsLayer graphicsLayerSelect;
		private GraphicsLayer graphicsLayerRouting;
		private int layerCount = 0;
		private Draw mapDraw;
		private GeometryService geometryService;
		private bool initalizationMap = true;
		private int layerTocSelected = -1;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="messageBoxCustom">message box interface</param>
		/// <param name="configuration">configuration interface</param>
		public GisOperations(IMessageBoxCustom messageBoxCustom, IConfiguration configuration)
		{
			this.messageBoxCustom = messageBoxCustom;
			this.configuration = configuration;
		}
		/// <summary>
		/// Expose the map
		/// </summary>
		/// <returns></returns>
		public Map GetMap()
		{
			return mapControl;
		}

		/// <summary>
		/// Initialize of gis operation class
		/// </summary>
		/// <param name="geometryServiceUrl"></param>
		public void Initialize(string geometryServiceUrl)
		{
			geometryService = new GeometryService(geometryServiceUrl);
		}

		/// <summary>
		/// Return the geometry service for geometry operations
		/// </summary>
		/// <returns></returns>
		public GeometryService GetGeometryService()
		{
			return geometryService;
		}

		/// <summary>
		/// Return select graphicslayer
		/// </summary>
		/// <returns></returns>
		public GraphicsLayer GetSelectLayer()
		{
			return graphicsLayerSelect;
		}

		/// <summary>
		/// Return routing graphicslayer
		/// </summary>
		/// <returns></returns>
		public GraphicsLayer GetRoutingLayer()
		{
			return graphicsLayerRouting;
		}

		/// <summary>
		/// Encapsulation of the retrieve of featurelayer from the map
		/// </summary>
		/// <param name="layerName"></param>
		/// <returns></returns>
		public FeatureLayer GetFeatureLayer(string layerName)
		{
			if (mapControl.Layers[layerName] is FeatureLayer)
				return mapControl.Layers[layerName] as FeatureLayer;
			else
				return null;
		}

		/// <summary>
		/// Retrieve layerinfo from the Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public FeatureLayerInfo GetFeatureLayerInfo(int id)
		{
			return featureLayerInfos[id];
		}

		/// <summary>
		/// Return layerinfo from the name
		/// </summary>
		/// <param name="layerName"></param>
		/// <returns></returns>
		public FeatureLayerInfo GetFeatureLayerInfo(string id)
		{
			return featureLayerInfos.FirstOrDefault<Silverlight.Helper.DataMapping.FeatureLayerInfo>(
			f => f.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase)
			);
		}

		/// <summary>
		/// Expose the list iwth feature layer info
		/// </summary>
		/// <returns></returns>
		public IList<FeatureLayerInfo> GetFeatureLayerInfos()
		{
			return featureLayerInfos;
		}

		/// <summary>
		/// List of all the basemap layers
		/// </summary>
		/// <returns></returns>
		public IList<BaseMapLayerInfo> GetBaseMapLayerInfos()
		{
			return baseMapLayerInfos;
		}

		/// <summary>
		/// Initialisation of the map with all the layers from the configuration
		/// </summary>
		/// <param name="mapControl"></param>
		public void SetLayers(Map mapControl)
		{
			try
			{
				this.mapControl = mapControl;
				//mapControl.SpatialReference =
				//  new SpatialReference(configuration.GetApplicationConfig().MapConfig.InitialExtent.spatialReference);
				mapDraw = new Draw(mapControl)
				{
					LineSymbol = lineSymbol,
					FillSymbol = fillSymbol
				};
				layerInitialised = 0;
				baseMapLayerInfos.Clear();
				layerCount = configuration.GetApplicationConfig().MapConfig.FeatureLayers.Length + 2;
				foreach (var item in configuration.GetApplicationConfig().MapConfig.BaseMapLayers)
					layerCount += item.Layers.Length;
				// Set base layers
				for (int i = 0; i < configuration.GetApplicationConfig().MapConfig.BaseMapLayers.Length; i++)
				{
					for (int j = 0; j < configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers.Length; j++)
					{
						if (configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers[j].ServiceType == ArcGISServiceType.Dynamic)
						{
							AddNewDynamicLayer(configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers[j].RESTURL,
								configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers[j].VisibleInitial,
								configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers[j].Title, ArcGISServiceType.Dynamic);
						}
						else if (configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers[j].ServiceType == ArcGISServiceType.Image)
						{
							AddNewDynamicLayer(configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers[j].RESTURL,
								configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers[j].VisibleInitial,
								configuration.GetApplicationConfig().MapConfig.BaseMapLayers[i].Layers[j].Title,ArcGISServiceType.Image);
						}
					}
				}

				// Set feature layers
				for (int i = 0; i < configuration.GetApplicationConfig().MapConfig.FeatureLayers.Length; i++)
				{
					AddNewFeatureLayer(configuration.GetApplicationConfig().MapConfig.FeatureLayers[i].RESTURL,
						configuration.GetApplicationConfig().MapConfig.FeatureLayers[i].VisibleInitial,
						configuration.GetApplicationConfig().MapConfig.FeatureLayers[i].Title);
				}
				// Add a graphiclayer for putting selections
				graphicsLayerSelect = new GraphicsLayer() { ID = Constants.GraphicsLayerId };
				graphicsLayerSelect.Initialized += InitializedGrLayer;
				graphicsLayerSelect.InitializationFailed += Gr_InitializationFailed;
				mapControl.Layers.Add(graphicsLayerSelect);
				// Add a graphiclayer for routing
				graphicsLayerRouting = new GraphicsLayer() { ID = Constants.GraphicsRoutingId };
				graphicsLayerRouting.Initialized += InitializedGrLayer;
				graphicsLayerRouting.InitializationFailed += Gr_InitializationFailed;
				//
				mapControl.Layers.Add(graphicsLayerRouting);

			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("SetLayers /{0}", ex.Message), GisTexts.SevereError, MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// Remove a dynamic layer from the map and the layer table
		/// </summary>
		/// <param name="layerName">Base Layer name</param>
		public void RemoveDynamicLayer(string layerName)
		{
			Layer layer = mapControl.Layers.FirstOrDefault(l => l.ID.Equals(layerName));
			if (layer != null && layer.GetType() == typeof(ArcGISDynamicMapServiceLayer))
			{
				mapControl.Layers.Remove(layer);
				BaseMapLayerInfo baseMapLayerInfo = baseMapLayerInfos.FirstOrDefault(l => l.BaseMapLayerId.Equals(layerName));
				if (baseMapLayerInfo != null)
					baseMapLayerInfos.Remove(baseMapLayerInfo);
			}
		}

		/// <summary>
		/// Add new dynamic base layer at the end of the layer list
		/// </summary>
		/// <param name="layerUrl">URL of the rest mapservice</param>
		/// <param name="visible">Visible or not</param>
		/// <param name="layerName">Layername allocated</param>
		public void AddNewDynamicLayer(string layerUrl, bool visible, string layerName, ArcGISServiceType serviceType)
		{
			AddNewDynamicLayer(layerUrl, visible, layerName, -1, serviceType);
		}

		/// <summary>
		/// Add new dynamic base layer at a specific position
		/// </summary>
		/// <param name="layerUrl">URL of the rest mapservice</param>
		/// <param name="visible">Visible or not</param>
		/// <param name="layerName">Layername allocated</param>
		/// <param name="index">Position in the map</param>
		public void AddNewDynamicLayer(string layerUrl, bool visible, string layerName, int index,ArcGISServiceType serviceType)
		{
			if (serviceType == ArcGISServiceType.Dynamic)
			{
				ArcGISDynamicMapServiceLayer mapLayer = new ArcGISDynamicMapServiceLayer()
				{
					Url = layerUrl,
					DisableClientCaching = true,
					ID = layerName,
					Visible = visible
				};
				mapLayer.Initialized += InitializedDynamicLayer;
				mapLayer.InitializationFailed += layer_InitializationFailed;
				if (index < 0)
					mapControl.Layers.Add(mapLayer);
				else
					mapControl.Layers.Insert(index, mapLayer);
			}
			else if (serviceType == ArcGISServiceType.Image)
			{
				ArcGISTiledMapServiceLayer mapLayer = new ArcGISTiledMapServiceLayer()
				{
					Url = layerUrl,
					ID = layerName,
					Visible = visible
				};
				mapLayer.Initialized += InitializedDynamicLayer;
				mapLayer.InitializationFailed += layer_InitializationFailed;
				if (index < 0)
					mapControl.Layers.Add(mapLayer);
				else
					mapControl.Layers.Insert(index, mapLayer);
			}
		}


		/// <summary>
		/// Add a new feature layer from a feature service
		/// </summary>
		/// <param name="layerUrl">Url Rest of the feature service</param>
		/// <param name="visible">visible or not</param>
		/// <param name="layerName">layername allocated</param>
		public void AddNewFeatureLayer(string layerUrl, bool visible, string layerName)
		{
			FeatureLayer featureLayer = new FeatureLayer()
			{
				Url = layerUrl,
				AutoSave = false,
				Mode = FeatureLayer.QueryMode.OnDemand,
				ID = layerName,
				Visible = visible
			};
			featureLayer.OutFields.Add("*");
			featureLayer.Where = string.Empty;
			featureLayer.Initialized += InitializedFtLayer;
			featureLayer.InitializationFailed += featurelayer_InitializationFailed;
			mapControl.Layers.Add(featureLayer);
		}

		void layer_InitializationFailed(object sender, EventArgs e)
		{
			VerifyInitialisationMap();
			Layer layer = sender as Layer;
			messageBoxCustom.Show(String.Format(GisTexts.DynamicLayerInitialisationFailed, layer.ID, layer.InitializationFailure), "Severe error", MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
		}

		void featurelayer_InitializationFailed(object sender, EventArgs e)
		{
			VerifyInitialisationMap();
			Layer layer = sender as Layer;
			messageBoxCustom.Show(String.Format(GisTexts.FeatureLayerInitialisationFailed, layer.ID, layer.InitializationFailure), "Severe error", MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
		}

		void InitializedGrLayer(object sender, EventArgs e)
		{
			VerifyInitialisationMap();
		}

		void Gr_InitializationFailed(object sender, EventArgs e)
		{
			Layer layer = sender as Layer;
			messageBoxCustom.Show(String.Format(GisTexts.GraphicLayerInitialisationFailed, sender, layer.InitializationFailure), "Severe error", MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
		}

		/// <summary>
		/// Using the initialized event, detail data form the layers are saved.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InitializedDynamicLayer(object sender, EventArgs e)
		{
			try
			{
				if (sender.GetType() == typeof(ArcGISDynamicMapServiceLayer))
				{
					ArcGISDynamicMapServiceLayer serviceLayer = (ArcGISDynamicMapServiceLayer)sender;
					if (serviceLayer.Layers == null)
						return;
					foreach (var item in serviceLayer.Layers)
					{
						baseMapLayerInfos.Add(new Helper.DataMapping.BaseMapLayerInfo()
						{
							Name = item.Name,
							Url = serviceLayer.Url,
							Id = item.ID,
							BaseMapLayerId = serviceLayer.ID
						}
						);
					}
				}
				else if (sender.GetType() == typeof(ArcGISTiledMapServiceLayer))
				{
					ArcGISTiledMapServiceLayer serviceLayer = (ArcGISTiledMapServiceLayer)sender;
					if (serviceLayer.Layers == null)
						return;
					foreach (var item in serviceLayer.Layers)
					{
						baseMapLayerInfos.Add(new Helper.DataMapping.BaseMapLayerInfo()
						{
							Name = item.Name,
							Url = serviceLayer.Url,
							Id = item.ID,
							BaseMapLayerId = serviceLayer.ID
						}
						);
					}
				}
				VerifyInitialisationMap();
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("InitializedDynamicLayer-{0}/{1}", sender, ex.Message), GisTexts.SevereError, MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InitializedFtLayer(object sender, EventArgs e)
		{
			try
			{
				FeatureLayer layer = (FeatureLayer)sender;
				featureLayerInfos.Add(new Helper.DataMapping.FeatureLayerInfo()
				{
					FeatureTemplates = layer.LayerInfo.Templates,
					Url = layer.Url,
					Name = layer.LayerInfo.Name,
					Id = layer.ID,
					FeatureTypes = layer.LayerInfo.FeatureTypes,
					LayerGeometryType = layer.LayerInfo.GeometryType,
					ObjectId = layer.LayerInfo.ObjectIdField
				});
				layersData.Add(new LayerData()
				{
					ID = layer.LayerInfo.Id,
					LayerName = layer.LayerInfo.Name,
					Selection = true
				});
				VerifyInitialisationMap();
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("InitializedFtLayer-{0}/{1}", sender, ex.Message), GisTexts.SevereError, MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// Trigger the finished event when all layers has been initialized (feature layers + dynamic layers) 
		/// </summary>
		private void VerifyInitialisationMap()
		{
			layerInitialised += 1;
			if (initalizationMap && layerInitialised >= layerCount)
			{
				initalizationMap = false;
				OnInitialiseMapComplete();
			}
		}

		/// <summary>
		/// Handling of the completion of the initialise of all layers for a map
		/// </summary>
		private InitialiseCompleteHandler mapInitialiseComplete;
		protected virtual void OnInitialiseMapComplete()
		{
			if (mapInitialiseComplete != null)
			{
				mapInitialiseComplete();
			}
		}

		/// <summary>
		/// Set handler for the initialize is complete for a map
		/// </summary>
		/// <param name="mapInitialiseComplete"></param>
		public void SetMapInitialiseCompleteEvent(InitialiseCompleteHandler mapInitialiseComplete)
		{
			this.mapInitialiseComplete = mapInitialiseComplete;
		}

		///// <summary>
		///// Reset handler for the initialize of the map
		///// </summary>
		///// <param name="mapInitialiseComplete"></param>
		//public void ResetMapInitialiseCompleteEvent(InitialiseCompleteHandler mapInitialiseComplete)
		//{
		//  this.mapInitialiseComplete -= mapInitialiseComplete;
		//}


		/// <summary>
		/// Handling of the async query operation and return control to the caller
		/// </summary>
		private ResultsHandler finishedFill;
		protected virtual void OnFinishedAttributeQuery(ResultsEventArgs e)
		{
			if (finishedFill != null)
			{
				finishedFill(this, e);
			}
		}


		/// <summary>
		/// Handling the draw complete event and return control to the caller.
		/// </summary>
		private event DrawCompleteHandeler drawComplete;
		protected virtual void OnDrawComplete(DrawEventArgs e)
		{
			if (drawComplete != null)
			{
				drawComplete(this, e);
			}
		}

		/// <summary>
		/// General query task, attribute query only + spatial query with attribute query combined. 
		/// </summary>
		/// <param name="whereValue">Where clause or single value</param>
		/// <param name="whereField">fieldname in case query on one field</param>
		/// <param name="layerName">layer name</param>
		/// <param name="fieldType">field type 'C','N'</param>
		/// <param name="geometry">geometry for the spatial query</param>
		public void AttributeQueryTask_Async(string whereValue, string whereField, string layerName,
		string fieldType, ESRI.ArcGIS.Client.Geometry.Geometry geometry)
		{
			try
			{
				int layerID = -1;
				var result = (from l in baseMapLayerInfos
											where l.Name.Equals(layerName)
											select l).FirstOrDefault<Helper.DataMapping.BaseMapLayerInfo>();
				string urlFeatureLayer = String.Format("{0}/{1}", result.Url, result.Id);
				layerID = result.Id;
				QueryTask queryTask = new QueryTask(urlFeatureLayer);
				Query query = new Query();
				// Specify fields to return from initial query
				query.OutFields.Add("*");
				if (whereField.Length == 0)
				{
					// In case no single field, asume where clause in the value 
					query.Where = whereValue;
				}
				else
				{
					// This query will just populate the attributes, so no need to return geometry
					if (fieldType == "C")
					{
						query.Where = String.Format("{0} like '{1}%'", whereField, whereValue.Trim());
					}
					else
						if (fieldType == "N")
						{
							double value = 0;
							if (double.TryParse(whereValue, out value))
							{
								query.Where = String.Format("{0} ={1}", whereField, whereValue.Trim());
							}
							else
							{
								//MessageBox.Show("No valid numeric value was entered!");
								return;
							}
						}
						else
						{
							//MessageBox.Show("Only numeric and character queries are supported!");
							return;
						}
				}
				// Hnadle spatial query if needed
				if (geometry != null)
				{
					if (geometry.GetType() == typeof(MapPoint))
					{
						MapPoint point = (MapPoint)geometry;
						const double offset = 5;
						geometry = new Envelope(point.X - offset, point.Y - offset, point.X + offset, point.Y + offset);
					}
					query.Geometry = geometry;
					query.SpatialRelationship = SpatialRelationship.esriSpatialRelIntersects;
				}
				// Return geometry with result features
				queryTask.ExecuteCompleted += AttributeQueryTask_ExecuteCompleted;
				queryTask.Failed += AttributeQueryTask_Failed; // Same as for the spatial query
				query.ReturnGeometry = true;
				queryTask.ExecuteAsync(query, layerID);
			}
			catch (Exception ex)
			{
				// Todo : error handling
				messageBoxCustom.Show(String.Format("AttributeQueryTask_Async-{0}", ex.Message),
					GisTexts.SevereError,
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// Query task is complete, send an event to the caller with the result in the event argument.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void AttributeQueryTask_ExecuteCompleted(object sender, QueryEventArgs args)
		{
			try
			{
				FeatureSet featureSet = args.FeatureSet;
				int iD = (int)args.UserState;
				ResultsEventArgs eventArgs = new ResultsEventArgs(featureSet, iD);
				OnFinishedAttributeQuery(eventArgs);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("AttributeQueryTask_ExecuteCompleted-{0}", ex.Message),
					GisTexts.SevereError,
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}

		}

		/// <summary>
		/// Query failed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void AttributeQueryTask_Failed(object sender, TaskFailedEventArgs args)
		{
			messageBoxCustom.Show(String.Format("AttributeQueryTask_Failed-{0}", args.Error.Message),
				GisTexts.SevereError,
				MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
		}


		/// <summary>
		/// Set the callback for the query operation.
		/// </summary>
		/// <param name="finishedOperation"></param>
		public void SetFinishedEvent(ResultsHandler finishedOperation)
		{
			finishedFill = finishedOperation;
		}



		/// <summary>
		/// Get the marker symbol for selection
		/// </summary>
		/// <returns></returns>
		public MarkerSymbol GetSelectionMarkerSymbol()
		{
			return markerSymbol;
		}

		/// <summary>
		/// Get the line symbol for selection
		/// </summary>
		/// <returns></returns>
		public LineSymbol GetSelectionLineSymbol()
		{
			return lineSymbol;
		}

		/// <summary>
		/// Get the fill symbol for selection
		/// </summary>
		/// <returns></returns>
		public FillSymbol GetSelectionFillSymbol()
		{
			return fillSymbol;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="symbol"></param>
		public void SetSelectionMarkerSymbol(MarkerSymbol symbol)
		{
			markerSymbol = symbol;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="symbol"></param>
		public void SetSelectionLineSymbol(LineSymbol symbol)
		{
			lineSymbol = symbol;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="symbol"></param>
		public void SetSelectionFillSymbol(FillSymbol symbol)
		{
			fillSymbol = symbol;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="drawMode"></param>
		public void SetDrawMode(DrawMode drawMode)
		{
			if (mapDraw != null)
			{
				mapDraw.DrawMode = drawMode;
				mapDraw.IsEnabled = (mapDraw.DrawMode != DrawMode.None);
				continuousMode = false;
				mapDraw.DrawComplete += MapDrawSurface_DrawComplete;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void MapDrawSurface_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
		{
			if (!continuousMode)
			{
				mapDraw.DrawMode = DrawMode.None;
				mapDraw.IsEnabled = false;
			}
			OnDrawComplete(args);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="drawComplete"></param>
		public void SetCompleteDrawEvent(DrawCompleteHandeler drawComplete)
		{
			this.drawComplete = null;
			this.drawComplete += drawComplete;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="drawComplete"></param>
		public void ResetCompleteDrawEvent(DrawCompleteHandeler drawComplete)
		{
			this.drawComplete -= drawComplete;
		}
		/// <summary>
		///
		/// </summary>
		/// <param name="drawMode"></param>
		/// <param name="drawComplete"></param>
		public void CreateGeometry(DrawMode drawMode, DrawCompleteHandeler drawComplete)
		{
			SetCompleteDrawEvent(drawComplete);
			continuousMode = false;
			SetDrawMode(drawMode);
		}
		/// <summary>
		/// Zoom in and out task
		/// </summary>
		/// <param name="action">ZoomIn or ZoomOut</param>
		/// <param name="geometry">Rectangle to be zoomed</param>
		public void MapZoom(string action, ESRI.ArcGIS.Client.Geometry.Geometry geometry)
		{
			try
			{
				if (action.Equals("ZoomIn"))
				{
					mapControl.ZoomTo(geometry as Envelope);
				}
				else
					if (action.Equals("ZoomOut"))
					{
						Envelope currentExtent = mapControl.Extent;
						Envelope zoomBoxExtent = geometry as Envelope;
						MapPoint zoomBoxCenter = zoomBoxExtent.GetCenter();

						double whRatioCurrent = currentExtent.Width / currentExtent.Height;
						double whRatioZoomBox = zoomBoxExtent.Width / zoomBoxExtent.Height;

						Envelope newEnv = null;

						if (whRatioZoomBox > whRatioCurrent)
						// use width
						{
							double multiplier = currentExtent.Width / zoomBoxExtent.Width;
							double newWidthMapUnits = currentExtent.Width * multiplier;
							newEnv = new Envelope(new MapPoint(zoomBoxCenter.X - (newWidthMapUnits / 2), zoomBoxCenter.Y),
							new MapPoint(zoomBoxCenter.X + (newWidthMapUnits / 2), zoomBoxCenter.Y));
						}
						else
						// use height
						{
							double multiplier = currentExtent.Height / zoomBoxExtent.Height;
							double newHeightMapUnits = currentExtent.Height * multiplier;
							newEnv = new Envelope(new MapPoint(zoomBoxCenter.X, zoomBoxCenter.Y - (newHeightMapUnits / 2)),
							new MapPoint(zoomBoxCenter.X, zoomBoxCenter.Y + (newHeightMapUnits / 2)));
						}
						if (newEnv != null)
							mapControl.ZoomTo(newEnv);
					}
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("MapZoom-{0}", ex.Message),
					GisTexts.SevereError,
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// Disable the drawing surface
		/// </summary>
		public void DisableDrawMode()
		{
			mapDraw.IsEnabled = false;
			mapDraw.DrawMode = DrawMode.None;
		}

		/// <summary>
		/// Enable the drawing surface
		/// </summary>
		public void EnableDrawMode()
		{
			mapDraw.IsEnabled = true;
		}

		/// <summary>
		/// Activate a drwaing mode without resetting the drawing af a finished draw operation
		/// </summary>
		/// <param name="drawMode"></param>
		public void SetDrawModeContinuous(DrawMode drawMode)
		{
			if (mapDraw != null)
			{
				mapDraw.DrawMode = drawMode;
				mapDraw.IsEnabled = (mapDraw.DrawMode != DrawMode.None);
				continuousMode = true;
				mapDraw.DrawComplete += MapDrawSurface_DrawComplete;
			}
		}

		/// <summary> Center and zoom in case of a point only based on a minimum extent, support
		/// method for center on results.
		/// </summary>
		/// <param name="map"></param>
		/// <param name="point"></param>
		/// <param name="resolution"></param>
		public void CenterAndZoom(MapPoint point, double resolution)
		{
			try
			{
				double ratio;
				if (mapControl.Resolution != 0)
					ratio = resolution / mapControl.Resolution;
				else
					ratio = 1.0;
				if (ratio == 1)
				{
					mapControl.PanTo(point);
				}
				else
				{
					MapPoint center = mapControl.Extent.GetCenter();
					double x = (point.X - ratio * center.X) / (1 - ratio);
					double y = (point.Y - ratio * center.Y) / (1 - ratio);
					mapControl.ZoomToResolution(resolution, new MapPoint(x, y));
				}
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("CenterAndZoom-{0}", ex.Message),
					GisTexts.SevereError,
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// List of all layers needed doing selections or other handling
		/// </summary>
		/// <returns></returns>
		public IList<LayerData> GetLayersData()
		{
			return layersData;
		}
		/// <summary>
		/// Utility function
		/// Scale calculations, this is ok in most cases if the screen dpi is at 96
		/// </summary>
		private const double dpi = 96;
		public double Resolution2Scale(double resolution)
		{
			return dpi * 39.37 * (resolution * 1);
		}
		public double Scale2Resolution(double scale)
		{
			if (scale > 0)
				return scale / (dpi * 39.37);
			else
				return 10000;
		}

		/// <summary>
		/// Resize map to the maximum base on the tab control available or not
		/// </summary>
		public void SetMapSize(int offset)
		{
			try
			{
				if (mapControl == null)
					return;
				double clientHeight = BrowserScreenInfo.ClientHeight;
				double clientWidth = BrowserScreenInfo.ClientWidth;
				if (clientWidth > 500 && clientHeight > 400)
				{
					mapControl.Height = clientHeight - 130;
					if (tabControlVisible)
						mapControl.Width = clientWidth - offset;
					else
						mapControl.Width = clientWidth;
				}
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("SetMapSize-{0}", ex.Message),
					GisTexts.SevereError,
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// Maintain tab control visible or not
		/// </summary>
		/// <param name="tabControlVisible"></param>
		public void SetTabControlVisible(bool tabControlVisible)
		{
			this.tabControlVisible = tabControlVisible;
		}

		public int GetLayerTocSelected()
		{
			return layerTocSelected;
		}

		public void SetLayerTocSelected(int layerIndex)
		{
			layerTocSelected = layerIndex;
		}

		/// <summary>
		/// Basic zoom function 
		/// </summary>
		/// <param name="geometry"></param>
		public void ZoomTo(ESRI.ArcGIS.Client.Geometry.Geometry geometry)
		{
			try
			{
				Envelope extent = geometry.Extent.Expand(1.2);
				if (geometry.SpatialReference.WKID != mapControl.SpatialReference.WKID)
				{
					// Conversion is required
					messageBoxCustom.Show(String.Format("ZoomTo, spatial reference different from map-{0}", geometry.SpatialReference.WKID),
						GisTexts.SevereError,
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
				}
				else
					mapControl.ZoomTo(extent);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("ZoomTo-{0}", ex.Message),
					GisTexts.SevereError,
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

/// <summary>
/// Calculate the extent taken into account of the round mapping possibilities
/// </summary>
/// <param name="extent">Extent initial value</param>
/// <returns></returns>
		public Envelope GetNormalizedExtent(Envelope extent)
		{
			Envelope newExtent = null;
			if (mapControl.WrapAroundIsActive)
			{
				Geometry normalizedExtent = Geometry.NormalizeCentralMeridian(extent);
				if (normalizedExtent is Polygon)
				{
					newExtent = new Envelope();

					foreach (MapPoint p in (normalizedExtent as Polygon).Rings[0])
					{
						if (p.X < newExtent.XMin || double.IsNaN(newExtent.XMin))
							newExtent.XMin = p.X;
						if (p.Y < newExtent.YMin || double.IsNaN(newExtent.YMin))
							newExtent.YMin = p.Y;
					}

					foreach (MapPoint p in (normalizedExtent as Polygon).Rings[1])
					{
						if (p.X > newExtent.XMax || double.IsNaN(newExtent.XMax))
							newExtent.XMax = p.X;
						if (p.Y > newExtent.YMax || double.IsNaN(newExtent.YMax))
							newExtent.YMax = p.Y;
					}
				}
				else if (normalizedExtent is Envelope)
					newExtent = normalizedExtent as Envelope;
			}
			else
				newExtent = extent;
			return extent;
		}
	}
}
