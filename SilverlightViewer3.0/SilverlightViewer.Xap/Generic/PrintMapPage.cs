using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
//using System.Diagnostics;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.SilverlightViewer.Utility;

namespace ESRI.SilverlightViewer.Generic
{
    public class PrintMapPage : Control
    {
        // Letter Paper Size 
        const double DEFAULT_PAPER_WIDTH = 780;
        const double DEFAULT_PAPER_HEIGHT = 1020;

        // Template Controls
        protected const string _PRINT_MAP = "PrintMap";
        protected const string _TITLE_BLOCK = "TitleBlock";
        protected const string _LEGEND_GRID = "LegendGrid";
        protected const string _LEGEND_PANEL = "LegendPanel";
        protected const string _OVERVIEW_MAP = "OverviewMap";
        protected const string _TEMPLATE_GRID = "TemplateGrid";

        protected Map PrintMap = null;
        protected Grid LegendGrid = null;
        protected TextBlock TitleBlock = null;
        protected StackPanel LegendPanel = null;
        protected OverviewMap OverviewMap = null;
        protected Grid TemplateGrid = null;

        private int nRows = 1;
        private int nCols = 1;

        private bool isMapResizing = false;
        private bool isProgressing = false;
        private DispatcherTimer loadTimer = null;
        private Size pageExtentSize = Size.Empty;
        private MapPoint topLeft = null;

        /// <summary>
        /// Event occurs when map layers for a page are loaded
        /// </summary>
        public event EventHandler<EventArgs> MapReady;

        #region Initialize Control
        public PrintMapPage()
        {
            this.DefaultStyleKey = typeof(PrintMapPage);
        }

        public override void OnApplyTemplate()
        {
            this.Width = DEFAULT_PAPER_WIDTH;
            this.Height = DEFAULT_PAPER_HEIGHT;

            loadTimer = new DispatcherTimer();
            loadTimer.Tick += new EventHandler(MapLoadTimer_Tick);

            PrintMap = this.GetTemplateChild(_PRINT_MAP) as Map;
            LegendGrid = this.GetTemplateChild(_LEGEND_GRID) as Grid;
            TitleBlock = this.GetTemplateChild(_TITLE_BLOCK) as TextBlock;
            LegendPanel = this.GetTemplateChild(_LEGEND_PANEL) as StackPanel;
            OverviewMap = this.GetTemplateChild(_OVERVIEW_MAP) as OverviewMap;
            TemplateGrid = this.GetTemplateChild(_TEMPLATE_GRID) as Grid;

            if (PrintMap != null)
            {
                this.CloneMap();
            }

            if (LegendPanel != null)
            {
                this.CreateLegend();
            }

            if (OverviewMap != null && OverviewMapLayer != null)
            {
                this.OverviewMap.Layer = OverviewMapLayer.Clone();
            }
        }
        #endregion

