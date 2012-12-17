using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;
using Silverlight.Helper.Dialogs;
using Silverlight.Helper.Interfaces;
using Silverlight.Services.General;
using Silverlight.Services.Gis;

namespace WebArcMap
{
	/// <summary>
	/// custom bootstrapper based on MEF
	/// </summary>
	public class JTBootstrapper : MefBootstrapper
	{
		private Configuration configuration;
		private GisOperations gisOperations;
		private GisRouting gisRouting;
		private GisCommonTasks gisCommonTasks;
		private readonly GisGeoProcessing gisGeoProcessing = new GisGeoProcessing();
		private GisEditing gisEditing;
		private readonly MessageBoxCustom messageBoxCustom = new MessageBoxCustom();
		private readonly CallbackLogger callbackLogger = new CallbackLogger();
		private readonly ModalDialogService modalDialogService = new ModalDialogService();
		private readonly Authentication authentication = new Authentication();
		private Geolocator geoLocator;
		private DialogManager dialogManager;
		private readonly IDictionary<string, string> parameters;

		public JTBootstrapper(IDictionary<string, string> parameters)
		{
			this.parameters = parameters;
		}

		protected override ILoggerFacade CreateLogger()
		{
			return this.callbackLogger;
		}

		protected override void ConfigureAggregateCatalog()
		{
			base.ConfigureAggregateCatalog();
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(JTBootstrapper).Assembly));
		}

		protected override DependencyObject CreateShell()
		{
			return this.Container.GetExportedValue<Shell>();
		}

		protected override void InitializeShell()
		{
			base.InitializeShell();
			Application.Current.RootVisual = (UIElement)this.Shell;
		}

		private const string ModuleCatalogUri =
		"/WebArcMap;component/ModulesCatalog.xaml";

		protected override IModuleCatalog CreateModuleCatalog()
		{
			try
			{
				Uri uri = new Uri(ModuleCatalogUri, UriKind.Relative);
				Logger.Log("Catalog:" + uri.OriginalString, Category.Info, Priority.Low);
				return Microsoft.Practices.Prism.Modularity.ModuleCatalog.CreateFromXaml(uri);
			}
			catch (ModuleInitializeException ex)
			{
				Logger.Log(ex.Message, Category.Exception, Priority.High);
				return null;
			}
		}

		/// <summary>
		/// Initialise the different services after the container is configured.
		/// </summary>
		protected override void ConfigureContainer()
		{
			try
			{
				base.ConfigureContainer();
				// Because we created the Configuration and it needs to be used immediately, we compose it to satisfy any imports it has.
				this.Container.ComposeExportedValue<IMessageBoxCustom>(this.messageBoxCustom);
				this.configuration = new Configuration(this.messageBoxCustom);
				this.gisOperations = new GisOperations(this.messageBoxCustom,this.configuration);
				this.gisRouting = new GisRouting(this.gisOperations, this.messageBoxCustom);
				this.gisCommonTasks = new GisCommonTasks(this.gisOperations, this.messageBoxCustom);
				this.gisEditing = new GisEditing(this.gisOperations, this.messageBoxCustom);
				this.geoLocator = new Geolocator(this.messageBoxCustom);
				this.Container.ComposeExportedValue<IConfiguration>(this.configuration);
				this.Container.ComposeExportedValue<IGisOperations>(this.gisOperations);
				this.Container.ComposeExportedValue<IGisRouting>(this.gisRouting);
				this.Container.ComposeExportedValue<IGisCommonTasks>(this.gisCommonTasks);
				this.Container.ComposeExportedValue<IGisGeoProcessing>(this.gisGeoProcessing);
				this.Container.ComposeExportedValue<IGisEditing>(this.gisEditing);
				this.Container.ComposeExportedValue<CompositionContainer>(this.Container);
				this.Container.ComposeExportedValue<CallbackLogger>(this.callbackLogger);
				this.Container.ComposeExportedValue<IModalDialogService>(this.modalDialogService);
				this.Container.ComposeExportedValue<IAuthentication>(this.authentication);
				this.Container.ComposeExportedValue<IGeolocator>(this.geoLocator);
				// Handle parameters from the URL request
				if (parameters.ContainsKey("Application"))
					configuration.SetApplicationId(parameters["Application"]);
				else
					configuration.SetApplicationId("DEMO");
			}
			catch (Exception ex)
			{
				if (callbackLogger != null)
					callbackLogger.Log(ex.Message, Category.Exception, Priority.High);
			}
		}
		protected override void InitializeModules()
		{
			base.InitializeModules();
			this.dialogManager = new DialogManager();
			this.Container.ComposeExportedValue<IDialogManager>(this.dialogManager);
		}
	}
}
