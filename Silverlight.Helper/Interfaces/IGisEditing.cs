using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Tasks;
using Silverlight.Helper.DataMapping;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;


namespace Silverlight.Helper.Interfaces
{
	public delegate void GeometryServiceCompleteHandler(object sender, GraphicsEventArgs args);
	public delegate void MultipleResultOperationComplete(IList<Graphic> results);
	public delegate void SingleResultOperationComplete(Geometry result);
	public delegate void EditOperationComplete(int status,string message);
	public interface IGisEditing
	{
		Editor GetEditorTool();
		ObservableCollection<SymbolMarkerInfo> GetMarkerInfo(string layerId);
		IList<EditLayerData> GetEditLayers();
		ObservableCollection<TemplateData> GetTemplates(string layerId);
		void Initialize();
		void StartEditOperation(string operation, EditOperationComplete editOperationComplete);
		void EndEditOperation(int status, string message);
		void Intersect(IList<Graphic> targetGraphics, Geometry intersectGeometry);
		void CutOperation(IList<Graphic> polygons, Polyline polyline, MultipleResultOperationComplete cutOperationComplete);
		void CreateConvexHull(IList<Graphic> pointList, SingleResultOperationComplete singleResultOperationComplete);
		bool SplitPolygon(string layerName, MultipleResultOperationComplete cutOperationComplete);
		void SnapPolygons(string layerName, MultipleResultOperationComplete multipleResultOperationComplete);
		void UnionGeometries(IList<Graphic> geometries);
		void SaveAll();
	}
}
