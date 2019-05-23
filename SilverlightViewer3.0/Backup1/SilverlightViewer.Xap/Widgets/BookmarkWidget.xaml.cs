using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.SilverlightViewer.Controls;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Config;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class BookmarkWidget : WidgetBase
    {
        private const string SITE_STORAGE_NAME = "Site";
        private const string APPLICATION_STORAGE_NAME = "Application";
        private const string BOOKMARK_SETTING_KEY = "ESRI_Silverlight_Viewer_Bookmarks";
        private const string DELETE_BUTTEN_IMAGE = "../images/buttons/btn_delete.png";

        private BitmapImage deleteButtonSource = new BitmapImage(new Uri(DELETE_BUTTEN_IMAGE, UriKind.Relative));
        private SolidColorBrush gridBackColor1 = new SolidColorBrush(Color.FromArgb(64, 176, 255, 255));

        private IsolatedStorageSettings storageSettings = null;
        private List<Bookmark> storedBookmarks = null;
        private BookmarkConfig widgetConfig = null;
        private int countBookmark = 0;
        private bool IsSaved = true;

        public BookmarkWidget()
        {
            InitializeComponent();
            this.CurrentApp.Exit += new EventHandler(OnApplicationExit);
        }

        #region Override Functions - Load Configuration and Save Bookmarks
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;

            widgetConfig = (BookmarkConfig)BookmarkConfig.Deserialize(xmlConfig, typeof(BookmarkConfig));

            if (widgetConfig != null && widgetConfig.Bookmarks != null)
            {
                for (int i = 0; i < widgetConfig.Bookmarks.Length; i++)
                {
                    widgetConfig.Bookmarks[i].UserAdded = false;
                    AddBookmark(widgetConfig.Bookmarks[i]);
                }
            }

            storageSettings = (SITE_STORAGE_NAME.Equals(widgetConfig.StoragePlace)) ? IsolatedStorageSettings.SiteSettings : IsolatedStorageSettings.ApplicationSettings;           
            bool stored = storageSettings.TryGetValue(BOOKMARK_SETTING_KEY, out storedBookmarks);

            if (stored)
            {
                foreach (Bookmark bookmark in storedBookmarks)
                {
                    bookmark.UserAdded = true;
                    AddBookmark(bookmark);
                }
            }
        }

        protected override void OnClose()
        {
            SaveBookmarks();
            base.OnClose();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            SaveBookmarks();
        }
        #endregion

        #region Add Bookmarks into the Application Window
        private void AddBookmark(Bookmark bookmark)
        {
            Grid gridBookmark = new Grid() { Margin = new Thickness(0, 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
            gridBookmark.ColumnDefinitions.Add(new ColumnDefinition());
            gridBookmark.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
            gridBookmark.Background = (countBookmark % 2 == 0) ? gridBackColor1 : null;

            SimpleLinkButton bookmarkNameLink = new SimpleLinkButton() { Text = bookmark.Name, Tag = bookmark.Extent, Margin = new Thickness(4, 4, 4, 4) };
            bookmarkNameLink.MouseOverBackColor = new SolidColorBrush(Colors.Transparent);
            bookmarkNameLink.MouseOverTextColor = new SolidColorBrush(Colors.Brown);
            bookmarkNameLink.Click += new RoutedEventHandler(BookmarkNameLink_Click);
            gridBookmark.Children.Add(bookmarkNameLink);
            Grid.SetColumn(bookmarkNameLink, 0);

            HyperlinkButton deleteLinkButton = new HyperlinkButton() { Tag = bookmark, Height = 16, Width = 16, Margin = new Thickness(4, 4, 4, 4), HorizontalAlignment = HorizontalAlignment.Right };
            deleteLinkButton.Background = new ImageBrush() { ImageSource = deleteButtonSource };
            deleteLinkButton.Visibility = (bookmark.UserAdded) ? Visibility.Visible : Visibility.Collapsed;
            deleteLinkButton.Click += new RoutedEventHandler(DeleteLinkButton_Click);
            gridBookmark.Children.Add(deleteLinkButton);
            Grid.SetColumn(deleteLinkButton, 1);

            StackBookmarkList.Children.Add(gridBookmark);
            countBookmark++;
        }
        #endregion

        #region Zoom the Map to the Bookmark's Extent
        private void BookmarkNameLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is SimpleLinkButton)
            {
                SimpleLinkButton nameLink = sender as SimpleLinkButton;
                Extent extent = (Extent)nameLink.Tag;

                if (extent != null)
                {
                    this.MapControl.ZoomTo(extent.ToEnvelope(this.MapSRWKID));
                }
            }
        }
        #endregion

        #region Handle Events - Delete and Add Bookmark from IsolatedStorage and the Window
        private void DeleteLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton)
            {
                HyperlinkButton deleteButton = sender as HyperlinkButton;
                Bookmark bookmark = (Bookmark)deleteButton.Tag;

                if (bookmark != null && storedBookmarks.Contains(bookmark))
                {
                    storedBookmarks.Remove(bookmark);
                }

                StackBookmarkList.Children.Remove(deleteButton.Parent as Grid);
                IsSaved = false;
            }
        }

        private void AddBookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (storedBookmarks == null) storedBookmarks = new List<Bookmark>();

            Envelope env = this.MapControl.Extent;
            Extent extent = new Extent() { xmin = env.XMin, ymin = env.YMin, xmax = env.XMax, ymax = env.YMax, spatialReference = env.SpatialReference.WKID };
            Bookmark bookmark = new Bookmark() { Name = txtBookmarkName.Text, Extent = extent, UserAdded = true };

            storedBookmarks.Add(bookmark);
            AddBookmark(bookmark);
            ToggleWidgetContent(0);
            IsSaved = false;
        }
        #endregion

        #region Save Bookmarks into IsolatedStorage
        private void SaveBookmarks()
        {
            if (storageSettings != null  && !IsSaved)
            {
                storageSettings[BOOKMARK_SETTING_KEY] = storedBookmarks;
                storageSettings.Save();
                IsSaved = true;
            }
        }
        #endregion
    }
}
