using System;
using System.Linq;
using System.Windows;
using System.Collections.Specialized;
using ESRI.ArcGIS.Client;

namespace ESRI.SilverlightViewer.Widget
{
    #region Event Delegates
    /// <summary>
    /// MapLoadComplete Event Delegate - Event happens when all map layers are completely loaded
    /// </summary>
    public delegate void MapLoadCompleteEventHandler(object sender, RoutedEventArgs e);

    /// <summary>
    /// BaseMapLayerChange Event Delegate - Event happens when the Base Map Layer changes
    /// </summary>
    public delegate void BaseMapLayerChangeEventHandler(object sender, BaseMapLayerChangeEventArgs e);

    /// <summary>
    ///  MapLayerVisibilityChange Event Delegate - Event happens when a map layer's visibility changes
    /// </summary>
    public delegate void MapLayerVisibilityChangeEventHandler(object sender, MapLayerVisibilityChangeEventArgs e);

    /// <summary>
    /// WidgetSelectedGraphicChange - Event happens when a widget's SelectedGraphic change
    /// </summary>
    public delegate void WidgetSelectedGraphicChangeEventHandler(object sender, SelectedItemChangeEventArgs e);

    /// <summary>
    /// WidgetFeatureSetsChange - triggerred to notify other widgets that a widget's Graphics change
    /// </summary>
    public delegate void WidgetFeatureSetsChangeEventHandler(object sender, NotifyCollectionChangedEventArgs e);

    /// <summary>
    /// SelectedItemChange Event Delegate - Event happens when an item is selected in a FeatureGrid
    /// </summary>
    public delegate void SelectedItemChangeEventHandler(object sender, SelectedItemChangeEventArgs args);

    /// <summary>
    /// WidgetContentChange Event Delegate - Event happens when a Widget head button is clicked to switch its content panel
    /// </summary>
    public delegate void WidgetContentChangeEvent(object sender, WidgetContentChangeEventArgs args);
    #endregion

    public class EventCenter
    {
        #region Define MapLoadComplete Event Handler and Event
        private static MapLoadCompleteEventHandler MapLoadCompleteHandler;
        public static event MapLoadCompleteEventHandler MapLoadComplete
        {
            add
            {
                if (MapLoadCompleteHandler == null || !(MapLoadCompleteHandler.GetInvocationList().Contains(value)))
                {
                    MapLoadCompleteHandler += value;
                }
            }
            remove
            {
                MapLoadCompleteHandler -= value;
            }
        }
        #endregion

        #region Define BaseMapLayerChange Event Handler and Event
        private static BaseMapLayerChangeEventHandler BaseMapLayerChangeHandler;
        public static event BaseMapLayerChangeEventHandler BaseMapLayerChange
        {
            add
            {
                if (BaseMapLayerChangeHandler == null || !(BaseMapLayerChangeHandler.GetInvocationList().Contains(value)))
                {
                    BaseMapLayerChangeHandler += value;
                }
            }
            remove
            {
                BaseMapLayerChangeHandler -= value;
            }
        }
        #endregion

        #region Define MapLayerVisibilityChange Event Handler and Event
        private static MapLayerVisibilityChangeEventHandler MapLayerVisibilityChangeHandler;
        public static event MapLayerVisibilityChangeEventHandler MapLayerVisibilityChange
        {
            add
            {
                if (MapLayerVisibilityChangeHandler == null || !(MapLayerVisibilityChangeHandler.GetInvocationList().Contains(value)))
                {
                    MapLayerVisibilityChangeHandler += value;
                }
            }
            remove
            {
                MapLayerVisibilityChangeHandler -= value;
            }
        }
        #endregion

        #region Define WidgetFeatureSetsChange Event Handler and Event
        private static WidgetFeatureSetsChangeEventHandler WidgetFeatureSetsChangeHandler = null;
        public static event WidgetFeatureSetsChangeEventHandler WidgetFeatureSetsChange
        {
            add
            {
                if (WidgetFeatureSetsChangeHandler == null || !WidgetFeatureSetsChangeHandler.GetInvocationList().Contains(value))
                    WidgetFeatureSetsChangeHandler += value;
            }
            remove
            {
                WidgetFeatureSetsChangeHandler -= value;
            }
        }
        #endregion

        #region Define WidgetSelectedGraphicChange Event Handler and Event
        private static WidgetSelectedGraphicChangeEventHandler SelectedGraphicChangeHandler = null;
        public static event WidgetSelectedGraphicChangeEventHandler WidgetSelectedGraphicChange
        {
            add
            {
                if (SelectedGraphicChangeHandler == null || !SelectedGraphicChangeHandler.GetInvocationList().Contains(value))
                    SelectedGraphicChangeHandler += value;
            }
            remove
            {
                SelectedGraphicChangeHandler -= value;
            }
        }
        #endregion

