using System;
//===================================================================================
// Base Editbar View Model
// All view models for an edit toolbar must be inherit of this class.
// All standard feature edit methods are already implemented here, but can be overwritten.
//===================================================================================
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Logging;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Interfaces;

namespace Silverlight.Helper.ViewModels
{
	/// <summary>
	/// Editor Commands parameter
	/// </summary>
	public class EditorParameterCommand
	{
		public string LayerId { get; set; }
		public bool VerticesEnabled { get; set; }
		public bool FreeHand { get; set; }
		public Graphic Graphic2Edited { get; set; }
		public FeatureTemplate FeatureTemplateUsed { get; set; }
		public FeatureType FeatureTypeUsed { get; set; }
		public DrawMode SelectionMode { get; set; }
		public string Action { get; set; }
	}

	public abstract class EditToolBarViewModel : ViewModelBase
	{
		//Set things public so that they can be used in the derived class
		public IConfiguration configuration;
		public bool IsMapLoaded { get; set; }
		public ObservableCollection<string> LayerIds;
		public FeatureTemplate currentTemplate;
		public string currentTemplateLayer = string.Empty;
		public IList<Graphic> currentGraphics;
		public bool editActionActive = false;
		public bool convexHullActive = false;

		#region Edit layer selections
		private ObservableCollection<EditLayerData> _editLayers = new ObservableCollection<EditLayerData>();
		private Graphic graphic2Edited;
		private Graphic graphicNew;
		private FeatureLayer pointLayer;
		private EditGeometry editGeometry;
		private Visibility _hidePolyButtons;
		private Visibility _hidePointButtons;

		public ObservableCollection<EditLayerData> EditLayers
		{
			get
			{
				return _editLayers;
			}
			set
			{
				_editLayers = value;
			}
		}

		private ObservableCollection<TemplateData> _editTemplates = new ObservableCollection<TemplateData>();
		public ObservableCollection<TemplateData> EditTemplates
		{
			get
			{
				return _editTemplates;
			}
			set
			{
				_editTemplates = value;
			}
		}

		private EditLayerData _selectedEditLayer;
		public EditLayerData SelectedEditLayer
		{
			get
			{
				return _selectedEditLayer;
			}
			set
			{
				_selectedEditLayer = value;
				this.RaisePropertyChanged(() => this.SelectedEditLayer);
			}
		}

		public Visibility HidePolyButtons
		{
			get
			{
				return _hidePolyButtons;
			}
			set
			{
				_hidePolyButtons = value;
				this.RaisePropertyChanged(() => this.HidePolyButtons);
			}
		}

		public Visibility HidePointButtons
		{
			get
			{
				return _hidePointButtons;
			}
			set
			{
				_hidePointButtons = value;
				this.RaisePropertyChanged(() => this.HidePointButtons);
			}
		}

		private EditLayerData currentEditLayer;

		#region Visibility setting (combobox en subtype list)
		private Visibility _subTypeVisibility;
		public Visibility SubTypeVisibility
		{
			get
			{
				return _subTypeVisibility;
			}
			set
			{
				_subTypeVisibility = value;
				this.RaisePropertyChanged(() => this.SubTypeVisibility);
			}
		}

		private Visibility _templateVisibility;

		public Visibility TemplateVisibility
		{
			get
			{
				return _templateVisibility;
			}
			set
			{
				_templateVisibility = value;
				this.RaisePropertyChanged(() => this.TemplateVisibility);
			}
		}

		private Visibility _selectLayerList;
		public Visibility SelectLayerList
		{
			get
			{
				return _selectLayerList;
			}
			set
			{
				_selectLayerList = value;
				this.RaisePropertyChanged(() => this.SelectLayerList);
			}
		}


		#endregion
		#endregion

		#region ArcGis components

		#endregion

		public InteractionRequest<Notification> ShowMessagebox { get; set; }

		public InteractionRequest<Notification> ShowErrorMessagebox { get; set; }

		#region Services
		[Import]
		public IGisEditing gisEditing;

		[Import]
		public IGisOperations gisOperations;
		#endregion

		#region FeatureLayer Symbol Definitions
		// Templates or Feature Types
		private ObservableCollection<SymbolMarkerInfo> _symbolMarkers;
		public ObservableCollection<SymbolMarkerInfo> SymbolMarkers
		{
			get
			{
				return _symbolMarkers;
			}
			set
			{
				_symbolMarkers = value;
				this.RaisePropertyChanged(() => this.SymbolMarkers);
			}
		}



		// Symbol definitions
		private Symbol _symbolPolygon;
		public Symbol SymbolPolygon
		{
			get
			{
				return _symbolPolygon;
			}
			set
			{
				_symbolPolygon = value;
				this.RaisePropertyChanged(() => this.SymbolPolygon);
			}
		}
		private Symbol _symbolPolyline;
		public Symbol SymbolPolyline
		{
			get
			{
				return _symbolPolyline;
			}
			set
			{
				_symbolPolyline = value;
				this.RaisePropertyChanged(() => this.SymbolPolyline);
			}
		}

