using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

using ESRI.SilverlightViewer;
using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Controls;
using ESRI.SilverlightViewer.Utility;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;

using GeoRSSWidget.Utility;
using GeoRSSWidget.Config;

namespace GeoRSSWidget
{
    public partial class MainPage : WidgetBase
    {
        GeoRSSConfig widgetConfig = null;
        DispatcherTimer refreshTimer = null;
        StackPanel selectedRssItemPanel = null;

        public MainPage()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Configuration / GeoRSS Feeds and Clear Graphics
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs args)
        {
            this.IsBusy = true;

            this.GraphicsTipTemplate = "Title={#Title};Content={#Description};Link={#Link}";

            string xmlConfig = args.Result;
            widgetConfig = (GeoRSSConfig)GeoRSSConfig.Deserialize(xmlConfig, typeof(GeoRSSConfig));

            WebClient rssClient = new WebClient();
            Uri rssUri = new Uri(widgetConfig.SourceURL, UriKind.Absolute);
            rssClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(RSSClient_DownloadStringCompleted);
            rssClient.DownloadStringAsync(rssUri);

            if (widgetConfig.RefreshRate > 0)
            {
                refreshTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(widgetConfig.RefreshRate) };
                refreshTimer.Tick += (o, e) => { this.IsBusy = true; rssClient.DownloadStringAsync(rssUri); };
                refreshTimer.Start();
            }
        }

        private void RSSClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlRSS = e.Result;

            if (!string.IsNullOrEmpty(xmlRSS))
            {
                GeoRSSSource rssSource = GeoRSSHelper.ParseGeoRSSXml(xmlRSS, widgetConfig.Filter, GeometryTool.IsWebMercatorSR(this.MapSRWKID));
                ListGeoRSSItems.ItemsSource = rssSource.Items;
                if (this.Visibility == Visibility.Visible) AddGeoRSSGraphics(rssSource.Items);
            }

            this.IsBusy = false;
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            if (ListGeoRSSItems.ItemsSource != null)
            {
                AddGeoRSSGraphics((List<GeoRSSItem>)ListGeoRSSItems.ItemsSource);
                if (refreshTimer != null) refreshTimer.Start();
            }
        }

        protected override void OnClose()
        {
            if (refreshTimer != null) refreshTimer.Stop();
            this.ClearGraphics(-1);
            base.OnClose();
        }
        #endregion

        #region Add GeoRSS Feeds into GraphicsLayer
        private void AddGeoRSSGraphics(List<GeoRSSItem> rssItems)
        {
            if (this.GraphicsLayer == null)
            {
                MessageBox.Show("GraphicsLayer for this widget is not created. Please set property HasGraphics 'true'."); return;
            }

            GeoFeatureCollection dataset = new GeoFeatureCollection("Title", "GeoRSS");
          
            foreach (GeoRSSItem rssItem in rssItems)
            {
                Graphic graphic = new Graphic();
                graphic.Geometry = rssItem.Geometry;
                graphic.Symbol = ChooseSymbol(rssItem.Geometry);
                graphic.Attributes.Add("Title", rssItem.Title);
                graphic.Attributes.Add("Description", rssItem.Description);
                graphic.Attributes.Add("PubDate", rssItem.pubDate);
                graphic.Attributes.Add("Link", rssItem.Link);
                this.GraphicsLayer.Graphics.Add(graphic);
                dataset.Add(graphic);
            }

            this.FeatureSets.Add(dataset); // Make it available for the Print Widget
        }

        private Symbol ChooseSymbol(ESRI.ArcGIS.Client.Geometry.Geometry geometry)
        {
            Symbol symbol = null;

            switch (geometry.GetType().Name)
            {
                case "Point":
                case "MapPoint":
                    if (string.IsNullOrEmpty(widgetConfig.SymbolImage))
                    {
                        symbol = this.CurrentApp.Resources[SymbolResources.SIMPLE_MARKER] as SimpleMarkerSymbol;
                    }
                    else
                    {
                        symbol = new PictureMarkerSymbol() { Source = new BitmapImage(new Uri(widgetConfig.SymbolImage, UriKind.Relative)), Width = 28, Height = 28 };
                    }
                    break;
                case "Polyline":
                    symbol = this.CurrentApp.Resources[SymbolResources.SIMPLE_LINE] as SimpleLineSymbol;
                    break;
                case "Polygon": symbol = this.CurrentApp.Resources[SymbolResources.SIMPLE_FILL] as SimpleFillSymbol;
                    break;
            }

            return symbol;
        }
        #endregion

        #region Open GeoRSS Feed Link
        private void RSSTitleLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is SimpleLinkButton)
            {
                if (selectedRssItemPanel != null)
                {
                    TextBlock oldRssDesc = selectedRssItemPanel.FindName("RssDescription") as TextBlock;
                    oldRssDesc.Visibility = Visibility.Collapsed;

                    HyperlinkButton oldRssLink = selectedRssItemPanel.FindName("RssHyperlink") as HyperlinkButton;
                    oldRssLink.Visibility = Visibility.Collapsed;
                }

                SimpleLinkButton titleLink = sender as SimpleLinkButton;
                StackPanel rssItemStack = titleLink.Parent as StackPanel;
                selectedRssItemPanel = rssItemStack;

                TextBlock textRssDesc = rssItemStack.FindName("RssDescription") as TextBlock;
                if (textRssDesc != null) textRssDesc.Visibility = Visibility.Visible;
                HyperlinkButton btnRssLink = rssItemStack.FindName("RssHyperlink") as HyperlinkButton;
                if (btnRssLink != null) btnRssLink.Visibility = Visibility.Visible;

                ESRI.ArcGIS.Client.Geometry.Geometry geometry = titleLink.Tag as ESRI.ArcGIS.Client.Geometry.Geometry;
                this.MapControl.ZoomTo(GeometryTool.ExpandGeometryExtent(geometry, 0.10));

                /* If other widgets need to know the SelectedGraphic */
                this.SelectedGraphic = new Graphic();
                this.SelectedGraphic.Geometry = geometry;
                this.SelectedGraphic.Attributes.Add("Title", titleLink.Text);
                this.SelectedGraphic.Attributes.Add("Description", textRssDesc.Text);
                this.SelectedGraphic.Attributes.Add("Link", btnRssLink.NavigateUri.AbsoluteUri);
            }
        }
        #endregion
    }
}