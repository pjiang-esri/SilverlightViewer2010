using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.SilverlightViewer.Data
{
    #region SortDescriptionGeoFeatureCollection Class - Implement NotifyCollectionChangedEvent
    /// <summary>
    /// Customized SortDescriptionCollection to implement NotifyCollectionChangedEvent 
    /// </summary>
    public class SortDescriptionGeoFeatureCollection : SortDescriptionCollection
    {
        public event NotifyCollectionChangedEventHandler SortDescriptionCollectionChanged
        {
            add
            {
                this.CollectionChanged += value;
            }
            remove
            {
                this.CollectionChanged -= value;
            }
        }
    }

    public class DeferRefreshHelper : IDisposable
    {
        private Action _callback;

        public DeferRefreshHelper(Action callback)
        {
            _callback = callback;
        }

        public void Dispose()
        {
            _callback();
        }
    }
    #endregion

    #region GeoFeatureCollection Class - Equivalent to FeatureSet
    public class GeoFeatureCollection : ObservableCollection<Graphic>
    {
        private string _outputFields = "";
        private string _outputLabels = "";
        private string _displayField = "";
        private string _featureLayer = "";
        private string _dataSourceName = "";
        private string _hyperlinkField = "";
        private GeometryType _geometryType;

        #region Properties
        public string OutputFields
        {
            get { return this._outputFields; }
            set { this._outputFields = value; }
        }

        public string OutputLabels
        {
            get { return this._outputLabels; }
            set { this._outputLabels = value; }
        }

        public string DisplayFieldName
        {
            get { return this._displayField; }
            set { this._displayField = value; }
        }

        public string FeatureLayerName
        {
            get { return this._featureLayer; }
            set { this._featureLayer = value; }
        }

        public string DataSourceName
        {
            get { return this._dataSourceName; }
            set { this._dataSourceName = value; }
        }

        public string HyperlinkField
        {
            get { return this._hyperlinkField; }
            set { this._hyperlinkField = value; }
        }

        public GeometryType GeometryType
        {
            get { return this._geometryType; }
            set { this._geometryType = value; }
        }
        #endregion

        public GeoFeatureCollection()
        {
        }

        public GeoFeatureCollection(FeatureSet fset, string featureLayerName) : this(fset, "", "", featureLayerName) { }

        public GeoFeatureCollection(FeatureSet fset, string outputFields, string outputLabels, string featureLayerName)
        {
            this._outputFields = outputFields;
            this._outputLabels = outputLabels; 
            this._featureLayer = featureLayerName;
            this._geometryType = fset.GeometryType;
            this._displayField = fset.DisplayFieldName;

            foreach (Graphic graphic in fset.Features)
            {
                graphic.Geometry.SpatialReference = fset.SpatialReference;
                this.Add(graphic);
            }
        }

        public GeoFeatureCollection(string displayFieldName, string featureLayerName)
        {
            this._displayField = displayFieldName;
            this._featureLayer = featureLayerName;
        }

        public GeoFeatureCollection(string displayFieldName, string featureLayerName, string dataSourceName)
        {
            this._displayField = displayFieldName;
            this._featureLayer = featureLayerName;
            this._dataSourceName = dataSourceName;
        }

        public GeoFeatureCollection(string outputFields, string outputLabels, string displayFieldName, string featureLayerName)
        {
            this._outputFields = outputFields;
            this._outputLabels = outputLabels; 
            this._displayField = displayFieldName;
            this._featureLayer = featureLayerName;
        }
    }
    #endregion 
}