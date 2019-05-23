using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Browser;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Bing.GeocodeService;
using ESRI.SilverlightViewer.Controls;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Config;
using StreetViewWidget.Config;

namespace StreetViewWidget
{
    public partial class MainPage : WidgetBase
    {
        StreetViewConfig widgetConfig = null;
        GeometryService geoService = null;

        public MainPage()
        {
            InitializeComponent();

            this.DrawObject.DrawComplete += new EventHandler<DrawEventArgs>(DrawObject_DrawComplete);
        }

        #region Override Function - Load Configuration and Clear Graphics
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (StreetViewConfig)StreetViewConfig.Deserialize(xmlConfig, typeof(StreetViewConfig));

            if (widgetConfig != null)
            {
                if (this.CurrentApp.IsWindowless) InsertJavaScriptObject();
            }
        }

        protected override void OnClose()
        {
            this.ClearGraphics(-1);
            base.OnClose();
        }
        #endregion

        #region Add JavaScript and StyleSheet used for StreetView Window
        /// <summary>
        /// Insert JavaScript Window Object into the Page
        /// </summary>
        private void InsertJavaScriptObject()
        {
            if ("Javascript".Equals(widgetConfig.DisplayWindow, StringComparison.CurrentCultureIgnoreCase))
            {
                HtmlElement pageHead = HtmlPage.Document.GetElementsByTagName("head")[0] as HtmlElement;

                if (!PageScriptHelper.IsJavaScriptWindowAdded())
                {
                    HtmlElement scriptSource = HtmlPage.Document.CreateElement("Script");
                    scriptSource.SetAttribute("type", "text/javascript");
                    scriptSource.SetAttribute("src", "Window.js");
                    pageHead.AppendChild(scriptSource);

                    HtmlElement cssFileRef = HtmlPage.Document.CreateElement("link");
                    cssFileRef.SetAttribute("rel", "stylesheet");
                    cssFileRef.SetAttribute("type", "text/css");
                    cssFileRef.SetAttribute("href", "Window.css");
                    pageHead.AppendChild(cssFileRef);
                }

                HtmlElement scriptBlock = HtmlPage.Document.CreateElement("Script");
                scriptBlock.SetAttribute("type", "text/javascript");
                scriptBlock.SetProperty("text", CreatePageScript());
                pageHead.AppendChild(scriptBlock);
            }
        }
        /// <summary>
        /// The script function will be invoked by the widget
        /// </summary>
        private string CreatePageScript()
        {
            string script = @"var streetViewWindow = null;
                function openStreetViewWindow(svUrl, width, height) {
                    if (streetViewWindow == null) {
                        var container = document.getElementById('silverlightControlHost');
                        streetViewWindow = new EsriFloatingWindow('winStreetView', 'Street View', container, width, height);
                        streetViewWindow.center(); 
                    }
                    else if (streetViewWindow.closed) {
                        streetViewWindow.toggleVisibility(); 
                    }

                    streetViewWindow.setContentSource(svUrl); 
                }";

            return script;
        }
        #endregion

        #region Override ResetDrawObjectMode - Enable DrawObject to Click Point Mode
        /// <summary>
        /// Implement IDrawObject
        /// </summary>
        public override void ResetDrawObjectMode()
        {
            // Enable DrawObject to enable reverse geocoding a click point on the map 
            this.DrawWidget = this.GetType();
            this.DrawObject.IsEnabled = true;
            this.DrawObject.DrawMode = DrawMode.Point;
            this.MapControl.Cursor = Cursors.Hand;
        }
        #endregion

