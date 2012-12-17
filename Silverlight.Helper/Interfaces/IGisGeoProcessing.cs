using Silverlight.Helper.DataMapping;

namespace Silverlight.Helper.Interfaces
{
	public delegate void MyPdfHandler(object sender, PdfEventArgs e);
	public interface IGisGeoProcessing
	{
		void CreateShape2PDF(string shapeFile, string layOut, MyPdfHandler pdfHandler);
		void StartGeoProcessingTask(string urlTask);
	}
}
