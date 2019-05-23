using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Controls;


namespace ESRI.SilverlightViewer.UIWidget
{
    /// <summary>
    /// Identification Mode can now be activated by opening this widget or 
    /// selecting the Identify button on the Taskbar if you add an Identify 
    /// ToolButton in the AppConfig.XML file.
    /// </summary>
    public partial class IdentifyWidget : WidgetBase
    {
        private MapPoint identifyPoint = null;
        private IdentifyConfig widgetConfig = null;

        public IdentifyWidget()
        {
            InitializeComponent();
            this.DrawObject.DrawComplete += new EventHandler<ESRI.ArcGIS.Client.DrawEventArgs>(DrawObject_DrawComplete);
        }

        #region Override Function - Load Configuration and Clear Graphics
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;

            try
            {
                widgetConfig = (IdentifyConfig)IdentifyConfig.Deserialize(xmlConfig, typeof(IdentifyConfig));

                //The identification task is uncompleted on the first load
                if (identifyPoint != null) DoIdentification(identifyPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override void OnClose()
        {
            ClearGraphics(0);
            base.OnClose();
        }
        #endregion

        #region Override ResetDrawObjectMode - Enable DrawObject to Click Point Mode
        public override void ResetDrawObjectMode()
        {
            this.DrawObject.IsEnabled = true;
            this.DrawWidget = typeof(IdentifyWidget);
            this.DrawObject.DrawMode = DrawMode.Point;
            this.MapControl.Cursor = Cursors.Hand;
        }
        #endregion

        #region Start Identification and Bind Results
        private void DrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs e)
        {
            if (this.DrawWidget == this.GetType())
            {
                MapPoint point = e.Geometry as MapPoint;
                if (!this.IsOpen) this.IsOpen = true;
                else this.Focus();

                if (widgetConfig != null)
                {
                    DoIdentification(point);
                }
                else // Wait for configuration file is loaded
                {
                    identifyPoint = point;
                }
            }
        }

        private void DoIdentification(MapPoint point)
        {
            IdentifyParameters identifyParams = new IdentifyParameters();
            identifyParams.LayerOption = (LayerOption)Enum.Parse(typeof(LayerOption), widgetConfig.IdentifyOption, true);
            identifyParams.Tolerance = widgetConfig.Tolerance;
            identifyParams.MapExtent = this.MapControl.Extent;
            identifyParams.Height = (int)this.MapControl.ActualHeight;
            identifyParams.Width = (int)this.MapControl.ActualWidth;
            identifyParams.SpatialReference = new SpatialReference(this.MapSRWKID);
            identifyParams.ReturnGeometry = true;
            identifyParams.Geometry = point;

            this.IsBusy = true;
            this.ClearGraphics(0);

            for (int i = 0; i < widgetConfig.IdentifyLayers.Length; i++)
            {
                LivingMapLayer livingMapConfig = GetLivingMapConfig(widgetConfig.IdentifyLayers[i].Title);

                if (livingMapConfig != null && !string.IsNullOrEmpty(livingMapConfig.RESTURL))
                {
                    identifyParams.LayerIds.Clear();

                    int[] doLayers = GetIdentifiableLayers(livingMapConfig.ID, widgetConfig.IdentifyLayers[i].LayerIDs);
                    foreach (int kk in doLayers) { identifyParams.LayerIds.Add(kk); }

                    IdentifyTask identifyTask = new IdentifyTask(livingMapConfig.RESTURL);
                    identifyTask.ExecuteCompleted += new EventHandler<IdentifyEventArgs>(IdentifyTask_ExecuteCompleted);
                    identifyTask.Failed += new EventHandler<TaskFailedEventArgs>(IdentifyTask_Failed);
                    identifyTask.Token = (livingMapConfig.Token == null) ? "" : livingMapConfig.Token;
                    identifyTask.ProxyURL = livingMapConfig.ProxyURL;
                    identifyTask.ExecuteAsync(identifyParams, livingMapConfig.Title);
                }
            }
        }

        private void IdentifyTask_ExecuteCompleted(object sender, IdentifyEventArgs e)
        {
            string mapName = (string)e.UserState;
            string preName = "^?^";

            GeoFeatureCollection features = null;
            foreach (IdentifyResult result in e.IdentifyResults)
            {
                if (result.LayerName != preName)
                {
                    if (features != null)
                    {
                        this.FeatureSets.Add(features);
                    }

                    AddToGraphicsLayer(result.Feature, (string)result.Value, result.DisplayFieldName, result.LayerName);
                    features = new GeoFeatureCollection(result.DisplayFieldName, result.LayerName, mapName);
                    features.Add(result.Feature);
                    preName = result.LayerName;
                }
                else if (features != null)
                {
                    AddToGraphicsLayer(result.Feature, (string)result.Value, result.DisplayFieldName, result.LayerName);
                    features.Add(result.Feature);
                }
            }

            // Add the last layer results
            if (features != null)
            {
                this.FeatureSets.Add(features);
            }

            this.IsBusy = false;
        }

        private void IdentifyTask_Failed(object sender, TaskFailedEventArgs e)
        {
            this.IsBusy = false;
            MessageBox.Show(e.Error.Message);
        }
        #endregion

        #region Add Graphics to Highlight Results
        private void AddToGraphicsLayer(Graphic graphic, string displayValue, string displayField, string layerName)
        {
            if (this.GraphicsLayer == null)
            {
                MessageBox.Show("GraphicsLayer for this widget is not created. Please set property HasGraphics 'true'."); return;
            }

            string tipTemplate = string.Format("Title={0}", layerName);

            if (!string.IsNullOrEmpty(displayValue))
            {
                if (displayValue.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (displayValue.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) || displayValue.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
                    {
                        tipTemplate += string.Format(";Image={{#{0}}}[200*160]", displayField);
                    }
                    else
                    {
                        tipTemplate += string.Format(";Link={{#{0}}}", displayField);
                    }
                }
                else
                {
                    tipTemplate += string.Format(";Content={{#{0}}}", displayField);
                }
            }

            string geoType = graphic.Geometry.GetType().Name;
            graphic.Symbol = ChooseSymbol(geoType, false);
            graphic.MapTip = new MapTipPopup(tipTemplate) { ShowCloseButton = false, Background = this.Background };
            this.GraphicsLayer.Graphics.Add(graphic);
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
            IdentifyResultGrid.SelectItem(e.Graphic);

            // Run the base's handler at last
            base.OnGraphicsLayerMouseUp(sender, e);
        }

        private void IdentifyResultGrid_SelectedItemChange(object sender, SelectedItemChangeEventArgs args)
        {
            Graphic feature = args.Feature;

            if (feature != null)
            {
                HighlightSelectedGraphic(feature);
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

        #region Help Functions
        private LivingMapLayer GetLivingMapConfig(string name)
        {
            LivingMapLayer layerConfig = null;

            foreach (LivingMapLayer layer in AppConfig.MapConfig.LivingMaps)
            {
                if (name.Equals(layer.Title, StringComparison.CurrentCultureIgnoreCase))
                {
                    layerConfig = layer;
                    break;
                }
            }

            return layerConfig;
        }

        //Get an intersect set of visible layers && configured identifiable layers 
        private int[] GetIdentifiableLayers(string mapID, string identifyLayerIDs)
        {
            int[] visLayers = { };

            LayerInfo[] lyrInfos = null;
            LayerOption lyrOption = (LayerOption)Enum.Parse(typeof(LayerOption), widgetConfig.IdentifyOption, true);

            if (this.MapControl.Layers[mapID] is ArcGISDynamicMapServiceLayer)
            {
                ArcGISDynamicMapServiceLayer mapLayer = this.MapControl.Layers[mapID] as ArcGISDynamicMapServiceLayer;
                lyrInfos = mapLayer.Layers;
                if (lyrOption == LayerOption.visible)
                {
                    visLayers = (mapLayer.VisibleLayers != null) ? mapLayer.VisibleLayers : lyrInfos.Where(info => info.DefaultVisibility).Select(info => info.ID).ToArray();
                }
            }
            else if (this.MapControl.Layers[mapID] is ArcGISTiledMapServiceLayer)
            {
                lyrInfos = (this.MapControl.Layers[mapID] as ArcGISTiledMapServiceLayer).Layers;
                if (lyrOption == LayerOption.visible)
                {
                    visLayers = lyrInfos.Where(info => info.DefaultVisibility).Select(info => info.ID).ToArray();
                }
            }

            if (lyrInfos != null)
            {
                if (lyrOption == LayerOption.all)
                {
                    if (identifyLayerIDs == null || identifyLayerIDs.Trim() == "*")
                    {
                        visLayers = new int[lyrInfos.Length];
                        for (int i = 0; i < lyrInfos.Length; i++) { visLayers[i] = lyrInfos[i].ID; }
                    }
                    else
                    {
                        MatchCollection matches = Regex.Matches(identifyLayerIDs.Trim(), @"(\d+)");
                        visLayers = new int[matches.Count];
                        for (int i = 0; i < matches.Count; i++) { visLayers[i] = int.Parse(matches[i].Value); }
                    }
                }
                else if (lyrOption == LayerOption.top && lyrInfos.Length > 0)
                    visLayers = new int[1] { 0 };
            }

            return visLayers;
        }
        #endregion
    }
}
 