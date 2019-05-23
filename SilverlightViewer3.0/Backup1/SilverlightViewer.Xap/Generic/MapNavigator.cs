using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;

using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;

namespace ESRI.SilverlightViewer.Generic
{
    [TemplatePart(Name = MapNavigator._ZOOM_BACK_BUTTON, Type = typeof(ButtonGrid))]
    [TemplatePart(Name = MapNavigator._ZOOM_NEXT_BUTTON, Type = typeof(ButtonGrid))]
    [TemplatePart(Name = MapNavigator._ZOOM_FULL_BUTTON, Type = typeof(Button))]
    public class MapNavigator : Navigation
    {
        protected const string _ZOOM_BACK_BUTTON = "ZoomBackButton";
        protected const string _ZOOM_NEXT_BUTTON = "ZoomNextButton";
        protected const string _ZOOM_FULL_BUTTON = "ZoomFullButton";

        protected ButtonGrid ZoomBackButton = null;
        protected ButtonGrid ZoomNextButton = null;
        protected Button ZoomFullButton = null;

        private bool isFirstLoad = true;
        private bool isNewExtent = true;
        private int currentExtentIndex = -1;
        private List<Envelope> extentHistory = new List<Envelope>();

        #region Readonly Properties
        /// <summary>
        /// Get the current Application
        /// </summary>
        public MapApp CurrentApp
        {
            get { return Application.Current as MapApp; }
        }

        /// <summary>
        /// Get the main Map Page of the Application  
        /// </summary>
        public MapPage CurrentPage
        {
            get { return (CurrentApp == null) ? null : CurrentApp.RootVisual as MapPage; }
        }
        #endregion

        public MapNavigator()
        {
            this.DefaultStyleKey = typeof(MapNavigator);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ZoomBackButton = this.GetTemplateChild(_ZOOM_BACK_BUTTON) as ButtonGrid;
            if (ZoomBackButton != null)
            {
                ZoomBackButton.IsEnabled = false;
                ZoomBackButton.MouseLeftButtonDown += new MouseButtonEventHandler(ZoomBackButton_MouseLeftButtonDown);
            }

            ZoomNextButton = this.GetTemplateChild(_ZOOM_NEXT_BUTTON) as ButtonGrid;
            if (ZoomNextButton != null)
            {
                ZoomNextButton.IsEnabled = false;
                ZoomNextButton.MouseLeftButtonDown += new MouseButtonEventHandler(ZoomNextButton_MouseLeftButtonDown);
            }

            ZoomFullButton = this.GetTemplateChild(_ZOOM_FULL_BUTTON) as Button;
            if (ZoomFullButton != null)
            {
                ZoomFullButton.Click += new RoutedEventHandler(ZoomFullButton_Click);
            }

            if (this.Map != null)
            {
                Map.ExtentChanged += new EventHandler<ExtentEventArgs>(Map_ExtentChanged);
            }
        }

        #region Map Navigator Event Handlers
        private void ZoomBackButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (currentExtentIndex > 0)
            {
                currentExtentIndex--;
                ZoomBackButton.IsEnabled = (currentExtentIndex > 0);

                isNewExtent = false;
                this.Map.ZoomTo(extentHistory[currentExtentIndex]);
                ZoomNextButton.IsEnabled = true;
            }
        }

        private void ZoomNextButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentExtentIndex < extentHistory.Count - 1)
            {
                currentExtentIndex++;
                ZoomNextButton.IsEnabled = (currentExtentIndex < extentHistory.Count - 1);

                isNewExtent = false;
                this.Map.ZoomTo(extentHistory[currentExtentIndex]);
                ZoomBackButton.IsEnabled = true;
            }
        }

        private void ZoomFullButton_Click(object sender, RoutedEventArgs e)
        {
            this.Map.ZoomTo(this.CurrentPage.FullMapExtent);
        }

        private void Map_ExtentChanged(object sender, ExtentEventArgs args)
        {

            if (isFirstLoad && args.OldExtent != null) // Add Initial Extent
            {
                extentHistory.Add(args.OldExtent.Clone());
                currentExtentIndex++;
                isFirstLoad = false;
            }

            if (isNewExtent)
            {
                if (extentHistory.Count - currentExtentIndex - 1 > 0)
                    extentHistory.RemoveRange(currentExtentIndex + 1, (extentHistory.Count - currentExtentIndex - 1));

                extentHistory.Add(args.NewExtent.Clone());
                currentExtentIndex++;

                ZoomNextButton.IsEnabled = false;
                ZoomBackButton.IsEnabled = (currentExtentIndex > 0);
            }

            isNewExtent = true;
        }
        #endregion
    }
}
