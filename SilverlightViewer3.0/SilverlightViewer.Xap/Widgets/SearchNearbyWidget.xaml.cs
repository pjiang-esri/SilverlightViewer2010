using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class SearchNearbyWidget : WidgetBase
    {
        private SearchNearbyConfig widgetConfig = null;
        private GeometryService geometryService = null;
        private QueryTool queryTool = null;

        // Used to suppress panning Map to the selection
        // or suppress refreshing the view of query results 
        // when query results are binded at the first time
        private bool IsFirstLoad = true;

        public SearchNearbyWidget()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Configuration and Clear Graphics
        protected override void OnWidgetLoaded()
        {
            base.OnWidgetLoaded();

            LoadWidgetWithSelection();
            this.SearchResultGrid.DataSources = this.FeatureSets;

            queryTool = new QueryTool();
            queryTool.ResultReady += new QueryTool.QueryResultReady(Query_ResultReady);

            EventCenter.WidgetSelectedGraphicChange += new WidgetSelectedGraphicChangeEventHandler(OnWidgetSelectedGraphicChange);
        }

        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (SearchNearbyConfig)SearchNearbyConfig.Deserialize(xmlConfig, typeof(SearchNearbyConfig));

            if (widgetConfig != null && widgetConfig.SearchLayers != null)
            {
                ArcGISQueryLayer[] searchLayers = widgetConfig.SearchLayers;

                for (int i = 0; i < searchLayers.Length; i++)
                {
                    ArcGISLayerInfo layerInfo = new ArcGISLayerInfo(searchLayers[i].RESTURL);
                    lstSearchLayer.Items.Add(new ComboBoxItem() { Content = searchLayers[i].Title, Tag = layerInfo });
                }

                geometryService = new GeometryService(this.AppConfig.GeometryService);
                geometryService.BufferCompleted += new EventHandler<GraphicsEventArgs>(GeometryService_BufferCompleted);
            }
        }

        protected override void OnClose()
        {
            this.ClearGraphics(0);
            base.OnClose();
        }
        #endregion

        #region Listen to the SelectedGraphicChange event of other widgets
        private void OnWidgetSelectedGraphicChange(object sender, SelectedItemChangeEventArgs args)
        {
            if (sender != this)
            {
                LoadWidgetWithSelection();
            }
        }

        private void LoadWidgetWithSelection()
        {
            lstGraphicWidget.Items.Clear();

            foreach (WidgetBase widget in WidgetManager.Widgets)
            {
                if (widget.Title != this.Title && widget.HasGraphics && widget.SelectedGraphic != null)
                {
                    lstGraphicWidget.Items.Add(widget.Title);
                }
            }
        }
        #endregion

        #region Prepare Search Parameters, Create Buffer Zone and Start Search
        private void SubmitSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (lstSearchLayer.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a search layer", "Message", MessageBoxButton.OK);
                return;
            }

            if (lstGraphicWidget.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a widget with selected graphic", "Message", MessageBoxButton.OK);
                return;
            }

            double bufferDistance = 0;
            if (!double.TryParse(txtBufferDistance.Text, out bufferDistance))
            {
                MessageBox.Show("Buffer Distance is invalid. Please input a valid number", "Message", MessageBoxButton.OK);
                return;
            }

            this.IsBusy = true;
            Graphic centerGraphic = WidgetManager.GetWidgetSelectedGraphic((string)lstGraphicWidget.SelectedItem);

            if (bufferDistance > 0)
            {
                string bufUnits = (string)(lstBufferUnits.SelectedItem as ComboBoxItem).Content;
                CreateBufferZone(bufferDistance, centerGraphic, bufUnits);
            }
            else
            {
                this.ClearGraphics(1); // Toggle to Results Panel
                StartSearch(centerGraphic.Geometry);
            }
        }

        private void CreateBufferZone(double bufferDistance, Graphic graphic, string units)
        {
            if (geometryService != null)
            {
                int bufferSRWKID = widgetConfig.ProjectToWKID;
                BufferParameters bufParams = new BufferParameters();
                bufParams.BufferSpatialReference = new SpatialReference((bufferSRWKID == 0) ? 102003 : bufferSRWKID);
                bufParams.OutSpatialReference = new SpatialReference(this.MapSRWKID);
                bufParams.Distances.Add(bufferDistance);
                bufParams.UnionResults = true;

                // Make the geometry's Spatial Reference consistent with Map's
                // QueryTask returns 3857 instead of 102100 when BING layers are used in the base map 
                graphic.Geometry.SpatialReference = new SpatialReference(this.MapSRWKID);

                if (RadioBufferCenter.IsChecked.Value && !(graphic.Geometry is MapPoint))
                {
                    MapPoint center = graphic.Geometry.Extent.GetCenter();
                    center.SpatialReference = new SpatialReference(this.MapSRWKID);
                    Graphic graphic1 = new Graphic();
                    graphic1.Geometry = center;

                    bufParams.Features.Add(graphic1);
                }
                else
                {
                    bufParams.Features.Add(graphic);
                }

                switch (units)
                {
                    case "Feet": bufParams.Unit = LinearUnit.Foot; break;
                    case "Yards": bufParams.Unit = LinearUnit.SurveyYard; break;
                    case "Miles": bufParams.Unit = LinearUnit.StatuteMile; break;
                    case "Meters": bufParams.Unit = LinearUnit.Meter; break;
                    default: bufParams.Unit = LinearUnit.Foot; break;
                }

                geometryService.BufferAsync(bufParams);
            }
            else
            {
                MessageBox.Show("Geometry Service must be configurated to create a buffer zone.");
            }
        }

        private void GeometryService_BufferCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.Results.Count > 0)
            {
                this.ClearGraphics(1); // Toggle to Results Panel

                Graphic graphic = e.Results[0];
                graphic.Symbol = this.CurrentApp.Resources[SymbolResources.SIMPLE_FILL] as SimpleFillSymbol;
                this.AddGraphic(graphic);

                StartSearch(graphic.Geometry);
            }
        }

        private void StartSearch(Geometry geometry)
        {
            if (widgetConfig.SearchLayers != null)
            {
                queryTool.QueryLayer = widgetConfig.SearchLayers[lstSearchLayer.SelectedIndex];
                queryTool.LayerInfo = (lstSearchLayer.SelectedItem as ComboBoxItem).Tag as ArcGISLayerInfo;
                queryTool.Search(geometry, this.MapSRWKID);
            }
        }
        #endregion

        #region Process Query Results after QueryTask is completed
        private void Query_ResultReady(object sender, QueryResultEventArgs args)
        {
            if (args.Results != null && args.Results.Features.Count > 0)
            {
                IsFirstLoad = true; // Suppress filter results by map extent or pan map
                SearchResultMessage.Visibility = Visibility.Collapsed;

                FeatureSet fset = args.Results;
                Symbol symbol = ChooseSymbol(fset.GeometryType.ToString(), false);
                GeoFeatureCollection dataset = new GeoFeatureCollection(args.QueryLayer.OutputFields, args.QueryLayer.OutputLabels, fset.DisplayFieldName, args.QueryLayer.Title);

                foreach (Graphic feature in fset.Features)
                {
                    feature.Symbol = symbol;
                    feature.Geometry.SpatialReference = fset.SpatialReference;
                    this.GraphicsLayer.Graphics.Add(feature);
                    dataset.Add(feature);
                }

                this.FeatureSets.Add(dataset);
                this.GraphicsTipTemplate = args.QueryLayer.MapTipTemplate;
                this.MapControl.ZoomTo(GeometryTool.ExpandGeometryExtent(this.GraphicsLayer.FullExtent, 0.25));
            }
            else
            {
                SearchResultMessage.Visibility = Visibility.Visible;
                SearchResultMessage.Text = string.Format("Sorry! {0}", (string.IsNullOrEmpty(args.ErrorMsg)) ? "No features are found." : args.ErrorMsg);
            }

            this.IsBusy = false;
        }

        private Symbol ChooseSymbol(string geoType, bool select)
        {
            Symbol symbol = null;

            switch (geoType)
            {
                case "Point":
                case "MapPoint":
                case "MultiPoint": symbol = (select)
                        ? (this.CurrentApp.Resources[SymbolResources.SELECTED_MARKER] as MarkerSymbol)
                        : (this.CurrentApp.Resources[SymbolResources.HIGHLIGHT_MARKER] as MarkerSymbol); break;
                case "Polyline": symbol = (select)
                        ? (this.CurrentApp.Resources[SymbolResources.SELECTED_LINE] as LineSymbol)
                        : (this.CurrentApp.Resources[SymbolResources.HIGHLIGHT_LINE] as LineSymbol); break;
                case "Polygon": symbol = (select)
                        ? (this.CurrentApp.Resources[SymbolResources.SELECTED_FILL] as FillSymbol)
                        : (this.CurrentApp.Resources[SymbolResources.HIGHLIGHT_FILL] as FillSymbol); break;
            }

            return symbol;
        }
        #endregion

        #region Event Handlers to Set and Highlight Selected Graphic
        protected override void OnGraphicsLayerMouseUp(object sender, GraphicMouseButtonEventArgs e)
        {
            HighlightSelectedGraphic(e.Graphic);
            SearchResultGrid.SelectItem(e.Graphic);

            // Run the base's handler at last
            base.OnGraphicsLayerMouseUp(sender, e);
        }

        private void SearchResultGrid_SelectedItemChange(object sender, SelectedItemChangeEventArgs args)
        {
            Graphic feature = args.Feature;
            if (feature != null)
            {
                /*
                 * If filter the results with changing map extent, remove this block
                 * Otherwise, add this block to pan the map to the selected graphic.
                 * But do not do it at the first load when the map just zooms to the 
                 * results, otherwise the map will be hanging without response.
                 * (See MapControl_ExtentChanged handler)
                 */
                if (!IsFirstLoad)
                {
                    this.MapControl.PanTo(feature.Geometry);
                }

                HighlightSelectedGraphic(feature);
                IsFirstLoad = false;
            }
        }

        private void HighlightSelectedGraphic(Graphic newGraphic)
        {
            string geoType = "";

            if (this.SelectedGraphic != null)
            {
                geoType = this.SelectedGraphic.Geometry.GetType().Name;
                this.SelectedGraphic.Symbol = ChooseSymbol(geoType, false);
                this.SelectedGraphic.Selected = false;
            }

            this.SelectedGraphic = newGraphic;
            geoType = this.SelectedGraphic.Geometry.GetType().Name;
            this.SelectedGraphic.Symbol = ChooseSymbol(geoType, true);
            this.SelectedGraphic.Selected = true;
        }
        #endregion
    }
}
