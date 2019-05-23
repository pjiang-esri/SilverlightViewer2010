using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class IdentifyConfig : WidgetConfigBase
    {
        [XmlElement(ElementName = "Tolerance")]
        public int Tolerance { get; set; }

        [XmlElement(ElementName = "IdentifyOption")]
        public string IdentifyOption { get; set; }

        [XmlArray(ElementName = "IdentifyLayers")]
        [XmlArrayItem(typeof(IdentifyLayer), ElementName = "Layer")]
        public IdentifyLayer[] IdentifyLayers { get; set; }
    }

    public partial class IdentifyLayer
    {
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "layerIDs")]
        public string LayerIDs { get; set; }
    }
}
