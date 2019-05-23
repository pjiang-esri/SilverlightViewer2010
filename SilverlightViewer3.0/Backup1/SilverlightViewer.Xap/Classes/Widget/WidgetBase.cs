using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.ValueConverters;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;

namespace ESRI.SilverlightViewer.Widget
{
    public class WidgetBase : FloatingWindow
    {
        protected int lastTabIndex = -1;
        protected int graphicsLayerIndex = -1;
        protected string graphicsLayerID = "";
        protected string graphicsTipTemplate = "";
        protected Graphic selectedGraphic = null;
        protected GraphicsLayer graphicsLayer = null;

        // Temporarily save DrawObject.IsEnabled value
        private bool wasDrawObjectEnabled = false;

        #region Define WidgetContentChange Event Handler
        private WidgetContentChangeEvent WidgetContentChangeHandler = null;
        public event WidgetContentChangeEvent WidgetContentChange
        {
            add
            {
                if (WidgetContentChangeHandler == null || !WidgetContentChangeHandler.GetInvocationList().Contains(value))
                {
                    WidgetContentChangeHandler += value;
                }
            }
            remove
            {
                WidgetContentChangeHandler -= value;
            }
        }
        #endregion

        public WidgetBase()
        {
            this.TabButtons = new ObservableCollection<WidgetTabButton>();
            this.FeatureSets = new ObservableCollection<GeoFeatureCollection>();
        }

        #region Readonly Properties
        /// <summary>
        /// Get the current Application
        /// </summary>
        public MapApp CurrentApp
        {
            get { return Application.Current as MapApp; }
        }

        /// <summary>
        /// Get the main Map Page of the Application  
        /// </summary>
        public MapPage CurrentPage
        {
            get { return (CurrentApp == null) ? null : CurrentApp.RootVisual as MapPage; }
        }

        /// <summary>
        /// Get the current Application's configuration
        /// </summary>
        public ApplicationConfig AppConfig
        {
            get { return (CurrentApp == null) ? null : CurrentApp.AppConfig; }
        }

        /// <summary>
        /// Get the DrawObject from the Map Page, which is shared by all widgets
        /// </summary>
        public Draw DrawObject
        {
            get { return CurrentPage.DrawObject; }
        }

        /// <summary>
        /// Get and Set which widget has the control on the DrawObject 
        /// </summary>
        public Type DrawWidget
        {
            get { return CurrentPage.DrawWidget; }
            protected set { CurrentPage.DrawWidget = value; }
        }

        /// <summary>
        /// Get the map's sptatial reference WKID
        /// </summary>
        public int MapSRWKID
        {
            get { return CurrentPage.MapSRWKID; }
        }

        /// <summary>
        /// Get the widget's current GeoFeature collections
        /// </summary>
        public ObservableCollection<GeoFeatureCollection> FeatureSets
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the Graphics Layer Index in the Map's Layer Collection
        /// </summary>
        public int GraphicsLayerIndex
        {
            get { return graphicsLayerIndex; }
        }

        /// <summary>
        /// Get the widget's Graphics Layer ID
        /// </summary>
        public string GraphicsLayerID
        {
            get { return graphicsLayerID; }
        }

        /// <summary>
        /// Get the widget's GraphicsLayer. If set HasGraphics = false, return null
        /// </summary>
        public GraphicsLayer GraphicsLayer
        {
            get { return graphicsLayer; }
        }

        /// <summary>
        /// Get and Set The Graphics Map Tip Template
        /// </summary>
        public string GraphicsTipTemplate
        {
            get { return graphicsTipTemplate; }
            protected set
            {
                graphicsTipTemplate = value;
                if (graphicsLayer != null)
                {
                    graphicsLayer.MapTip = new MapTipPopup(graphicsTipTemplate) { ShowCloseButton = false, Background = this.Background };
                }
            }
        }

