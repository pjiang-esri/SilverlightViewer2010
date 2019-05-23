using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class SearchNearbyConfig : WidgetConfigBase
    {
        [XmlElement(ElementName = "ProjectToWKID")]
        public int ProjectToWKID { get; set; }

        [XmlArray(ElementName = "SearchLayers")]
        [XmlArrayItem(typeof(ArcGISQueryLayer), ElementName = "Layer")]
        public ArcGISQueryLayer[] SearchLayers { get; set; }
    }
}
