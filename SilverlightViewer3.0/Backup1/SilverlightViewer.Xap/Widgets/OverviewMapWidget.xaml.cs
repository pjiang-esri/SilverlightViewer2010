using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.SilverlightViewer;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class OverviewMapWidget : UserControl
    {
        private OverviewMapConfig OverviewConfig = null;
        // The names of storyboards for OverviewMap
        private const string SB_OPEN_OVERVIEWMAP = "OpenOverviewStoryBoard";
        private const string SB_HIDE_OVERVIEWMAP = "HideOverviewStoryBoard";

        #region Initialize Widget
        public OverviewMapWidget()
        {
            InitializeComponent();
            WidgetManager.OverviewMap = this;
            EventCenter.MapLoadComplete += new MapLoadCompleteEventHandler(OnMapLoadComplete);
        }

        private void Widget_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard sbHide = this.Resources[SB_HIDE_OVERVIEWMAP] as Storyboard;
            sbHide.Completed += new EventHandler(HideStoryboard_Completed);
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
            OverviewConfig = (OverviewMapConfig)OverviewMapConfig.Deserialize(e.Result, typeof(OverviewMapConfig));

            if (OverviewConfig != null)
            {
                this.IsOpen = OverviewConfig.OpenInitial;
                this.Position = (OverviewConfig.Position == OverviewMapPosition.Undefined) ? OverviewMapPosition.LowerRight : OverviewConfig.Position;

                myOverviewMap.Width = (OverviewConfig.Width > 0) ? OverviewConfig.Width : 250;
                myOverviewMap.Height = (OverviewConfig.Height > 0) ? OverviewConfig.Height : 200;
                myOverviewMap.MaximumExtent = OverviewConfig.MaximumExtent.ToEnvelope(this.CurrentPage.MapSRWKID);

                if (OverviewConfig.MapLayer != null)
                {
                    myOverviewMap.Layer = new ArcGISTiledMapServiceLayer() { ID = "Overview_Layer", Url = OverviewConfig.MapLayer.RESTURL, ProxyURL = OverviewConfig.MapLayer.ProxyURL };
                }
                else if (this.MapControl.Layers[0] is ArcGISTiledMapServiceLayer)
                {
                    ArcGISTiledMapServiceLayer agisTiledLayer = this.MapControl.Layers[0] as ArcGISTiledMapServiceLayer;
                    myOverviewMap.Layer = new ArcGISTiledMapServiceLayer() { ID = "Overview_Layer", Url = agisTiledLayer.Url, ProxyURL = agisTiledLayer.ProxyURL };
                }
                else if (this.MapControl.Layers[0] is ArcGISDynamicMapServiceLayer)
                {
                    ArcGISDynamicMapServiceLayer agisDynamicLayer = this.MapControl.Layers[0] as ArcGISDynamicMapServiceLayer;
                    myOverviewMap.Layer = new ArcGISDynamicMapServiceLayer() { ID = "Overview_Layer", Url = agisDynamicLayer.Url, ProxyURL = agisDynamicLayer.ProxyURL };
                }
                else if (this.MapControl.Layers[0] is ESRI.ArcGIS.Client.Bing.TileLayer)
                {
                    TileLayer bingTiledLayer = this.MapControl.Layers[0] as TileLayer;
                    myOverviewMap.Layer = new TileLayer() { ID = "Overview_Layer", ServerType = bingTiledLayer.ServerType, LayerStyle = bingTiledLayer.LayerStyle, Token = bingTiledLayer.Token };
                }

                // Initialize Map extent here to sychronize with OverviewMap extent on the first loading
                if (this.CurrentPage.InitMapExtent != null)
                    this.MapControl.ZoomTo(this.CurrentPage.InitMapExtent);
                else if (this.CurrentPage.FullMapExtent != null)
                    this.MapControl.ZoomTo(this.CurrentPage.FullMapExtent);
            }
        }
        #endregion

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
        #endregion

        #region Binded Map Property
        /// <summary>
        /// Binded to the map control on the main page
        /// </summary>
        public ESRI.ArcGIS.Client.Map MapControl { get; set; }
        #endregion

        #region Configuration File Property
        /// <summary>
        /// Get and Set the widget's configuration file path
        /// </summary>
        public string ConfigFile { get; set; }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(OverviewMapWidget), new PropertyMetadata(true, new PropertyChangedCallback(OnIsOpenChanged)));
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(OverviewMapPosition), typeof(OverviewMapWidget), new PropertyMetadata(OverviewMapPosition.Undefined, new PropertyChangedCallback(OnPositionChanged)));

        /// <summary>
        /// Indicator of OverviewMap's visibility state
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Set and Get OverviewMap position: LowerRight, UpperRight, LowerLeft or UpperLeft
        /// </summary>
        [System.ComponentModel.TypeConverter(typeof(OverviewMapPositionConverter))]
        public OverviewMapPosition Position
        {
            get { return (OverviewMapPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        protected static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OverviewMapWidget me = d as OverviewMapWidget;
            if (me != null)
            {
                me.ChangeOverviewState((bool)e.NewValue);
            }
        }

        protected static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OverviewMapWidget me = d as OverviewMapWidget;
            if (me != null)
            {
                me.ResetOverviewPosition((OverviewMapPosition)e.NewValue);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Don't Initialize OverviewMap until the Map on the MainPage is Initialized
        /// </summary>
        private void OnMapLoadComplete(object sender, RoutedEventArgs e)
        {
            myOverviewMap.Map = this.MapControl;
            LoadConfigXML();
        }

        private void OverviewMapToggler_Click(object sender, RoutedEventArgs e)
        {
            this.IsOpen = !this.IsOpen;
        }

        private void ChangeOverviewState(bool isOpen)
        {
            if (isOpen)
            {
                switch (Position)
                {
                    case OverviewMapPosition.LowerRight:
                        OverviewPanelTransform.X = myOverviewMap.Width;
                        OverviewPanelTransform.Y = myOverviewMap.Height;
                        break;
                    case OverviewMapPosition.UpperRight:
                        OverviewPanelTransform.X = myOverviewMap.Width;
                        OverviewPanelTransform.Y = -myOverviewMap.Height;
                        break;
                    case OverviewMapPosition.LowerLeft:
                        OverviewPanelTransform.X = -myOverviewMap.Width;
                        OverviewPanelTransform.Y = myOverviewMap.Height;
                        break;
                    case OverviewMapPosition.UpperLeft:
                        OverviewPanelTransform.X = -myOverviewMap.Width;
                        OverviewPanelTransform.Y = -myOverviewMap.Height;
                        break;
                }

                (this.Resources[SB_OPEN_OVERVIEWMAP] as Storyboard).Begin();
                ToolTipService.SetToolTip(OverviewToggler, "Click to close overview map");
            }
            else
            {
                (this.Resources[SB_HIDE_OVERVIEWMAP] as Storyboard).Begin();
                ToolTipService.SetToolTip(OverviewToggler, "Click to open overview map");
            }
        }

        private void HideStoryboard_Completed(object sender, EventArgs e)
        {
            OverviewPanelTransform.X = 0;
            OverviewPanelTransform.Y = 0;
        }
        #endregion

        #region Reset Translate Tranform Frame Values Based on the Position
        private void ResetOverviewPosition(OverviewMapPosition position)
        {
            Storyboard sbOpen = this.Resources[SB_OPEN_OVERVIEWMAP] as Storyboard;
            Storyboard sbHide = this.Resources[SB_HIDE_OVERVIEWMAP] as Storyboard;

            // DoubleAnimation in Open StoryBoard for rotating the toggle button
            DoubleAnimation dblAnimO = sbOpen.Children[1] as DoubleAnimation;

            // DoubleAnimation in Hide StoryBoard for rotating the toggle button
            DoubleAnimation dblAnimA = sbHide.Children[1] as DoubleAnimation;

            // DoubleAnimation in Hide StoryBoard for moving in X direction
            DoubleAnimation dblAnimX = sbHide.Children[2] as DoubleAnimation;

            // DoubleAnimation in Hide StoryBoard for moving in Y direction
            DoubleAnimation dblAnimY = sbHide.Children[3] as DoubleAnimation;

            switch (position)
            {
                case OverviewMapPosition.LowerRight:
                    dblAnimO.To = 180;
                    dblAnimA.To = 0;
                    dblAnimX.To = myOverviewMap.Width;
                    dblAnimY.To = myOverviewMap.Height;
                    this.OverviewTogglerTransform.Angle = (this.IsOpen) ? 180 : 0;
                    this.OverviewMapPanel.BorderThickness = new Thickness(2, 2, 0, 0);
                    this.OverviewToggler.HorizontalAlignment = HorizontalAlignment.Left;
                    this.OverviewToggler.VerticalAlignment = VerticalAlignment.Top;
                    this.HorizontalAlignment = HorizontalAlignment.Right;
                    this.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case OverviewMapPosition.UpperRight:
                    dblAnimO.To = 90;
                    dblAnimA.To = -90;
                    dblAnimX.To = myOverviewMap.Width;
                    dblAnimY.To = -myOverviewMap.Height;
                    this.OverviewTogglerTransform.Angle = (this.IsOpen) ? 90 : -90;
                    this.OverviewMapPanel.BorderThickness = new Thickness(2, 0, 0, 2);
                    this.OverviewToggler.HorizontalAlignment = HorizontalAlignment.Left;
                    this.OverviewToggler.VerticalAlignment = VerticalAlignment.Bottom;
                    this.HorizontalAlignment = HorizontalAlignment.Right;
                    this.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case OverviewMapPosition.LowerLeft:
                    dblAnimO.To = -90;
                    dblAnimA.To = 90;
                    dblAnimX.To = -myOverviewMap.Width;
                    dblAnimY.To = myOverviewMap.Height;
                    this.OverviewTogglerTransform.Angle = (this.IsOpen) ? -90 : 90;
                    this.OverviewMapPanel.BorderThickness = new Thickness(0, 2, 2, 0);
                    this.OverviewToggler.HorizontalAlignment = HorizontalAlignment.Right;
                    this.OverviewToggler.VerticalAlignment = VerticalAlignment.Top;
                    this.HorizontalAlignment = HorizontalAlignment.Left;
                    this.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case OverviewMapPosition.UpperLeft:
                    dblAnimO.To = 0;
                    dblAnimA.To = 180;
                    dblAnimX.To = -myOverviewMap.Width;
                    dblAnimY.To = -myOverviewMap.Height;
                    this.OverviewTogglerTransform.Angle = (this.IsOpen) ? 0 : 180;
                    this.OverviewMapPanel.BorderThickness = new Thickness(0, 0, 2, 2);
                    this.OverviewToggler.HorizontalAlignment = HorizontalAlignment.Right;
                    this.OverviewToggler.VerticalAlignment = VerticalAlignment.Bottom;
                    this.HorizontalAlignment = HorizontalAlignment.Left;
                    this.VerticalAlignment = VerticalAlignment.Top;
                    break;
            }
        }
        #endregion
    }
}
