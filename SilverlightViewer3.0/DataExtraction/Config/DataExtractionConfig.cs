using System;
using System.Xml.Serialization;
using ESRI.SilverlightViewer.Config;

namespace DataExtraction.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class DataExtractionConfig : WidgetConfigBase
    {
        [XmlElement(ElementName = "AOISelectionMethod")]
        public string AOISelectionMethod { get; set; }

        [XmlArray(ElementName = "DataExtractionServices")]
        [XmlArrayItem(typeof(DataExtractionService), ElementName = "Service")]
        public DataExtractionService[] DataExtractionServices { get; set; }
    }

    public partial class DataExtractionService
    {
        [XmlAttribute(AttributeName = "title")]
        public string title { get; set; }

        [XmlAttribute(AttributeName = "restURL")]
        public string restURL { get; set; }
    }
}