        /// <summary>
        /// Get and Set the Selected Graphic of a Widget
        /// </summary>
        public Graphic SelectedGraphic
        {
            get { return selectedGraphic; }

            protected set
            {
                if (selectedGraphic != value)
                {
                    selectedGraphic = value;
                    EventCenter.DispatchWidgetSelectedGraphicChangeEvent(this, new SelectedItemChangeEventArgs(value));
                }
            }
        }
        #endregion

        #region Normal Properties
        /// <summary>
        /// Get and Set the widget's configuration file path
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Get and Set clear buttom image source URI.
        /// Only need when the widget has graphics
        /// </summary>
        public ImageSource ClearButtonImage { get; set; }

        /// <summary>
        /// Get ans Set the Map control of the Application
        /// </summary>
        public ESRI.ArcGIS.Client.Map MapControl { get; set; }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty TabButtonsProperty = DependencyProperty.Register("TabButtons", typeof(ObservableCollection<WidgetTabButton>), typeof(WidgetBase), null);
        public static readonly DependencyProperty HasGraphicsProperty = DependencyProperty.Register("HasGraphics", typeof(bool), typeof(WidgetBase), new PropertyMetadata(false));
        public static readonly DependencyProperty SelectedTabProperty = DependencyProperty.Register("SelectedTab", typeof(WidgetTabButton), typeof(WidgetBase), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedTabIndexProperty = DependencyProperty.Register("SelectedTabIndex", typeof(int), typeof(WidgetBase), new PropertyMetadata(-1));
        public static readonly DependencyProperty AllowClickGraphicsProperty = DependencyProperty.Register("AllowClickGraphics", typeof(bool), typeof(WidgetBase), new PropertyMetadata(false));

        /// <summary>
        /// True - Create a graphics layer for this widget
        /// False - No graphics layer for this widget
        /// </summary>
        public bool HasGraphics
        {
            get { return (bool)GetValue(HasGraphicsProperty); }
            set { SetValue(HasGraphicsProperty, value); }
        }

        /// <summary>
        /// True - Hook a MouseLeftButtonUp event handler to the GraphicsLayer
        /// An inheriting Widget must override OnGraphicsLayerMouseUp method
        /// </summary>
        public bool AllowClickGraphics
        {
            get { return (bool)GetValue(AllowClickGraphicsProperty); }
            set { SetValue(AllowClickGraphicsProperty, value); }
        }

        /// <summary>
        /// Set and Get a collection of Tab buttons of the widget
        /// </summary>
        public ObservableCollection<WidgetTabButton> TabButtons
        {
            get { return (ObservableCollection<WidgetTabButton>)GetValue(TabButtonsProperty); }
            set { SetValue(TabButtonsProperty, value); }
        }

        /// <summary>
        /// Get only - Selected Content Tab Button 
        /// </summary>
        public WidgetTabButton SelectedTab
        {
            get { return (WidgetTabButton)GetValue(SelectedTabProperty); }
            private set { SetValue(SelectedTabProperty, value); }
        }

        /// <summary>
        /// Get only - Selected Content Tab Index. Use ToggleWidgetContent to Switch Tab
        /// </summary>
        public int SelectedTabIndex
        {
            get { return (int)GetValue(SelectedTabIndexProperty); }
            private set { SetValue(SelectedTabIndexProperty, value); }
        }

        private void TabButtons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.Action != NotifyCollectionChangedAction.Reset) 
            AddWidgetTabButtons();
        }

