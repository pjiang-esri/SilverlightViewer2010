using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;

namespace ESRI.SilverlightViewer.Utility
{
    public delegate void ProjectionEventHandler(object sender, ProjectionEventArgs args);

    public class GeometryTool
    {
        // When zoom map to point, add a buffer around the point 
        public const double MINIMUM_MAP_WIDTH = 1000; // Meters

        #region Check the spatial reference of a geometry
        /// <summary>
        /// Check if the Spatial Reference equals the Mercator Spatial Reference (WKID 102100)
        /// </summary>
        public static bool IsWebMercatorSR(int wkid)
        {
            //return (wkid == 102100 || wkid == 102113 || wkid == 3857 || wkid == 3785);
            return SpatialReference.AreEqual(new SpatialReference(102100), new SpatialReference(wkid), false);
        }

        /// <summary>
        /// Check if the Spatial Reference equals the Mercator Spatial Reference (WKID 102100)
        /// </summary>
        public static bool IsWebMercatorSR(SpatialReference sr)
        {
            //return (wkid == 102100 || wkid == 102113 || wkid == 3857 || wkid == 3785);
            return SpatialReference.AreEqual(new SpatialReference(102100), sr, false);
        }

        /// <summary>
        /// Check if the Spatial Reference equals Geographic Spatial Reference (WKID 4326)
        /// </summary>
        public static bool IsGeographicSR(int wkid)
        {
            return SpatialReference.AreEqual(new SpatialReference(4326), new SpatialReference(wkid), false);
        }

        /// <summary>
        /// Check if the Spatial Reference equals Geographic Spatial Reference (WKID 4326)
        /// </summary>
        public static bool IsGeographicSR(SpatialReference sr)
        {
            return SpatialReference.AreEqual(new SpatialReference(4326), sr, false);
        }
        #endregion

        #region Convert geometries of other type to polygons
        /// <summary>
        /// Convert Freehand Polyline to a Freehand Polygon
        /// </summary>
        /// <param name="freeline">Freehandly drawn polyline</param>
        /// <returns>A freehand polygon</returns>
        public static Polygon FreehandToPolygon(Polyline freeline)
        {
            if (freeline == null) return null;

            Polygon polygonFromFreeline = null;

            //  Reference the PointCollection of the drawn Freehand Polyline
            ObservableCollection<PointCollection> pointsCollection = freeline.Paths;

            if (pointsCollection.Count > 0)
            {
                if (pointsCollection[0].Count > 0)
                {
                    // Get the first Point and "close" the shape and form a polygon.
                    MapPoint startingPt = pointsCollection[0][0];
                    pointsCollection[0].Add(startingPt.Clone());

                    polygonFromFreeline = new ESRI.ArcGIS.Client.Geometry.Polygon();
                    polygonFromFreeline.SpatialReference = freeline.SpatialReference;
                    polygonFromFreeline.Rings.Add(pointsCollection[0]);
                }
            }

            return polygonFromFreeline;
        }

        /// <summary>
        /// Convert an Envelope geometry to a Polygon
        /// </summary>
        /// <param name="envelope">An envelope geometry</param>
        /// <returns>A polygon geometry</returns>
        public static Polygon EnvelopeToPolygon(Envelope envelope)
        {
            if (envelope == null) return null;

            Polygon polygon = new Polygon() { SpatialReference = envelope.SpatialReference };

            PointCollection pColl = new PointCollection();
            pColl.Add(new MapPoint() { X = envelope.XMin, Y = envelope.YMax });
            pColl.Add(new MapPoint() { X = envelope.XMax, Y = envelope.YMax });
            pColl.Add(new MapPoint() { X = envelope.XMax, Y = envelope.YMin });
            pColl.Add(new MapPoint() { X = envelope.XMin, Y = envelope.YMin });
            pColl.Add(new MapPoint() { X = envelope.XMin, Y = envelope.YMax });
            polygon.Rings.Add(pColl);

            return polygon;
        }
        #endregion

