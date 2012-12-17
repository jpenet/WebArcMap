using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Silverlight.Helper.General;
using Silverlight.Helper.Interfaces;
using Silverlight.UI.Esri.JTToolbarEditGeneral.Views;
using System.ComponentModel.Composition.Hosting;
using Silverlight.Helper.Dialogs;

namespace Silverlight.UI.Esri.JTToolbarEditGeneral
{
	[ModuleExport(typeof(EditModule))]
	public class EditModule : IModule
	{
		private readonly IRegionManager regionManager;
		private readonly ILoggerFacade loggerFacade;
		private readonly CompositionContainer container;

		[Import]
		public IMessageBoxCustom messageBoxCustom;

		[ImportingConstructor]
		public EditModule(IRegionManager regionManager, ILoggerFacade loggerFacade, CompositionContainer container)
		{
			this.regionManager = regionManager;
			this.loggerFacade = loggerFacade;
			this.container = container;
		}

		public void Initialize()
		{
			try
			{
				this.regionManager.RegisterViewWithRegion(Constants.RegionToolbarEdit, typeof(EditToolBarGeneralView));
				this.regionManager.RegisterViewWithRegion(Constants.RegionToolbarEditAdvanced, typeof(EditToolbarProcessingView));
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
