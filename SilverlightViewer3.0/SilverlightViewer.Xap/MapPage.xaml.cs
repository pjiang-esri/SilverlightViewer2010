using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Resources;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Threading;
using System.ComponentModel;
using System.Reflection;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.FeatureService;

using ESRI.SilverlightViewer.Controls;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.UIWidget;

namespace ESRI.SilverlightViewer
{
    public partial class MapPage : UserControl, INotifyPropertyChanged
    {
        // Map Scale 
        private double _mapScale = 0.0;

        // DrawWidget - widget using DrawObject
        private Type _drawWidget = null;

        // DrawObject shared by all widgets 
        private Draw _drawObject = new Draw();

        // Implement INotifyPropertyChanged to Dispatch MapScale Property Change Event
        public event PropertyChangedEventHandler PropertyChanged;

        #region General Properties
        public MapApp CurrentApp
        {
            get { return Application.Current as MapApp; }
        }

        public int MapSRWKID { get; private set; }

        public double MapScale
        {
            get { return _mapScale; }
            private set { _mapScale = value; NotifyPropertyChanged("MapScale"); }
        }

        public ScaleLine.ScaleLineUnit MapUnits { get; private set; }

        public Envelope InitMapExtent { get; private set; }

        public Envelope FullMapExtent { get; private set; }

        // DrawObject shared by all Widgets
        public Draw DrawObject { get { return _drawObject; } }

        // Trace which widget is using DrawObject
        public Type DrawWidget { get { return _drawWidget; } set { _drawWidget = value; } }

        #endregion

        /// <summary>
        /// Map Control Constructor
        /// </summary>
        /// <param name="app"></param>
        public MapPage()
        {
            InitializeComponent();
        }

