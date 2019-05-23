using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;
//using ESRI.ArcGIS.Client.Toolkit.Primitives;
using ESRI.ArcGIS.Client.Geometry;

using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class TOCWidget : WidgetBase
    {
        private int layerCount = 0;
        private ScaleConverter scaleConverter = new ScaleConverter();

        public TOCWidget()
        {
            InitializeComponent();

            this.IsBusy = true;
            EventCenter.MapLoadComplete += new MapLoadCompleteEventHandler(OnMapLoadComplete);
            MapContentTree.SizeChanged += (o, e) => { HideConextMenu(); }; // Node expand and collapse
        }

        protected override void OnIsActiveChanged(object sender, IsActiveChangedEventArgs e)
        {
            base.OnIsActiveChanged(sender, e);
            if (!e.IsActive) HideConextMenu();
        }

        #region Start to create Legend after Map is fully loaded
        private void OnMapLoadComplete(object sender, RoutedEventArgs e)
        {
            LivingMapLayer[] livingMaps = this.AppConfig.MapConfig.LivingMaps;

            if (livingMaps != null && livingMaps.Length > 0)
            {
                for (int i = 0; i < livingMaps.Length; i++)
                {
                    // Create Map Layer Nodes
                    TreeViewItem tvItem = CreateMapLayerNode(livingMaps[i]);
                    MapContentTree.Items.Add(tvItem);

                    if (this.MapControl.Layers[livingMaps[i].ID] is ArcGISTiledMapServiceLayer)
                    {
                        ArcGISTiledMapServiceLayer cachedLayer = this.MapControl.Layers[livingMaps[i].ID] as ArcGISTiledMapServiceLayer;
                        cachedLayer.QueryLegendInfos((legendInfo => OnLegendInfoSucceed(legendInfo, tvItem)), (exception => OnLegendInfoFailed(exception, livingMaps[i].Title)));
                    }
                    else if (this.MapControl.Layers[livingMaps[i].ID] is ArcGISDynamicMapServiceLayer)
                    {
                        ArcGISDynamicMapServiceLayer dynamicLayer = this.MapControl.Layers[livingMaps[i].ID] as ArcGISDynamicMapServiceLayer;
                        dynamicLayer.QueryLegendInfos((legendInfo => OnLegendInfoSucceed(legendInfo, tvItem)), (exception => OnLegendInfoFailed(exception, livingMaps[i].Title)));
                    }
                    else if (this.MapControl.Layers[livingMaps[i].ID] is FeatureLayer)
                    {
                        FeatureLayer featureLayer = this.MapControl.Layers[livingMaps[i].ID] as FeatureLayer;
                        featureLayer.QueryLegendInfos((legendInfo => OnLegendInfoSucceed(legendInfo, tvItem)), (exception => OnLegendInfoFailed(exception, livingMaps[i].Title)));
                    }
                }
            }
        }
        #endregion

        #region Create Table of Content (TOC) for Living Maps
        private void OnLegendInfoSucceed(LayerLegendInfo legendInfo, TreeViewItem mapTreeNode)
        {
            layerCount++; // Count Legend-Initialized LivingMap

            if (mapTreeNode != null && mapTreeNode.Tag != null)
            {
                LivingMapLayer mapConfig = mapTreeNode.Tag as LivingMapLayer;

                if (legendInfo.LayerLegendInfos != null)
                {
                    if (this.MapControl.Layers[mapConfig.ID] is ArcGISDynamicMapServiceLayer)
                    {
                        ArcGISDynamicMapServiceLayer dynamicLayer = this.MapControl.Layers[mapConfig.ID] as ArcGISDynamicMapServiceLayer;
                        List<int> layerIDs = GetDynamicMapLayerIDs(mapConfig, dynamicLayer.Layers);
                        List<int> visibleLayerIDs = new List<int>();

                        CreateDynamicLayerTree(mapConfig.ID, legendInfo, mapTreeNode, dynamicLayer.Layers, layerIDs, visibleLayerIDs, mapConfig.ToggleLayer, true);

                        // Initialize map visibilities
                        dynamicLayer.VisibleLayers = visibleLayerIDs.ToArray();
                        EventCenter.DispatchMapLayerVisibilityChangeEvent(this, new MapLayerVisibilityChangeEventArgs(dynamicLayer, new int[0]));
                    }
                    else
                    {
                        CreateCachedLayerTree(mapConfig.ID, legendInfo, mapTreeNode);
                    }
                }
                else if (legendInfo.LegendItemInfos != null) // Feature Layer
                {
                    mapTreeNode.ItemTemplate = this.Resources["SymbolTreeNode"] as DataTemplate;
                    mapTreeNode.ItemsSource = legendInfo.LegendItemInfos;
                }
            }

            if (layerCount == MapContentTree.Items.Count) this.IsBusy = false;
        }

        private void OnLegendInfoFailed(Exception ex, string layerTitle)
        {
            MessageBox.Show(string.Format("Failed to get the legend info of layer {0}\n{1}", layerTitle, ex.Message));
        }

        /// <summary>
        /// Iteratively called to create nodes for all sub-layers in a Dynamic Map Service
        /// </summary>
        private void CreateDynamicLayerTree(string mapID, LayerLegendInfo legendInfo, TreeViewItem layerNode, LayerInfo[] lyrInfos, List<int> layerIDs, List<int> visibleLayerIDs, bool toggleLayer, bool parentVisible)
        {
            if (legendInfo.LayerLegendInfos != null)
            {
                foreach (LayerLegendInfo layerLegendInfo in legendInfo.LayerLegendInfos)
                {
                    if (layerIDs.IndexOf(layerLegendInfo.SubLayerID) > -1)
                    {
                        LayerInfo lyrInfo = lyrInfos[layerLegendInfo.SubLayerID];
                        bool hasSubLayers = lyrInfo.SubLayerIds != null;
                        bool lyrVisib = (toggleLayer) ? ((visibleLayerIDs.Count > 0) ? false : lyrInfo.DefaultVisibility) : lyrInfo.DefaultVisibility;
                        if (!hasSubLayers && lyrVisib && parentVisible) visibleLayerIDs.Add(layerLegendInfo.SubLayerID);

                        TreeViewItem fLayerNode = CreateFeatureLayerNode(mapID, lyrInfo.Name, lyrInfo.ID, layerLegendInfo.MinimumScale, layerLegendInfo.MaximumScale, lyrVisib, hasSubLayers, toggleLayer);
                        fLayerNode.ItemTemplate = this.Resources["SymbolTreeNode"] as DataTemplate;
                        fLayerNode.ItemsSource = layerLegendInfo.LegendItemInfos;
                        layerNode.Items.Add(fLayerNode);

                        CreateDynamicLayerTree(mapID, layerLegendInfo, fLayerNode, lyrInfos, layerIDs, visibleLayerIDs, false, lyrVisib && parentVisible);
                    }
                }
            }
        }

        /// <summary>
        /// Iteratively called to create nodes for all sub-layers in a Cached Map Service
        /// </summary>
        private void CreateCachedLayerTree(string mapID, LayerLegendInfo legendInfo, TreeViewItem layerNode)
        {
            if (legendInfo.LayerLegendInfos != null)
            {
                foreach (LayerLegendInfo layerLegendInfo in legendInfo.LayerLegendInfos)
                {
                    TreeViewItem tLayerNode = new TreeViewItem() { Header = legendInfo.LayerName, Tag = new TOCNodeInfo() { MapID = mapID, IsTiledMap = true, LayerID = legendInfo.SubLayerID } };
                    tLayerNode.MouseRightButtonDown += new MouseButtonEventHandler(LayerNode_RightClick);
                    tLayerNode.ItemTemplate = this.Resources["SymbolTreeNode"] as DataTemplate;
                    tLayerNode.ItemsSource = layerLegendInfo.LegendItemInfos;
                    layerNode.Items.Add(tLayerNode);

                    CreateCachedLayerTree(mapID, layerLegendInfo, tLayerNode);
                }
            }
        }

        /// <summary>
        /// Create a Node for ArcGIS Map Service Layer (Living Maps in the configuration)
        /// </summary>
        private TreeViewItem CreateMapLayerNode(LivingMapLayer mapConfig)
        {
            TreeViewItem tvItem = new TreeViewItem();
            tvItem.Tag = mapConfig;

            CheckBox checkBox = new CheckBox() { Tag = tvItem, IsChecked = mapConfig.VisibleInitial };
            checkBox.MouseRightButtonDown += new MouseButtonEventHandler(LayerNode_RightClick);
            checkBox.Click += new RoutedEventHandler(OnToggleMapLayer);

            if (mapConfig.OpacityBar)
            {
                StackPanel headPanel = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(0, 0, 0, -20) };
                headPanel.Children.Add(new TextBlock() { Text = mapConfig.Title });

                Slider opacitySlider = new Slider() { Tag = mapConfig.ID, Value = mapConfig.Opacity, Minimum = 0.0, Maximum = 1.0, Width = 100 };
                opacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(OpacitySlider_ValueChanged);
                opacitySlider.HorizontalAlignment = HorizontalAlignment.Left;
                opacitySlider.Orientation = Orientation.Horizontal;
                ToolTipService.SetToolTip(opacitySlider, "Opacity");
                opacitySlider.Margin = new Thickness(0, 2, 0, 0);
                headPanel.Children.Add(opacitySlider);

                checkBox.Content = headPanel;
                tvItem.ItemContainerStyle = this.Resources["TreeViewItemsContainerStyle"] as Style;
                tvItem.Margin = new Thickness(0, 0, 0, 24);
                tvItem.Header = checkBox;
            }
            else
            {
                checkBox.Content = mapConfig.Title;
                tvItem.Margin = new Thickness(0, 0, 0, 4);
                tvItem.Header = checkBox;
            }

            return tvItem;
        }

        /// <summary>
        /// Create a Node for a feature layer in an ArcGIS Dynamic Map Service Layer
        /// </summary>
        private TreeViewItem CreateFeatureLayerNode(string mapID, string lyrName, int lyrID, double minScale, double maxScale, bool visible, bool hasSubLayer, bool toggleLayer)
        {
            Control checkButton = null;
            TreeViewItem childItem = new TreeViewItem();
            childItem.Tag = new TOCNodeInfo() { MapID = mapID, LayerID = lyrID, IsGroupLayer = hasSubLayer };

            if (toggleLayer)
            {
                RadioButton radioBox = new RadioButton();
                checkButton = radioBox;
                radioBox.Tag = childItem;
                radioBox.Content = lyrName;
                radioBox.IsChecked = visible;
                radioBox.GroupName = "Radio_" + mapID;
                radioBox.Click += new RoutedEventHandler(OnToggleFeatureLayer);
                radioBox.MouseRightButtonDown += new MouseButtonEventHandler(LayerNode_RightClick);
            }
            else
            {
                CheckBox checkBox = new CheckBox();
                checkButton = checkBox;
                checkBox.Tag = childItem;
                checkBox.Content = lyrName;
                checkBox.IsChecked = visible;
                checkBox.Click += new RoutedEventHandler(OnToggleFeatureLayer);
                checkBox.MouseRightButtonDown += new MouseButtonEventHandler(LayerNode_RightClick);
            }

            if (minScale > 0.0 || maxScale > 0.0)
            {
                Binding scaleBinding = new Binding("MapScale") { ConverterParameter = new double[2] { minScale, maxScale }, Converter = scaleConverter };
                checkButton.DataContext = this.CurrentPage;
                checkButton.SetBinding(Control.ForegroundProperty, scaleBinding);
            }

            childItem.Header = checkButton;
            return childItem;
        }
        #endregion

        #region Event Handlers - Toggle Layer Visibility and Show Context Menu
        /// <summary>
        /// Toggle the visibility of a feature layer in an ArcGIS Dynamic Map Service Layer
        /// </summary>
        private void OnToggleFeatureLayer(object sender, RoutedEventArgs e)
        {
            HideConextMenu();

            TOCNodeInfo nodeInfo = null;
            TreeViewItem treeNode = null;

            if (sender is CheckBox)
            {
                CheckBox checkBox = sender as CheckBox;
                treeNode = checkBox.Tag as TreeViewItem;
                nodeInfo = treeNode.Tag as TOCNodeInfo;
            }
            else if (sender is RadioButton)
            {
                RadioButton radioBox = sender as RadioButton;
                treeNode = radioBox.Tag as TreeViewItem;
                nodeInfo = treeNode.Tag as TOCNodeInfo;
            }

            if (treeNode != null && nodeInfo != null)
            {
                TreeViewItem mapNode = GetMapLayerNode(treeNode);
                ArcGISDynamicMapServiceLayer dynamicLayer = this.MapControl.Layers[nodeInfo.MapID] as ArcGISDynamicMapServiceLayer;
                int[] oldVisibles = (int[])dynamicLayer.VisibleLayers.Clone();

                List<int> visLayers = new List<int>();
                GetVisibleLayerIDs(mapNode, visLayers);
                dynamicLayer.VisibleLayers = visLayers.ToArray();
                EventCenter.DispatchMapLayerVisibilityChangeEvent(this, new MapLayerVisibilityChangeEventArgs(dynamicLayer, oldVisibles));
            }
        }

        /// <summary>
        /// Toggle the visibility of an ArcGIS Map Service Layer (Living Maps)
        /// </summary>
        private void OnToggleMapLayer(object sender, RoutedEventArgs e)
        {
            HideConextMenu();

            if (sender is CheckBox)
            {
                CheckBox check = sender as CheckBox;
                LivingMapLayer mapConfig = (check.Tag as TreeViewItem).Tag as LivingMapLayer;
                Layer theLayer = this.MapControl.Layers[mapConfig.ID] as Layer;

                if (theLayer != null)
                {
                    theLayer.Visible = check.IsChecked.Value;
                    EventCenter.DispatchMapLayerVisibilityChangeEvent(this, new MapLayerVisibilityChangeEventArgs(theLayer));
                }
            }
        }

        /// <summary>
        /// Layer Node Right Click Event Handler - Show Context Menu
        /// </summary>
        private void LayerNode_RightClick(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            e.Handled = true;
            Point p = e.GetPosition(this.ContentContainer);
            ContextMenuBlock.Visibility = System.Windows.Visibility.Visible;
            ContextMenuBlock.Margin = new Thickness(p.X - 4 + this.ScrollHorizontalOffset, p.Y - 4 + this.ScrollVerticalOffset, 0, 0);
            TreeViewItem nodeTreeItem = (sender is TreeViewItem) ? (sender as TreeViewItem) : (sender as Control).Tag as TreeViewItem;
            MenuItemZoomTo.Tag = nodeTreeItem.Tag;
            bool isGroupNode = false;

            if (nodeTreeItem.Tag is LivingMapLayer)
            {
                LivingMapLayer mapConfig = nodeTreeItem.Tag as LivingMapLayer;
                isGroupNode = mapConfig.ServiceType == ArcGISServiceType.Dynamic && !mapConfig.ToggleLayer;
            }
            else if (nodeTreeItem.Tag is TOCNodeInfo)
            {
                isGroupNode = (nodeTreeItem.Tag as TOCNodeInfo).IsGroupLayer;
            }

            if (isGroupNode)
            {
                MenuItemShowAll.Tag = nodeTreeItem;
                MenuItemShowNon.Tag = nodeTreeItem;
            }

            MenuItemShowAll.IsEnabled = isGroupNode;
            MenuItemShowNon.IsEnabled = isGroupNode;
        }

        private void HideConextMenu()
        {
            if (ContextMenuBlock.Visibility == System.Windows.Visibility.Visible)
            {
                ContextMenuBlock.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        #endregion

        #region Context Menu Item Event Handlers - Zoom To, Show/Hide Layers
        /// <summary>
        /// Zoom map to a feature layer's extent. If its SR is different than the base map's, re-project the extent
        /// </summary>
        private void MenuItemZoomTo_Click(object sender, MenuItemClickEventArgs e)
        {
            HideConextMenu();

            if (e.ItemTag is LivingMapLayer)
            {
                LivingMapLayer mapConfig = e.ItemTag as LivingMapLayer;
                Envelope extent = this.MapControl.Layers[mapConfig.ID].FullExtent;
                if (!SpatialReference.AreEqual(extent.SpatialReference, this.MapControl.SpatialReference, false))
                {
                    GeometryTool.ProjectEnvelope(this.AppConfig.GeometryService, extent, this.MapSRWKID, (s, g) =>
                    {
                        if (g.ProjectedGeometry != null)
                        {
                            this.MapControl.ZoomTo(g.ProjectedGeometry);
                        }
                    });
                }
                else
                {
                    this.MapControl.ZoomTo(extent);
                }
            }
            else if (e.ItemTag is TOCNodeInfo)
            {
                TOCNodeInfo nodeInfo = e.ItemTag as TOCNodeInfo;
                if (nodeInfo.LayerExtent == null)
                {
                    this.IsBusy = true;
                    Layer layer = this.MapControl.Layers[nodeInfo.MapID];
                    string layerUrl = (nodeInfo.IsTiledMap) ? (layer as ArcGISTiledMapServiceLayer).Url : (layer as ArcGISDynamicMapServiceLayer).Url;
                    ArcGISLayerInfoReader layerInfoReader = new ArcGISLayerInfoReader(layerUrl + "/" + nodeInfo.LayerID);
                    layerInfoReader.InfoReady += (obj, arg) =>
                    {
                        if (!SpatialReference.AreEqual(arg.LayerInfo.Extent.SpatialReference, this.MapControl.SpatialReference, false))
                        {
                            GeometryTool.ProjectEnvelope(this.AppConfig.GeometryService, arg.LayerInfo.Extent, this.MapSRWKID, (s, g) =>
                            {
                                this.IsBusy = false;
                                if (g.ProjectedGeometry != null)
                                {
                                    nodeInfo.LayerExtent = g.ProjectedGeometry as Envelope;
                                    this.MapControl.ZoomTo(nodeInfo.LayerExtent);
                                }
                            });
                        }
                        else
                        {
                            this.IsBusy = false;
                            nodeInfo.LayerExtent = nodeInfo.LayerExtent = arg.LayerInfo.Extent;
                            this.MapControl.ZoomTo(nodeInfo.LayerExtent);
                        }
                    };
                }
                else
                {
                    this.MapControl.ZoomTo(nodeInfo.LayerExtent);
                }
            }
        }

        /// <summary>
        /// Toggle the Visibilities of a Group of Layers
        /// </summary>
        private void MenuItemToggleGroup_Click(object sender, MenuItemClickEventArgs e)
        {
            if (e.ItemTag is TreeViewItem)
            {
                bool show = (sender == MenuItemShowAll);
                TreeViewItem treeItem = e.ItemTag as TreeViewItem;
                ToggleChildNodes(treeItem, show);

                string mapID = "";
                bool isMapNode = false;
                TreeViewItem mapLayerNode = null;
                if (treeItem.Tag is LivingMapLayer)
                {
                    mapID = (treeItem.Tag as LivingMapLayer).ID;
                    mapLayerNode = treeItem;
                    isMapNode = true;
                }
                else if (treeItem.Tag is TOCNodeInfo)
                {
                    mapID = (treeItem.Tag as TOCNodeInfo).MapID;
                    mapLayerNode = GetMapLayerNode(treeItem);
                }

                if (treeItem.Header is CheckBox)
                {
                    (treeItem.Header as CheckBox).IsChecked = show;
                }
                else if (treeItem.Header is RadioButton)
                {
                    (treeItem.Header as RadioButton).IsChecked = show;
                }

                if (mapID != "")
                {
                    ArcGISDynamicMapServiceLayer dynamicMap = this.MapControl.Layers[mapID] as ArcGISDynamicMapServiceLayer;
                    int[] oldVisibles = (int[])dynamicMap.VisibleLayers.Clone();

                    List<int> layerIDs = new List<int>();
                    GetVisibleLayerIDs(mapLayerNode, layerIDs);
                    dynamicMap.VisibleLayers = layerIDs.ToArray();
                    if (isMapNode) dynamicMap.Visible = show;

                    EventCenter.DispatchMapLayerVisibilityChangeEvent(this, new MapLayerVisibilityChangeEventArgs(dynamicMap, oldVisibles));
                }

                HideConextMenu();
            }
        }

        /// <summary>
        /// Toggle the Checkboxed in the Child Nodes
        /// </summary>
        private void ToggleChildNodes(TreeViewItem treeNode, bool show)
        {
            if (treeNode.HasItems)
            {
                foreach (object item in treeNode.Items)
                {
                    if (item is TreeViewItem)
                    {
                        TreeViewItem childItem = item as TreeViewItem;
                        
                        if (childItem.Header is CheckBox)
                        {
                            (childItem.Header as CheckBox).IsChecked = show;
                            ToggleChildNodes(childItem, show);
                        }
                    }
                }
            }
        }
        #endregion

        #region Helping Functions - Get All Layer IDs and Visible Layer IDs
        /// <summary>
        /// Get All Layer IDs in a Dynamic Map Service
        /// </summary>
        private List<int> GetDynamicMapLayerIDs(LivingMapLayer mapConfig, LayerInfo[] lyrInfos)
        {
            List<int> layerIDs = new List<int>();

            if (string.IsNullOrEmpty(mapConfig.VisibleLayers) || mapConfig.VisibleLayers == "*")
            {
                for (int j = 0; j < lyrInfos.Length; j++)
                {
                    layerIDs.Add(lyrInfos[j].ID);
                }
            }
            else
            {
                string[] tmpIDs = mapConfig.VisibleLayers.Split(',');
                for (int j = 0; j < tmpIDs.Length; j++)
                {
                    layerIDs.Add(int.Parse(tmpIDs[j]));
                }
            }

            return layerIDs;
        }

        /// <summary>
        /// Get Visible Layer IDs in a Dynamic Map Service
        /// </summary>
        private void GetVisibleLayerIDs(TreeViewItem layerNode, List<int> visLayers)
        {
            if (layerNode.HasItems)
            {
                foreach (object item in layerNode.Items)
                {
                    if (item is TreeViewItem)
                    {
                        TreeViewItem treeItem = item as TreeViewItem;
                        TOCNodeInfo nodeInfo = treeItem.Tag as TOCNodeInfo;

                        if (treeItem.Header is CheckBox)
                        {
                            CheckBox checkBox = treeItem.Header as CheckBox;

                            if (checkBox.IsChecked.Value && checkBox.Tag is TreeViewItem)
                            {
                                if (nodeInfo.IsGroupLayer) // Get SubLayers' Visibilities
                                    GetVisibleLayerIDs(checkBox.Tag as TreeViewItem, visLayers);
                                else
                                    visLayers.Add(nodeInfo.LayerID);
                            }
                        }
                        else if (treeItem.Header is RadioButton)
                        {
                            RadioButton radioBox = treeItem.Header as RadioButton;

                            if (radioBox.IsChecked.Value && radioBox.Tag is TreeViewItem)
                            {
                                if (nodeInfo.IsGroupLayer) // Get SubLayers' Visibilities
                                    GetVisibleLayerIDs(radioBox.Tag as TreeViewItem, visLayers);
                                else
                                    visLayers.Add(nodeInfo.LayerID);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the Map Layer (Toppest) Node in the TreeView
        /// </summary>
        private TreeViewItem GetMapLayerNode(TreeViewItem layerNode)
        {
            if (layerNode.Parent is TreeViewItem)
            {
                return GetMapLayerNode(layerNode.Parent as TreeViewItem);
            }
            else
            {
                return layerNode;
            }
        }
        #endregion

        #region Layer Opacity Slider Event Handler
        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HideConextMenu();

            if (sender is Slider)
            {
                string mapID = (string)(sender as Slider).Tag;

                if (!string.IsNullOrEmpty(mapID))
                {
                    Layer theLayer = this.MapControl.Layers[mapID] as ESRI.ArcGIS.Client.Layer;
                    theLayer.Opacity = e.NewValue;
                }
            }
        }
        #endregion
    }

    internal class TOCNodeInfo
    {
        public string MapID = "";
        public int LayerID = -1;
        public bool IsTiledMap = false;
        public bool IsGroupLayer = false;
        public Envelope LayerExtent = null;
    }
}
