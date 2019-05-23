using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.UIWidget;
using ESRI.SilverlightViewer.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.SilverlightViewer.Widget
{
    public static class WidgetManager
    {
        /// <summary>
        /// The application's Taskbar
        /// </summary>
        public static TaskbarWidget Taskbar { get; set; }

        /// <summary>
        /// The application's OverviewMap
        /// </summary>
        public static OverviewMapWidget OverviewMap { get; set; }

        /// <summary>
        /// The collection of all the application widgets
        /// </summary>
        public static List<WidgetBase> Widgets = new List<WidgetBase>();

        /// <summary>
        /// Find Widget by Title
        /// </summary>
        /// <param name="title">Widget's title</param>
        /// <returns>Widget found</returns>
        public static WidgetBase FindWidgetByTitle(string title)
        {
            return Widgets.FirstOrDefault(w => w.Title == title);
        }

        /// <summary>
        /// Find Widget by Type
        /// </summary>
        /// <param name="title">Widget's Type</param>
        /// <returns>Widget found</returns>
        public static WidgetBase FindWidgetByType(Type type)
        {
            return Widgets.FirstOrDefault(w => w.GetType() == type);
        }

        /// <summary>
        /// Find Active Widget
        /// </summary>
        /// <returns>Active Widget found</returns>
        public static WidgetBase FindActiveWidget()
        {
            return Widgets.FirstOrDefault(w => w.IsActive);
        }

        /// <summary>
        /// Get a widget's Selected Graphic
        /// </summary>
        public static Graphic GetWidgetSelectedGraphic(string title)
        {
            WidgetBase widget = FindWidgetByTitle(title);

            if (widget != null && widget.HasGraphics)
            {
                return widget.SelectedGraphic;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get a widget's current Graphics 
        /// </summary>
        public static ObservableCollection<GeoFeatureCollection> GetWidgetGraphics(string title)
        {
            WidgetBase widget = FindWidgetByTitle(title);

            if (widget != null && widget.HasGraphics)
                return widget.FeatureSets;
            else
                return new ObservableCollection<GeoFeatureCollection>();
        }

        public static List<int> GetLivingMapVisibleLayerIDs(string title)
        {
            List<int> visLayerIDs = new List<int>();
            Config.LivingMapLayer[] livingMaps = (Application.Current as MapApp).AppConfig.MapConfig.LivingMaps;

            return visLayerIDs;
        }

        /// <summary>
        /// Reset the DrawObject Mode upon the Taskbar Widget's Tool Mode
        /// </summary>
        public static void ResetDrawObject()
        {
            Taskbar.ResetDrawObjectMode();
        }
    }
}