		#endregion

		#region ArcGis API Editor Commands
		private ICommand _addCommand;
		private ICommand _cancelActiveCommand;
		private ICommand _clearSelectionCommand;
		private ICommand _cutCommand;
		private ICommand _deleteSelectedCommand;
		private ICommand _editVerticesCommand;
		private ICommand _reshapeCommand;
		private ICommand _saveCommand;
		private ICommand _selectCommand;
		private ICommand _unionCommand;

		public ICommand UnionCommand
		{
			get
			{
				return _unionCommand;
			}
			set
			{
				_unionCommand = value;
				this.RaisePropertyChanged(() => this.UnionCommand);
			}
		}
		public ICommand SelectCommand
		{
			get
			{
				return _selectCommand;
			}
			set
			{
				_selectCommand = value;
				this.RaisePropertyChanged(() => this.SelectCommand);
			}
		}


		public ICommand SaveCommand
		{
			get
			{
				return _saveCommand;
			}
			set
			{
				_saveCommand = value;
				this.RaisePropertyChanged(() => this.SaveCommand);
			}
		}

		public ICommand ReshapeCommand
		{
			get
			{
				return _reshapeCommand;
			}
			set
			{
				_reshapeCommand = value;
				this.RaisePropertyChanged(() => this.ReshapeCommand);
			}
		}

		public ICommand EditVerticesCommand
		{
			get
			{
				return _editVerticesCommand;
			}
			set
			{
				_editVerticesCommand = value;
				this.RaisePropertyChanged(() => this.EditVerticesCommand);
			}
		}

		public ICommand DeleteSelectedCommand
		{
			get
			{
				return _deleteSelectedCommand;
			}
			set
			{
				_deleteSelectedCommand = value;
				this.RaisePropertyChanged(() => this.DeleteSelectedCommand);
			}
		}

		public ICommand CutCommand
		{
			get
			{
				return _cutCommand;
			}
			set
			{
				_cutCommand = value;
				this.RaisePropertyChanged(() => this.CutCommand);
			}
		}

		public ICommand ClearSelectionCommand
		{
			get
			{
				return _clearSelectionCommand;
			}
			set
			{
				_clearSelectionCommand = value;
				this.RaisePropertyChanged(() => this.ClearSelectionCommand);
			}
		}

		public ICommand CancelActiveCommand
		{
			get
			{
				return _cancelActiveCommand;
			}
			set
			{
				_cancelActiveCommand = value;
				this.RaisePropertyChanged(() => this.CancelActiveCommand);
			}
		}

		public ICommand AddCommand
		{
			get
			{
				return _addCommand;
			}
			set
			{
				_addCommand = value;
				this.RaisePropertyChanged(() => this.AddCommand);
			}
		}

		#endregion

		#region Custom edit commands
		private ICommand _symbolSelected;						// Create feature based on a symbol of the feature type
		private ICommand _moveGeometryCommand;			// Move a feature mappoint
		private ICommand _editAttributesCommand;		// Edit attribure data
		private ICommand _convexHullCommand;				// Start the action for the creation of a polygon based on the points entered 
		private ICommand _createConvexHullCommand;	// Create the polygon of the points created
		private ICommand _snapPolygonCommand;				// Snap two polygon based on a polyline

		public ICommand SnapPolygonCommand
		{
			get
			{
				return _snapPolygonCommand;
			}
			set
			{
				_snapPolygonCommand = value;
				this.RaisePropertyChanged(() => this.CreateConvexHullCommand);
			}
		}

		public ICommand CreateConvexHullCommand
		{
			get
			{
				return _createConvexHullCommand;
			}
			set
			{
				_createConvexHullCommand = value;
				this.RaisePropertyChanged(() => this.CreateConvexHullCommand);
			}
		}

		public ICommand ConvexHullCommand
		{
			get
			{
				return _convexHullCommand;
			}
			set
			{
				_convexHullCommand = value;
				this.RaisePropertyChanged(() => this.ConvexHullCommand);
			}
		}

		public ICommand EditAttributesCommand
		{
			get
			{
				return _editAttributesCommand;
			}
			set
			{
				_editAttributesCommand = value;
				this.RaisePropertyChanged(() => this.EditAttributesCommand);
			}
		}

		public ICommand MoveGeometryCommand
		{
			get
			{
				return _moveGeometryCommand;
			}
			set
			{
				_moveGeometryCommand = value;
				this.RaisePropertyChanged(() => this.MoveGeometryCommand);
			}
		}

		public ICommand SymbolSelected
		{
			get
			{
				return _symbolSelected;
			}
			set
			{
				_symbolSelected = value;
				this.RaisePropertyChanged(() => this.SymbolSelected);
			}
		}
		#endregion

		#region Selection commands

		private ICommand _editLayerSelectCommand;

		public ICommand EditLayerSelectCommand
		{
			get
			{
				return _editLayerSelectCommand;
			}
			set
			{
				_editLayerSelectCommand = value;
				this.RaisePropertyChanged(() => this.EditLayerSelectCommand);
			}
		}

