using System;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Logging;
using Silverlight.Helper.Interfaces;
using Silverlight.Helper.MessageBoxCustom;
using Silverlight.Helper.General;

namespace Silverlight.UI.Esri.JTToolbarGeometryOperations
{
	 [ModuleExport(typeof(GeometryServicesModule))]
	public class GeometryServicesModule:IModule
	{

    private readonly IRegionManager regionManager;
    private readonly ILoggerFacade loggerFacade;

    [Import]
    public IMessageBoxCustom messageBoxCustom;

    [ImportingConstructor]
		public GeometryServicesModule(IRegionManager regionManager, ILoggerFacade loggerFacade)
    {
      this.regionManager = regionManager;
      this.loggerFacade = loggerFacade;
    }

    public void Initialize()
    {
      try
      {
        this.regionManager.RegisterViewWithRegion(Constants.RegionToolbarEdit, typeof(EditToolBarGeneralView));
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