        /// <summary>
        /// Notify widgets that all the map layers are completely loaded
        /// </summary>
        /// <param name="sender">Current Map Control</param>
        public static void DispatchMapLoadCompleteEvent(object sender, RoutedEventArgs args)
        {
            if (MapLoadCompleteHandler != null)
            {
                MapLoadCompleteHandler(sender, args);
            }
        }

        /// <summary>
        /// Notify widgets that the Base Map Layer has changed
        /// </summary>
        /// <param name="sender">The Taskbar Widget</param>
        public static void DispatchBaseMapLayerChangeEvent(object sender, BaseMapLayerChangeEventArgs args)
        {
            if (BaseMapLayerChangeHandler != null)
            {
                BaseMapLayerChangeHandler(sender, args);
            }
        }

        /// <summary>
        /// Notify other widgets that a map layer's or a feature layer's visibility has changed
        /// </summary>
        /// <param name="sender">TOC Widget</param>
        public static void DispatchMapLayerVisibilityChangeEvent(object sender, MapLayerVisibilityChangeEventArgs args)
        {
            if (MapLayerVisibilityChangeHandler != null)
            {
                MapLayerVisibilityChangeHandler(sender, args);
            }
        }

        /// <summary>
        /// Notify other widgets that the SelectedGraphic of a widget has changed
        /// </summary>
        /// <param name="sender">Widget whose SelectedGraphic has changed</param>
        public static void DispatchWidgetSelectedGraphicChangeEvent(object sender, SelectedItemChangeEventArgs args)
        {
            if (SelectedGraphicChangeHandler != null)
            {
                SelectedGraphicChangeHandler(sender, args);
            }
        }

        /// <summary>
        /// Notify other widgets that the FeatureSet collection of a widget has changed
        /// </summary>
        /// <param name="sender">The widget whose FeatureSet collection has changed</param>
        public static void DispacthWidgetFeatureSetsChangeEvent(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (WidgetFeatureSetsChangeHandler != null)
            {
                WidgetFeatureSetsChangeHandler(sender, args);
            }
        }
    }

    #region Event Arguments

    /// <summary>
    /// BaseMapLayerChange Event Argument - Used in the Taskbar widget
    /// </summary>
    public class BaseMapLayerChangeEventArgs : EventArgs
    {
        private string _baseMapID = "";

        public BaseMapLayerChangeEventArgs(string baseMapID)
        {
            this._baseMapID = baseMapID;
        }

        public string BaseMapID { get { return this._baseMapID; } }
    }

    /// <summary>
    /// MapLayerVisibilityChange Event Argument - Used in the TOC widget
    /// </summary>
    public class MapLayerVisibilityChangeEventArgs : EventArgs
    {
        Layer _mapLayer;
        int[] _oldVisibles = null;

        public MapLayerVisibilityChangeEventArgs(Layer mapLayer)
        {
            this._mapLayer = mapLayer;
        }

        public MapLayerVisibilityChangeEventArgs(Layer mapLayer, int[] oldVisibleLayers)
        {
            this._mapLayer = mapLayer;
            this._oldVisibles = oldVisibleLayers;
        }

        /// <summary>
        /// ArcGIS cached map service, dynamic map service, or feature layer
        /// </summary>
        public Layer MapLayer { get { return _mapLayer; } }

        /// <summary>
        /// Old visible feature layer ID(s). If it equals NULL, the MapLayer's visibility has changed 
        /// </summary>
        public int[] OldVisibleLayers { get { return _oldVisibles; } }
    }

    /// <summary>
    /// SelectedItemChange Event Argument - Used in FeaturesGrid and Widget
    /// </summary>
    public class SelectedItemChangeEventArgs : EventArgs
    {
        Graphic _feature = null;

        public SelectedItemChangeEventArgs(Graphic feature)
        {
            this._feature = feature;
        }

        public Graphic Feature
        {
            get { return this._feature; }
        }
    }

    /// <summary>
    /// WidgetContentChange Event Argument - Used in WidegetBase 
    /// </summary>
    public class WidgetContentChangeEventArgs : EventArgs
    {
        int _oldIndex = -1;
        int _newIndex = -1;

        public WidgetContentChangeEventArgs(int oldIndex, int newIndex)
        {
            this._oldIndex = oldIndex;
            this._newIndex = newIndex;
        }

        public int OldIndex
        {
            get { return this._oldIndex; }
        }

        public int NewIndex
        {
            get { return this._newIndex; }
        }
    }
    #endregion
}