        #region Page_Load Event Handler
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentApp.AppConfig != null)
            {
                InitializeBaseMap(CurrentApp.AppConfig.MapConfig);
                InitializeLiveMaps(CurrentApp.AppConfig.MapConfig);

                //Load widgets after map layers are added to ensuer graphcis layer on the top
                LoadWidgets(CurrentApp.AppConfig.WidgetsConfig);

                if (!string.IsNullOrEmpty(CurrentApp.AppConfig.OverviewMapConfigFile))
                {
                    OverviewMapWidget overviewWidget = new OverviewMapWidget() { Margin = new Thickness(0, 0, 0, 0) };
                    overviewWidget.ConfigFile = CurrentApp.AppConfig.OverviewMapConfigFile;
                    overviewWidget.MapControl = this.myMap;
                    LayoutRoot.Children.Add(overviewWidget);
                }

                if (this.CurrentApp.Resources.Contains("NavigatorStyle"))
                {
                    myNavigator.Style = this.CurrentApp.Resources["NavigatorStyle"] as Style;
                }

                if (this.CurrentApp.Resources.Contains("TaskbarStyle"))
                {
                    myTaskbarWidget.Style = this.CurrentApp.Resources["TaskbarStyle"] as Style;
                }
            }
        }
        #endregion

        #region Implement INotifyPropertyChanged - Notify Property Changed
        // NotifyPropertyChanged will raise the PropertyChanged event.
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Load Widgets which are configurated in AppConfig.xml
        private void LoadWidgets(WidgetConfig[] widgetsConfig)
        {
            if (widgetsConfig != null)
            {
                foreach (WidgetConfig config in widgetsConfig)
                {
                    if (string.IsNullOrEmpty(config.XapFile))
                    {
                        string[] asmClass = config.ClassName.Split(';');
                        if (asmClass.Length > 1 && asmClass[0].EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                        {
                            WebClient widgetRead = new WebClient();
                            widgetRead.OpenReadCompleted += new OpenReadCompletedEventHandler(WidgetDLL_OpenReadCompleted);
                            widgetRead.OpenReadAsync(new Uri(asmClass[0], UriKind.Relative), config);
                        }
                        else if (asmClass.Length == 1)
                        {
                            WidgetBase widget = Activator.CreateInstance(Type.GetType(asmClass[0])) as WidgetBase;
                            SetWidgetProperties(widget, config);
                        }
                    }
                    else
                    {
                        WebClient widgetRead = new WebClient();
                        widgetRead.OpenReadCompleted += new OpenReadCompletedEventHandler(WidgetXAP_OpenReadCompleted);
                        widgetRead.OpenReadAsync(new Uri(config.XapFile, UriKind.Relative), config);
                    }
                }
            }
        }

        private void WidgetDLL_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            WidgetConfig config = (WidgetConfig)e.UserState;
            string[] asmClass = config.ClassName.Split(';');
            if (asmClass.Length < 2) return;

            AssemblyPart assemblyPart = new AssemblyPart();
            Deployment.Current.Parts.Add(assemblyPart);
            Assembly assembly = assemblyPart.Load(e.Result);
            WidgetBase widget = assembly.CreateInstance(asmClass[1]) as WidgetBase;
            SetWidgetProperties(widget, config);
        }

        private void WidgetXAP_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            WidgetConfig config = (WidgetConfig)e.UserState;
            string[] asmClass = config.ClassName.Split(';');
            if (asmClass.Length < 2) return;

            List<AssemblyPart> currentParts = Deployment.Current.Parts.ToList();

            StreamResourceInfo xapStreamInfo = new StreamResourceInfo(e.Result, "application/binary");
            string appManifest = new StreamReader(Application.GetResourceStream(xapStreamInfo, new Uri("AppManifest.xaml", UriKind.Relative)).Stream).ReadToEnd();
            XElement deploymentRoot = XDocument.Parse(appManifest).Root;
            List<XElement> deploymentParts = (from assemblyParts in deploymentRoot.Elements().Elements() select assemblyParts).ToList();

            Assembly widgetAssembly = null;
            foreach (XElement deployPart in deploymentParts)
            {
                string source = deployPart.Attribute("Source").Value; // Assembly Name
                StreamResourceInfo asmStreamInfo = Application.GetResourceStream(xapStreamInfo, new Uri(source, UriKind.Relative));

                if (currentParts.FirstOrDefault(part => part.Source.Equals(source, StringComparison.CurrentCulture)) == null)
                {
                    AssemblyPart assemblyPart = new AssemblyPart();

                    if (source.Equals(asmClass[0], StringComparison.CurrentCultureIgnoreCase))
                        widgetAssembly = assemblyPart.Load(asmStreamInfo.Stream);
                    else
                        assemblyPart.Load(asmStreamInfo.Stream);

                    Deployment.Current.Parts.Add(assemblyPart);
                }
            }

            if (widgetAssembly != null)
            {
                WidgetBase widget = widgetAssembly.CreateInstance(asmClass[1]) as WidgetBase;
                SetWidgetProperties(widget, config);
            }
        }

        private void SetWidgetProperties(WidgetBase widget, WidgetConfig config)
        {
            if (widget != null)
            {
                widget.Title = config.Title;
                widget.IsOpen = config.OpenInitial;
                widget.MapControl = myMap;
                widget.IconSource = new BitmapImage(new Uri(config.IconSource, UriKind.Relative));
                widget.InitialTop = config.InitialTop;
                widget.InitialLeft = config.InitialLeft;
                widget.HasGraphics = config.HasGraphics;
                widget.ConfigFile = config.ConfigFile;
                widget.Style = this.CurrentApp.Resources["WidgetStyle"] as Style;
                WidgetManager.Widgets.Add(widget);
                WidgetsCanvas.Children.Add(widget);
            }
        }
        #endregion

        #region Initialize Base Map, Living Maps, Map Extent, etc...
        /// <summary>
        /// Initialize Base Map
        /// </summary>
        private void InitializeBaseMap(MapConfig mapConfig)
        {
            if (mapConfig == null || mapConfig.BaseMap == null) return;

            BaseMapConfig baseMapConfig = mapConfig.BaseMap;

            if (baseMapConfig.EnableBase == ServiceSource.ArcGIS)
            {
                foreach (ArcGISBaseMapLayer layerConfig in baseMapConfig.ArcGISBaseMap.Layers)
                {
                    CreateArcGISMapLayer(layerConfig, 1.0);
                }

                if (baseMapConfig.ArcGISBaseMap.LabelLayer != null)
                {
                    CreateArcGISMapLayer(baseMapConfig.ArcGISBaseMap.LabelLayer, 1.0);
                }
            }
            else if (baseMapConfig.EnableBase == ServiceSource.BING)
            {
                string token = baseMapConfig.BingBaseMap.Token;
                ServerType serverType = baseMapConfig.BingBaseMap.ServerType;

                if (string.IsNullOrEmpty(token))
                {
                    token = CurrentApp.GetAppParamValue(MapApp.BING_TOKEN);
                    serverType = (ServerType)Enum.Parse(typeof(ServerType), CurrentApp.GetAppParamValue(MapApp.BING_SERVER), true);
                }

                foreach (BingBaseMapLayer layerConfig in baseMapConfig.BingBaseMap.Layers)
                {
                    TileLayer bingLayer = new TileLayer()
                    {
                        ID = layerConfig.ID,
                        ServerType = serverType,
                        Visible = layerConfig.VisibleInitial,
                        Opacity = 1.0,
                        Token = token
                    };

                    switch (layerConfig.LayerType)
                    {
                        case BingLayerType.Road:
                            bingLayer.LayerStyle = ESRI.ArcGIS.Client.Bing.TileLayer.LayerType.Road; break;
                        case BingLayerType.Aerial:
                            bingLayer.LayerStyle = ESRI.ArcGIS.Client.Bing.TileLayer.LayerType.Aerial; break;
                        case BingLayerType.AerialWithLabels:
                            bingLayer.LayerStyle = ESRI.ArcGIS.Client.Bing.TileLayer.LayerType.AerialWithLabels; break;
                    }

                    bingLayer.Initialized += new EventHandler<EventArgs>(MapLayer_Initialized);
                    bingLayer.InitializationFailed += new EventHandler<EventArgs>(MapLayer_InitializationFailed);
                    myMap.Layers.Add(bingLayer);
                }
            }
        }

        /// <summary>
        /// Initialize Living Maps and Feature Layers
        /// </summary>
        private void InitializeLiveMaps(MapConfig mapConfig)
        {
            if (mapConfig == null || mapConfig.LivingMaps == null) return;

            if (mapConfig.LivingMaps != null)
            {
                foreach (LivingMapLayer livingMap in mapConfig.LivingMaps)
                {
                    if (livingMap.ServiceType == ArcGISServiceType.Cached)
                    {
                        CreateArcGISMapLayer(livingMap, livingMap.Opacity);
                    }
                    else if (livingMap.ServiceType == ArcGISServiceType.Dynamic)
                    {
                        ArcGISDynamicMapServiceLayer agisDynamicMap = CreateArcGISMapLayer(livingMap, livingMap.Opacity) as ArcGISDynamicMapServiceLayer;

                        if (livingMap.RefreshRate > 0)
                        {
                            agisDynamicMap.DisableClientCaching = true;
                            DispatcherTimer refreshTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(livingMap.RefreshRate) };
                            refreshTimer.Tick += (o, e) => { agisDynamicMap.Refresh(); };
                            refreshTimer.Start();
                        }
                    }
                    else if (livingMap.ServiceType == ArcGISServiceType.Feature)
                    {
                        FeatureLayer agisFeatureLayer = CreateArcGISMapLayer(livingMap, livingMap.Opacity) as FeatureLayer;

                        if (livingMap.FeatureLayerConfig != null)
                        {
                            agisFeatureLayer.Geometry = (livingMap.FeatureLayerConfig.EnvelopeFilter == null) ? null : (livingMap.FeatureLayerConfig.EnvelopeFilter.ToEnvelope());
                            agisFeatureLayer.Where = livingMap.FeatureLayerConfig.WhereString;

                            if (livingMap.FeatureLayerConfig.UseCluster)
                            {
                                agisFeatureLayer.Clusterer = new FlareClusterer()
                                {
                                    Radius = 15,
                                    MaximumFlareCount = 5,
                                    FlareBackground = this.CurrentApp.Resources[SymbolResources.FLARE_BACKGROUND] as Brush,
                                    FlareForeground = this.CurrentApp.Resources[SymbolResources.FLARE_FOREGROUND] as Brush,
                                    Gradient = this.CurrentApp.Resources[SymbolResources.CLUSTER_GRADIENT] as LinearGradientBrush
                                };
                            }

                            if (!string.IsNullOrEmpty(livingMap.FeatureLayerConfig.SymbolImage))
                            {
                                agisFeatureLayer.Renderer = new SimpleRenderer() { Symbol = new PictureMarkerSymbol() { Source = new BitmapImage(new Uri(livingMap.FeatureLayerConfig.SymbolImage, UriKind.Relative)) } };
                            }

                            if (!string.IsNullOrEmpty(livingMap.FeatureLayerConfig.OutFields))
                            {
                                string[] fields = livingMap.FeatureLayerConfig.OutFields.Split(',');
                                foreach (string field in fields) { agisFeatureLayer.OutFields.Add(field); }
                            }
                        }

                        if (livingMap.RefreshRate > 0)
                        {
                            agisFeatureLayer.DisableClientCaching = true;
                            DispatcherTimer refreshTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(livingMap.RefreshRate) };
                            refreshTimer.Tick += (o, e) => { agisFeatureLayer.Refresh(); };
                            refreshTimer.Start();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create ArcGIS Server Map Service Layer
        /// </summary>
        private Layer CreateArcGISMapLayer(ArcGISMapLayer layerConfig, double opacity)
        {
            Layer layer = null;

            if (layerConfig.ServiceType == ArcGISServiceType.Cached)
            {
                ArcGISTiledMapServiceLayer cachedLayer = new ArcGISTiledMapServiceLayer()
                {
                    Url = layerConfig.RESTURL,
                    Visible = layerConfig.VisibleInitial,
                    ProxyURL = layerConfig.ProxyURL,
                    Token = (layerConfig.Token == null) ? "" : layerConfig.Token
                };

                layer = cachedLayer;
            }
            else if (layerConfig.ServiceType == ArcGISServiceType.Image)
            {
                ArcGISImageServiceLayer imageLayer = new ArcGISImageServiceLayer()
                {
                    Url = layerConfig.RESTURL,
                    Visible = layerConfig.VisibleInitial,
                    ProxyURL = layerConfig.ProxyURL,
                    Token = (layerConfig.Token == null) ? "" : layerConfig.Token
                };

                layer = imageLayer;
            }
            else if (layerConfig.ServiceType == ArcGISServiceType.Dynamic)
            {
                ArcGISDynamicMapServiceLayer dynamicLayer = new ArcGISDynamicMapServiceLayer()
                {
                    Url = layerConfig.RESTURL,
                    Visible = layerConfig.VisibleInitial,
                    ProxyURL = layerConfig.ProxyURL,
                    Token = (layerConfig.Token == null) ? "" : layerConfig.Token
                };

                layer = dynamicLayer;
            }
            else if (layerConfig.ServiceType == ArcGISServiceType.Feature)
            {
                FeatureLayer featureLayer = new FeatureLayer()
                {
                    Url = layerConfig.RESTURL,
                    Visible = layerConfig.VisibleInitial,
                    ProxyUrl = layerConfig.ProxyURL,
                    Token = (layerConfig.Token == null) ? "" : layerConfig.Token
                };

                layer = featureLayer;
            }

            if (layer != null)
            {
                layer.ID = layerConfig.ID;
                layer.Opacity = opacity;
                layer.Initialized += new EventHandler<EventArgs>(MapLayer_Initialized);
                layer.InitializationFailed += new EventHandler<EventArgs>(MapLayer_InitializationFailed);
                myMap.Layers.Add(layer);
            }

            return layer;
        }

        /// <summary>
        /// Initialize map Scalebar and set map units value
        /// </summary>
        private void InitializeMapUnits()
        {
            // Set Scalebar Units
            if (myMap.Layers.Count > 0)
            {
                string mapUnits = "esriMeters";

                if (myMap.Layers[0] is ArcGISTiledMapServiceLayer)
                {
                    mapUnits = (myMap.Layers[0] as ArcGISTiledMapServiceLayer).Units;
                }
                else if (myMap.Layers[0] is ArcGISDynamicMapServiceLayer)
                {
                    mapUnits = (myMap.Layers[0] as ArcGISDynamicMapServiceLayer).Units;
                }

                this.MapUnits = (ScaleLine.ScaleLineUnit)Enum.Parse(typeof(ScaleLine.ScaleLineUnit), mapUnits.Remove(0, 4), true);
                this.MapSRWKID = myMap.SpatialReference.WKID;
                this.myScaleBar.MapUnit = this.MapUnits;
            }
        }

        /// <summary>
        /// Set an initial map extent after all map layers are initialized
        /// </summary>
        private void InitializeMapExtent()
        {
            MapConfig mapConfig = CurrentApp.AppConfig.MapConfig;

            if (mapConfig.InitialExtent != null)
            {
                InitMapExtent = mapConfig.InitialExtent.ToEnvelope(this.MapSRWKID);
            }

            if (mapConfig.FullExtent != null)
            {
                FullMapExtent = mapConfig.FullExtent.ToEnvelope(this.MapSRWKID);
            }

            // If OverviewMap exits, initialize Map extent here, otherwise in OverviewMapWidget
            if (string.IsNullOrEmpty(this.CurrentApp.AppConfig.OverviewMapConfigFile))
            {
                if (InitMapExtent != null)
                {
                    myMap.Extent = InitMapExtent;
                }
                else if (FullMapExtent != null)
                {
                    myMap.Extent = FullMapExtent;
                }

                // Initial set does not fire ExtentChange event
                double ratioScaleResolution = GeometryTool.RatioScaleResolution(myMap.Extent.GetCenter().Y, this.MapSRWKID, this.MapUnits);
                MapScale = ratioScaleResolution * myMap.Resolution;
            }
        }
        #endregion

        #region Map and Feature Layer Initialization Event Handlers
        private void MapLayer_Initialized(object sender, EventArgs e)
        {
            bool mapIsLoaded = true;

            foreach (Layer layer in myMap.Layers)
            {
                if (!layer.IsInitialized) { mapIsLoaded = false; break; }

                if (layer is FeatureLayer)
                {
                    FeatureLayer fLayer = layer as FeatureLayer;
                    FeatureLayerInfo lyrInfo = fLayer.LayerInfo;
                    if (lyrInfo != null)
                    {
                        fLayer.MapTip = new MapTipPopup(lyrInfo.Fields, fLayer.OutFields, lyrInfo.DisplayField, lyrInfo.Name) { ShowArrow = false, ShowCloseButton = false, Background = this.myTaskbarWidget.Background };
                    }
                }
            }

            if (mapIsLoaded)
            {
                InitializeMapUnits();
                InitializeMapExtent();
                EventCenter.DispatchMapLoadCompleteEvent(this.myMap, new RoutedEventArgs());
            }
        }

        private void MapLayer_InitializationFailed(object sender, EventArgs e)
        {
            if (sender is Layer)
            {
                MessageBox.Show("Failed to initialize the layer: " + (sender as Layer).ID);
            }
        }
        #endregion

        #region Map Progress and Extent Change Event Handlers
        private void MyMap_Progress(object sender, ESRI.ArcGIS.Client.ProgressEventArgs args)
        {
            if (args.Progress < 100)
            {
                progressGrid.Visibility = Visibility.Visible;
                myProgressBar.Value = args.Progress;
                ProgressValueTextBlock.Text = String.Format("{0}%", args.Progress);
            }
            else
            {
                progressGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void MyMap_ExtentChanged(object sender, ExtentEventArgs e)
        {
            this.CurrentApp.SetWindowStatus(string.Format("Map Extent: [{0:f}, {1:f}] - [{2:f}, {3:f}]", e.NewExtent.XMin, e.NewExtent.YMin, e.NewExtent.XMax, e.NewExtent.YMax));

            double ratioScaleResolution = GeometryTool.RatioScaleResolution(e.NewExtent.GetCenter().Y, this.MapSRWKID, this.MapUnits);
            this.MapScale = ratioScaleResolution * myMap.Resolution;
        }
        #endregion
    }
}