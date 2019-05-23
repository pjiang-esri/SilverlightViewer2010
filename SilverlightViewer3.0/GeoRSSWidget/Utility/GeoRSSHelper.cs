using System;
using System.Xml.Linq;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Bing;
using GeoRSSWidget.Config;

namespace GeoRSSWidget.Utility
{
    public class GeoRSSHelper
    {
        public const string NS_GEORSS = "http://www.georss.org/georss";
        public const string NS_ATOM = "http://www.w3.org/2005/Atom";
        public const string NS_GEO = "http://www.w3.org/2003/01/geo/wgs84_pos#";
        public const string NS_GML = "http://www.opengis.net/gml";

        public static GeoRSSSource ParseGeoRSSXml(string xmlGeoRSS, string filter, bool toMercator)
        {
            GeoRSSSource rssSource = new GeoRSSSource();

            XDocument xmlDoc = XDocument.Parse(xmlGeoRSS, LoadOptions.None);
            IEnumerable<XElement> children = null;

            switch (xmlDoc.Root.Name.LocalName.ToLower())
            {
                case "rss":
                    XElement channelElem = ((XElement)xmlDoc.Root.FirstNode);
                    children = channelElem.Elements();
                    rssSource.SourceType = GeoRSSSourceType.RSS;
                    break;

                case "feed":
                    children = xmlDoc.Root.Elements();
                    rssSource.SourceType = GeoRSSSourceType.ATOM;
                    break;
            }

            foreach (XElement elem in children)
            {
                switch (elem.Name.LocalName.ToLower())
                {
                    case "title": 
                        rssSource.Title = elem.Value; break;
                    case "link":
                        rssSource.Link = (elem.HasAttributes) ? elem.FirstAttribute.Value : elem.Value; break;
                    case "summary":
                    case "content":
                    case "description":
                        rssSource.Description = elem.Value; break;
                    case "pubdate":
                    case "updated":
                        DateTime pubdate;
                        DateTime.TryParse(elem.Value, out pubdate);
                        rssSource.pubDate = pubdate;
                        break;
                    case "item":
                    case "entry":
                        GeoRSSItem item = ParseGeoRSSItem(elem, toMercator);
                        bool containKey = string.IsNullOrEmpty(filter) ? true : item.Title.Contains(filter);
                        if (containKey) rssSource.Items.Add(item);
                        break;
                }
            }

            return rssSource;
        }

        public static GeoRSSItem ParseGeoRSSItem(XElement itemElement, bool toMercator)
        {
            GeoRSSItem item = new GeoRSSItem();
            IEnumerable<XElement> children = itemElement.Elements();
            List<XElement> geoElems = new List<XElement>();

            foreach (XElement elem in children)
            {
                if (elem.Name.NamespaceName.ToLower() == NS_GEO)
                {
                    geoElems.Add(elem);
                }
                else if (elem.Name.NamespaceName.ToLower() == NS_GEORSS)
                {
                    item.Geometry = ParseGeoRSSGeometry(elem, toMercator);
                }
                else
                {
                    switch (elem.Name.LocalName.ToLower())
                    {
                        case "title":
                            item.Title = elem.Value; break;
                        case "link":
                            item.Link = (elem.HasAttributes) ? elem.FirstAttribute.Value : elem.Value; break;
                        case "summary":
                        case "content":
                        case "description":
                            item.Description = elem.Value; break;
                        case "pubdate":
                        case "updated":
                            item.pubDate = DateTime.Parse(elem.Value); break;
                    }
                }

                if (geoElems.Count > 0 && item.Geometry == null)
                {
                    item.Geometry = ParseW3CGeometry(geoElems, toMercator);
                }
            }

            return item;
        }

        public static MapPoint ParseW3CGeometry(List<XElement> geoElems, bool toMercator)
        {
            MapPoint point = null;
            double lat = -999.0, lon = -999.0;

            foreach (XElement elem in geoElems)
            {
                if (elem.Name.LocalName.ToLower() == "point")
                {
                    foreach (XElement geoElem in elem.Elements())
                    {
                        if (geoElem.Name.LocalName == "lat")
                        {
                            lat = double.Parse(geoElem.Value);
                        }
                        else if (geoElem.Name.LocalName == "long")
                        {
                            lon = double.Parse(geoElem.Value);
                        }
                    }
                }
                else if (elem.Name.LocalName == "lat")
                {
                    lat = double.Parse(elem.Value);
                }
                else if (elem.Name.LocalName == "long")
                {
                    lon = double.Parse(elem.Value);
                }

                if (lat > -999.0 && lon > -999.0)
                {
                    point = new MapPoint(lon, lat, new SpatialReference(4326));
                    if (toMercator) point = point.GeographicToWebMercator();
                }
            }

            return point;
        }

