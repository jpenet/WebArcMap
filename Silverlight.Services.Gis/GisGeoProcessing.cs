//===================================================================================
// GeoProcessing serice.
// This will implement the Geoprocessing functionality available in the ArcGis API
//===================================================================================
using System.Collections.Generic;
using System.ComponentModel.Composition;
using ESRI.ArcGIS.Client.Tasks;
using Silverlight.Helper.Interfaces;
using Silverlight.Helper.DataMapping;

namespace Silverlight.Services.Gis
{
	[Export(typeof(IGisGeoProcessing))]
	public class GisGeoProcessing : IGisGeoProcessing
	{
		private Geoprocessor geoprocessorTask;
		private MyPdfHandler pdfHandler;
		public void CreateShape2PDF(string shapeFile, string layOut, MyPdfHandler pdfHandler)
		{
			this.pdfHandler = pdfHandler;
			List<GPParameter> parameters = new List<GPParameter>();
			parameters.Add(new GPString("ShpaeFile", shapeFile));
			parameters.Add(new GPString("Layout", layOut));
			geoprocessorTask.ExecuteCompleted += GeoProcessingTaskExport2PDF_ExecuteCompleted;
			geoprocessorTask.Failed += GeoprocessorTask_Failed;
			geoprocessorTask.ExecuteAsync(parameters);
		}

		public void StartGeoProcessingTask(string urlTask)
		{
			geoprocessorTask = new Geoprocessor(urlTask);
		}

		private void GeoProcessingTaskExport2PDF_ExecuteCompleted(object sender, GPExecuteCompleteEventArgs e)
		{
			GPParameter gpParameter = e.Results.OutParameters[0];
			PdfEventArgs pdfEvent = new PdfEventArgs() { pdfFile = gpParameter.ToString() };
			pdfHandler(this, pdfEvent);
		}


		private void GeoprocessorTask_Failed(object sender, TaskFailedEventArgs e)
		{
			//MessageBox.Show("Geoprocessor service failed: " + e.Error);
		}
	}
}
