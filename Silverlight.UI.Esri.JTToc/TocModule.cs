using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.Esri.JTToc.Views;
using Silverlight.Helper.Dialogs;

namespace Silverlight.UI.Esri.JTToc
{
	[ModuleExport(typeof(TocModule))]
	public class TocModule : IModule
	{
		[Import]
		public TocView TocView;

		[Import]
		public IMessageBoxCustom messageBoxCustom;

		private readonly IRegionManager regionManager;
		private readonly ILoggerFacade loggerFacade;

		[ImportingConstructor]
		public TocModule(IRegionManager regionManager, ILoggerFacade loggerFacade)
		{
			this.regionManager = regionManager;
			this.loggerFacade = loggerFacade;
		}

		public void Initialize()
		{
			try
			{
				this.regionManager.RegisterViewWithRegion(Constants.RegionLegend, typeof(TocView));
			}
			catch (System.Exception ex)
			{
				this.loggerFacade.Log(ex.Message, Category.Exception, Priority.High);
				var result = messageBoxCustom.Show(ex.Message, "Error message",
				MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}
	}
}
