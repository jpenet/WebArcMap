//===================================================================================
// Configuration service
// After the intialisation, all configuration data is available in this class.
// Methods are available so all modules and services can request the data.
// This service must be intitalized before any other application service can start. Therefor this service
// is started after the authentication service.
//===================================================================================
using System;
using System.ComponentModel.Composition;
using System.Net;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Interfaces;


namespace Silverlight.Services.General
{
	[Export(typeof(IConfiguration))]
	public class Configuration : IConfiguration
	{
		private string gisApplication = string.Empty;
		private string gisApplicationId = string.Empty;
		private readonly IMessageBoxCustom messageBoxCustom;
		private ApplicationConfig appConfig;

		public Configuration(IMessageBoxCustom messageBoxCustom)
		{
			this.messageBoxCustom = messageBoxCustom;
		}

		private ConfigurationCompleted configurationCompleted;
		public void GetConfigurationXML(ConfigurationCompleted configurationCompleted)
		{
			appConfig = new ApplicationConfig();
			this.configurationCompleted = configurationCompleted;
			LoadConfigXML();
		}

		private void LoadConfigXML()
		{
			WebClient xmlClient = new WebClient();
			xmlClient.DownloadStringCompleted += DownloadConfigXMLCompleted;
			xmlClient.DownloadStringAsync(new Uri(String.Format("{0}\\AppConfig.xml", gisApplicationId), UriKind.RelativeOrAbsolute));
		}

		private void DownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			try
			{
				string xmlConfig = e.Result;
				appConfig = ApplicationConfig.Deserialize(xmlConfig);
				gisApplication = appConfig.ApplicationTitle;
				configurationCompleted();
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(ex.Message, "Error message");
			}
		}

		public  ApplicationConfig GetApplicationConfig()
		{
			return appConfig;
		}


		public void SetApplication(string application)
		{
			gisApplication = application;
		}

		public void SetApplicationId(string applicationId)
		{
			gisApplicationId = applicationId;
		}

		public string GetApplication()
		{
			return gisApplication;
		}


		public string GetApplicationId()
		{
			return gisApplicationId;
		}
	}
}
