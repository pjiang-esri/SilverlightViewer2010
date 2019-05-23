using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.SilverlightViewer.Config;

namespace GeoRSSWidget.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public class GeoRSSConfig : WidgetConfigBase
    {
        [XmlElement(ElementName = "Source")]
        public string SourceURL { get; set; }

        [XmlElement(ElementName = "Filter")]
        public string Filter { get; set; }

        [XmlElement(ElementName = "OutputFields")]
        public string OutputFields { get; set; }

        [XmlElement(ElementName = "TitleField")]
        public string TitleField { get; set; }

        [XmlElement(ElementName = "LinkField")]
        public string LinkField { get; set; }

        [XmlElement(ElementName = "RefreshRate")]
        public int RefreshRate { get; set; }

        [XmlElement(ElementName = "SymbolImage")]
        public string SymbolImage { get; set; }
    }

    public class GeoRSSSource
    {
        List<GeoRSSItem> _items = null;

        public GeoRSSSource()
        {
            _items = new List<GeoRSSItem>();
        }

        public GeoRSSSourceType SourceType {get; set;}

        public string Title { get; set; }

        public string Link { get; set; }

        public string Description { get; set; }

        public DateTime pubDate { get; set; }

        public List<GeoRSSItem> Items
        {
            get { return _items; }
        }
    }

    public class GeoRSSItem
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Link { get; set; }

        public DateTime pubDate { get; set; }

        public Geometry Geometry { get; set; }
    }

    public enum GeoRSSSourceType
    {
        RSS = 0,
        ATOM = 1
    }
}
