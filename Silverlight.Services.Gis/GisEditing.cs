using System;
//===================================================================================
// GIS editing service.
// This service depends on the fine grain service GisOperation and therefor need to be initiated after
// the GisOperation
//===================================================================================
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Interfaces;
using Silverlight.Helper.Dialogs;
using Silverlight.Services.Gis.Resources;

namespace Silverlight.Services.Gis
{
	[Export(typeof(IGisEditing))]
	public class GisEditing : IGisEditing
	{
		private readonly IGisOperations gisOperations;
		private readonly IList<EditLayerData> editLayers = new List<EditLayerData>();
		private readonly IMessageBoxCustom messageBoxCustom;
		private string currentLayer;
		private readonly Editor editorTool;

		/// <summary>
		/// Expose the editor control
		/// </summary>
		/// <returns></returns>
		public Editor GetEditorTool()
		{
			return editorTool;
		}

		/// <summary>
		/// Use IGisOperation interface to access the GisOperation service.
		/// </summary>
		/// <param name="gisOperations"></param>
		public GisEditing(IGisOperations gisOperations, IMessageBoxCustom messageBoxCustom)
		{
			editorTool = new Editor();
			this.gisOperations = gisOperations;
			this.messageBoxCustom = messageBoxCustom;
		}

		/// <summary>
		/// Before editing can be done, the initialize must be executed.
		/// </summary>
		public void Initialize()
		{
			try
			{
				this.editorTool.Map = gisOperations.GetMap();
				// Fill edit layers
				editLayers.Clear();
				var el = from l in gisOperations.GetFeatureLayerInfos()
								 where l.LayerGeometryType == GeometryType.Point || l.LayerGeometryType == GeometryType.Polyline
									|| l.LayerGeometryType == GeometryType.Polygon
								 orderby l.Name
								 select new EditLayerData()
								 {
									 LayerName = l.Name,
									 LayerGeometryType = l.LayerGeometryType
								 };
				foreach (var item in el)
					editLayers.Add(item);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("Initialize /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		private EditOperationComplete editOperationComplete;
		public void StartEditOperation(string operation, EditOperationComplete editOperationComplete)
		{
			this.editOperationComplete = editOperationComplete;
		}
	
		public void EndEditOperation(int status,string message)
		{
			if (editOperationComplete != null)
				editOperationComplete(status, message);
		}

		/// <summary>
		/// Get the list of all edit layers
		/// </summary>
		/// <returns></returns>
		public IList<EditLayerData> GetEditLayers()
		{
			return editLayers;
		}

		/// <summary>
		/// Build a list of the symbol markers for a feature layer
		/// </summary>
		/// <param name="layerId">ID for the layer</param>
		/// <returns>Collection of symbol information, symbol (type and templates) + attributes</returns>
		public ObservableCollection<SymbolMarkerInfo> GetMarkerInfo(string layerId)
		{
			try
			{
				ObservableCollection<SymbolMarkerInfo> symbolMarkers =
					new ObservableCollection<SymbolMarkerInfo>();
				IDictionary<object, FeatureType> featureTypes =
					gisOperations.GetFeatureLayerInfo(layerId).FeatureTypes;
				FeatureLayer featLayer = gisOperations.GetFeatureLayer(layerId);
				if (featureTypes != null && featureTypes.Count > 0)
				{
					// Subtypes defined for the feature
					foreach (var item in featureTypes)
					{
						FeatureType featureType = item.Value as FeatureType;
						var featureTemplateItem = featureType.Templates.FirstOrDefault(p => p.Key != "");
						FeatureTemplate featureTemplate = featureTemplateItem.Value as FeatureTemplate;
						symbolMarkers.Add(new SymbolMarkerInfo()
						{
							SymbolMarker = featureTemplate.GetSymbol(featLayer.Renderer),
							Name = featureTemplate.Name,
							ObjectFeatureType = item.Key,
							LayerId = layerId
						});
					}
				}
				else
				{
					// No sub type and no template defined - can only be for attribute tables
				}
				return symbolMarkers;
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("GetMarkerInfo /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
				return null;
			}
		}
		/// <summary>
		/// Get the templates for a layer
		/// </summary>
		/// <param name="layerId"></param>
		/// <returns></returns>
		public ObservableCollection<TemplateData> GetTemplates(string layerId)
		{
			try
			{
				ObservableCollection<TemplateData> templates = new ObservableCollection<TemplateData>();
				IDictionary<string, FeatureTemplate> featureTemplates =
					gisOperations.GetFeatureLayerInfo(layerId).FeatureTemplates;
				if (featureTemplates != null && featureTemplates.Count > 0)
				{
					// Different templates defined for the feature
					foreach (var item in featureTemplates)
					{
						templates.Add(new TemplateData()
						{
							EditTemplate = item.Value,
							Description = (item.Value.Description == null || item.Value.Description.Length == 0 ?
								item.Value.Name : item.Value.Description),
							LayerId = layerId
						});
					}
				}
				return templates;
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("GetTemplates /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
				return null;
			}
		}

		#region Geometry services

		private event GeometryServiceCompleteHandler OnGeometryServiceComplete;
		/// <summary>
		/// Intersect operation
		/// </summary>
		/// <param name="targetGraphics"></param>
		/// <param name="intersectGeometry"></param>
		public void Intersect(IList<Graphic> targetGraphics, Geometry intersectGeometry)
		{
			gisOperations.GetGeometryService().IntersectCompleted += GeometryService_IntersectCompleted;
			gisOperations.GetGeometryService().Failed += GeometryService_Failed;
			gisOperations.GetGeometryService().IntersectAsync(targetGraphics, intersectGeometry);
		}

		private void GeometryService_IntersectCompleted(object sender, GraphicsEventArgs e)
		{
			OnGeometryServiceComplete(this, e);
		}

		private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
		{
			messageBoxCustom.Show(String.Format("{0}/{1}", sender, e.Error.Message), "Severe error during split", MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			if (editOperationComplete != null)
				editOperationComplete(99, e.Error.Message);
		}


		#endregion



		#region Split of two polygons
		private MultipleResultOperationComplete multipleResultOperationComplete;

		public bool SplitPolygon(string layerName, MultipleResultOperationComplete multipleResultOperationComplete)
		{
			try
			{
				this.multipleResultOperationComplete += multipleResultOperationComplete;
				FeatureLayer featureLayer =
					gisOperations.GetFeatureLayer(layerName);
				// Only one polygon may be selected
				if (featureLayer.SelectedGraphics.Count() != 1)
				{
					IList<Graphic> emptyList = new List<Graphic>();
					multipleResultOperationComplete(emptyList);
					return false;
				}
				// Start now the drawing of a polyline
				currentLayer = layerName;
				gisOperations.SetCompleteDrawEvent(OnDrawPolyLineCompleted);
				gisOperations.SetDrawMode(DrawMode.Polyline);
				return true;
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("SplitPolygon /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
				return false;
			}
		}

		/// <summary>
		/// Draw of a polyline for doing the split operation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnDrawPolyLineCompleted(object sender, DrawEventArgs args)
		{
			try
			{
				gisOperations.ResetCompleteDrawEvent(OnDrawPolyLineCompleted);
				Polyline polyline = args.Geometry as Polyline;
				polyline.SpatialReference = gisOperations.GetMap().SpatialReference;
				List<Graphic> currentGraphics = new List<Graphic>();
				FeatureLayer featureLayer =
				gisOperations.GetFeatureLayer(currentLayer);
				foreach (var item in featureLayer.SelectedGraphics)
					currentGraphics.Add(item);
				CutOperation(currentGraphics, polyline, OnSplitCompleted);

			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("OnDrawPolyLineCompleted /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		private void OnSplitCompleted(IList<Graphic> resultSplit)
		{
			try
			{
				IList<string> layerIDs = new List<string>();
				layerIDs.Add(currentLayer);
				this.editorTool.LayerIDs = layerIDs;
				FeatureLayer featureLayer =
				gisOperations.GetFeatureLayer(currentLayer);
				// Attribute handling, transfer attribute contents of the first polygon in the list
				Graphic graphic = featureLayer.SelectedGraphics.FirstOrDefault();
				string objectID = gisOperations.GetFeatureLayerInfo(currentLayer).ObjectId;
				if (graphic != null)
					foreach (var feature in resultSplit)
						foreach (var attrib in graphic.Attributes)
							if (!attrib.Key.Equals(objectID) && !feature.Attributes.ContainsKey(attrib.Key))
								feature.Attributes.Add(attrib.Key, attrib.Value);
				// Delete the initials graphic
				this.editorTool.DeleteSelected.Execute(null);
				// Insert the new values
				foreach (var item in resultSplit)
					featureLayer.Graphics.Add(item);
				multipleResultOperationComplete(resultSplit);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("OnSplitCompleted /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// Split operation between polygons and a polyline
		/// </summary>
		/// <param name="polygons">List of selected polygons involved</param>
		/// <param name="polyline">split line</param>
/// <param name="cutOperationComplete">calling complete event handler</param>
		private MultipleResultOperationComplete stepAfterCut;
		public void CutOperation(IList<Graphic> polygons, Polyline polyline, MultipleResultOperationComplete cutOperationComplete)
		{
			stepAfterCut = cutOperationComplete;
			gisOperations.GetGeometryService().CutCompleted += GeometryService_CutCompleted;
			gisOperations.GetGeometryService().Failed += GeometryService_Failed;
			try
			{
				gisOperations.GetGeometryService().CutAsync(polygons, polyline);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("CutOperation/{0}", ex.Message), "Severe error during split", MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}

		}

		void GeometryService_CutCompleted(object sender, CutEventArgs e)
		{
			gisOperations.GetGeometryService().CutCompleted -= GeometryService_CutCompleted;
			if (stepAfterCut != null)
				stepAfterCut(e.Results);
		}
		#endregion


		#region Create polygon based on points entered
		private event SingleResultOperationComplete singleResultOperationComplete;
		public void CreateConvexHull(IList<Graphic> pointList, SingleResultOperationComplete singleResultOperationComplete)
		{
			this.singleResultOperationComplete += singleResultOperationComplete;
			gisOperations.GetGeometryService().ConvexHullCompleted += GeometryService_ConvexHullCompleted;
			gisOperations.GetGeometryService().Failed += GeometryService_Failed;
			gisOperations.GetGeometryService().ConvexHullAsync(pointList);
		}

		void GeometryService_ConvexHullCompleted(object sender, GeometryEventArgs e)
		{
			gisOperations.GetGeometryService().ConvexHullCompleted -= GeometryService_ConvexHullCompleted;
			if (singleResultOperationComplete != null)
				singleResultOperationComplete(e.Result);
		}
		#endregion

		#region Snap two polygons based on user line definition
		public void SnapPolygons(string layerName, MultipleResultOperationComplete multipleResultOperationComplete)
		{
			try
			{
				this.multipleResultOperationComplete += multipleResultOperationComplete;
				FeatureLayer featureLayer =
					gisOperations.GetFeatureLayer(layerName);
				// Only one polygon may be selected
				if (featureLayer.SelectedGraphics.Count() != 2)
				{
					IList<Graphic> emptyList = new List<Graphic>();
					multipleResultOperationComplete(emptyList);
					return;
				}
				// Start now the drawing of a polyline
				currentLayer = layerName;
				gisOperations.SetCompleteDrawEvent(OnDrawPolyLineSnapCompleted);
				gisOperations.SetDrawMode(DrawMode.Polyline);

			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("SnapPolygons /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnDrawPolyLineSnapCompleted(object sender, DrawEventArgs args)
		{
			try
			{
				gisOperations.ResetCompleteDrawEvent(OnDrawPolyLineSnapCompleted);
				Polyline polyline = args.Geometry as Polyline;
				polyline.SpatialReference = gisOperations.GetMap().SpatialReference;
				// Automcomplete operation
				gisOperations.GetGeometryService().Failed += GeometryService_Failed;
				gisOperations.GetGeometryService().AutoCompleteCompleted += GeometryService_AutoCompleted;
				IList<Graphic> polylines = new List<Graphic>();
				polylines.Add(new Graphic()
				{
					Geometry = polyline as Geometry
				});
				FeatureLayer featureLayer =
					gisOperations.GetFeatureLayer(currentLayer);
				gisOperations.GetGeometryService().AutoCompleteAsync(featureLayer.SelectedGraphics.ToList(), polylines);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("OnDrawPolyLineSnapCompleted /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		void GeometryService_AutoCompleted(object sender, GraphicsEventArgs e)
		{
			try
			{
				if (e.Results.Count == 1)
				{
					// Merge the three polygons into one polygon
					FeatureLayer featureLayer =
						gisOperations.GetFeatureLayer(currentLayer);
					IList<Graphic> selectedGraphics = featureLayer.SelectedGraphics.ToList();
					selectedGraphics.Add(e.Results[0]);
					UnionGeometries(selectedGraphics);
				}
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("GeometryService_AutoCompleted /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}
		#endregion

		#region Merge polygons action
		public void UnionGeometries(IList<Graphic> geometries)
		{
			gisOperations.GetGeometryService().UnionCompleted += GeometryService_UnionCompleted;
			gisOperations.GetGeometryService().Failed += GeometryService_Failed;
			gisOperations.GetGeometryService().UnionAsync(geometries);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void GeometryService_UnionCompleted(object sender, GeometryEventArgs e)
		{
			try
			{
				gisOperations.GetGeometryService().UnionCompleted -= GeometryService_UnionCompleted;
				FeatureLayer featureLayer =
					gisOperations.GetFeatureLayer(currentLayer);
				IList<Graphic> selectedGraphics = featureLayer.SelectedGraphics.ToList();
				// Move the result into the first feature, deletes the rest
				Graphic graphic = selectedGraphics[0];
				graphic.Geometry = e.Result;
				graphic.UnSelect();
				IList<string> layerIDs = new List<string>();
				layerIDs.Add(currentLayer);
				this.editorTool.LayerIDs = layerIDs;
				this.editorTool.DeleteSelected.Execute(null);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("GeometryService_UnionCompleted /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}
		#endregion

		#region Save all pending save
		/// <summary>
		/// 
		/// </summary>
		public void SaveAll()
		{
			try
			{
				bool hasUpdates = false;
				IList<string> layerIds = new List<string>();
				foreach (var item in editorTool.GraphicsLayers)
				{
					if (item is FeatureLayer)
					{
						FeatureLayer featureLayer = item as FeatureLayer;
						if (featureLayer.HasEdits)
						{
							hasUpdates = true;
							layerIds.Add(featureLayer.LayerInfo.Name);
						}
					}
				}
				if (hasUpdates)
				{
					editorTool.LayerIDs = layerIds;
					editorTool.Save.Execute(null);
					editOperationComplete(0, GisTexts.SaveSuccessfull);
				}
				else
				{
					editOperationComplete(0, GisTexts.NoGeometry);
				}
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("SaveAll /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}
		#endregion
	}
}
