using System;
using System.Collections.Generic;
using Silverlight.Helper.DataMapping;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace Silverlight.Helper.Interfaces
{
	public delegate void ResultsHandler(object sender, ResultsEventArgs e);
	public delegate void ResultsListHandler(object sender, ResultsListEventArgs e);
	public delegate void ResultsInfoHandler(object sender, ResultInfoEventArgs e);
	public delegate void DrawCompleteHandeler(object sender, DrawEventArgs args);
	public delegate void InitialiseCompleteHandler();
	public interface IGisOperations
	{
		Map GetMap();
		void Initialize(string geometryServiceUrl);
		GraphicsLayer GetRoutingLayer();
		GraphicsLayer GetSelectLayer();
		FeatureLayer GetFeatureLayer(string layerName);
		void SetLayers(Map mapControl);
		void AddNewDynamicLayer(string layerUrl, bool visible, string layerName, ArcGISServiceType serviceType);
		void AddNewDynamicLayer(string layerUrl, bool visible, string layerName, int index, ArcGISServiceType serviceType);
		void AddNewFeatureLayer(string layerUrl, bool visible, string layerName);
		void RemoveDynamicLayer(string layerName);
		void AttributeQueryTask_Async(string whereValue, string whereField, string layerName, string fieldType, ESRI.ArcGIS.Client.Geometry.Geometry geometry);
		void AttributeQueryTask_ExecuteCompleted(object sender, QueryEventArgs args);
		void AttributeQueryTask_Failed(object sender, TaskFailedEventArgs args);
		void SetFinishedEvent(ResultsHandler finishedOperation);
		void SetCompleteDrawEvent(DrawCompleteHandeler drawComplete);
		void ResetCompleteDrawEvent(DrawCompleteHandeler drawComplete);
		void SetMapInitialiseCompleteEvent(InitialiseCompleteHandler mapInitialiseComplete);
		MarkerSymbol GetSelectionMarkerSymbol();
		LineSymbol GetSelectionLineSymbol();
		FillSymbol GetSelectionFillSymbol();
		void SetSelectionMarkerSymbol(MarkerSymbol symbol);
		void SetSelectionLineSymbol(LineSymbol symbol);
		void SetSelectionFillSymbol(FillSymbol symbol);
		void SetDrawMode(DrawMode drawMode);
		void SetDrawModeContinuous(DrawMode drawMode);
		void DisableDrawMode();
		void EnableDrawMode();
		void MapZoom(string action, Geometry geometry);
		FeatureLayerInfo GetFeatureLayerInfo(int id);
		FeatureLayerInfo GetFeatureLayerInfo(string layerName);
		GeometryService GetGeometryService();
		void CenterAndZoom(MapPoint point, double resolution);
		IList<LayerData> GetLayersData();
		double Resolution2Scale(double resolution);
		double Scale2Resolution(double scale);
		IList<FeatureLayerInfo> GetFeatureLayerInfos();
		IList<BaseMapLayerInfo> GetBaseMapLayerInfos();
		void CreateGeometry(DrawMode drawMode, DrawCompleteHandeler drawComplete);
		void SetMapSize(int offset);
		void SetTabControlVisible(bool tabControlVisible);
		int GetLayerTocSelected();
		void SetLayerTocSelected(int layerIndex);
		void ZoomTo(Geometry geometry);
		Envelope GetNormalizedExtent(Envelope extent);
	}
}
