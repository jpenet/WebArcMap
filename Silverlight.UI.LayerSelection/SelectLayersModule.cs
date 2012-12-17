using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.LayerSelection.Views;
using Silverlight.Helper.Dialogs;


namespace Silverlight.UI.LayerSelection
{
	[ModuleExport(typeof(SelectLayersModule))]
	public class SelectLayersModule : IModule
	{
		private readonly IRegionManager regionManager;
		private readonly ILoggerFacade loggerFacade;

		[Import]
		public IMessageBoxCustom messageBoxCustom;

		[ImportingConstructor]
		public SelectLayersModule(IRegionManager regionManager, ILoggerFacade loggerFacade)
		{
			this.regionManager = regionManager;
			this.loggerFacade = loggerFacade;
		}

		public void Initialize()
		{
			try
			{
				this.regionManager.RegisterViewWithRegion(Constants.RegionSelectie, typeof(SelectLayersView));
			}
			catch (System.Exception ex)
			{
				loggerFacade.Log(ex.Message, Category.Exception, Priority.High);
				var result = messageBoxCustom.Show(ex.Message, "Error message",
				MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}
	}
}
