using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class QueryConfig : WidgetConfigBase
    {
        [XmlArray(ElementName = "QueryLayers")]
        [XmlArrayItem(typeof(ArcGISQueryLayer), ElementName = "Layer")]
        public ArcGISQueryLayer[] QueryLayers { get; set; }
    }

    public partial class ArcGISQueryLayer
    {
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "restURL")]
        public string RESTURL { get; set; }

        [XmlElement(ElementName = "QueryFields")]
        public string QueryFields { get; set; }

        [XmlElement(ElementName = "OutputFields")]
        public string OutputFields { get; set; }

        [XmlElement(ElementName = "OutputLabels")]
        public string OutputLabels { get; set; }

        [XmlElement(ElementName = "MapTipTemplate")]
        public string MapTipTemplate { get; set; }

        [XmlElement(ElementName = "ProxyUrl")]
        public string ProxyUrl { get; set; }

        [XmlElement(ElementName = "Token")]
        public string Token { get; set; }
    }
}