		private ICommand _templateSelectCommand;

		public ICommand TemplateSelectCommand
		{
			get
			{
				return _templateSelectCommand;
			}
			set
			{
				_templateSelectCommand = value;
				this.RaisePropertyChanged(() => this.TemplateSelectCommand);
			}
		}


		#endregion

		#region Edit parameters
		public EditorParameterCommand EditVerticesParameter { get; set; }
		public EditorParameterCommand MovePointParameter { get; set; }
		public EditorParameterCommand AddGeometryParameter { get; set; }
		public EditorParameterCommand DeleteGeometryParameter { get; set; }
		public EditorParameterCommand SelectParameter { get; set; }
		public EditorParameterCommand ClearSelectionParameter { get; set; }
		#endregion

		#region Constructor and initialisations

		/// <summary>
		/// Constructor used within the PRISM framework
		/// </summary>
		/// <param name="eventAggregator"></param>
		/// <param name="loggerFacade"></param>
		/// <param name="regionManager"></param>
		[ImportingConstructor]
		public EditToolBarViewModel(IEventAggregator eventAggregator, ILoggerFacade loggerFacade, IConfiguration configuration)
		{
			if (eventAggregator != null)
				eventAggregator.GetEvent<CompositePresentationEvent<MapData>>().Subscribe(OnMapChanged);
			// Build the editor command that can be used in the template picker
			this.AddCommand = new DelegateCommand<object>(
			this.OnAddCommandClicked, this.CanAddCommandClicked);
			this.CancelActiveCommand = new DelegateCommand<object>(
			this.OnCancelCommandClicked, this.CanCancelCommandClicked);
			this.ClearSelectionCommand = new DelegateCommand<object>(
			this.OnClearSelectionCommandClicked, this.CanClearSelectionCommandClicked);
			this.CutCommand = new DelegateCommand<object>(
			this.OnCutCommandClicked, this.CanCutCommandClicked);
			this.DeleteSelectedCommand = new DelegateCommand<object>(
			this.OnDeleteSelectedCommandClicked, this.CanDeleteSelectedCommandClicked);
			this.EditVerticesCommand = new DelegateCommand<object>(
			this.OnEditVerticesCommandClicked, this.CanEditVerticesCommandClicked);
			this.ReshapeCommand = new DelegateCommand<object>(
			this.OnReshapeCommandClicked, this.CanReshapeCommandClicked);
			this.SaveCommand = new DelegateCommand<object>(
			this.OnSaveCommandClicked, this.CanSaveCommandClicked);
			this.SelectCommand = new DelegateCommand<object>(
			this.OnSelectCommandClicked, this.CanSelectCommandClicked);
			this.UnionCommand = new DelegateCommand<object>(
			this.OnUnionCommandClicked, this.CanUnionCommandClicked);
			this.EditLayerSelectCommand = new DelegateCommand<object>(
			OnEditLayerSelect, CanEditLayerSelect);
			this.TemplateSelectCommand = new DelegateCommand<object>(
			OnTemplateSelect, CanTemplateSelect);
			this.MoveGeometryCommand = new DelegateCommand<object>(
			this.OnMoveGeometryCommandClicked, this.CanMoveGeometryCommandClicked);
			this.ConvexHullCommand = new DelegateCommand<object>(
			OnConvexHullCommandClicked, CanConvexHullCommandClicked);
			this.CreateConvexHullCommand = new DelegateCommand<object>(
			OnCreateConvexHullCommandClicked, CanCreateConvexHullCommandClicked);
			this.EditAttributesCommand = new DelegateCommand<object>(
			this.OnEditAttributesCommandClicked, this.CanEditAttributesCommandClicked);
			this.SnapPolygonCommand = new DelegateCommand<object>(
				this.OnSnapPolygonCommandClicked, this.CanSnapPolygonCommandClicked);


			// Initialise layers
			this.LayerIds = new ObservableCollection<string>();
			this.configuration = configuration;
			ShowMessagebox = new InteractionRequest<Notification>();
			ShowErrorMessagebox = new InteractionRequest<Notification>();
		}