        /// <summary>
        /// Notify other widgets that the graphics collection of this widget has changed
        /// </summary>
        private void FeatureSets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EventCenter.DispacthWidgetFeatureSetsChangeEvent(this, e);
        }
        #endregion

        #region Override Functions - ApplyTemplate and LoadConfig
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            AddWidgetTabButtons();
            CreateGraphicsLayer();

            // Add this event handler after initialize current available buttons
            this.TabButtons.CollectionChanged += new NotifyCollectionChangedEventHandler(TabButtons_CollectionChanged);
            this.FeatureSets.CollectionChanged += new NotifyCollectionChangedEventHandler(FeatureSets_CollectionChanged);

            // Final call to let inheriting classes make other initialization
            this.OnWidgetLoaded();
        }

        /// <summary>
        /// Called after the Widget's Template is applied and dynamic components are initialized
        /// </summary>
        protected virtual void OnWidgetLoaded()
        {
            if (!string.IsNullOrEmpty(ConfigFile))
            {
                LoadConfigXML();
            }
        }

        protected override void OnToggle(WindowToggleEventArgs e)
        {
            base.OnToggle(e);

            if (this.graphicsLayer != null)
            {
                this.graphicsLayer.Visible = e.Expanded;
            }
        }

        protected override void OnIsActiveChanged(object sender, IsActiveChangedEventArgs e)
        {
            if (e.IsActive)
            {
                ResetDrawObjectMode();
            }
            else
            {
                WidgetManager.ResetDrawObject();
            }
        }
        #endregion

        #region Overridable Function - ResetDrawObjectMode
        /// <summary>
        /// Virtual Function - Reset Draw Object Mode 
        /// Overriding this function is an alternative of implementing IDrawObject
        /// </summary>
        public virtual void ResetDrawObjectMode()
        {
        }
        #endregion

        #region Initialize Widget Tab Buttons
        protected void AddWidgetTabButtons()
        {
            if (TabButtons != null && TabButtons.Count > 0)
            {
                this.SelectedTabIndex = 0;
                this.SelectedTab = TabButtons[0];
                this.HeaderPanel.Children.Clear();

                for (int k = 0; k < TabButtons.Count; k++)
                {
                    WidgetTabButton buttonItem = TabButtons[k];
                    HyperlinkButton linkButton = new HyperlinkButton() { Tag = k, Width = 24, Height = 24, Margin = new Thickness(4, 0, 4, 0) };
                    linkButton.Click += new RoutedEventHandler(HeadButton_Click);
                    this.HeaderPanel.Children.Add(linkButton);

                    if (buttonItem.ButtonImage != null)
                    {
                        linkButton.Background = new ImageBrush() { ImageSource = buttonItem.ButtonImage };
                        ToolTipService.SetToolTip(linkButton, buttonItem.Title);
                    }
                    else if (string.IsNullOrEmpty(buttonItem.Title))
                        linkButton.Content = buttonItem.Title;

                    FrameworkElement contentElem = this.FindName(buttonItem.ContentPanel) as FrameworkElement;
                    if (contentElem != null) contentElem.Visibility = (k == this.SelectedTabIndex) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            if (ClearButtonImage != null)
            {
                HyperlinkButton clearButton = new HyperlinkButton() { Tag = 0, Width = 24, Height = 24, Margin = new Thickness(4, 0, 4, 0) };
                clearButton.Background = new ImageBrush() { ImageSource = ClearButtonImage };
                clearButton.Click += new RoutedEventHandler(ClearButton_Click);
                ToolTipService.SetToolTip(clearButton, "Clear Graphics");
                this.HeaderPanel.Children.Add(clearButton);
            }
        }
        #endregion

        #region Load Configuration File
        private void LoadConfigXML()
        {
            WebClient xmlClient = new WebClient();
            xmlClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnDownloadConfigXMLCompleted);
            xmlClient.DownloadStringAsync(new Uri(ConfigFile, UriKind.RelativeOrAbsolute));
        }

        protected virtual void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
        }
        #endregion

        #region Handle Head Button Events
        private void HeadButton_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = (int)(sender as HyperlinkButton).Tag;
            if (newIndex != this.SelectedTabIndex)
            {
                ToggleWidgetContent(newIndex);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.ClearGraphics(0);
        }
        #endregion

        #region Toggle Widget Content
        public void ToggleWidgetContent(int newIndex)
        {
            if (newIndex > -1 && newIndex < TabButtons.Count)
            {
                this.lastTabIndex = this.SelectedTabIndex;
                this.SelectedTab = TabButtons[newIndex];
                this.SelectedTabIndex = newIndex;

                RunToggleStoryBoard();
                OnSelectedContentChange(newIndex);
            }
        }

        protected virtual void RunToggleStoryBoard()
        {
            //Move out the current panel
            if (this.lastTabIndex != -1)
            {
                FrameworkElement oldContent = this.FindName(this.TabButtons[this.lastTabIndex].ContentPanel) as FrameworkElement;
                oldContent.RenderTransform = new TranslateTransform() { X = 0, Y = 0 };

                double distance = this.ContainerHeight;
                DoubleAnimation animTogo = new DoubleAnimation();
                animTogo.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                animTogo.To = (this.lastTabIndex < this.SelectedTabIndex) ? -distance : distance;
                animTogo.From = 0;

                Storyboard sbTogo = new Storyboard();
                sbTogo.Completed += new EventHandler(StoryboardTogo_Completed);
                Storyboard.SetTargetProperty(animTogo, new PropertyPath("Y"));
                Storyboard.SetTarget(animTogo, oldContent.RenderTransform);
                sbTogo.Children.Add(animTogo);
                sbTogo.Begin();
            }
        }

        protected void StoryboardTogo_Completed(object sender, EventArgs e)
        {
            FrameworkElement oldContent = this.FindName(this.TabButtons[this.lastTabIndex].ContentPanel) as FrameworkElement;
            FrameworkElement newContent = this.FindName(this.TabButtons[this.SelectedTabIndex].ContentPanel) as FrameworkElement;
            oldContent.Visibility = Visibility.Collapsed;
            newContent.Visibility = Visibility.Visible;

            //Move in the new panel
            double distance = this.ContainerHeight;
            newContent.RenderTransform = new TranslateTransform() { X = 0, Y = distance };

            DoubleAnimation animCome = new DoubleAnimation();
            animCome.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            animCome.From = (this.lastTabIndex < this.SelectedTabIndex) ? distance : -distance;
            animCome.To = 0;

            Storyboard sbCome = new Storyboard();
            Storyboard.SetTargetProperty(animCome, new PropertyPath("Y"));
            Storyboard.SetTarget(animCome, newContent.RenderTransform);
            sbCome.Children.Add(animCome);
            sbCome.Begin();
        }

        protected virtual void OnSelectedContentChange(int newIndex)
        {
            if (WidgetContentChangeHandler != null) WidgetContentChangeHandler(this, new WidgetContentChangeEventArgs(this.lastTabIndex, newIndex));
        }
        #endregion

        #region Create GraphicsLayer with a Tip Popup
        protected void CreateGraphicsLayer()
        {
            if (HasGraphics)
            {
                this.graphicsLayerID = "graphics_" + Guid.NewGuid().ToString("N");
                this.graphicsLayer = new ESRI.ArcGIS.Client.GraphicsLayer() { ID = this.graphicsLayerID };
                this.graphicsLayerIndex = this.MapControl.Layers.Count;
                this.MapControl.Layers.Add(this.graphicsLayer);

                if (!string.IsNullOrEmpty(graphicsTipTemplate))
                {
                    this.graphicsLayer.MapTip = new MapTipPopup(graphicsTipTemplate) { ShowCloseButton = false, Background = this.Background };
                }

                if (AllowClickGraphics)
                {
                    this.GraphicsLayer.MouseLeftButtonDown += new GraphicsLayer.MouseButtonEventHandler(OnGraphicsLayerMouseDown);
                    this.graphicsLayer.MouseLeftButtonUp += new GraphicsLayer.MouseButtonEventHandler(OnGraphicsLayerMouseUp);
                }
            }
        }
        #endregion

        #region Overridable Funtions - Graphic Click Event Handler
        protected virtual void OnGraphicsLayerMouseDown(object sender, GraphicMouseButtonEventArgs e)
        {
            this.wasDrawObjectEnabled = this.DrawObject.IsEnabled;
            this.DrawObject.IsEnabled = false;
        }

        /// <summary>
        /// Graphic Click Event Handler; An inheriting widget must override this method, but run this method at last
        /// </summary>
        protected virtual void OnGraphicsLayerMouseUp(object sender, GraphicMouseButtonEventArgs e)
        {
            PopupWindow tipGrid = ((this.graphicsLayer.MapTip == null) ? e.Graphic.MapTip : this.graphicsLayer.MapTip) as PopupWindow;

            if (tipGrid != null)
            {
                ModifierKeys keys = Keyboard.Modifiers;
                bool controlKey = (keys & ModifierKeys.Control) != 0;
                if (controlKey && tipGrid.Resources.Contains("HyperlinkField"))
                {
                    string hyperlinkField = tipGrid.Resources["HyperlinkField"] as string;
                    string sUrl = (e.Graphic.Attributes.ContainsKey(hyperlinkField)) ? (e.Graphic.Attributes[hyperlinkField] as string) : "";

                    if (!string.IsNullOrEmpty(sUrl))
                    {
                        System.Windows.Browser.HtmlPopupWindowOptions winOptions = new System.Windows.Browser.HtmlPopupWindowOptions() { Resizeable = true, Width = 800, Height = 700 };
                        System.Windows.Browser.HtmlWindow win = System.Windows.Browser.HtmlPage.PopupWindow(new Uri(sUrl), "_blank", winOptions);
                    }
                }
            }

            this.DrawObject.IsEnabled = this.wasDrawObjectEnabled;
        }
        #endregion

        #region Get the GraphicsLayer visual Panel from the Map control
        //public Panel GetGraphicsLayerCanvas()
        //{
        //    Panel graphicsPanel = null;

        //    DependencyObject grid1 = VisualTreeHelper.GetChild(this.MapControl, 0);
        //    DependencyObject grid2 = VisualTreeHelper.GetChild(grid1, 0);
        //    foreach (UIElement elem in (grid2 as Grid).Children)
        //    {
        //        if (elem is Canvas)
        //        {
        //            UIElement layersCanvas = (elem as Canvas).Children[0]; // LayerCanvasCollection : Canvas
        //            graphicsPanel = (layersCanvas as Canvas).Children[this.graphicsLayerIndex] as Panel;
        //            break;
        //        }
        //    }

        //    return graphicsPanel;
        //}
        #endregion

        #region Functions to Manipulate Graphics
        /// <summary>
        /// Add a new graphic to the graphics layer of this widget
        /// </summary>
        /// <param name="graphic"></param>
        public virtual void AddGraphic(ESRI.ArcGIS.Client.Graphic graphic)
        {
            if (graphicsLayer != null)
            {
                graphicsLayer.Graphics.Add(graphic);
            }
        }

        /// <summary>
        /// Clear all the graphics of the widget on the map
        /// </summary>
        /// <param name="newTab">Toggle widget to new content panel. If equal -1, stay with the same panel</param>
        public virtual void ClearGraphics(int newTab)
        {
            this.SelectedGraphic = null;
            this.FeatureSets.Clear();

            if (graphicsLayer != null)
            {
                graphicsLayer.Graphics.Clear();
            }

            if (this.SelectedTabIndex != newTab)
            {
                ToggleWidgetContent(newTab);
            }
        }

        /// <summary>
        /// Zoom map to a specific graphic
        /// </summary>
        public void ZoomToGraphics()
        {
            if (graphicsLayer != null)
            {
                // Expand the extent in case only one point graphic available in the graphics layer
                this.MapControl.ZoomTo(GeometryTool.ExpandGeometryExtent(this.GraphicsLayer.FullExtent, 0.25));
            }
        }

        /// <summary>
        /// The FeatureSet to which the selected graphic belongs
        /// </summary>
        /// <returns>The FeatureSet to which the selected graphic belongs</returns>
        public GeoFeatureCollection FindFeatureSetWithSelection()
        {
            if (this.SelectedGraphic == null) return null;

            if (this.FeatureSets.Count == 1)
            {
                return this.FeatureSets[0];
            }
            else if (this.FeatureSets.Count > 0)
            {
                int k = 0;
                foreach (GeoFeatureCollection dataset in this.FeatureSets)
                {
                    if (dataset.Contains(this.SelectedGraphic)) break;
                    k++;
                }

                return this.FeatureSets[k];
            }
            else return null;
        }
        #endregion
    }
}
