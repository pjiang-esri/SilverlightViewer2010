using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class IncidentWidgetConfig : WidgetConfigBase
    {
        private Guid _guid = Guid.NewGuid();

        public Guid ID { get { return _guid; } }

        [XmlElement(ElementName = "LayerUrl")]
        public string LayerUrl { get; set; }

        [XmlElement(ElementName = "FilterFields")]
        public string FilterFields { get; set; }

        [XmlElement(ElementName = "OutputFields")]
        public string OutputFields { get; set; }

        [XmlElement(ElementName = "UseTimeInfo")]
        public bool UseTimeInfo { get; set; }

        [XmlElement(ElementName = "TimeInterval")]
        public int TimeInterval { get; set; }

        [XmlElement(ElementName = "InitInterval")]
        public string InitInterval { get; set; }

        [XmlAttribute(AttributeName = "Opacity")]
        public double Opacity { get; set; }

        [XmlElement(ElementName = "ProxyUrl")]
        public string ProxyUrl { get; set; }

        [XmlElement(ElementName = "Token")]
        public string Token { get; set; }
    }
}