		/// <summary>
		/// Subscription method to handle the map initialisation
		/// </summary>
		/// <param name="mapData"></param>
		public void OnMapChanged(MapData mapData)
		{
			try
			{
				if (mapData != null)
				{
					this.IsMapLoaded = true;
					this.RaisePropertyChanged(() => this.IsMapLoaded);
					gisEditing.GetEditorTool().EditCompleted += Editor_EditCompleted;
					// Initialise editable layers
					this.SubTypeVisibility = Visibility.Collapsed;
					this.TemplateVisibility = Visibility.Collapsed;
					this.SymbolMarkers = new ObservableCollection<SymbolMarkerInfo>();
					foreach (var item in gisEditing.GetEditLayers())
					{
						this.EditLayers.Add(item);
						switch (item.LayerGeometryType)
						{
							case ESRI.ArcGIS.Client.Tasks.GeometryType.MultiPoint:
								if (gisEditing.GetMarkerInfo(item.LayerName).FirstOrDefault(l => l.ObjectFeatureType != null) != null)
								{
									foreach (var item2 in gisEditing.GetMarkerInfo(item.LayerName))
										this.SymbolMarkers.Add(item2);
									this.SubTypeVisibility = Visibility.Visible;
								}

								break;
							case ESRI.ArcGIS.Client.Tasks.GeometryType.Point:
								if (gisEditing.GetMarkerInfo(item.LayerName).FirstOrDefault(l => l.ObjectFeatureType != null) != null)
								{
									foreach (var item2 in gisEditing.GetMarkerInfo(item.LayerName))
										this.SymbolMarkers.Add(item2);
									this.SubTypeVisibility = Visibility.Visible;
								}
								break;
							case ESRI.ArcGIS.Client.Tasks.GeometryType.Polygon:
								if (gisEditing.GetMarkerInfo(item.LayerName).FirstOrDefault(l => l.ObjectFeatureType != null) != null)
								{
									foreach (var item2 in gisEditing.GetMarkerInfo(item.LayerName))
										this.SymbolMarkers.Add(item2);
									this.SubTypeVisibility = Visibility.Visible;
								}
								else
								{
									this.EditTemplates = gisEditing.GetTemplates(item.LayerName);
									if (this.EditTemplates.Count > 0)
									{
										this.TemplateVisibility = Visibility.Visible;
									}
								}
								break;
							case ESRI.ArcGIS.Client.Tasks.GeometryType.Polyline:
								if (gisEditing.GetMarkerInfo(item.LayerName).FirstOrDefault(l => l.ObjectFeatureType != null) != null)
								{
									foreach (var item2 in gisEditing.GetMarkerInfo(item.LayerName))
										this.SymbolMarkers.Add(item2);
									this.SubTypeVisibility = Visibility.Visible;
								}
								break;
							default:
								break;
						}
					}
					EditVerticesParameter = new EditorParameterCommand();
					AddGeometryParameter = new EditorParameterCommand();
					MovePointParameter = new EditorParameterCommand() { Action = "Move" };
					DeleteGeometryParameter = new EditorParameterCommand();
					SelectParameter = new EditorParameterCommand() { SelectionMode = DrawMode.Rectangle };
					this.SelectLayerList = (this.EditLayers.Count > 1 ? Visibility.Visible : Visibility.Collapsed);
					RaisePropertyChanged(() => this.EditLayers);
					RaisePropertyChanged(() => this.EditTemplates);
					if (this.EditLayers.Count > 0)
					{
						currentEditLayer = this.EditLayers[0];
						this.SelectedEditLayer = this.EditLayers[0];
					}
					// Define edit parameters
					this.RaisePropertyChanged(() => this.EditVerticesParameter);
					this.RaisePropertyChanged(() => this.AddGeometryParameter);			
					this.RaisePropertyChanged(() => this.MovePointParameter);
					this.RaisePropertyChanged(() => this.DeleteGeometryParameter);
					this.RaisePropertyChanged(() => this.SelectParameter);
					this.SymbolSelected = new DelegateCommand<object>(
						this.OnSymbolSelected, this.CanSymbolSelected);
					// Initialisation EditGeometry
					editGeometry = new EditGeometry(gisOperations.GetMap());
					editGeometry.GeometryEdit += EditGeometryEdited;
					RefreshButtonStatus();
					SetCustomCommands();
				}
			}
			catch (System.Exception ex)
			{
				// Error handling
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("OnMapChanged-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		public void RefreshButtonStatus()
		{
			DelegateCommand<object> delegateCommand = this._addCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._cancelActiveCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._clearSelectionCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._cutCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._deleteSelectedCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._editVerticesCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._reshapeCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._saveCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._selectCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._unionCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._editAttributesCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._moveGeometryCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._convexHullCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._createConvexHullCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			delegateCommand = this._snapPolygonCommand as DelegateCommand<object>;
			delegateCommand.RaiseCanExecuteChanged();
			RaisePropertyChanged(() => this.EditLayers);
			RaisePropertyChanged(() => this.EditTemplates);
		}

		public void EditOperationCompleted(int status, string message)
		{
			editActionActive = false;
			RefreshButtonStatus();
			if (status > 90)
			{
				// Error handling
				ShowMessagebox.Raise(new Notification
				{
					Content = "Fout tijdens het afhandelen van de wijziging en of creatie van een feature!",
					Title = "Foutmelding"
				});
			}
		}

		public void EditOperationStarted(string editOperation)
		{
			editActionActive = true;
			RefreshButtonStatus();
			gisEditing.StartEditOperation(editOperation, EditOperationCompleted);
		}
		#endregion

		#region Entry points created in the base edit class to be overridden
		/// <summary>
		/// Method allwoing some post processing after the edit occured
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void Editor_EditCompleted(object sender, Editor.EditEventArgs e)
		{
		}

		/// <summary>
		/// Method allowing some custom action after the map is loaded to be intialised
		/// </summary>
		protected virtual void SetCustomCommands()
		{
		}

		#endregion

		private void EditGeometryEdited(object sender, EditGeometry.GeometryEditEventArgs e)
		{
			// To handle post editing of a geometry
		}

		#region Editor commands handling
		/// <summary>
		/// Add a new feature to a feature layer
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnAddCommandClicked(object arg)
		{
			try
			{
				if (gisEditing.GetEditorTool() != null)
				{
					if (currentEditLayer == null)
					{
						ShowMessagebox.Raise(new Notification
						{
							Content = Silverlight.Helper.Resources.Helper.NoEditLayerSelected,
							Title = Silverlight.Helper.Resources.Helper.Warning
						}, confirmation =>
						{
							// No action required
						});

						return;
					}

					IList<string> layerIDs = new List<string>();
					layerIDs.Add(currentEditLayer.LayerName);
					gisEditing.GetEditorTool().LayerIDs = layerIDs;
					gisEditing.GetEditorTool().Freehand = false;
					Silverlight.Helper.DataMapping.FeatureLayerInfo layerInfo =
					gisOperations.GetFeatureLayerInfo(currentEditLayer.LayerName);
					if (layerInfo.FeatureTemplates != null && layerInfo.FeatureTemplates.Count > 0)
						gisEditing.GetEditorTool().Add.Execute(layerInfo.FeatureTemplates.First(l => l.Key.Length > 0).Value);
					else
					{
						if (layerInfo.FeatureTypes != null && layerInfo.FeatureTypes.Count > 0)
						{
							FeatureType featureType = layerInfo.FeatureTypes.FirstOrDefault(l => l.Key != null).Value as FeatureType;
							gisEditing.GetEditorTool().Add.Execute(featureType.Id);
						}
						else
							gisEditing.GetEditorTool().Add.Execute(null);
					}
					EditOperationStarted("Add");
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("OnAddCommandClicked-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}
		protected virtual bool CanAddCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Cancel operation
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnCancelCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() != null)
			{
				EditOperationStarted("Cancel");
				gisEditing.GetEditorTool().CancelActive.Execute(arg);
			}
		}
		protected virtual bool CanCancelCommandClicked(object arg)
		{
			return IsMapLoaded;
		}

		/// <summary>
		/// Clear the selections of features available in the graphicslayers
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnClearSelectionCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() != null)
			{
				bool hasSelections = false;
				foreach (var item in gisEditing.GetEditorTool().GraphicsLayers)
				{
					if (item.SelectionCount > 0)
					{
						hasSelections = true;
						break;
					}
				}
				if (hasSelections)
				{
					EditOperationStarted("ClearSelection");
					gisEditing.GetEditorTool().ClearSelection.Execute(arg);
				}
			}
		}
		protected virtual bool CanClearSelectionCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Implementation of the split command. 
		/// 1. Select polygon to be split 
		/// 2. Draw a line through polygon -> result are two or more polygons with the same attributes
		/// </summary>
		/// <param name="arg">1</param>
		protected virtual void OnCutCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() == null)
				return;
			// Verify a polygon is selected on the layer specified
			if (currentEditLayer == null || currentEditLayer.LayerGeometryType != GeometryType.Polygon)
			{
				ShowMessagebox.Raise(new Notification
				{
					Content = Silverlight.Helper.Resources.Helper.NoPolygonLayerSelected,
					Title = Silverlight.Helper.Resources.Helper.ErrorMessage
				});
				return;
			}
			EditOperationStarted("SplitPolygon");
			if (!gisEditing.SplitPolygon(currentEditLayer.LayerName, OnSplitCompleted))
			{
				ShowMessagebox.Raise(new Notification
				{
					Content = Silverlight.Helper.Resources.Helper.NoOrMultiplePolygonSelected,
					Title = Silverlight.Helper.Resources.Helper.ErrorMessage
				});
			}
			//
		}

		protected virtual bool CanCutCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		private void OnSplitCompleted(IList<Graphic> resultSplit)
		{
			EditOperationCompleted(0, "");
		}

		/// <summary>
		/// Delete of the selected feature
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnDeleteSelectedCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() != null)
			{
				bool hasSelections = false;
				foreach (var item in gisEditing.GetEditorTool().GraphicsLayers)
				{
					if (item.SelectionCount > 0)
					{
						hasSelections = true;
						break;
					}
				}
				if (hasSelections)
				{
					EditOperationStarted("DeleteSelected");
					gisEditing.GetEditorTool().DeleteSelected.Execute(arg);
				}
				else
				{
					ShowMessagebox.Raise(new Notification
					{
						Content = Silverlight.Helper.Resources.Helper.NoFeatureSelectedDeleteFailed,
						Title = Silverlight.Helper.Resources.Helper.ErrorMessage
					});
				}
			}
		}

