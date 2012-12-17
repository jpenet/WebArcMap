using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Silverlight.Helper.Dialogs;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.Esri.JTToolbarCommon.Views;

namespace MyToolbar
{
	[ModuleExport(typeof(ToolbarModule))]
	public class ToolbarModule : IModule
	{
		private readonly IRegionManager regionManager;
		private readonly ILoggerFacade loggerFacade;

		[Import]
		public IMessageBoxCustom messageBoxCustom;

		[ImportingConstructor]
		public ToolbarModule(IRegionManager regionManager, ILoggerFacade loggerFacade)
		{
			this.regionManager = regionManager;
			this.loggerFacade = loggerFacade;
		}
		public void Initialize()
		{
			try
			{
				this.regionManager.RegisterViewWithRegion(Constants.RegionToolbarCommon, typeof(MenuToolbar));
			}
			catch (System.Exception ex)
			{
				loggerFacade.Log(ex.Message, Category.Exception, Priority.High);
				var result = messageBoxCustom.Show(ex.Message, Silverlight.UI.Esri.JTToolbarCommon.Resources.ToolbarCommon.ErrorMessage,
				MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}
	}
}