        #region Handle DrawObject DrawComplete Event and Open Street View
        private void DrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (this.DrawWidget == this.GetType())
            {
                string viewURL = "";
                MapPoint point = e.Geometry as MapPoint;
                AddLocationGraphic(point);

                if (GeometryTool.IsGeographicSR(this.MapSRWKID))
                {
                    viewURL = GetViewContentURL(point.X, point.Y);
                    OpenSteetViewWindow(viewURL);
                }
                if (GeometryTool.IsWebMercatorSR(this.MapSRWKID))
                {
                    point = point.WebMercatorToGeographic();
                    viewURL = GetViewContentURL(point.X, point.Y);
                    OpenSteetViewWindow(viewURL);
                }
                else
                {
                    if (geoService == null)
                    {
                        geoService = new GeometryService(this.AppConfig.GeometryService);
                        geoService.ProjectCompleted += new EventHandler<GraphicsEventArgs>(GeometryService_ProjectCompleted);
                    }

                    geoService.ProjectAsync(this.GraphicsLayer.Graphics, new SpatialReference(2446));
                }
            }
        }
        #endregion

        #region Handle GeometryService_ProjectCompleted Event and Open Street View
        private void GeometryService_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            Graphic graphic = e.Results[0] as Graphic;
            MapPoint point = graphic.Geometry as MapPoint;
            string viewURL = GetViewContentURL(point.X, point.Y);
            OpenSteetViewWindow(viewURL);
        }
        #endregion

        #region Construct Street View Content URL and Open Street View Window
        private string GetViewContentURL(double x, double y)
        {
            string viewURL = "";
            StreetViewContent viewContent = (StreetViewContent)Enum.Parse(typeof(StreetViewContent), (comboxViewContent.SelectedItem as ComboBoxItem).Tag as string, true);

            switch (viewContent)
            {
                case StreetViewContent.DualViewer: // Dual View
                    viewURL = "http://data.mapchannels.com/mm/dual2/map.htm?x=" + x + "&y=" + y + "&z=16&gm=0&ve=3&gc=0&xb=" + x + "&yb=" + y + "&zb=1&db=0&bar=0&mw=1&sv=1&svb=0&mi=0&mg=1&mv=1%20marginwidth=0%20marginheight=0%20frameborder=0%20scrolling=no";
                    break;
                case StreetViewContent.BingMaps: // BING Oblique
                    viewURL = "http://www.bing.com/maps/?v=2&cp=" + y + "~" + x + "&lvl=15&sty=b";
                    break;
                case StreetViewContent.GoogleMap: // Google Map
                    viewURL = "http://data.mapchannels.com/locationmap/100/map.htm?mx=" + x + "&my=" + y + "&mz=16&mt=2&dm=0&mw=250&tc=2&mn=3";
                    break;
                case StreetViewContent.StreetViewOnly: // Street View Only
                    viewURL = "http://data.mapchannels.com/locationmap/100/map.htm?mx=" + x + "&my=" + y + "&mz=15&mt=2&dm=1&mw=250&tc=2&mn=3";
                    break;
                case StreetViewContent.StreetViewGoogleMap: // Street View & Google Map Vertical
                    viewURL = "http://data.mapchannels.com/locationmap/100/map.htm?mx=" + x + "&my=" + y + "&mz=15&mt=2&dm=3&mw=250&tc=2&mn=3";
                    break;
            }

            return viewURL;
        }

        public void OpenSteetViewWindow(string svUrl)
        {
            if (this.CurrentApp.IsWindowless && "Javascript".Equals(widgetConfig.DisplayWindow, StringComparison.CurrentCultureIgnoreCase))
            {
                HtmlPage.Window.Invoke("openStreetViewWindow", new object[] { svUrl, widgetConfig.WindowWidth, widgetConfig.WindowHeight });
            }
            else
            {
                HtmlPopupWindowOptions options = new HtmlPopupWindowOptions() { Resizeable = true, Menubar = false, Scrollbars = false, Status = false, Toolbar = false, Width = widgetConfig.WindowWidth, Height = widgetConfig.WindowHeight, Top = 40, Left = 100 };
                HtmlPage.PopupWindow(new Uri(svUrl), "_blank", options);
            }
        }
        #endregion

        #region Create Geocode Location Graphics
        public void AddLocationGraphic(ESRI.ArcGIS.Client.Geometry.MapPoint location)
        {
            if (this.GraphicsLayer == null)
            {
                MessageBox.Show("GraphicsLayer for this widget is not created. Please set property HasGraphics 'true'."); return;
            }

            this.ClearGraphics(-1);
            Graphic graphic = new Graphic();
            graphic.Geometry = location;
            graphic.Symbol = CreateMarkerSymbol(widgetConfig.GraphicIcon, 0, 24);
            this.GraphicsLayer.Graphics.Add(graphic);
        }

        private PictureMarkerSymbol CreateMarkerSymbol(string symbolUri, double offsetX, double offsetY)
        {
            if (!string.IsNullOrEmpty(symbolUri))
            {
                PictureMarkerSymbol pictureSymbol = new PictureMarkerSymbol() { Width = 28, Height = 28 };
                pictureSymbol.OffsetX = offsetX;
                pictureSymbol.OffsetY = offsetY;

                if (symbolUri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                {
                    pictureSymbol.Source = new BitmapImage(new Uri(symbolUri, UriKind.Absolute));
                }
                else
                {
                    pictureSymbol.Source = new BitmapImage(new Uri(symbolUri, UriKind.Relative));
                }

                return pictureSymbol;
            }

            return null;
        }
        #endregion
    }

    public enum StreetViewContent
    {
        DualViewer = 0,
        BingMaps = 1,
        GoogleMap = 2,
        StreetViewOnly = 3,
        StreetViewGoogleMap = 4
    }
}