		protected virtual bool CanDeleteSelectedCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Edit of a feature (polygon or polyline)
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnEditVerticesCommandClicked(object arg)
		{
			try
			{
				if (gisEditing.GetEditorTool() != null)
				{
					EditOperationStarted("EditVertices");
					IList<string> layerIDs = new List<string>();
					layerIDs.Add(currentEditLayer.LayerName);
					gisEditing.GetEditorTool().LayerIDs = layerIDs;
					gisEditing.GetEditorTool().EditVertices.Execute(graphic2Edited);
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("OnEditVerticesCommandClicked-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		protected virtual bool CanEditVerticesCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnReshapeCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() != null)
				gisEditing.GetEditorTool().Reshape.Execute(arg);
		}

		protected virtual bool CanReshapeCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Save all pending edits
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnSaveCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() != null)
			{
				EditOperationStarted("Save");
				gisEditing.SaveAll();
			}
		}

		protected virtual bool CanSaveCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Select a feature
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnSelectCommandClicked(object arg)
		{
			try
			{
				if (gisEditing.GetEditorTool() != null)
				{
					EditorParameterCommand parameter = (EditorParameterCommand)arg;
					IList<string> layerIDs = new List<string>();
					if (currentEditLayer.LayerName == null || currentEditLayer.LayerName.Length == 0)
						return;
					EditOperationStarted("Select");
					layerIDs.Add(currentEditLayer.LayerName);
					gisEditing.GetEditorTool().LayerIDs = layerIDs;
					gisEditing.GetEditorTool().Select.Execute(DrawMode.Point);
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("OnSelectCommandClicked-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		protected virtual bool CanSelectCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnUnionCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() != null)
				gisEditing.GetEditorTool().Union.Execute(arg);
			// Verify at least 2 features are selected
			if (currentEditLayer == null || currentEditLayer.LayerGeometryType != GeometryType.Polygon)
			{
				ShowMessagebox.Raise(new Notification
				{
					Content = Silverlight.Helper.Resources.Helper.NoPolygonLayerSelected,
					Title = Silverlight.Helper.Resources.Helper.ErrorMessage
				});
				return;
			}

			FeatureLayer featureLayer = gisOperations.GetFeatureLayer(currentEditLayer.LayerName);
			if (featureLayer.SelectionCount < 2)
			{
				ShowMessagebox.Raise(new Notification
				{
					Content = Silverlight.Helper.Resources.Helper.Less2PolygonSelected,
					Title = Silverlight.Helper.Resources.Helper.ErrorMessage
				});
				return;
			}

			EditOperationStarted("Union");
			gisEditing.UnionGeometries(featureLayer.SelectedGraphics.ToList());
		}

		protected virtual bool CanUnionCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		#region Custom edit commands

		/// <summary>
		/// Snap two polygons together using a user interaction by means of a polyline
		/// </summary>
		/// <param name="arg"></param>
		private void OnSnapPolygonCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() == null)
				return;
			// Verify a polygon is selected on the layer specified
			if (currentEditLayer == null || currentEditLayer.LayerGeometryType != GeometryType.Polygon)
			{
				ShowMessagebox.Raise(new Notification
				{
					Content = Silverlight.Helper.Resources.Helper.NoPolygonLayerSelected,
					Title = Silverlight.Helper.Resources.Helper.ErrorMessage
				});
				return;
			}
			EditOperationStarted("SnapPolygon");
			gisEditing.SnapPolygons(currentEditLayer.LayerName, SnapPolygonComplete);
		}

		private void SnapPolygonComplete(IList<Graphic> results)
		{
			// Operation terminated
			if (results == null)
			{
				int status = 91;
				string message = "Fout tijdens het snappen van vlakken,selecteer 2 vlakken.";
				EditOperationCompleted(status, message);
			}
			
		}

		private bool CanSnapPolygonCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Edit of the attributes of a feature
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnEditAttributesCommandClicked(object arg)
		{
			try
			{
				// Verify selected items
				string layerID = string.Empty;
				Graphic feature = null;
				if (gisEditing.GetEditorTool() != null)
				{
					// Verify if a selected item
					bool hasSelections = false;
					foreach (var item in gisEditing.GetEditorTool().GraphicsLayers)
					{
						if (item.SelectionCount > 0)
						{
							hasSelections = true;
							layerID = item.ID;
							feature = item.SelectedGraphics.FirstOrDefault(g => g.Attributes.Count > 0);
							break;
						}
					}
					if (hasSelections && feature != null)
					{
						// Start attribute editing on the selected item
						gisEditing.StartEditOperation("AttributeEdit", null);
						HandleAttributeEdit(feature, layerID);
					}
					else
					{
						ShowMessagebox.Raise(new Notification
						{
							Content = "Geen feature is geselecteerd!",
							Title = Silverlight.Helper.Resources.Helper.Warning
						}, confirmation =>
						{
							// No action required
						});
						return;
					}
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("OnEditAttributesCommandClicked-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		protected virtual void HandleAttributeEdit(Graphic feature, string layerID)
		{
		}

		protected virtual bool CanEditAttributesCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Convex Hull consist of 2 steps
		/// 1. Put different point on the map
		/// 2. Create a convex hull for the points entered
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnConvexHullCommandClicked(object arg)
		{
			if (gisEditing.GetEditorTool() == null)
				return;
			// Verify a polygon layer is selected on the layer specified
			if (currentEditLayer == null || currentEditLayer.LayerGeometryType != GeometryType.Polygon)
			{
				ShowMessagebox.Raise(new Notification
				{
					// No action required
					Content = "Geen polygon laag is geselecteerd!",
					Title = "Waarschuwing"
				}, confirmation =>
				{
				});
				return;
			}

			// Start now the drawing of a points, can only be stopped by the terminate convex hull
			gisOperations.SetCompleteDrawEvent(OnDrawPointConvexCompleted);
			convexHullActive = true;
			editActionActive = true;
			RefreshButtonStatus();
			gisOperations.GetSelectLayer().Graphics.Clear();
			gisOperations.SetDrawModeContinuous(DrawMode.Point);
		}

		/// <summary>
		/// Add a point
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnDrawPointConvexCompleted(object sender, DrawEventArgs args)
		{
			Graphic graphic = new Graphic()
			{
				Geometry = args.Geometry,
				Symbol = gisOperations.GetSelectionMarkerSymbol()
			};
			gisOperations.GetSelectLayer().Graphics.Add(graphic);
		}

		protected virtual bool CanConvexHullCommandClicked(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Step 2, create the convex hull
		/// </summary>
		/// <param name="arg"></param>
		protected virtual void OnCreateConvexHullCommandClicked(object arg)
		{
			// Start creating convex hull based on points on the graphic layer
			gisOperations.ResetCompleteDrawEvent(OnDrawPointConvexCompleted);
			gisOperations.DisableDrawMode();
			//
			gisEditing.CreateConvexHull(
			gisOperations.GetSelectLayer().Graphics.ToList(), CreateConvexHullComplete);
		}

		protected virtual bool CanCreateConvexHullCommandClicked(object arg)
		{
			return convexHullActive;
		}

		/// <summary>
		/// Finalisation of the polygon creation
		/// </summary>
		/// <param name="geometry"></param>
		protected virtual void CreateConvexHullComplete(Geometry geometry)
		{
			try
			{
				gisOperations.GetSelectLayer().ClearGraphics();
				FeatureLayer featureLayer =
				gisOperations.GetFeatureLayer(currentEditLayer.LayerName);
				string objectID = gisOperations.GetFeatureLayerInfo(currentEditLayer.LayerName).ObjectId;
				Graphic graphic = new Graphic() { Geometry = geometry };
				foreach (var item in featureLayer.LayerInfo.Fields)
				{
					if (!item.Name.Equals(objectID) && !graphic.Attributes.ContainsKey(item.Name))
						graphic.Attributes.Add(item.Name, null);
				}
				if (gisOperations.GetFeatureLayerInfo(currentEditLayer.LayerName).FeatureTypes.Count > 0)
				{
					FeatureType featureType =
					gisOperations.GetFeatureLayerInfo(currentEditLayer.LayerName).FeatureTypes.FirstOrDefault(t => t.Key != null).Value;
					if (featureType != null)
					{
						FeatureTemplate featureTemplate =
						gisOperations.GetFeatureLayerInfo(currentEditLayer.LayerName).FeatureTypes[featureType.Id].Templates.FirstOrDefault(t => t.Key != null).Value;
						foreach (var item in featureTemplate.PrototypeAttributes)
						{
							if (graphic.Attributes.ContainsKey(item.Key))
								graphic.Attributes[item.Key] = item.Value;
						}
					}
				}
				featureLayer.Graphics.Add(graphic);
				convexHullActive = false;
				editActionActive = false;
				RefreshButtonStatus();
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("CreateConvexHullCompleted-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		/// <summary>
		/// A template is selected for creation of a feature
		/// </summary>
		/// <param name="arg"></param>
		private void OnTemplateSelect(object arg)
		{
			TemplateData templateSelect = (TemplateData)arg;
			currentTemplateLayer = templateSelect.LayerId;
			currentTemplate = templateSelect.EditTemplate;
			if (this.EditLayers.Count > 0)
			{
				//this.SelectedEditLayer = this.EditLayers.FirstOrDefault(l => l.LayerName.Equals(currentTemplateLayer));
				//this.RaisePropertyChanged(() => this.SelectedEditLayer);
			}
		}

		private bool CanTemplateSelect(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}

		/// <summary>
		/// Create a feature based on the symbol selected
		/// </summary>
		/// <param name="arg"></param>
		private void OnSymbolSelected(object arg)
		{
			try
			{
				SymbolMarkerInfo symbolSelected = arg as SymbolMarkerInfo;
				IDictionary<object, FeatureType> featureTypes =
				gisOperations.GetFeatureLayerInfo(symbolSelected.LayerId).FeatureTypes;
				IList<string> layerIDs = new List<string>();
				layerIDs.Add(symbolSelected.LayerId);
				gisEditing.GetEditorTool().LayerIDs = layerIDs;
				editActionActive = true;
				RefreshButtonStatus();
				gisEditing.GetEditorTool().Add.Execute(featureTypes[symbolSelected.ObjectFeatureType].Id);
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("OnSymbolSelected-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		private bool CanSymbolSelected(object arg)
		{
			return (this.SubTypeVisibility == Visibility.Visible) && !editActionActive;
		}

		/// <summary>
		/// Move a mappoint
		/// </summary>
		/// <param name="arg"></param>
		private void OnMoveGeometryCommandClicked(object arg)
		{
			if (currentEditLayer == null || currentEditLayer.LayerGeometryType != GeometryType.Point)
			{
				ShowMessagebox.Raise(new Notification
				{
					// No action required
					Content = "Geen punt laag is geselecteerd!",
					Title = Silverlight.Helper.Resources.Helper.Warning
				}, confirmation =>
				{
				});
				return;
			}
			editActionActive = true;
			RefreshButtonStatus();
			// Add mouse events to the map
			pointLayer = gisOperations.GetFeatureLayer(currentEditLayer.LayerName);
			pointLayer.MouseLeftButtonDown += LayerMouseLeftButtonDown;
			gisOperations.GetMap().MouseMove += MapMouseMove;
			pointLayer.MouseLeftButtonUp += LayerMouseLeftButtomUp;
			graphic2Edited = null;
			graphicNew = null;
		}

		#region Mouse handling for geometry editing
		/// <summary>
		/// Editing start with mouse down on the layer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LayerMouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
		{
			e.Handled = true;
			if (e.Graphic.Geometry is MapPoint)
			{
				graphic2Edited = e.Graphic;
				graphicNew = e.Graphic;
			}
		}

		/// <summary>
		/// Track the mouse movements on the map, needed for the convertion of screen coordinates to map coordinates.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MapMouseMove(object sender, MouseEventArgs e)
		{
			if (graphicNew != null)
			{
				graphicNew.Geometry = gisOperations.GetMap().ScreenToMap(e.GetPosition(gisOperations.GetMap()));
			}
		}

		/// <summary>
		/// In case the mouse release happens, stop the edit operation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LayerMouseLeftButtomUp(object sender, GraphicMouseButtonEventArgs e)
		{
			if (graphicNew != null)
			{
				graphicNew = null;
				graphic2Edited = null;
				gisOperations.GetMap().MouseMove -= MapMouseMove;
				if (pointLayer != null)
				{
					pointLayer.MouseLeftButtonDown -= LayerMouseLeftButtonDown;
					pointLayer.MouseLeftButtonUp -= LayerMouseLeftButtomUp;
				}
			}
			editActionActive = false;
			RefreshButtonStatus();
		}
		#endregion

		private bool CanMoveGeometryCommandClicked(object arg)
		{
			if (!IsMapLoaded)
				return false;
			if (currentEditLayer != null && currentEditLayer.LayerGeometryType == GeometryType.Point)
				return true;
			else
				return false;
		}
		#endregion
		private void OnEditLayerSelect(object arg)
		{
			if (arg == null)
			{
				// In case of templates, arg is null
				//currentEditLayer = this.SelectedEditLayer;
			}
			else
			{
				currentEditLayer = arg as EditLayerData;
			}
			if (currentEditLayer != null)
				HandleEditSelect(currentEditLayer);
		}

		private void HandleEditSelect(EditLayerData editSelect)
		{
			try
			{
				if (editSelect.LayerGeometryType != GeometryType.Point)
				{
					AddGeometryParameter.LayerId = editSelect.LayerName;
					this.RaisePropertyChanged(() => this.AddGeometryParameter);
				}
				if (editSelect.LayerGeometryType == GeometryType.Point)
				{
					MovePointParameter.LayerId = editSelect.LayerName;
					this.RaisePropertyChanged(() => this.MovePointParameter);
				}
				DeleteGeometryParameter.LayerId = editSelect.LayerName;
				this.RaisePropertyChanged(() => this.DeleteGeometryParameter);
				if (editSelect.LayerGeometryType != GeometryType.Point)
					EditVerticesParameter.LayerId = editSelect.LayerName;
				this.RaisePropertyChanged(() => this.EditVerticesParameter);
				SelectParameter.LayerId = editSelect.LayerName;
				this.RaisePropertyChanged(() => this.SelectParameter);
				DelegateCommand<object> delegateCommand = this._moveGeometryCommand as DelegateCommand<object>;
				delegateCommand.RaiseCanExecuteChanged();
			}
			catch (Exception ex)
			{
				ShowErrorMessagebox.Raise(new Notification
				{
					Content = String.Format("HandleEditSelect-{0}[{1}]", ex.Message, ex.StackTrace),
					Title = "System error"
				});
			}
		}

		private bool CanEditLayerSelect(object arg)
		{
			return IsMapLoaded && !editActionActive;
		}
		#endregion
		public override string ViewName
		{
			get
			{
				return "Edit toolbar viewmodel";
			}
		}
	}
}
