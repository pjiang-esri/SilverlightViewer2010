using System;
using System.Xml.Serialization;
using ESRI.SilverlightViewer.Controls;

namespace ESRI.SilverlightViewer.Config
{
    public partial class TaskbarConfig 
    {
        [XmlAttribute(AttributeName = "initialTop")]
        public double InitialTop { get; set; }

        [XmlAttribute(AttributeName = "initialLeft")]
        public double InitialLeft { get; set; }

        [XmlAttribute(AttributeName = "barWidth")]
        public double BarWidth { get; set; }

        [XmlAttribute(AttributeName = "barHeight")]
        public double BarHeight { get; set; }

        [XmlAttribute(AttributeName = "dockHeight")]
        public double DockHeight { get; set; }

        [XmlAttribute(AttributeName = "dockPosition")]
        public DockPosition DockPosition { get; set; }

        [XmlArray(ElementName = "MapToolButtons")]
        [XmlArrayItem(typeof(MapToolButton), ElementName = "ToolButton")]
        public MapToolButton[] MapToolButtons { get; set; }
    }

    public partial class MapToolButton
    {
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "isDefault")]
        public bool IsDefault { get; set; }

        [XmlAttribute(AttributeName = "mapAction")]
        public string MapAction { get; set; }

        [XmlAttribute(AttributeName = "buttonImage")]
        public string ButtonImage { get; set; }
    }
}