        #region Expand a geometry's extent - Help map zoom to the geometry
        /// <summary>
        /// Expand a geometry's extent - Help Map zoom to the geometry
        /// </summary>
        /// <param name="geometry">Geometry parameter: Point, Polyline, or Polygon</param>
        /// <param name="percent">Percentage to expand the geometry's extent</param>
        /// <returns>An expanded envelope</returns>
        public static Envelope ExpandGeometryExtent(Geometry geometry, double percent)
        {
            if (geometry != null && Application.Current.RootVisual != null)
            {
                Envelope ext = geometry.Extent;
                ScaleLine.ScaleLineUnit mapUnits = (Application.Current.RootVisual as MapPage).MapUnits;

                double radiusX = MINIMUM_MAP_WIDTH / 2.0;
                double radiusY = MINIMUM_MAP_WIDTH / 2.0;
                double minimum = MINIMUM_MAP_WIDTH;

                if (mapUnits == ScaleLine.ScaleLineUnit.DecimalDegrees)
                {
                    double[] conversion = UnitsHelper.GetConversionToDegree((ext.YMin + ext.YMax) / 2.0, ScaleLine.ScaleLineUnit.Meters);
                    radiusX *= conversion[0];
                    radiusY *= conversion[1];
                    minimum *= conversion[0];
                }
                else
                {
                    double conversion = UnitsHelper.GetConversionFromMeter(mapUnits);
                    radiusX *= conversion;
                    radiusY *= conversion;
                    minimum *= conversion;
                }

                double bufferX = (ext.Width < minimum) ? radiusX : (ext.Width * percent / 2.0);
                double bufferY = (ext.Height < minimum) ? radiusY : (ext.Height * percent / 2.0);

                ext.XMin = ext.XMin - bufferX;
                ext.XMax = ext.XMax + bufferX;
                ext.YMin = ext.YMin - bufferY;
                ext.YMax = ext.YMax + bufferY;

                return ext;
            }
            else return null;
        }
        #endregion

        #region Project an extent to the map's spatial references
        public static void ProjectEnvelope(string geometryServiceUrl, Envelope srcEnvelope, int outSRWKID, ProjectionEventHandler callback)
        {
            GeometryService geometryService = new GeometryService(geometryServiceUrl);
            geometryService.Failed += (obj, arg) =>
            {
                if (callback != null) callback(null, new ProjectionEventArgs(arg.Error.Message));
            };

            geometryService.ProjectCompleted += (obj, arg) =>
            {
                if (arg.Results.Count > 0)
                {
                    Envelope resultEnvelope = arg.Results[0].Geometry as Envelope;
                    if (callback != null) callback(null, new ProjectionEventArgs(resultEnvelope));
                }
            };

            List<Graphic> graphics = new List<Graphic>();
            graphics.Add(new Graphic() { Geometry = srcEnvelope });
            geometryService.ProjectAsync(graphics, new SpatialReference(outSRWKID));
        }
        #endregion

        #region Scale Help Functions
        private const int SCREEN_DPI = 120;
        private const double EARTH_RADIUS = 6378137; // Earth radius in meters (defaults to WGS84 / GRS80)
        private const double DEGREE_RADIANS = 0.017453292519943295769236907684886;
        private const double DEGREE_DISTANCE = EARTH_RADIUS * DEGREE_RADIANS;// distance of 1 degree at equator in meters

        public static double RatioScaleResolution(double centerY, int srWKID, ScaleLine.ScaleLineUnit mapUnits)
        {
            double ratio = 1.0;

            if (IsWebMercatorSR(srWKID))
            {
                // Transform Y center from web mercator to decimal degree
                centerY = Math.Min(Math.Max(centerY, -20037508.3427892), 20037508.3427892);

                MapPoint point = new MapPoint(0, centerY);
                centerY = point.WebMercatorToGeographic().Y;
                ratio = Math.Cos(centerY * DEGREE_RADIANS) * SCREEN_DPI * 39.3700787; // 39.3700787 = ScaleLine.ScaleLineUnit.Meters/ScaleLine.ScaleLineUnit.Inches
            }
            else if (mapUnits == ScaleLine.ScaleLineUnit.DecimalDegrees || mapUnits == ScaleLine.ScaleLineUnit.Undefined)
            {
                if (Math.Abs(centerY) > 90)
                    ratio = 0.0;
                else
                    ratio = Math.Cos(centerY * DEGREE_RADIANS) * DEGREE_DISTANCE * SCREEN_DPI * 39.3700787;
            }
            else
            {
                ratio = (double)SCREEN_DPI * (int)mapUnits / (int)ScaleLine.ScaleLineUnit.Inches;
            }

            return ratio;
        }
        #endregion
    }

    #region Projection Event Arguments
    public class ProjectionEventArgs
    {
        private string _errMsg = "";
        private Geometry _resultGeometry = null;

        public ProjectionEventArgs(string error)
        {
            this._errMsg = error;
        }

        public ProjectionEventArgs(Geometry result)
        {
            this._resultGeometry = result;
        }

        public string ErrorMsg { get { return this._errMsg; } }
        public Geometry ProjectedGeometry { get { return this._resultGeometry; } }
    }
    #endregion
}
