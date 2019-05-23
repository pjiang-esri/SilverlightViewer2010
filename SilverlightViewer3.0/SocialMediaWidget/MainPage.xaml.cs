using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Bing;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;
using ESRI.SilverlightViewer;
using SocialMediaWidget.Config;
using SocialMediaWidget.Utility;

namespace SocialMediaWidget
{
    public partial class MainPage : WidgetBase
    {
        private SocialMediaConfig widgetConfig = null;
        private PopupWindow flickrTipTemplate = null;
        private PopupWindow youtubeTipTemplate = null;
        private PopupWindow twitterTipTemplate = null;

        private PictureMarkerSymbol flickrSymbol = null;
        private PictureMarkerSymbol youtubeSymbol = null;
        private PictureMarkerSymbol twitterSymbol = null;

        private GeoFeatureCollection flickrsSet = null;
        private GeoFeatureCollection youtubeSet = null;
        private GeoFeatureCollection twitterSet = null;

        private Graphic highlighGraphic = null;

        public MainPage()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Configuration and Clear Graphics
        protected override void OnWidgetLoaded()
        {
            base.OnWidgetLoaded();

            LoadWidgetWithSelection();

            dateFlickrTo.SelectedDate = DateTime.Now;
            dateFlickrFrom.SelectedDate = DateTime.Now.AddDays(-7);
 
            flickrTipTemplate = new MapTipPopup("Title={#title};Image={#photoUrl};Content=Owner: {#ownerName} | Date Taken: {#dateTaken};Link={#photoUrl}");
            youtubeTipTemplate = new MapTipPopup("Title={#title};Image={#thumbnail};Note={#description};Content=Post Date: {#publishDate}") { Width = 372 };
            twitterTipTemplate = CreateTweetTipWindow(); // Create a custom Tip Window for tweets to support Rich Text Box

            flickrSymbol = new PictureMarkerSymbol() { Source = new BitmapImage(new Uri("./Images/s_flickr.jpg", UriKind.Relative)), Width = 20, Height = 20 };
            youtubeSymbol = new PictureMarkerSymbol() { Source = new BitmapImage(new Uri("./Images/s_youtube.jpg", UriKind.Relative)), Width = 20, Height = 20 };
            twitterSymbol = new PictureMarkerSymbol() { Source = new BitmapImage(new Uri("./Images/s_twitter.jpg", UriKind.Relative)), Width = 20, Height = 20 };

            this.FeatureSets.Add(flickrsSet);
            this.FeatureSets.Add(youtubeSet);
            this.FeatureSets.Add(twitterSet);
            socialMediaGrid.DataSources = this.FeatureSets;
            socialMediaGrid.SelectedItemChange += new SelectedItemChangeEventHandler(SocialMediaGrid_SelectedItemChange);

            // Add a listener to trace the SelectedGraphic of other widgets
            EventCenter.WidgetSelectedGraphicChange += new WidgetSelectedGraphicChangeEventHandler(OnWidgetSelectedGraphicChange);

            // Create highligh graphic
            highlighGraphic = new Graphic() { Symbol = this.CurrentApp.Resources[SymbolResources.HIGHLIGHT_MARKER] as MarkerSymbol, Selected = true };
        }

        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (SocialMediaConfig)SocialMediaConfig.Deserialize(xmlConfig, typeof(SocialMediaConfig));

            if (widgetConfig != null)
            {
                if (this.CurrentApp.IsWindowless) InsertJavaScriptObject();
            }
        }

        protected override void OnClose()
        {
            ClearGraphics(0);
            base.OnClose();
        }
        #endregion

        #region Add JavaScript and StyleSheet used for StreetView Window
        /// <summary>
        /// Insert JavaScript Window Object into the Page
        /// </summary>
        private void InsertJavaScriptObject()
        {
            if ("Javascript".Equals(widgetConfig.YouTubePlayerWindow, StringComparison.CurrentCultureIgnoreCase))
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
            string script = @"var ytPlayerWindow = null;
                function openYouTubePlayerWindow(ytUrl, width, height) {
                    if (ytPlayerWindow == null) {
                        var container = document.getElementById('silverlightControlHost');
                        ytPlayerWindow = new EsriFloatingWindow('winStreetView', 'YouTube Video', container, width, height);
                        ytPlayerWindow.center(); 
                    }
                    else if (ytPlayerWindow.closed) {
                        ytPlayerWindow.toggleVisibility(); 
                    }

                    ytPlayerWindow.setContentSource(ytUrl); 
                }";

            return script;
        }
        #endregion

