using System.Collections.Generic;
using ESRI.ArcGIS.Client;

namespace Silverlight.Helper.Interfaces
{
	public interface IGisCommonTasks
	{
		void ZoomInTask();
		void ZoomOutTask();
		void AttributeQueryTask(string whereValue, string whereField, string layerName,
		string fieldType);
		void SpatialQueryTask(string whereValue, string whereField, string layerName,
		string fieldType, ESRI.ArcGIS.Client.Geometry.Geometry geometry);
		void SetFinishedEvent(ResultsHandler finishedOperation);
		//void ResetFinishedEvent(ResultsHandler finishedOperation);
		void SetFinishedInfoEvent(ResultsInfoHandler finishedOperation);
		//void ResetFinishedInfoEvent(ResultsInfoHandler finishedOperation);
		void InfoQuery(bool spatialQuery);
	}
}
