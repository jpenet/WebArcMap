
using System.Collections.Generic;
using Silverlight.Helper.DataMapping;
using System.Net;
using System;
namespace Silverlight.UI.Esri.JTToolbarCommon.Models
{
	public class CompleteEvent : EventArgs
	{
		public IList<ArcGISMapLayer> LayerList;
		public string ErrorMessage;
	}
	public delegate void RetrieveLayersCompleted(object sender,CompleteEvent e);

	public class ListLayers
	{
		private IList<ArcGISMapLayer> layerList = null;

		public event RetrieveLayersCompleted layersListCompleted;
		/// <summary>
		/// Retrieve the list of available layers to be added
		/// </summary>
		/// <param name="applicationId">Application ID to identify the layerlist</param>
		/// <param name="retrieveCompleted">Callback method, only used the first time the list is retrieved</param>
		/// <returns></returns>
		public void GetLayerList(string applicationId)
		{
			if (layerList != null)
			{
				CompleteEvent e = new CompleteEvent() { LayerList = layerList,ErrorMessage= string.Empty };
				OnLoaded(e);
			}
			else
			{
				layerList = new List<ArcGISMapLayer>();
				WebClient xmlClient = new WebClient();
				xmlClient.DownloadStringCompleted += DownloadListXMLCompleted;
				xmlClient.DownloadStringAsync(new Uri(String.Format("{0}\\Layers.xml", applicationId), UriKind.RelativeOrAbsolute));
			}
		}

		private void DownloadListXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			try
			{
				string xmlConfig = e.Result;
				layerList = LayerLibConfig.Deserialize(xmlConfig).Layers;
				CompleteEvent completeEvent = new CompleteEvent() { LayerList = layerList,ErrorMessage = string.Empty };
				OnLoaded(completeEvent);
			}
			catch (Exception ex)
			{
				// Force error 
				CompleteEvent completeEvent = new CompleteEvent() { LayerList = null, ErrorMessage = ex.Message };
				OnLoaded(completeEvent);
			}
		}

		protected virtual void OnLoaded(CompleteEvent e)
		{
			if (layersListCompleted != null)
				layersListCompleted(this, e);
		}
	}
}
