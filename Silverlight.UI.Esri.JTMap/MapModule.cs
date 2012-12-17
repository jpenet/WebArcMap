using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.Esri.JTMap.View;
using Silverlight.UI.Esri.JTMap.ViewModels;

namespace Silverlight.UI.Esri.JTMap
{
	[ModuleExport(typeof(MapModule))]
	public class MapModule : IModule
	{
		private readonly IRegionManager regionManager;
		private readonly IAuthentication authentication;

		[ImportingConstructor]
		public MapModule(IRegionManager regionManager, IAuthentication authentication, IEventAggregator eventAggregator)
		{
			this.regionManager = regionManager;
			this.authentication = authentication;
			if (eventAggregator != null)
				eventAggregator.GetEvent<CompositePresentationEvent<LogonData>>().Subscribe(OnLogonChanged);
		}

		public void OnLogonChanged(LogonData logonData)
		{
			configuration.GetConfigurationXML(ConfigurationCompleted);
		}


		private void ConfigurationCompleted()
		{
			mapViewModel.SetInitialExtent();
		}

		[Import]
		public IConfiguration configuration;

		[Import]
		public MapViewModel mapViewModel;

		public void Initialize()
		{
			this.regionManager.RegisterViewWithRegion(Constants.RegionMap, typeof(MapView));
		}
	}
}
