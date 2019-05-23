using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.SilverlightViewer.Controls;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Config;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class EditWidget : WidgetBase
    {
        private string lastAddParam = "";
        private string editingLayer = "";
        private object commandParam = "New";
        private string activeCommand = "Select";
        private Editor featureEditor = null;
        private EditWidgetConfig widgetConfig = null;
        private List<string> featureLayerIDs = new List<string>();
        private List<SymbolTypeGroup> symbolTypeGroups = new List<SymbolTypeGroup>();

        private PopupWindow dataEditorWindow = null;
        private FeatureDataForm dataEditorForm = null;
        private AttachmentEditor attachmentEditor = null;

        // Indicator of the AttachmentEditor is being loaded first time
        private bool IsFirstLoad = true;

        public EditWidget()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Configuration and Initialize Editor
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (EditWidgetConfig)EditWidgetConfig.Deserialize(xmlConfig, typeof(EditWidgetConfig));

            if (widgetConfig != null)
            {
                FeatureLayer agisFeatureLayer = null;

                foreach (EditFeatureLayer fLayer in widgetConfig.EditableLayers)
                {
                    agisFeatureLayer = new FeatureLayer() { AutoSave = widgetConfig.AutoSave, Url = fLayer.RESTURL, Visible = fLayer.VisibleInitial, ProxyUrl = fLayer.ProxyURL };
                    agisFeatureLayer.ID = string.Format("edit_{0}", fLayer.ID.ToString());
                    agisFeatureLayer.Opacity = (fLayer.Opacity == 0.0) ? 1.0 : fLayer.Opacity;
                    agisFeatureLayer.Initialized += new EventHandler<EventArgs>(FeatureLayer_Initialized);
                    agisFeatureLayer.InitializationFailed += new EventHandler<EventArgs>(FeatureLayer_InitializationFailed);
                    agisFeatureLayer.MouseLeftButtonUp += new ArcGIS.Client.GraphicsLayer.MouseButtonEventHandler(FeatureLayer_MouseLeftButtonUp);
                    this.featureLayerIDs.Add(agisFeatureLayer.ID);
                    this.MapControl.Layers.Add(agisFeatureLayer);
                }

                // Initialize Feature Editor
                InitializeFeatureEditor();

                // Initialize Feature Data Editor Form
                dataEditorForm = new FeatureDataForm() { Margin = new Thickness(0, 0, 0, 0), Width = 400, MaxHeight = 300 };
                dataEditorForm.EditEnded += new EventHandler<EventArgs>(DataEditorForm_EditEnded);
                dataEditorForm.IsReadOnly = false;

                // Initialize Attachment Editor Form
                attachmentEditor = new AttachmentEditor() { Margin = new Thickness(0, 4, 0, 0), Width = 400, Height = 100, FilterIndex = 1, Multiselect = true };
                attachmentEditor.Filter = "All Files (*.*)|*.*|Image Files|*.tif;*.jpg;*.gif;*.png;*.bmp|Text Files (.txt)|*.txt";
                attachmentEditor.Loaded += new RoutedEventHandler(AttachmentEditor_Loaded);

                StackPanel editorStack = new StackPanel() { Margin = new Thickness(4, 4, 4, 4), Orientation = Orientation.Vertical };
                editorStack.Children.Add(dataEditorForm);
                editorStack.Children.Add(attachmentEditor);

                // Initialize a PopupWindow That Contains Data Editor Attachment Editor
                dataEditorWindow = new PopupWindow() { Background = this.Background, ShowArrow = false, IsResizable = false, IsFloatable = true };
                //dataEditorWindow.TitleFormat = "Attributes Editor: {0}";
                dataEditorWindow.Content = editorStack;
            }
        }

        /// <summary>
        /// Initialize Feature Editor
        /// </summary>
        private void InitializeFeatureEditor()
        {
            featureEditor = this.Resources["FeatureEditor"] as Editor;
            featureEditor.Map = this.MapControl;
            featureEditor.LayerIDs = this.featureLayerIDs.ToArray();
            featureEditor.AutoSelect = widgetConfig.AutoSelect;
            featureEditor.ContinuousMode = widgetConfig.ContinuousAction;
            featureEditor.GeometryServiceUrl = this.AppConfig.GeometryService;
            featureEditor.EditCompleted += new EventHandler<Editor.EditEventArgs>(FeatureEditor_EditCompleted);

            ButtonSaveEdits.Visibility = (widgetConfig.AutoSave) ? Visibility.Collapsed : Visibility.Visible;
            ButtonAddFreehandPolygon.Visibility = (widgetConfig.UseFreehand) ? Visibility.Visible : Visibility.Collapsed;
            ButtonAddFreehandPolyline.Visibility = (widgetConfig.UseFreehand) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// This AttachmentEditor Load Event Handler is Used to Make Some Changes to the AttachmentEditor
        /// </summary>
        private void AttachmentEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsFirstLoad) return;

            if (attachmentEditor.FeatureLayer != null && attachmentEditor.GraphicSource != null)
            {
                FrameworkElement root = VisualTreeHelper.GetChild(attachmentEditor, 0) as FrameworkElement;

                if (root is Border)
                {
                    Button button = root.FindName("AddNewButton") as Button;
                    button.IsEnabled = attachmentEditor.FeatureLayer.LayerInfo.HasAttachments;
                    button.Content = "Add Attachment";
                    button.Width = 120;
                }

                IsFirstLoad = false;

                // Or remove the event handler
                attachmentEditor.Loaded -= AttachmentEditor_Loaded;
            }
        }

        /// <summary>
        /// Restore the Editor command when the Widget is re-activated, or cancel the current command 
        /// </summary>
        protected override void OnIsActiveChanged(object sender, IsActiveChangedEventArgs e)
        {
            // Editor Widget uses a different DrawObject
            WidgetManager.ResetDrawObject();

            if (e.IsActive)
            {
                this.DrawWidget = this.GetType();
                ResetFeatureEditorCommand();
            }
            else if (featureEditor != null)
            {
                if (featureEditor.CancelActive.CanExecute(commandParam))
                {
                    featureEditor.CancelActive.Execute(commandParam);
                }
            }
        }

        /// <summary>
        /// Used to restore the Editor command when the widget is activated
        /// </summary>
        private void ResetFeatureEditorCommand()
        {
            if (featureEditor == null) return;

            if (widgetConfig != null)
            {
                if (widgetConfig.DefaultAction == "Edit")
                {
                    commandParam = null;
                    activeCommand = "Edit";
                    ResetButtonStyle(ButtonEditVertices);
                }
                else
                {
                    commandParam = "New";
                    activeCommand = "Select";
                    ResetButtonStyle(ButtonNewSelection);
                }
            }

            switch (activeCommand)
            {
                case "Select": featureEditor.Select.Execute(commandParam); break;
                case "Edit": featureEditor.EditVertices.Execute(null); break;
                case "Cut": featureEditor.Cut.Execute(null); break;
                case "Reshape": featureEditor.Reshape.Execute(null); break;
                case "Union": featureEditor.Union.Execute(null); break;
                case "Add": featureEditor.Add.Execute(commandParam); break;
            }
        }
        #endregion

        #region Override Functions - Save Edits OnClose and OnApplicationExit
        protected override void OnOpen()
        {
            base.OnOpen();

            foreach (string layerID in featureLayerIDs)
            {
                FeatureLayer fLayer = this.MapControl.Layers[layerID] as FeatureLayer;
                fLayer.Visible = symbolTypeGroups.First(group => group.LayerID == layerID).LayerVisibility;
            }
        }

        protected override void OnClose()
        {
            if (featureEditor != null)
            {
                MessageBoxResult answer = MessageBoxResult.Cancel;
                bool answered = false;

                foreach (string layerID in featureLayerIDs)
                {
                    FeatureLayer fLayer = this.MapControl.Layers[layerID] as FeatureLayer;
                    if (fLayer.HasEdits && !widgetConfig.AutoSave)
                    {
                        if (!answered)
                        {
                            answer = MessageBox.Show("Do you want to save edits?", "Edit Widget", MessageBoxButton.OKCancel);
                            answered = true;
                        }

                        if (answer == MessageBoxResult.OK) fLayer.SaveEdits();
                    }

                    fLayer.Visible = false;
                }
            }

            base.OnClose();
        }
        #endregion

        #region Feature Layer Initialization Event Handlers and Click Event Handler
        /// <summary>
        /// Handle FeatureLayer Initialization Event
        /// </summary>
        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            FeatureLayer fLayer = sender as FeatureLayer;
            EditFeatureLayer editLayer = widgetConfig.EditableLayers.FirstOrDefault(layer => fLayer.ID.Equals(string.Format("edit_{0}", layer.ID.ToString())));

            if (string.IsNullOrEmpty(editLayer.OutFields) || editLayer.OutFields == "*")
            {
                foreach (Field f in fLayer.LayerInfo.Fields)
                {
                    fLayer.OutFields.Add(f.Name);
                }
            }
            else
            {
                string[] fields = editLayer.OutFields.Split(',');
                foreach (string field in fields)
                {
                    fLayer.OutFields.Add(field);
                }
            }

            if (editLayer != null)
            {
                SymbolTypeGroup typeGroup = new SymbolTypeGroup();
                typeGroup.LayerID = fLayer.ID;
                typeGroup.LayerName = editLayer.Title;
                typeGroup.LayerVisibility = editLayer.VisibleInitial;
                symbolTypeGroups.Add(typeGroup);

                if (fLayer.LayerInfo.Renderer is SimpleRenderer)
                {
                    SimpleRenderer renderer = fLayer.LayerInfo.Renderer as SimpleRenderer;
                    SymbolType symbolType = new SymbolType(fLayer.LayerInfo.Name, null, renderer.Symbol, "");
                    typeGroup.SymbolTypes.Add(symbolType);
                }
                if (fLayer.LayerInfo.Renderer is UniqueValueRenderer)
                {
                    UniqueValueRenderer renderer = fLayer.LayerInfo.Renderer as UniqueValueRenderer;
                    foreach (UniqueValueInfo valueInfo in renderer.Infos)
                    {
                        SymbolType symbolType = new SymbolType(valueInfo.Label, valueInfo.Value, valueInfo.Symbol, valueInfo.Description);
                        typeGroup.SymbolTypes.Add(symbolType);
                    }
                }
                else if (fLayer.LayerInfo.Renderer is ClassBreaksRenderer)
                {
                    ClassBreaksRenderer renderer = fLayer.LayerInfo.Renderer as ClassBreaksRenderer;
                    foreach (ClassBreakInfo classInfo in renderer.Classes)
                    {
                        SymbolType symbolType = new SymbolType(classInfo.Label, null, classInfo.Symbol, classInfo.Description);
                        typeGroup.SymbolTypes.Add(symbolType);
                    }
                }
            }

            if (symbolTypeGroups.Count == widgetConfig.EditableLayers.Count()) // All layers are initialized
            {
                FeatureTemplateList.ItemsSource = symbolTypeGroups;
            }
        }

        /// <summary>
        /// Handle FeatureLayer InitializationFailed Event
        /// </summary>
        private void FeatureLayer_InitializationFailed(object sender, EventArgs e)
        {
            if (sender is Layer)
            {
                MessageBox.Show("Failed to initialize the feature layer: " + (sender as Layer).ID);
            }
        }

        /// <summary>
        /// FeatureLayer MouseUp Event Handler
        /// </summary>
        private void FeatureLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            if (activeCommand == "Attributes")
            {
                FeatureLayer fLayer = sender as FeatureLayer;
                OpenFeatureDataEditor(fLayer, e.Graphic);
            }
        }
        #endregion

        #region Handle FeatureEditor EditComplete Event and Launch Attribute Editor
        /// <summary>
        /// After a new feature is added, open Attribute Editor automatically 
        /// </summary>
        private void FeatureEditor_EditCompleted(object sender, Editor.EditEventArgs e)
        {
            if (e.Action == Editor.EditAction.Add)
            {
                Editor.Change edit = e.Edits.FirstOrDefault<Editor.Change>();
                if (!widgetConfig.AutoSave) featureEditor.Save.Execute(null);

                if (edit != null)
                {
                    FeatureLayer fLayer = edit.Layer as FeatureLayer;
                    OpenFeatureDataEditor(fLayer, edit.Graphic);
                }
            }
        }

        /// <summary>
        /// Open FeatureDataForm to Edit Attributes
        /// </summary>
        private void OpenFeatureDataEditor(FeatureLayer layer, Graphic graphic)
        {
            if (featureEditor.ClearSelection.CanExecute(null))
            {
                featureEditor.ClearSelection.Execute(null);
            }

            graphic.Select();
            dataEditorForm.FeatureLayer = layer;
            dataEditorForm.GraphicSource = graphic;
            dataEditorWindow.Title = layer.LayerInfo.Name;

            if (layer.LayerInfo.HasAttachments)
            {
                attachmentEditor.GraphicSource = graphic;
                attachmentEditor.FeatureLayer = layer;
                attachmentEditor.Visibility = Visibility.Visible;
            }
            else
            {
                attachmentEditor.Visibility = Visibility.Collapsed;
            }

            FloatingPopup.Show(dataEditorWindow);
            //Show Attribute Editing Window at Feature Geometry's Centroid
            //FloatingPopup.Show(dataEditorWindow, MapControl.MapToScreen(graphic.Geometry.Extent.GetCenter()));
        }

        private void DataEditorForm_EditEnded(object sender, EventArgs e)
        {
            FloatingPopup.Close();
        }
        #endregion

        #region Event Handlers of Feature Type Template Picker
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton)
            {
                ToggleButton toggler = sender as ToggleButton;
                StackPanel itemPanel = (toggler.Parent as StackPanel).Parent as StackPanel;
                if (itemPanel.Children[1] is ListBox)
                {
                    ListBox templatePanel = itemPanel.Children[1] as ListBox;
                    if (templatePanel.Visibility == System.Windows.Visibility.Visible)
                    {
                        templatePanel.Visibility = System.Windows.Visibility.Collapsed;
                        toggler.State = ToggleButtonState.STATE_ROTATE_90;
                    }
                    else
                    {
                        templatePanel.Visibility = System.Windows.Visibility.Visible;
                        toggler.State = ToggleButtonState.STATE_ORIGIN;
                    }
                }
            }
        }

        private void LayerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                CheckBox checkBox = sender as CheckBox;
                string layerID = (string)checkBox.Tag;
                this.MapControl.Layers[layerID].Visible = checkBox.IsChecked.Value;

                if (layerID == editingLayer && activeCommand == "Add")
                {
                    if (checkBox.IsChecked.Value)
                    { featureEditor.LayerIDs = new string[] { layerID }; }
                    else  //Enable Action Buttons
                    { featureEditor.LayerIDs = featureLayerIDs.ToArray(); }
                }
            }
        }

        private void SymbolTypeListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                string layerID = (string)listBox.Tag;
                SymbolType symType = listBox.SelectedItem as SymbolType;

                //Save Command State
                activeCommand = "Add";
                editingLayer = layerID;
                commandParam = symType.TypeID;
                StackAddFeatureButtons.Tag = symType.TypeID;
                ResetAddFeatureButtons(layerID);

                featureEditor.LayerIDs = new string[] { layerID };
                featureEditor.Add.Execute(symType.TypeID);
            }
        }

        private void ResetAddFeatureButtons(string layerID)
        {
            FeatureLayer fLayer = this.MapControl.Layers[layerID] as FeatureLayer;
            bool isPoint = fLayer.LayerInfo.GeometryType == GeometryType.Point;
            bool isPolygon = fLayer.LayerInfo.GeometryType == GeometryType.Polygon;
            bool isPolyline = fLayer.LayerInfo.GeometryType == GeometryType.Polyline;

            ButtonAddPoint.IsEnabled = isPoint;
            ButtonAddPolyline.IsEnabled = isPolyline;
            ButtonAddFreehandPolyline.IsEnabled = isPolyline;
            ButtonAddPolygon.IsEnabled = isPolygon;
            ButtonAddFreehandPolygon.IsEnabled = isPolygon;
            ButtonAddAutoCompletePolygon.IsEnabled = isPolygon;

            if (isPoint)
            {
                lastAddParam = "Point";
                ResetButtonStyle(ButtonAddPoint);
            }
            else if (isPolyline)
            {
                if (lastAddParam.Equals("Polyline_Freehand"))
                {
                    ResetButtonStyle(ButtonAddFreehandPolyline);
                    featureEditor.Freehand = true;
                }
                else
                {
                    lastAddParam = "Polyline";
                    ResetButtonStyle(ButtonAddPolyline);
                }
            }
            else if (isPolygon)
            {
                if (lastAddParam.Equals("Polygon_AutoComplete"))
                {
                    ResetButtonStyle(ButtonAddAutoCompletePolygon);
                    featureEditor.AutoComplete = true;
                }
                else if (lastAddParam.Equals("Polygon_Freehand"))
                {
                    ResetButtonStyle(ButtonAddFreehandPolygon);
                    featureEditor.Freehand = true;
                }
                else
                {
                    lastAddParam = "Polygon";
                    ResetButtonStyle(ButtonAddPolygon);
                }
            }
        }
        #endregion

        #region Event Handlers of Editor Buttons
        private void EditActionButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            activeCommand = button.Tag as string;
            commandParam = button.CommandParameter;
            featureEditor.LayerIDs = featureLayerIDs.ToArray();
            featureEditor.Freehand = false;
            ResetButtonStyle(button);

            if (activeCommand == "Attributes")
            {
                featureEditor.CancelActive.Execute(null);
                featureEditor.Select.Execute(null);
            }
        }

        private void AddFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            lastAddParam = button.Tag as string;
            button.CommandParameter = StackAddFeatureButtons.Tag;
            featureEditor.Freehand = lastAddParam.EndsWith("Freehand");
            featureEditor.AutoComplete = lastAddParam.EndsWith("AutoComplete");
            ResetButtonStyle(button);
        }

        /// <summary>
        /// Shared Function - Reset the style of all buttons  to null 
        /// </summary>
        /// <param name="parentPanel"></param>
        private void ResetButtonStyle(Button activeButton)
        {
            if (activeButton.Style != null) return;

            foreach (UIElement elem in StackEditActionButtons.Children)
            {
                if (elem is Button) (elem as Button).Style = null;
            }

            foreach (UIElement elem in StackAddFeatureButtons.Children)
            {
                if (elem is Button) (elem as Button).Style = null;
            }

            activeButton.Style = this.Resources["ActiveButtonStyle"] as Style;
        }
        #endregion
    }
}