        public static Geometry ParseGeoRSSGeometry(XElement geoElem, bool toMercator)
        {
            Geometry geometry = null;
            string[] coords = { };

            switch (geoElem.Name.LocalName)
            {
                case "box":
                    coords = geoElem.Value.Trim().Split(' ');
                    if (coords.Length == 2)
                    {
                        MapPoint point1 = new MapPoint(double.Parse(coords[1]), double.Parse(coords[0]), new SpatialReference(4326));
                        MapPoint point2 = new MapPoint(double.Parse(coords[3]), double.Parse(coords[2]), new SpatialReference(4326));
                        geometry = (toMercator) ? (new Envelope(point1.GeographicToWebMercator(), point2.GeographicToWebMercator())) : (new Envelope(point1, point2));
                    }
                    break;
                case "point":
                    coords = geoElem.Value.Trim().Split(' ');
                    if (coords.Length == 2)
                    {
                        MapPoint point = new MapPoint(double.Parse(coords[1]), double.Parse(coords[0]), new SpatialReference(4326));
                        geometry = (toMercator) ? point.GeographicToWebMercator() : point;
                    }
                    break;
                case "line":
                    geometry = CreatePolyline(geoElem.Value.Trim(), toMercator);
                    break;
                case "polygon":
                    geometry = CreatePolygon(geoElem.Value.Trim(), "", toMercator);
                    break;
                case "where":
                    string pointString = "";
                    XElement firstElem = (XElement)geoElem.FirstNode;
                    if (firstElem.Name.NamespaceName.ToLower() == NS_GML)
                    {
                        switch (firstElem.Name.LocalName.ToLower())
                        {
                            case "point":
                                pointString = ((XElement)firstElem.FirstNode).Value.Trim();
                                coords = pointString.Split(' ');
                                if (coords.Length == 2)
                                {
                                    MapPoint point = new MapPoint(double.Parse(coords[1]), double.Parse(coords[0]), new SpatialReference(4326));
                                    geometry = (toMercator) ? point.GeographicToWebMercator() : point;
                                }
                                break;
                            case "envelope":
                                pointString = ((XElement)firstElem.FirstNode).Value.Trim();
                                coords = pointString.Split(' ');
                                MapPoint point1 = new MapPoint(double.Parse(coords[1]), double.Parse(coords[0]), new SpatialReference(4326));
                                pointString = ((XElement)firstElem.LastNode).Value.Trim();
                                coords = pointString.Split(' ');
                                MapPoint point2 = new MapPoint(double.Parse(coords[1]), double.Parse(coords[0]), new SpatialReference(4326));
                                geometry = (toMercator) ? (new Envelope(point1.GeographicToWebMercator(), point2.GeographicToWebMercator())) : (new Envelope(point1, point2));
                                break;
                            case "linestring":
                                pointString = ((XElement)firstElem.FirstNode).Value.Trim();
                                geometry = CreatePolyline(pointString, toMercator);
                                break;
                            case "polygon":
                                string exteriorPoints = "";
                                string interiorPoints = "";
                                foreach (XElement elem in firstElem.Elements())
                                {
                                    if (elem.Name.LocalName.ToLower() == "exterior")
                                    {
                                        exteriorPoints = ((XElement)((XElement)elem.FirstNode).FirstNode).Value;
                                    }

                                    if (elem.Name.LocalName.ToLower() == "interior")
                                    {
                                        interiorPoints = ((XElement)((XElement)elem.FirstNode).FirstNode).Value;
                                    }
                                }

                                geometry = CreatePolygon(exteriorPoints, interiorPoints, toMercator);
                                break;
                        }
                    }

                    break;
            }

            return geometry;
        }

        private static Polyline CreatePolyline(string pointslist, bool toMercator)
        {
            PointCollection path = new PointCollection();
            string[] coords = pointslist.Split(' ');
            MapPoint point = null;
            double lat, lon;

            for (int i = 0; i < coords.Length; i += 2)
            {
                lat = double.Parse(coords[i]);
                lon = double.Parse(coords[i + 1]);
                point = new MapPoint(lon, lat, new SpatialReference(4326));
                path.Add((toMercator) ? point.GeographicToWebMercator() : point);
            }

            Polyline polyline = new Polyline();
            if (path.Count > 0)
            {
                polyline.SpatialReference = path[0].SpatialReference;
                polyline.Paths.Add(path);
            }

            return polyline;
        }

        private static Polygon CreatePolygon(string exteriorPoints, string interiorPoints, bool toMercator)
        {
            PointCollection exPath = new PointCollection();
            string[] exCoords = exteriorPoints.Split(' ');
            MapPoint point = null;
            double lat, lon;

            for (int i = 0; i < exCoords.Length; i += 2)
            {
                lat = double.Parse(exCoords[i]);
                lon = double.Parse(exCoords[i + 1]);
                point = new MapPoint(lon, lat, new SpatialReference(4326));
                exPath.Add((toMercator) ? point.GeographicToWebMercator() : point);
            }

            Polygon polygon = new Polygon();
            if (exPath.Count > 0)
            {
                polygon.SpatialReference = exPath[0].SpatialReference;
                polygon.Rings.Add(exPath);
            }

            if (!string.IsNullOrEmpty(interiorPoints))
            {
                PointCollection inPath = new PointCollection();
                string[] inCoords = interiorPoints.Split(' ');

                for (int i = 0; i < exCoords.Length; i += 2)
                {
                    lat = double.Parse(exCoords[i]);
                    lon = double.Parse(exCoords[i + 1]);
                    point = new MapPoint(lon, lat, new SpatialReference(4326));
                    inPath.Add((toMercator) ? point.GeographicToWebMercator() : point);
                }

                polygon.Rings.Add(inPath);
            }

            return polygon;
        }
    }
}
