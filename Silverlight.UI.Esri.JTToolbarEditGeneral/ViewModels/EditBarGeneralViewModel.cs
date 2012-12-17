using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Toolkit;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Logging;
using Silverlight.Helper.Interfaces;
using Silverlight.Helper.ViewModels;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;


namespace Silverlight.UI.Esri.JTToolbarEditGeneral.ViewModels
{
	[Export(typeof(EditToolBarGeneralViewModel))]
	public class EditToolBarGeneralViewModel : EditToolBarViewModel
	{
		private ICommand _toolboxCommand;						// Geoprocessing toolbox

		public ICommand ToolboxCommand
		{
			get
			{
				return _toolboxCommand;
			}
			set
			{
				_toolboxCommand = value;
				this.RaisePropertyChanged(() => this.ToolboxCommand);
			}
		}

		/// <summary>
		/// Initialisation of the toolbar, custom initialisation must be added here
		/// </summary>
		/// <param name="eventAggregator"></param>
		/// <param name="loggerFacade"></param>
		/// <param name="configuration"></param>
		[ImportingConstructor]
		public EditToolBarGeneralViewModel(IEventAggregator eventAggregator, ILoggerFacade loggerFacade, IConfiguration configuration)
			: base(eventAggregator, loggerFacade, configuration)
		{
			this.ToolboxCommand = new DelegateCommand<object>(
				this.OnToolboxCommandClicked, this.CanToolboxCommandClicked);
		}

		protected override void Editor_EditCompleted(object sender, Editor.EditEventArgs e)
		{
			try
			{
				if (e.Action == Editor.EditAction.Add)
				{
					EditorParameterCommand editorParameter = null;
					foreach (var change in e.Edits)
					{
						editorParameter = AddGeometryParameter;
						if (editorParameter != null)
						{
							if (editorParameter.FeatureTypeUsed != null)
							{
								FeatureTemplate template = (FeatureTemplate)editorParameter.FeatureTypeUsed.Templates.FirstOrDefault(p => p.Key != "").Value;
								if (template != null)
								{
									foreach (var item in template.PrototypeAttributes)
									{
										if (!change.Graphic.Attributes.ContainsKey(item.Key))
											change.Graphic.Attributes.Add(item.Key, item.Value);
									}
								}
							}
							else
								if (currentTemplateLayer.Equals(change.Layer.ID) && currentTemplate != null)
								{
									foreach (var item in currentTemplate.PrototypeAttributes)
									{
										if (!change.Graphic.Attributes.ContainsKey(item.Key))
											change.Graphic.Attributes.Add(item.Key, item.Value);
									}
								}
							if (change.Graphic.Attributes.Count > 0)
							{
								ChildWindow window = new ChildWindow();
								FeatureDataForm form = new FeatureDataForm()
								{
									GraphicSource = change.Graphic,
									FeatureLayer = (FeatureLayer)change.Layer,
									IsReadOnly = false,
									CommitButtonContent = Silverlight.UI.Esri.JTToolbarEditGeneral.Resources.ToolbarEditGeneral.Save,
									Width = 250,
									Height = 350
								};
								window.Content = form;
								form.EditEnded += (s, ev) =>
								{
									window.Close();
								};
								window.Show();
							}
						}
					}
				}
				if (e.Action == Editor.EditAction.Add || e.Action == Editor.EditAction.EditVertices ||
					e.Action == Editor.EditAction.ClearSelection || e.Action == Editor.EditAction.Union ||
					e.Action == Editor.EditAction.DeleteSelected || e.Action == Editor.EditAction.Save ||
					e.Action == Editor.EditAction.Select || e.Action == Editor.EditAction.Cancel)
					EditOperationCompleted(0, "");
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("Editor_EditCompleted-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		/// <summary>
		/// Executed after the map has been initialized
		/// </summary>
		protected override void SetCustomCommands()
		{
			base.SetCustomCommands();
			DelegateCommand<object> delegateCommand =this._toolboxCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			//
		}

		/// <summary>
		/// Handling of the attribute edit 
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="layerID"></param>
		protected override void HandleAttributeEdit(Graphic feature, string layerID)
		{
			try
			{
				string objectIDField = gisOperations.GetFeatureLayerInfo(layerID).ObjectId;
				FeatureLayer featureLayer = gisOperations.GetFeatureLayer(layerID);
				feature.Attributes.Remove(objectIDField);
				if (feature.Attributes.Count > 0)
				{
					ChildWindow window = new ChildWindow();
					FeatureDataForm form = new FeatureDataForm()
					{
						GraphicSource = feature,
						FeatureLayer = featureLayer,
						IsReadOnly = false,
						CommitButtonContent = "Opslaan",
						Width = 250,
						Height = 350
					};
					// Clear selection
					feature.Selected = false;
					window.Content = form;
					form.EditEnded += (s, ev) =>
					{
						window.Close();
					};
					window.Show();
				}
				gisEditing.EndEditOperation(0, string.Empty);
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("HandleAttributeEdit-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		private void OnToolboxCommandClicked(object arg)
		{
			ShowMessagebox.Raise(new Notification
			{
				Content = Silverlight.UI.Esri.JTToolbarEditGeneral.Resources.ToolbarEditGeneral.ToolboxNotImplemented,
				Title = Silverlight.UI.Esri.JTToolbarEditGeneral.Resources.ToolbarEditGeneral.Information
			});
		}

		private bool CanToolboxCommandClicked(object arg)
		{
			return IsMapLoaded;
		}
	}
}