        #region Normal Property
        public bool IsMapResizing
        {
            get { return isMapResizing; }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty ViewerMapProperty = DependencyProperty.Register("ViewerMap", typeof(Map), typeof(PrintMapPage), new PropertyMetadata(null, new PropertyChangedCallback(OnViewerMapChanged)));
        public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register("PageTitle", typeof(string), typeof(PrintMapPage), new PropertyMetadata("", new PropertyChangedCallback(OnPageTitleChanged)));
        public static readonly DependencyProperty RotateMapProperty = DependencyProperty.Register("RotateMap", typeof(bool), typeof(PrintMapPage), new PropertyMetadata(false, new PropertyChangedCallback(OnRotateMapChanged)));
        public static readonly DependencyProperty PrintLegendProperty = DependencyProperty.Register("PrintLegend", typeof(bool), typeof(PrintMapPage), new PropertyMetadata(true, new PropertyChangedCallback(OnPrintLegendChanged)));
        public static readonly DependencyProperty FitIntoPageProperty = DependencyProperty.Register("FitIntoPage", typeof(bool), typeof(PrintMapPage), new PropertyMetadata(true, new PropertyChangedCallback(OnFitIntoPageChanged)));
        public static readonly DependencyProperty MaintainScaleProperty = DependencyProperty.Register("MaintainScale", typeof(bool), typeof(PrintMapPage), new PropertyMetadata(true, new PropertyChangedCallback(OnMaintainScaleChanged)));
        public static readonly DependencyProperty PrintOverviewProperty = DependencyProperty.Register("PrintOverview", typeof(bool), typeof(PrintMapPage), new PropertyMetadata(false, new PropertyChangedCallback(OnPrintOverviewChanged)));
        public static readonly DependencyProperty OverviewMapLayerProperty = DependencyProperty.Register("OverviewMapLayer", typeof(Layer), typeof(PrintMapPage), new PropertyMetadata(null, new PropertyChangedCallback(OnOverviewMapLayerChanged)));
        public static readonly DependencyProperty LegendItemsProperty = DependencyProperty.Register("LegendItems", typeof(List<LegendItemInfo>), typeof(PrintMapPage), new PropertyMetadata(null, new PropertyChangedCallback(OnLegendItemsChanged)));

        /// <summary>
        /// The Map of the Viewer to be printed
        /// </summary>
        public Map ViewerMap
        {
            get { return (Map)GetValue(ViewerMapProperty); }
            set { SetValue(ViewerMapProperty, value); }
        }

        /// <summary>
        /// A title for the Map-Print page 
        /// </summary>
        public string PageTitle
        {
            get { return (string)GetValue(PageTitleProperty); }
            set { SetValue(PageTitleProperty, value); }
        }

        public bool RotateMap
        {
            get { return (bool)GetValue(RotateMapProperty); }
            set { SetValue(RotateMapProperty, value); }
        }

        public bool PrintLegend
        {
            get { return (bool)GetValue(PrintLegendProperty); }
            set { SetValue(PrintLegendProperty, value); }
        }

        public bool FitIntoPage
        {
            get { return (bool)GetValue(FitIntoPageProperty); }
            set { SetValue(FitIntoPageProperty, value); }
        }

        public bool MaintainScale
        {
            get { return (bool)GetValue(MaintainScaleProperty); }
            set { SetValue(MaintainScaleProperty, value); }
        }

        public bool PrintOverview
        {
            get { return (bool)GetValue(PrintOverviewProperty); }
            set { SetValue(PrintOverviewProperty, value); }
        }

        public Layer OverviewMapLayer
        {
            get { return (Layer)GetValue(OverviewMapLayerProperty); }
            set { SetValue(OverviewMapLayerProperty, value); }
        }

        public List<LegendItemInfo> LegendItems
        {
            get { return (List<LegendItemInfo>)GetValue(LegendItemsProperty); }
            set { SetValue(LegendItemsProperty, value); }
        }

        private static void OnViewerMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.PrintMap != null)
            {
                me.CloneMap();
            }
        }

