using System;
using System.Text;
using System.Windows;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.SilverlightViewer.Config;

namespace ESRI.SilverlightViewer.Utility
{
    public class QueryTool
    {
        public delegate void QueryResultReady(object sender, QueryResultEventArgs args);
        public event QueryResultReady ResultReady;

        public QueryTool()
        {
        }

        public QueryTool(ArcGISQueryLayer queryLayer, ArcGISLayerInfo layerInfo)
        {
            this.QueryLayer = queryLayer;
            this.LayerInfo = layerInfo;
        }

        public ArcGISQueryLayer QueryLayer { get; set; }

        public ArcGISLayerInfo LayerInfo { get; set; }

        /// <summary>
        /// Do a where clause query
        /// </summary>
        /// <param name="where">The where clause used to filter the results</param>
        /// <param name="outSRWKID">The Spatial Reference WKID in which the results will be output</param>
        public void Search(string where, int outSRWKID)
        {
            Search(null, where, "", outSRWKID);
        }

        /// <summary>
        /// Do a where clause query
        /// </summary>
        /// <param name="where">The where clause used to filter the results</param>
        /// <param name="extraOutFields">List fields with ',' as a separator, which are not included in the QueryLayer.OutputFields, but must be output for other uses.</param>
        /// <param name="outSRWKID">The Spatial Reference WKID in which the query results will be output</param>
        public void Search(string where, string extraOutFields, int outSRWKID)
        {
            Search(null, where, extraOutFields, outSRWKID);
        }

        /// <summary>
        /// Do a spatial query 
        /// </summary>
        /// <param name="geometry">A spatial geometry used to filter the results</param>
        /// <param name="outSRWKID">The Spatial Reference WKID in which the results will be output</param>
        public void Search(ESRI.ArcGIS.Client.Geometry.Geometry geometry, int outSRWKID)
        {
            Search(geometry, "", "", outSRWKID);
        }

        /// <summary>
        /// Do a spatial query 
        /// </summary>
        /// <param name="where">The where clause used to filter the results</param>
        /// <param name="geometry">A spatial geometry used to filter the results</param>
        /// <param name="extraOutFields">List fields with ',' as a separator, which are not included in the QueryLayer.OutputFields, but must be output for other uses.</param>
        /// <param name="outSRWKID">The Spatial Reference WKID in which the results will be output</param>
        public void Search(ESRI.ArcGIS.Client.Geometry.Geometry geometry, string extraOutFields, int outSRWKID)
        {
            Search(geometry, "", extraOutFields, outSRWKID);
        }

        /// <summary>
        /// Do a spatial and where clause combination query 
        /// </summary>
        /// <param name="where">The where clause used to filter the results</param>
        /// <param name="geometry">A spatial geometry used to filter the results</param>
        /// <param name="extraOutFields">List fields with ',' as a separator, which are not included in the QueryLayer.OutputFields, but must be output for other uses.</param>
        /// <param name="outSRWKID">The Spatial Reference WKID in which the results will be output</param>
        public void Search(ESRI.ArcGIS.Client.Geometry.Geometry geometry, string where, string extraOutFields, int outSRWKID)
        {
            Query filter = new Query();
            filter.Where = where;
            filter.Geometry = geometry;
            filter.ReturnGeometry = true;
            filter.OutSpatialReference = new SpatialReference(outSRWKID);
            filter.SpatialRelationship = SpatialRelationship.esriSpatialRelIntersects;

            string[] outFields = this.QueryLayer.OutputFields.Split(',');
            for (int i = 0; i < outFields.Length; i++)
            {
                filter.OutFields.Add(outFields[i]);
            }

            if (this.LayerInfo != null && !filter.OutFields.Contains(this.LayerInfo.DisplayField))
            {
                filter.OutFields.Add(this.LayerInfo.DisplayField);
            }

            if (!string.IsNullOrEmpty(extraOutFields))
            {
                string[] extraFields = extraOutFields.Split(',');
                for (int i = 0; i < extraFields.Length; i++)
                {
                    if (!filter.OutFields.Contains(extraFields[i])) filter.OutFields.Add(extraFields[i]);
                }
            }

            QueryTask queryTask = new QueryTask();
            queryTask.Url = this.QueryLayer.RESTURL;
            queryTask.Failed += new EventHandler<TaskFailedEventArgs>(QueryTask_Failed);
            queryTask.ExecuteCompleted += new EventHandler<QueryEventArgs>(QueryTask_ExecuteCompleted);
            queryTask.ProxyURL = this.QueryLayer.ProxyUrl;
            queryTask.Token = this.QueryLayer.Token;
            queryTask.ExecuteAsync(filter);
        }

