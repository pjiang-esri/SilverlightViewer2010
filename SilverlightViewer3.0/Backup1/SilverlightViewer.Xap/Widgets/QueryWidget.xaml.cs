using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

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
    public partial class QueryWidget : WidgetBase
    {
        private string drawMode = "Rectangle"; // Default Draw Mode
        private QueryTool queryTool = null;
        private QueryConfig widgetConfig = null;

        // Used to suppress panning Map to the selection
        // or suppress refreshing the view of query results 
        // when query results are binded at the first time
        private bool isFirstLoad = true;

        // Track the status of layer info requests
        private int queryLayerCount = -1;
        private int layerReadyCount = 0;

        public QueryWidget()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Configuration and Clear Graphics
        protected override void OnWidgetLoaded()
        {
            base.OnWidgetLoaded();
            this.QueryResultGrid.DataSources = this.FeatureSets;
            
            queryTool = new QueryTool();
            queryTool.ResultReady += new QueryTool.QueryResultReady(Query_ResultReady);

            this.DrawObject.DrawComplete += new EventHandler<DrawEventArgs>(DrawObject_DrawComplete);
        }

        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (QueryConfig)QueryConfig.Deserialize(xmlConfig, typeof(QueryConfig));

            if (widgetConfig != null && widgetConfig.QueryLayers != null)
            {
                ArcGISQueryLayer[] queryLayers = widgetConfig.QueryLayers;
                queryLayerCount = queryLayers.Length;
                this.IsBusy = true;

                for (int i = 0; i < queryLayers.Length; i++)
                {
                    ArcGISLayerInfo layerInfo = new ArcGISLayerInfo(queryLayers[i].RESTURL, OnLayerInfoReady);
                    lstAttQueryLayer.Items.Add(new ComboBoxItem() { Content = queryLayers[i].Title, Tag = layerInfo });
                    lstGeoQueryLayer.Items.Add(new ComboBoxItem() { Content = queryLayers[i].Title, Tag = layerInfo });
                }
            }
        }

        private void OnLayerInfoReady(object sender, ArcGISLayerInfoEventArgs args)
        {
            if (args.LayerInfo.IsReady) layerReadyCount++;

            if (layerReadyCount == queryLayerCount)
            {
                this.IsBusy = false;
            }
        }

        protected override void OnClose()
        {
            this.ClearGraphics(0);
            base.OnClose();
        }
        #endregion

        #region Override Function - Reset DrawObject Mode
        protected override void OnSelectedContentChange(int newIndex)
        {
            base.OnSelectedContentChange(newIndex);
            this.ResetDrawObjectMode();
        }

        /// <summary>
        /// Override ResetDrawObjectMode - IDrawObject has been removed
        /// </summary>
        public override void ResetDrawObjectMode()
        {
            // If the change is from SpatialQuery tab to Results tab, the DrawObject is continuously usable
            if ((this.SelectedTabIndex == 1) || (this.lastTabIndex == 1 && this.SelectedTabIndex == 2))
            {
                this.DrawWidget = this.GetType();
                this.DrawObject.IsEnabled = true;
                this.MapControl.Cursor = Cursors.Arrow;

                switch (drawMode)
                {
                    case "Point":
                        this.DrawObject.DrawMode = DrawMode.Point; break;
                    case "Polyline":
                        this.DrawObject.DrawMode = DrawMode.Polyline;
                        this.DrawObject.LineSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_LINE] as LineSymbol; break;
                    case "Polygon":
                        this.DrawObject.DrawMode = DrawMode.Polygon;
                        this.DrawObject.FillSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol; break;
                    case "Freeline":
                    case "Freepoly":
                        this.DrawObject.DrawMode = DrawMode.Freehand;
                        this.DrawObject.LineSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_LINE] as LineSymbol; break;
                    case "Rectangle":
                        this.DrawObject.DrawMode = DrawMode.Rectangle;
                        this.DrawObject.FillSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol; break;
                    default:
                        this.DrawObject.IsEnabled = false; break;
                }
            }
            else
            {
                WidgetManager.ResetDrawObject();
            }
        }
        #endregion

        #region UI Element Event Handlers - Select features by an attribute value
        private void SearchLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtAttQueryWhere.Text = "";
            lstAttQueryField.Items.Clear();

            ComboBoxItem item = lstAttQueryLayer.SelectedItem as ComboBoxItem;
            ArcGISLayerInfo layerInfo = item.Tag as ArcGISLayerInfo;
            ArcGISQueryLayer queryLayer = widgetConfig.QueryLayers[lstAttQueryLayer.SelectedIndex];

            if (layerInfo.IsReady)
            {
                string fieldType = "";

                if (!string.IsNullOrEmpty(queryLayer.QueryFields))
                {
                    string[] queryFields = queryLayer.QueryFields.Split(',');
                    foreach (ArcGISLayerField field in layerInfo.Fields)
                    {
                        if (queryFields.Contains(field.Name, StringComparer.CurrentCultureIgnoreCase))
                        {
                            fieldType = field.Type.Substring(13); // "esriFieldType".Length
                            lstAttQueryField.Items.Add(new ListBoxItem() { Content = string.Format("{0} ({1})", field.Name, fieldType), Tag = field, Height = 20 });
                        }
                    }
                }
                else
                {
                    foreach (ArcGISLayerField field in layerInfo.Fields)
                    {
                        fieldType = field.Type.Substring(13); // "esriFieldType".Length
                        if (!fieldType.Equals("Geometry") && !fieldType.Equals("Raster") && !fieldType.Equals("Blob"))
                        {
                            lstAttQueryField.Items.Add(new ListBoxItem() { Content = string.Format("{0} ({1})", field.Name, fieldType), Tag = field, Height = 20 });
                        }
                    }
                }
            }
        }

        private void SearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            ListBoxItem item = e.AddedItems[0] as ListBoxItem;

            if (item.Tag != null)
            {
                ArcGISLayerField queryField = item.Tag as ArcGISLayerField;
                txtAttQueryWhere.Text = txtAttQueryWhere.Text.Insert(txtAttQueryWhere.SelectionStart, queryField.Name + " ");
                txtAttQueryWhere.SelectionStart = txtAttQueryWhere.Text.Length;
                txtAttQueryWhere.Focus();

                EsriFieldType fieldType = (EsriFieldType)Enum.Parse(typeof(EsriFieldType), queryField.Type, false);
                switch (fieldType)
                {
                    case EsriFieldType.esriFieldTypeOID:
                    case EsriFieldType.esriFieldTypeGUID:
                    case EsriFieldType.esriFieldTypeGlobalID:
                    case EsriFieldType.esriFieldTypeSmallInteger:
                    case EsriFieldType.esriFieldTypeInteger:
                    case EsriFieldType.esriFieldTypeSingle:
                    case EsriFieldType.esriFieldTypeDouble:
                        btnOperIs.IsEnabled = false;
                        btnOperNot.IsEnabled = false;
                        btnOperNULL.IsEnabled = false;
                        btnOperLike.IsEnabled = false;
                        break;
                    default:
                        btnOperIs.IsEnabled = true;
                        btnOperNot.IsEnabled = true;
                        btnOperNULL.IsEnabled = true;
                        btnOperLike.IsEnabled = true;
                        break;
                }
            }
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string sqlText = txtAttQueryWhere.Text;

            if (button.Content.Equals("Like"))
            {
                txtAttQueryWhere.Text = sqlText.Insert(txtAttQueryWhere.SelectionStart, "LIKE '%[value]%' ");
            }
            else
            {
                txtAttQueryWhere.Text = sqlText.Insert(txtAttQueryWhere.SelectionStart, button.Content.ToString().ToUpper() + " ");
            }

            txtAttQueryWhere.SelectionStart = txtAttQueryWhere.Text.Length;
            txtAttQueryWhere.Focus();
        }

        private void WhereClearButton_Click(object sender, RoutedEventArgs e)
        {
            txtAttQueryWhere.Text = "";
            lstAttQueryField.SelectedIndex = -1;
        }

        private void SubmitQueryButton_Click(object sender, RoutedEventArgs e)
        {
            string sWhere = txtAttQueryWhere.Text.Trim();
            Geometry extent = (checkWithinMap.IsChecked.Value) ? MapControl.Extent : null;

            if (!string.IsNullOrEmpty(sWhere))
            {
                this.IsBusy = true;
                queryTool.QueryLayer = widgetConfig.QueryLayers[lstAttQueryLayer.SelectedIndex];
                queryTool.LayerInfo = (lstAttQueryLayer.SelectedItem as ComboBoxItem).Tag as ArcGISLayerInfo;
                queryTool.Search(extent, sWhere, "", this.MapSRWKID);
            }
        }
        #endregion

        #region Draw a geometry using DrawObject - Select features by a geometry
        private void GeometryDrawMode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton)
            {
                foreach (HyperlinkButton button in DrawModeButtonStack.Children)
                {
                    if (button.Effect != null) { button.Effect = null; break; }
                }

                HyperlinkButton linkButton = sender as HyperlinkButton;
                linkButton.Effect = new DropShadowEffect() { Color = System.Windows.Media.Colors.Cyan, BlurRadius = 40, ShadowDepth = 0 };

                this.drawMode = (string)linkButton.Tag;
                this.ResetDrawObjectMode();

                if (drawMode.StartsWith("Free"))
                {
                    if (drawMode == "Freeline")
                    {
                        txtDrawModeStatus.Text = "Select by drawing a free line on the map.";
                    }
                    else
                    {
                        txtDrawModeStatus.Text = "Select by drawing a free polygon on the map.";
                    }
                }
                else
                {
                    txtDrawModeStatus.Text = string.Format("Select by drawing a {0} on the map.", this.drawMode.ToLower());
                }
            }
        }

        private void DrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (this.DrawWidget == this.GetType())
            {
                Graphic graphic = null;

                if (this.drawMode == "Freepoly")
                {
                    // Create a Graphic with the newly closed Polygon
                    graphic = new ESRI.ArcGIS.Client.Graphic()
                    {
                        Geometry = GeometryTool.FreehandToPolygon(e.Geometry as Polyline),
                        Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol
                    };
                }
                else
                {
                    // Create a Graphic with the drawn geometry
                    graphic = new ESRI.ArcGIS.Client.Graphic();
                    graphic.Geometry = e.Geometry;
                    switch (e.Geometry.GetType().Name)
                    {
                        case "MapPoint":
                            graphic.Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_MARKER] as MarkerSymbol; break;
                        case "Polyline":
                            graphic.Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_LINE] as LineSymbol; break;
                        case "Envelope":
                        case "Polygon":
                            graphic.Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol; break;
                    }
                }

                if (lstGeoQueryLayer.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a feature layer to query against"); return;
                }

                if (this.GraphicsLayer == null)
                {
                    MessageBox.Show("GraphicsLayer for this widget is not created. Please set property HasGraphics 'true'."); return;
                }

                if (graphic != null)
                {
                    this.IsBusy = true;

                    // Temporarily show the draw object
                    this.ClearGraphics(1);
                    this.AddGraphic(graphic);

                    queryTool.QueryLayer = widgetConfig.QueryLayers[lstGeoQueryLayer.SelectedIndex];
                    queryTool.LayerInfo = (lstGeoQueryLayer.SelectedItem as ComboBoxItem).Tag as ArcGISLayerInfo;
                    queryTool.Search(graphic.Geometry, this.MapSRWKID);
                }
            }
        }
        #endregion

        #region Process Query Results after QueryTask is completed
        private void Query_ResultReady(object sender, QueryResultEventArgs args)
        {
            this.ClearGraphics(2); // Toggle to Results Panel

            if (args.Results != null && args.Results.Features.Count > 0)
            {
                isFirstLoad = true; // Suppress filter results by map extent or pan map
                QueryResultMessage.Visibility = Visibility.Collapsed;

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
                QueryResultMessage.Visibility = Visibility.Visible;
                QueryResultMessage.Text = string.Format("Sorry! {0}", (string.IsNullOrEmpty(args.ErrorMsg)) ? "No features are found." : args.ErrorMsg);
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
            QueryResultGrid.SelectItem(e.Graphic);

            // Run the base's handler at last
            base.OnGraphicsLayerMouseUp(sender, e);
        }

        private void QueryResultGrid_SelectedItemChange(object sender, SelectedItemChangeEventArgs args)
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
                if (!isFirstLoad)
                {
                    this.MapControl.PanTo(feature.Geometry);
                }

                HighlightSelectedGraphic(feature);
                isFirstLoad = false;
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
