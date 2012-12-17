using System;
//===================================================================================
// GIS Common Task service
// This service contains elementary ArcGis tasks and tools. The interface of the low level methods
// is injected during the initialize of the bootstrapper. 
//===================================================================================
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Dialogs;
using Silverlight.Helper.Interfaces;
using Silverlight.Services.Gis.Resources;

namespace Silverlight.Services.Gis
{
	[Export(typeof(IGisCommonTasks))]
	public class GisCommonTasks : IGisCommonTasks
	{
		private readonly IGisOperations gisOperations;
		private string currentCommand;
		private readonly IMessageBoxCustom messageBoxCustom;
		private readonly List<FeatureSet> resultSets = new List<FeatureSet>();

		/// <summary>
		/// Constructor makes thee link to the more fine grained GIS functions, interface injection
		/// </summary>
		/// <param name="gisOperations"></param>
		public GisCommonTasks(IGisOperations gisOperations, IMessageBoxCustom messageBoxCustom)
		{
			this.gisOperations = gisOperations;
			this.messageBoxCustom = messageBoxCustom;
		}

		/// <summary>
		/// Trigger the end of an async query
		/// </summary>
		private ResultsHandler finishedQuery;
		protected virtual void OnFinishedQuery(ResultsEventArgs e)
		{
			if (finishedQuery != null)
			{
				finishedQuery(this, e);
			}
		}

		/// <summary>
		/// Trigger the end of an async info request
		/// </summary>
		private ResultsInfoHandler finishedInfo;
		protected virtual void OnFinishedInfo(ResultInfoEventArgs e)
		{
			if (finishedInfo != null)
			{
				finishedInfo(this, e);
			}
		}

		/// <summary>
		/// Zoom In task
		/// </summary>
		public void ZoomInTask()
		{
			currentCommand = "ZoomIn";
			gisOperations.SetCompleteDrawEvent(DrawComplete);
			gisOperations.SetDrawMode(DrawMode.Rectangle); // Start draw tool with a rectangle
		}

		/// <summary>
		/// Zoom out task
		/// </summary>
		public void ZoomOutTask()
		{
			currentCommand = "ZoomOut";
			gisOperations.SetCompleteDrawEvent(DrawComplete);
			gisOperations.SetDrawMode(DrawMode.Rectangle); // Start draw tool with a rectangle
		}

		/// <summary>
		/// End of a draw operation - is tool depended
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void DrawComplete(object sender, DrawEventArgs args)
		{
			try
			{
				if (currentCommand.Equals("Info") || currentCommand.Equals("AddressInfo"))
				{
					// Use ESRI API for Info
					Point screenPoint =
					gisOperations.GetMap().MapToScreen(args.Geometry as MapPoint);
					GeneralTransform generalTransform =
					gisOperations.GetMap().TransformToVisual(Application.Current.RootVisual);
					Point transformScreenPnt = generalTransform.Transform(screenPoint);
					IEnumerable<Graphic> selected = null;
					Graphic result = null;
					FeatureLayer featureLayer = null;
					if (currentCommand.Equals("Info"))
					{
						var selectedLayers = from l in gisOperations.GetLayersData()
																 where l.Selection
																 select l;
						foreach (var item in selectedLayers)
						{
							var layer = (from f in gisOperations.GetMap().Layers
													 where f.ID.Equals(item.LayerName)
													 select f).FirstOrDefault();
							if (layer.GetType() == typeof(FeatureLayer))
							{
								featureLayer = layer as FeatureLayer;
								selected = featureLayer.FindGraphicsInHostCoordinates(transformScreenPnt);
								if (selected != null && selected.Count() > 0)
								{
									// return only one item, the first
									result = selected.FirstOrDefault(g => g.Attributes.Count > 0);
									if (result != null && result.Attributes != null)
									{
										break;
									}
									else
										result = null;
								}
							}
						}
						gisOperations.SetDrawMode(DrawMode.None);
						gisOperations.ResetCompleteDrawEvent(DrawComplete);
					}
					else
					{
						// Address info, stop handling of points
						gisOperations.SetDrawMode(DrawMode.None);
						gisOperations.ResetCompleteDrawEvent(DrawComplete);
					}
					ResultInfoEventArgs resultEventArgs = 
						new ResultInfoEventArgs(screenPoint, args.Geometry as MapPoint, result == null) 
					{ 
						LayerId = string.Empty 
					};
					if (result != null)
					{
						resultEventArgs.Attributes = result.Attributes;
						if (featureLayer != null)
							resultEventArgs.LayerId = featureLayer.ID;
					}
					OnFinishedInfo(resultEventArgs);
				}
				else
				{
					gisOperations.MapZoom(currentCommand, args.Geometry);
					gisOperations.SetDrawMode(DrawMode.Rectangle);
				}

			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("DrawComplete /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// GIS attribute query task
		/// </summary>
		/// <param name="whereValue">where clausule or single value</param>
		/// <param name="whereField">fieldname in case of single field</param>
		/// <param name="layerName">layer name</param>
		/// <param name="fieldType">field type 'C','N'</param>
		public void AttributeQueryTask(string whereValue, string whereField, string layerName, string fieldType)
		{
			gisOperations.SetFinishedEvent(OnFinishedGisQuery);
			gisOperations.AttributeQueryTask_Async(whereValue, whereField, layerName, fieldType, null);
		}

		/// <summary>
		/// Gis spatial and attribute query.
		/// </summary>
		/// <param name="whereValue">where clausule or single value<<></<>
		///<param name="whereField">fieldname in case of single field</param>
		///<param name="layerName">layer name</param>
		///<param name="fieldType">field type 'C','N'</param>
		///<param name="geometry">geometry for the spatial query</param>
		///  </param>
		public void SpatialQueryTask(string whereValue, string whereField, string layerName,
		string fieldType, ESRI.ArcGIS.Client.Geometry.Geometry geometry)
		{
			gisOperations.SetFinishedEvent(OnFinishedGisQuery);
			gisOperations.AttributeQueryTask_Async(whereValue, whereField, layerName, fieldType, geometry);
		}

		/// <summary>
		/// Handles the end of the async query
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnFinishedGisQuery(object sender, ResultsEventArgs e)
		{
			// Send response to the requesting component			
			OnFinishedQuery(e);
		}

		/// <summary>
		/// Set the end handler for an info async request
		/// </summary>
		/// <param name="finishedOperation"></param>
		public void SetFinishedInfoEvent(ResultsInfoHandler finishedOperation)
		{
			finishedInfo = finishedOperation;
		}

		/// <summary>
		/// Set the callback function from the caller to the query request
		/// </summary>
		/// <param name="finishedOperation"></param>
		public void SetFinishedEvent(ResultsHandler finishedOperation)
		{
			finishedQuery = finishedOperation;
		}

		/// <summary>
		/// Info query task
		/// </summary>
		/// <param name="drawMode"></param>
		/// <param name="layerName"></param>
		public void InfoQuery(bool spatialQuery)
		{
			// Info implementation , using ESRI API
			gisOperations.ResetCompleteDrawEvent(DrawComplete);
			if (spatialQuery)
				currentCommand = "Info";
			else
				currentCommand = "AddressInfo";
			gisOperations.SetCompleteDrawEvent(DrawComplete);
			gisOperations.SetDrawMode(DrawMode.Point);
		}
	}
}