        #region Populate Widgets with Selected Graphics
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

        #region Submit Social Media Search Requests
        private void RadioMedia_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsTemplateApplied) return;
            if (widgetConfig == null) return;

            string media = (sender as RadioButton).Tag as string;

            // Maximum Radius: YouTube - 1000km; Flickr - 32km; Twitter - n/a
            switch (media)
            {
                case "YouTube":
                    panelYTTimeParam.Visibility = Visibility.Visible;
                    panelFrTimeParam.Visibility = Visibility.Collapsed;
                    descTwitterTime.Visibility = Visibility.Collapsed;
                    textSearchContent.Text = "for videos around the";
                    sliderSearchRadius.Maximum = (widgetConfig.MaximumSearchRadius > 1000) ? 1000 : widgetConfig.MaximumSearchRadius;
                    sliderSearchRadius.Value = widgetConfig.DefaultSearchRadius;
                    break;
                case "Twitter":
                    descTwitterTime.Visibility = Visibility.Visible;
                    panelYTTimeParam.Visibility = Visibility.Collapsed;
                    panelFrTimeParam.Visibility = Visibility.Collapsed;
                    textSearchContent.Text = "for tweets around the";
                    sliderSearchRadius.Maximum = widgetConfig.MaximumSearchRadius;
                    sliderSearchRadius.Value = widgetConfig.DefaultSearchRadius;
                    break;
                case "Flickr":
                    panelFrTimeParam.Visibility = Visibility.Visible;
                    panelYTTimeParam.Visibility = Visibility.Collapsed;
                    descTwitterTime.Visibility = Visibility.Collapsed;
                    textSearchContent.Text = "for photos around the";
                    sliderSearchRadius.Maximum = 32;
                    sliderSearchRadius.Value = 8;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MapPoint centroid = null;

            if (radioSearchGeometry.IsChecked.Value)
            {
                if (lstGraphicWidget.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a widget with selected graphic", "Message", MessageBoxButton.OK);
                    return;
                }
                else
                {
                    Graphic centerGraphic = WidgetManager.GetWidgetSelectedGraphic((string)lstGraphicWidget.SelectedItem);
                    centroid = centerGraphic.Geometry.Extent.GetCenter();
                }
            }
            else
            {
                centroid = this.MapControl.Extent.GetCenter();
            }

            if (radioMediaYouTube.IsChecked.Value)
            {
                SearchYouTube(textKeyWord.Text, (string)(lstYouTubeTime.SelectedItem as ComboBoxItem).Tag, centroid.WebMercatorToGeographic(), sliderSearchRadius.Value);
            }
            else if (radioMediaFlickr.IsChecked.Value)
            {
                SearchFlickr(textKeyWord.Text, dateFlickrFrom.SelectedDate.Value.ToString("yyyy-MM-dd"), dateFlickrTo.SelectedDate.Value.ToString("yyyy-MM-dd"), centroid.WebMercatorToGeographic(), sliderSearchRadius.Value);
            }
            else if (radioMediaTwitter.IsChecked.Value)
            {
                SearchTwitter(textKeyWord.Text, centroid.WebMercatorToGeographic(), sliderSearchRadius.Value);
            }

            // Zoom Map to the search extent
            double radius = 1000 * sliderSearchRadius.Value; // meters
            Envelope extent = new Envelope(centroid.X - radius, centroid.Y - radius, centroid.X + radius, centroid.Y + radius);
            this.MapControl.ZoomTo(extent.Expand(1.2));
        }
        #endregion

        #region Search YouTube for Videos
        private void SearchYouTube(string keyword, string fromDay, MapPoint location, double radius, double maxResults = 25, int startIndex = 1)
        {
            while (true) // Clear old youtube graphics
            {
                Graphic ytGraphic = graphicsLayer.Graphics.FirstOrDefault(g => g.Symbol == youtubeSymbol);
                if (ytGraphic == null) break;
                graphicsLayer.Graphics.Remove(ytGraphic);
            }

            string youTubeUrl = "http://gdata.youtube.com/feeds/api/videos?format=5&v=2&alt=atom";
            youTubeUrl += string.Format("&q={0}&time={1}&start-index={2}&max-results={3}", keyword, fromDay, startIndex, maxResults);
            youTubeUrl += string.Format("&location={0},{1}!&location-radius={2}km", location.Y, location.X, radius);

            this.IsBusy = true;
            WebClient requestYouTube = new WebClient();
            requestYouTube.DownloadStringCompleted += new DownloadStringCompletedEventHandler(YouTubeRequest_DownloadStringCompleted);
            requestYouTube.DownloadStringAsync(new Uri(youTubeUrl));
        }

        private void YouTubeRequest_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                this.FeatureSets.Remove(youtubeSet);
                YouTubeResult ytResult = YouTubeResult.Deserialize(e.Result);
                youtubeSet = new GeoFeatureCollection("title,thumbnail,description,publishDate,playerUrl", "Title,Thumbnail,Description,Publish Date,Player Url", "title", "YouTube");

                if (ytResult != null)
                {
                    foreach (YouTubeItem ytItem in ytResult.items)
                    {
                        Graphic graphic = new Graphic();
                        graphic.Geometry = (new MapPoint(ytResult.items[0].geometry.rssPoint.x, ytResult.items[0].geometry.rssPoint.y, new SpatialReference(4326))).GeographicToWebMercator();
                        graphic.MouseLeftButtonDown += new MouseButtonEventHandler(YouTube_MouseLeftButtonDown);
                        graphic.Attributes.Add("title", ytItem.media.title);
                        graphic.Attributes.Add("thumbnail", ytItem.media.thumbnail[1].url); //hddefault.jpg (480 x 360)
                        graphic.Attributes.Add("aspectRatio", ytItem.media.aspectRatio);
                        graphic.Attributes.Add("description", ytItem.media.description);
                        graphic.Attributes.Add("publishDate", ytItem.media.publishDate);
                        graphic.Attributes.Add("contentUrl", ytItem.media.content[0].source);
                        graphic.Attributes.Add("playerUrl", "http://www.youtube.com/watch?v=" + ytItem.media.videoID);
                        graphic.Symbol = youtubeSymbol;
                        graphic.MapTip = youtubeTipTemplate;
                        graphicsLayer.Graphics.Add(graphic);
                        youtubeSet.Add(graphic);
                    }

                    this.FeatureSets.Add(youtubeSet);
                    this.ToggleWidgetContent(1);
                }
            }

            this.IsBusy = false;
        }

        private void YouTube_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Graphic g = sender as Graphic;
            bool isWideScrn = ("widescreen".Equals(g.Attributes["aspectRaito"]));
            string mediaUrl = g.Attributes["contentUrl"] as string;
            int videoWidth, videoHeight;

            switch (widgetConfig.YouTubePlayerSize)
            {
                case "Small": videoWidth = 320; break;
                case "Medium": videoWidth = 640; break;
                case "Large": videoWidth = 854; break;
                case "HD720": videoWidth = 1024; break;
                default: videoWidth = 640; break; //Medium by default
            }

            videoHeight = (isWideScrn) ? videoWidth * 9 / 16 : videoWidth * 3 / 4;

            if (this.CurrentApp.IsWindowless && "Javascript".Equals(widgetConfig.YouTubePlayerWindow, StringComparison.CurrentCultureIgnoreCase))
            {
                HtmlPage.Window.Invoke("openYouTubePlayerWindow", new object[] { mediaUrl, videoWidth, videoHeight });
            }
            else
            {
                System.Windows.Browser.HtmlPopupWindowOptions winOptions = new System.Windows.Browser.HtmlPopupWindowOptions() { Resizeable = false, Width = videoWidth, Height = videoHeight };
                System.Windows.Browser.HtmlWindow win = System.Windows.Browser.HtmlPage.PopupWindow(new Uri(mediaUrl), "_blank", winOptions);
            }
        }
        #endregion

        #region Search Flickr for Photos
        public void SearchFlickr(string keyword, string dateFrom, string dateTo, Geometry geometry, double radius = 32, string units = "km", double numPerPage = 250, double page = 1)
        {
            while (true) // Clear old flickr graphics
            {
                Graphic frGraphic = graphicsLayer.Graphics.FirstOrDefault(g => g.Symbol == flickrSymbol);
                if (frGraphic == null) break;
                graphicsLayer.Graphics.Remove(frGraphic);
            }

            string flickrUrl = "http://api.flickr.com/services/rest/?method=flickr.photos.search&api_key=fe7e074f8dad46678841c585f38620b7";
            string fparams = "&tags=" + keyword + "&tag_mode=all&accuracy=6&has_geo=1&license=0,1,2,3&page=" + page + "&per_page=" + numPerPage;
            string fextras = "&extras=date_taken,all_extras,geo,owner_name,license,o_dims";

            if (geometry is MapPoint)
            {
                MapPoint loc = geometry as MapPoint;
                fparams += "&lat=" + loc.Y + "&lon=" + loc.X + "&radius=" + radius + "&radius_units=" + units;
            }
            else if (geometry is Envelope)
            {
                Envelope ext = geometry as Envelope;
                fparams += "&bbox=" + ext.XMin + "," + ext.YMin + "," + ext.XMax + "," + ext.YMax;
            }

            if (dateFrom != null && dateFrom != "") fparams += "&min_upload_date=" + dateFrom;
            if (dateTo != null && dateTo != "") fparams += "&max_upload_date=" + dateTo;

            this.IsBusy = true;
            WebClient requestFlickr = new WebClient();
            requestFlickr.DownloadStringCompleted += new DownloadStringCompletedEventHandler(FlickrRequest_DownloadStringCompleted);
            requestFlickr.DownloadStringAsync(new Uri(flickrUrl + fparams + fextras));
        }

        private void FlickrRequest_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                this.FeatureSets.Remove(flickrsSet);
                FlickrResult frResult = FlickrResult.Deserialize(e.Result);
                flickrsSet = new GeoFeatureCollection("title,photoUrl,ownerName,dateTaken", "Title,Photo,Owner,Date", "title", "Flickr");

                if (frResult != null && frResult.photoPage != null && frResult.photoPage.photos != null)
                {
                    foreach (FlickrPhoto item in frResult.photoPage.photos)
                    {
                        Graphic graphic = new Graphic();
                        graphic.Geometry = (new MapPoint(item.longitude, item.latitude, new SpatialReference(4326))).GeographicToWebMercator();
                        graphic.Attributes.Add("title", item.title);
                        graphic.Attributes.Add("photoUrl", item.photoUrl);
                        graphic.Attributes.Add("ownerName", item.ownerName);
                        graphic.Attributes.Add("dateTaken", item.dateTaken);
                        graphic.Symbol = flickrSymbol;
                        graphic.MapTip = flickrTipTemplate;
                        graphicsLayer.Graphics.Add(graphic);
                        flickrsSet.Add(graphic);
                    }

                    this.FeatureSets.Add(flickrsSet);
                    this.ToggleWidgetContent(1);
                }
            }

            this.IsBusy = false;
        }
        #endregion

        #region Search Twitter for Tweets
        public void SearchTwitter(string keyword, MapPoint location, double radius, string units = "km", int numPerPage = 500, int page = 1)
        {
            while (true) // Clear old twitter graphics
            {
                Graphic twGraphic = graphicsLayer.Graphics.FirstOrDefault(g => g.Symbol == twitterSymbol);
                if (twGraphic == null) break;
                graphicsLayer.Graphics.Remove(twGraphic);
            }

            string twitterUrl = "http://search.twitter.com/search.atom?";
            twitterUrl += "q=" + keyword + "&page=" + page + "&rpp=" + numPerPage;
            twitterUrl += "&geocode=" + location.Y + "," + location.X + "," + radius + units;
            twitterUrl += "&result_type=mixed";

            this.IsBusy = true;
            WebClient requestTwitter = new WebClient();
            requestTwitter.DownloadStringCompleted += new DownloadStringCompletedEventHandler(TwitterRequest_DownloadStringCompleted);
            requestTwitter.DownloadStringAsync(new Uri(twitterUrl));
        }

        private void TwitterRequest_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                this.FeatureSets.Remove(twitterSet);
                TwitterResult twResult = TwitterResult.Deserialize(e.Result);
                twitterSet = new GeoFeatureCollection("title,photoUrl,authorUri,content", "Title,Photo,Author,Content", "title", "Tweets");

                if (twResult != null)
                {
                    foreach (TweetItem item in twResult.items)
                    {
                        Graphic graphic = new Graphic();
                        graphic.Geometry = (new MapPoint(item.geoLocation.x, item.geoLocation.y, new SpatialReference(4326))).GeographicToWebMercator();
                        graphic.Attributes.Add("title", item.author.authorName);
                        graphic.Attributes.Add("photoUrl", item.authorLinks[1].linkUrl);
                        graphic.Attributes.Add("authorUri", item.author.authorUri);
                        graphic.Attributes.Add("content", item.content);
                        graphic.Symbol = twitterSymbol;
                        graphic.MapTip = twitterTipTemplate;
                        graphicsLayer.Graphics.Add(graphic);
                        twitterSet.Add(graphic);
                    }

                    this.FeatureSets.Add(twitterSet);
                    this.ToggleWidgetContent(1);
                }
            }

            this.IsBusy = false;
        }

        private PopupWindow CreateTweetTipWindow()
        {
            PopupWindow tipWindow = new PopupWindow() { Width = 400, ShowArrow = false, ShowCloseButton = false, Background = this.Background, TitleWrapping = TextWrapping.Wrap };
            tipWindow.Resources.Add("HyperlinkField", "authorUri");

            StackPanel tipPanel = new StackPanel() { Margin = new Thickness(2, 2, 2, 2), Orientation = Orientation.Horizontal };
            tipWindow.Content = tipPanel;

            Binding titleBinding = new Binding() { Path = new PropertyPath("[title]") };
            tipWindow.SetBinding(PopupWindow.TitleProperty, titleBinding);

            Image tipImage = new Image() { Stretch = System.Windows.Media.Stretch.Uniform, VerticalAlignment = System.Windows.VerticalAlignment.Top };
            Binding imgBinding = new Binding() { Path = new PropertyPath("[photoUrl]") };
            tipImage.ImageOpened += new EventHandler<RoutedEventArgs>(tipImage_ImageOpened);
            tipImage.SetBinding(Image.SourceProperty, imgBinding);
            tipPanel.Children.Add(tipImage);

            RichTextBox contentBox = new RichTextBox() { Width = 340, BorderThickness = new Thickness(0) };
            Binding contentBinding = new Binding() { Path = new PropertyPath("[content]") };
            contentBox.SetBinding(StackPanel.TagProperty, contentBinding);
            tipPanel.Children.Add(contentBox);

            return tipWindow;
        }

        private void tipImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            StackPanel tipPanel = (sender as Image).Parent as StackPanel;
            RichTextBox contentBox = tipPanel.Children[1] as RichTextBox;
            contentBox.Blocks.Clear();

            try
            {
                string content = contentBox.Tag as string;
                string[] splits = Regex.Split(content, "<(.*?)>(.*?)<(.*?)>");
                //MatchCollection matches = Regex.Matches(content, "<a*>(.*?)</a>");

                Paragraph paragraph = new Paragraph();
                contentBox.Blocks.Add(paragraph);

                for (int i = 0; i < splits.Length; i++)
                {
                    if (!splits[i].Equals(""))
                    {
                        if (i < (splits.Length - 2) && splits[i].Equals("em") && splits[i + 2].Equals("/em"))
                        {
                            Bold boldText = new Bold();
                            boldText.Inlines.Add(new Run() { Text = splits[i + 1] });
                            paragraph.Inlines.Add(boldText);
                            i += 2;
                        }
                        else if (i < (splits.Length - 2) && splits[i].StartsWith("a") && splits[i + 2].Equals("/a"))
                        {
                            string sUri = Regex.Match(splits[i], "href=\"(.*?)\"").Groups[1].Value;
                            Hyperlink linkLine = new Hyperlink();
                            linkLine.NavigateUri = new Uri(sUri);
                            linkLine.Inlines.Add(splits[i + 1]);
                            paragraph.Inlines.Add(linkLine);
                            i += 2;
                        }
                        else
                        {
                            Run runText = new Run() { Text = splits[i] };
                            paragraph.Inlines.Add(runText);
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Highlight Selected Social Media Item
        private void SocialMediaGrid_SelectedItemChange(object sender, SelectedItemChangeEventArgs args)
        {
            this.highlighGraphic.Geometry = args.Feature.Geometry;
            this.MapControl.PanTo(args.Feature.Geometry);

            if (!graphicsLayer.Graphics.Contains(highlighGraphic))
            {
                graphicsLayer.Graphics.Insert(0, highlighGraphic);
            }
        }
        #endregion
    }
}
