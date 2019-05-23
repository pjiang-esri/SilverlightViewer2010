using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

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

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class LocatorWidget : WidgetBase
    {
        private GeocodeTool geocodeTool = null;
        private LocatorConfig widgetConfig = null;

        public LocatorWidget()
        {
            InitializeComponent();

            this.GraphicsTipTemplate = @"Title={#Title};Content={#Address}";
            this.DrawObject.DrawComplete += new EventHandler<DrawEventArgs>(DrawObject_DrawComplete);
        }

        #region Override Function - Load Configuration and Clear Graphics
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (LocatorConfig)LocatorConfig.Deserialize(xmlConfig, typeof(LocatorConfig));

            if (widgetConfig.EnableLocator == ServiceSource.BING)
            {
                if (string.IsNullOrEmpty(widgetConfig.BingLocator.Token))
                {
                    widgetConfig.BingLocator.Token = CurrentApp.GetAppParamValue(MapApp.BING_TOKEN);
                    widgetConfig.BingLocator.ServerType = (ServerType)Enum.Parse(typeof(ServerType), CurrentApp.GetAppParamValue(MapApp.BING_SERVER), true);
                }
            }

            geocodeTool = new GeocodeTool(widgetConfig, this.AppConfig.GeometryService, this.MapSRWKID);
            geocodeTool.ResultReady += new GeocodeTool.GeocodeResultReady(GeocodeUtil_ResultReady);
        }

        public override void ClearGraphics(int newTab)
        {
            base.ClearGraphics(newTab);
            this.listGeoocodeResults.ItemsSource = null;
        }

        protected override void OnClose()
        {
            this.ClearGraphics(0);
            base.OnClose();
        }
        #endregion

        #region Override ResetDrawObjectMode - Enable DrawObject to Click Point Mode
        public override void ResetDrawObjectMode()
        {
            // Enable DrawObject to enable reverse geocoding a click point on the map 
            if (locatorRadio_Coords.IsChecked.Value)
            {
                this.DrawWidget = this.GetType();
                this.DrawObject.IsEnabled = true;
                this.DrawObject.DrawMode = DrawMode.Point;
                this.MapControl.Cursor = Cursors.Arrow;
            }
            else
            {
                WidgetManager.ResetDrawObject();
            }
        }
        #endregion

        #region Handle DrawObject DrawComplete Event to Start Reverse Geocoding
        private void DrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (this.DrawWidget == this.GetType())
            {
                MapPoint point = e.Geometry as MapPoint;
                geocodeTool.ReverseGeocode(point);
            }
        }
        #endregion

        #region Geocode Utility Event Handlers
        private void LocatorRadio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton)
            {
                gridAddressLocator.Visibility = (locatorRadio_Address.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
                gridCoordsLocator.Visibility = (locatorRadio_Coords.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;

                this.ResetDrawObjectMode();
            }
        }

        private void LocatorButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsBusy = true; // Make Busy Signal Visible

            if (locatorRadio_Address.IsChecked.Value && !string.IsNullOrEmpty(txtAddress.Text))
            {
                //AddressHelper addressParser = new AddressHelper(txtAddress.Text);
                //geocodeTool.GeocodeAddress(addressParser.Street1, addressParser.City, addressParser.State, addressParser.ZipCode, addressParser.Country);
                //With Single Line Address Input of ArcGIS Locator Service 10.0 or later, no need to use AddressHelper anymore
                geocodeTool.GeocodeAddress(txtAddress.Text);
            }
            else if (locatorRadio_Coords.IsChecked.Value && !string.IsNullOrEmpty(txtLongitude.Text) && !string.IsNullOrEmpty(txtLatitude.Text))
            {
                double lon = double.Parse(txtLongitude.Text);
                double lat = double.Parse(txtLatitude.Text);
                MapPoint point = new MapPoint(lon, lat, new SpatialReference(4326));
                geocodeTool.ReverseGeocode(point);
            }
        }

        private void GeocodeUtil_ResultReady(object sender, GeocodeResultEventArgs args)
        {
            List<LocationCandidate> candidates = args.GeocodeResults;
            if (candidates.Count > 0)
            {
                this.SelectedGraphic = null;
                this.ClearGraphics(0);

                Dictionary<string, object> attributes = null;
                textBlockErrorMsg.Visibility = Visibility.Collapsed;
                listGeoocodeResults.Visibility = Visibility.Visible;

                foreach (LocationCandidate candidate in candidates)
                {
                    attributes = new Dictionary<string, object>();
                    attributes.Add("Title", "Location");
                    attributes.Add("Address", candidate.Address);
                    AddLocationGraphic(candidate.Location, attributes);
                }

                listGeoocodeResults.ItemsSource = candidates;
                HighlightSelectedGraphic(candidates[0].Location, candidates[0].Address);
                listGeoocodeResults.SelectedIndex = 0;
                this.ZoomToGraphics();

                // Show Geocode Results in the Results Tab
                ToggleWidgetContent(1);
            }
            else
            {
                textBlockErrorMsg.Visibility = Visibility.Visible;
                listGeoocodeResults.Visibility = Visibility.Collapsed;
                textBlockErrorMsg.Text = "Sorry! Your geocoding request failed.\n" + args.ErrorMessage;
                ToggleWidgetContent(1);
            }

            this.IsBusy = false; // Make Busy Signal Invisible
        }
        #endregion

        #region Create Geocode Location Graphics
        public void AddLocationGraphic(ESRI.ArcGIS.Client.Geometry.MapPoint location, Dictionary<string, object> attributes)
        {
            if (this.GraphicsLayer == null)
            {
                MessageBox.Show("GraphicsLayer for this widget is not created. Please set property HasGraphics 'true'."); return;
            }

            Graphic graphic = new Graphic();
            graphic.Geometry = location;
            graphic.Symbol = CreateMarkerSymbol(widgetConfig.LocationSymbol.ImageSource, widgetConfig.LocationSymbol.OffsetX, widgetConfig.LocationSymbol.OffsetY);
            foreach (string key in attributes.Keys)
            {
                graphic.Attributes.Add(key, attributes[key]);
            }

            this.GraphicsLayer.Graphics.Add(graphic);
        }

        private PictureMarkerSymbol CreateMarkerSymbol(string symbolUri, double offsetX, double offsetY)
        {
            if (!string.IsNullOrEmpty(symbolUri))
            {
                PictureMarkerSymbol pictureSymbol = new PictureMarkerSymbol();
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

        #region Highlight/Zoom To Location Graphic
        private void AddressCandidate_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock)
            {
                TextBlock block = sender as TextBlock;

                if (block.Tag != null && block.Tag is MapPoint)
                {
                    MapPoint location = (MapPoint)block.Tag;
                    HighlightSelectedGraphic(location, block.Text);
                    this.MapControl.ZoomTo(GeometryTool.ExpandGeometryExtent(this.SelectedGraphic.Geometry, 0.5));
                }
            }
        }

        public void HighlightSelectedGraphic(MapPoint location, string address)
        {
            if (this.SelectedGraphic != null && this.GraphicsLayer.Graphics.Contains(this.SelectedGraphic))
            {
                this.GraphicsLayer.Graphics.Remove(this.SelectedGraphic);
            }

            this.SelectedGraphic = new Graphic();
            this.SelectedGraphic.Geometry = location;
            this.SelectedGraphic.Symbol = this.CurrentApp.Resources[SymbolResources.SELECTED_MARKER] as MarkerSymbol;
            this.SelectedGraphic.Attributes["Title"] = "Location";
            this.SelectedGraphic.Attributes["Address"] = address;
            this.SelectedGraphic.Selected = true;
            this.GraphicsLayer.Graphics.Add(this.SelectedGraphic);
        }
        #endregion
    }
}