        /// <summary>
        /// Search arround a point within a specific radius
        /// </summary>
        /// <param name="center">The center point</param>
        /// <param name="radius">Search distance in meters</param>
        /// <param name="where">SQL where string</param>
        /// <param name="outSRWKID">Buffer circle output spatial reference</param>
        public void SearchNearby(MapPoint center, double radius, string where, int outSRWKID)
        {
            Polygon circle = null;
            SearchNearby(center, radius, where, outSRWKID, false, out circle);
        }

        /// <summary>
        /// Search arround a point within a specific radius
        /// </summary>
        /// <param name="center">The center point</param>
        /// <param name="radius">Search distance in meters</param>
        /// <param name="where">SQL where string</param>
        /// <param name="outSRWKID">Buffer circle output spatial reference</param>
        /// <param name="toMercator">Project the buffer circle's spatial reference into Mercator</param>
        /// <param name="circle">Output circle shape</param>
        public void SearchNearby(MapPoint center, double radius, string where, int outSRWKID, bool toMercator, out ESRI.ArcGIS.Client.Geometry.Polygon circle)
        {
            circle = CreateBufferCircle(center, radius, toMercator);
            Search(circle, where, outSRWKID);
        }

        private void QueryTask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            if (ResultReady != null)
            {
                ResultReady(this, new QueryResultEventArgs(e.FeatureSet, this.QueryLayer));
            }
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            ServiceException ex = e.Error as ServiceException;
            string error = ex.Message + ((ex.Details.Count > 0) ? ex.Details[0] : "Unknown Error");

            if (ResultReady != null)
            {
                ResultReady(this, new QueryResultEventArgs(error, this.QueryLayer));
            }
        }

        private Polygon CreateBufferCircle(MapPoint center, double radius, bool toMercator)
        {
            double pi36 = Math.PI / 180.0;
            double rx = radius, ry = radius;
            double dx = 1.0, dy = 1.0;

            ScaleLine.ScaleLineUnit units = UnitsHelper.GetUnitsByWKID(center.SpatialReference.WKID);
            if (units == ScaleLine.ScaleLineUnit.DecimalDegrees)
            {
                double[] conversion = UnitsHelper.GetConversionToDegree(center.Y, ScaleLine.ScaleLineUnit.Meters);
                rx = radius * conversion[0];
                ry = radius * conversion[1];
            }
            else
            {
                double conversion = UnitsHelper.GetConversionFromMeter(units);
                rx = radius * conversion;
                ry = radius * conversion;
            }

            PointCollection pointSet = new PointCollection();

            for (int i = 0; i <= 36; i++)
            {
                dx = center.X + rx * Math.Cos(i * 10 * pi36);
                dy = center.Y + ry * Math.Sin(i * 10 * pi36);
                MapPoint p = new MapPoint(dx, dy, new SpatialReference(center.SpatialReference.WKID));
                pointSet.Add((center.SpatialReference.WKID == 4326 && toMercator) ? p.GeographicToWebMercator() : p);
            }

            Polygon circle = new Polygon();
            circle.Rings.Add(pointSet);
            circle.SpatialReference = new SpatialReference((center.SpatialReference.WKID == 4326 && toMercator) ? 102113 : center.SpatialReference.WKID);

            return circle;
        }
    }

    public class QueryResultEventArgs : System.EventArgs
    {
        private string errorMsg = "";
        private FeatureSet _fset = null;
        private ArcGISQueryLayer _queryLayer = null;

        public QueryResultEventArgs(FeatureSet fset, ArcGISQueryLayer queryLayer)
        {
            this._fset = fset;
            this._queryLayer = queryLayer;
        }

        public QueryResultEventArgs(string error, ArcGISQueryLayer queryLayer)
        {
            this.errorMsg = error;
            this._queryLayer = queryLayer;
        }

        public string ErrorMsg { get { return errorMsg; } }
        public FeatureSet Results { get { return _fset; } }
        public ArcGISQueryLayer QueryLayer { get { return _queryLayer; } }
    }
}
