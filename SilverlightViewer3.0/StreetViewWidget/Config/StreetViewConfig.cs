using System;
using System.Globalization;
using System.ComponentModel;
using System.Xml.Serialization;
using ESRI.SilverlightViewer.Config;

namespace StreetViewWidget.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class StreetViewConfig : WidgetConfigBase
    {
        [XmlElement(typeof(string), ElementName = "DisplayWindow")]
        public string DisplayWindow { get; set; }

        [XmlElement(typeof(int), ElementName = "WindowWidth")]
        public int WindowWidth { get; set; }

        [XmlElement(typeof(int), ElementName = "WindowHeight")]
        public int WindowHeight { get; set; }

        [XmlElement(typeof(string), ElementName = "GraphicIconSource")]
        public string GraphicIcon { get; set; }
    }
}
