using Silverlight.Helper.DataMapping;

namespace Silverlight.Helper.Interfaces
{
	public delegate  void ConfigurationCompleted();

	public interface IConfiguration
	{
		//void GetConfiguration();
		//object Setting(string key);
		//void SetFinishedEvent(EventHandler finishedFillConfiguration);
		//void ResetFinishedEvent(EventHandler finishedFillConfiguration);
		//object GetKeyValue(string section, object key);
		//IList<string> GetBaseLayerNames();
		void SetApplication(string applicationTitle);
		string GetApplication();
		void SetApplicationId(string applicationId);
		string GetApplicationId();
		void GetConfigurationXML( ConfigurationCompleted configurationCompleted);
		ApplicationConfig GetApplicationConfig();
	}
}