        private static void OnPageTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.TitleBlock != null)
            {
                me.TitleBlock.Text = (string)e.NewValue;
            }
        }

        private static void OnRotateMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.PrintMap != null)
            {
                try // Work around the bug in API 2.2 - Crash if OverviewMap.Visibility = Collapsed
                {
                    me.PrintMap.Rotation = ((bool)e.NewValue) ? -90.0 : 0.0;
                }
                catch { }
            }
        }

        private static void OnPrintLegendChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.LegendGrid != null)
            {
                me.LegendGrid.Visibility = ((bool)e.NewValue) ? Visibility.Visible : Visibility.Collapsed;
                me.UpdateMapSize(true);
            }
        }

        private static void OnFitIntoPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.PrintMap != null && me.ViewerMap != null)
            {
                if ((bool)e.NewValue)
                    me.ScaleMapIntoPage();
                else
                    me.PrintMap.Extent = me.ViewerMap.Extent;
            }
        }

        private static void OnMaintainScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.PrintMap != null && me.ViewerMap != null)
            {
                if (me.FitIntoPage)
                    me.ScaleMapIntoPage();
                else
                    me.PrintMap.Extent = me.ViewerMap.Extent;
            }
        }

        private static void OnPrintOverviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.OverviewMap != null)
            {
                me.OverviewMap.Visibility = ((bool)e.NewValue) ? Visibility.Visible : Visibility.Collapsed;
                me.CreateLegend(); // Re-arrange Legend Items 
            }
        }

        private static void OnOverviewMapLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.OverviewMap != null)
            {
                me.OverviewMap.Layer = (e.NewValue as Layer).Clone();
            }
        }

        private static void OnLegendItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PrintMapPage me = d as PrintMapPage;
            if (me != null && me.LegendPanel != null)
            {
                me.CreateLegend();
            }
        }
        #endregion

        #region Prepare Printable Pages
        /// <summary>
        /// Split map into multiple printable pages
        /// </summary>
        /// <param name="printableArea"></param>
        internal int PreparePages(Size printableArea)
        {
            double elemHeight = MeasureElementHeight();

            // The Viewer Map Actual Size
            Size neededSize = (RotateMap) ?
                (new Size(this.ViewerMap.ActualHeight, this.ViewerMap.ActualWidth + elemHeight)) :
                (new Size(this.ViewerMap.ActualWidth, this.ViewerMap.ActualHeight + elemHeight));

            // Resize Page based on Paper Size
            this.Width = (printableArea.Width > neededSize.Width) ? neededSize.Width : printableArea.Width;
            this.Height = (printableArea.Height > neededSize.Height) ? neededSize.Height : printableArea.Height;

            UpdateMapSize(false); // Extent will be set later

            nRows = 1;
            nCols = 1;

            if (!FitIntoPage)
            {
                Size mapSize = (RotateMap) ?
                               (new Size(this.Height - elemHeight, this.Width)) :
                               (new Size(this.Width, this.Height - elemHeight));

                // Ignore size difference less than 4.0
                nCols = (int)Math.Ceiling(ViewerMap.ActualWidth / (mapSize.Width + 4.0));
                nRows = (int)Math.Ceiling(ViewerMap.ActualHeight / (mapSize.Height + 4.0));

                double printExtentWidth = nCols * mapSize.Width * this.ViewerMap.Resolution;
                double printExtentHeight = nRows * mapSize.Height * this.ViewerMap.Resolution;

                MapPoint center = this.ViewerMap.Extent.GetCenter();
                topLeft = new MapPoint(center.X - printExtentWidth / 2.0, center.Y + printExtentHeight / 2.0);
                pageExtentSize = new Size(printExtentWidth / nCols, printExtentHeight / nRows);
            }

            return nRows * nCols;
        }

        /// <summary>
        /// Sets the print map extent to the extent of a page
        /// </summary>
        /// <param name="page">The page.</param>
        internal void SetPrintExtent(int page)
        {
            if (this.FitIntoPage)
            {
                ScaleMapIntoPage();
                WaitForLoaded();
            }
            else if (nCols > 0)
            {
                int row = (page - 1) / nCols;
                int col = (page - 1) % nCols;
                double x = topLeft.X + col * pageExtentSize.Width;
                double y = topLeft.Y - row * pageExtentSize.Height;
                PrintMap.Extent = new Envelope(x, y - pageExtentSize.Height, x + pageExtentSize.Width, y);
                //Debug.WriteLine(string.Format("Page {0} Extent: {1}-{2}, {3}-{4}", page, x, x + pageExtentSize.Width, y - pageExtentSize.Height, y));
                WaitForLoaded();
            }
        }
        #endregion

        #region Fit/Scale Print Map into One Page
        private void ScaleMapIntoPage()
        {
            if (MaintainScale)
            {
                double elemHeight = MeasureElementHeight();

                Size mapSize = (RotateMap) ?
                               (new Size(this.Height - elemHeight, this.Width)) :
                               (new Size(this.Width, this.Height - elemHeight));

                if (ViewerMap.Extent != null)
                {
                    MapPoint center = ViewerMap.Extent.GetCenter();
                    double extW2 = ViewerMap.Extent.Width * mapSize.Width / ViewerMap.ActualWidth / 2.0;
                    double extH2 = ViewerMap.Extent.Height * mapSize.Height / ViewerMap.ActualHeight / 2.0;
                    PrintMap.Extent = new Envelope(center.X - extW2, center.Y - extH2, center.X + extW2, center.Y + extH2);
                }
            }
            else
            {
                PrintMap.Extent = ViewerMap.Extent;
            }
        }
        #endregion

        #region Map Load Timer to Send Map-Ready Message
        /// <summary>
        /// Security timer to avoid infinite waiting
        /// </summary>
        private void MapLoadTimer_Tick(object sender, EventArgs e)
        {
            if (isProgressing)
            {
                // Keep waiting 30 seconds
                isMapResizing = false;
                isProgressing = false;
                loadTimer.Interval = new TimeSpan(0, 0, 30);
            }
            else
            {
                OnMapReady();
            }
        }

        private void Map_Progress(object sender, ProgressEventArgs e)
        {
            isProgressing = true;

            if (e.Progress == 100)
            {
                OnMapReady(); // map is ready
            }
        }

        private void OnMapReady()
        {
            CancelWait();

            if (MapReady != null)
            {
                MapReady(this, new EventArgs());
            }
        }

        internal void WaitForLoaded()
        {
            // Wait for map loaded
            PrintMap.Progress += Map_Progress;
            if (loadTimer.IsEnabled) loadTimer.Stop();
            loadTimer.Interval = new TimeSpan(0, 0, 10);
            loadTimer.Start();
        }

        /// <summary>
        /// Cancels the wait.
        /// </summary>
        internal void CancelWait()
        {
            loadTimer.Stop();
            PrintMap.Progress -= Map_Progress;
            isProgressing = false;
            isMapResizing = false;
        }
        #endregion

        #region Clone Map and Layers for Printing
        /// <summary>
        /// Clone Map
        /// </summary>
        internal void CloneMap()
        {
            if (ViewerMap != null)
            {
                PrintMap.Layers.Clear();
                PrintMap.MinimumResolution = ViewerMap.MinimumResolution;
                PrintMap.MaximumResolution = ViewerMap.MaximumResolution;

                foreach (Layer layer in ViewerMap.Layers)
                {
                    if (layer.Visible)
                    {
                        Layer clonedLayer = CloneLayer(layer);
                        clonedLayer.Initialized += (o, e) => { };
                        PrintMap.Layers.Add(clonedLayer);
                    }
                }

                GraphicsLayer frameGraphic = new GraphicsLayer();
                SimpleFillSymbol frameSymbol = new SimpleFillSymbol() { BorderBrush = new SolidColorBrush(Colors.Red), BorderThickness = 2, Fill = new SolidColorBrush(Colors.Transparent) };
                frameGraphic.Graphics.Add(new Graphic() { Geometry = ViewerMap.Extent, Symbol = frameSymbol });
                PrintMap.Layers.Add(frameGraphic);
            }
        }

        /// <summary>
        /// Clone Map Layers
        /// </summary>
        private Layer CloneLayer(Layer layer)
        {
            Layer toLayer = null;

            if (layer is GraphicsLayer) // Include FeatureLayer and GeoRSS layers
            {
                GraphicsLayer fromLayer = layer as GraphicsLayer;
                GraphicsLayer printLayer = new GraphicsLayer();

                printLayer.Renderer = fromLayer.Renderer;
                printLayer.Clusterer = fromLayer.Clusterer; // todo : clone ?

                GraphicCollection graphicCollection = new GraphicCollection();

                foreach (Graphic graphic in fromLayer.Graphics)
                {
                    Graphic clone = new Graphic();

                    foreach (var kvp in graphic.Attributes)
                    {
                        clone.Attributes.Add(kvp);
                    }

                    clone.Geometry = graphic.Geometry;
                    clone.Symbol = graphic.Symbol;
                    clone.Selected = graphic.Selected;
                    clone.TimeExtent = graphic.TimeExtent;
                    graphicCollection.Add(clone);
                }

                printLayer.Graphics = graphicCollection;

                toLayer = printLayer;
                toLayer.ID = layer.ID;
                toLayer.Opacity = layer.Opacity;
                toLayer.Visible = layer.Visible;
                toLayer.MaximumResolution = layer.MaximumResolution;
                toLayer.MinimumResolution = layer.MinimumResolution;
            }
            else
            {
                // Clone other layer types
                toLayer = layer.Clone();
            }

            return toLayer;
        }
        #endregion

        #region Create Map Element - Legend
        private void CreateLegend()
        {
            LegendPanel.Children.Clear();

            if (LegendItems != null && LegendItems.Count > 0)
            {
                int columns = (LegendItems.Count > 2) ? ((this.PrintOverview) ? 2 : 3) : 1;
                int nPerCol = (int)Math.Round((double)LegendItems.Count / columns);

                List<LegendItemInfo> columnItems = null;
                DataTemplate itemTemplate = LegendPanel.Resources["SymbolTreeNode"] as DataTemplate;

                for (int i = 0; i < LegendItems.Count; i++)
                {
                    if (i % nPerCol == 0)
                    {
                        ListBox listBox = new ListBox()
                        {
                            ItemTemplate = itemTemplate,
                            Margin = new Thickness(4, 0, 4, 0),
                            BorderBrush = new SolidColorBrush(Colors.Transparent),
                            VerticalAlignment = System.Windows.VerticalAlignment.Top
                        };

                        ScrollViewer.SetVerticalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);
                        columnItems = new List<LegendItemInfo>();
                        listBox.ItemsSource = columnItems;
                        LegendPanel.Children.Add(listBox);
                    }

                    columnItems.Add(LegendItems[i]);
                }
            }

            UpdateMapSize(true);
        }
        #endregion

        #region Update Print Map Size
        private void UpdateMapSize(bool setExtent)
        {
            WaitForLoaded();
            isMapResizing = true;

            double mapHeight = this.Height - MeasureElementHeight();
            TemplateGrid.RowDefinitions[1].Height = new GridLength(mapHeight);

            this.PrintMap.Height = mapHeight;
            this.PrintMap.Width = this.Width;

            if (setExtent && ViewerMap != null)
            {
                if (this.FitIntoPage)
                    ScaleMapIntoPage();
                else
                    PrintMap.Extent = ViewerMap.Extent;
            }
        }

        private double MeasureElementHeight()
        {
            double legHeight = 200.0;
            double ovwHeight = (PrintOverview) ? 200.0 : 0.0;

            if (PrintLegend)
            {
                LegendPanel.Measure(new Size(this.Width, this.Height));
                legHeight = LegendPanel.DesiredSize.Height + 36; // 36 - The height of the Legend title
            }

            return Math.Max(legHeight, ovwHeight) + 40.0; // 40 - Space for the Print Page Title
        }
        #endregion
    }
}
