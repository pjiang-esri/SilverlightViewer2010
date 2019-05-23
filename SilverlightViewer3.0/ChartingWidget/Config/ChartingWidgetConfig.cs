using System;
using System.Xml.Serialization;
using ESRI.SilverlightViewer.Config;

namespace ChartingWidget.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class ChartingConfig : WidgetConfigBase
    {
        [XmlArray(ElementName = "ChartingLayers")]
        [XmlArrayItem(typeof(ChartQueryLayer), ElementName = "Layer")]
        public ChartQueryLayer[] ChartLayers { get; set; }
    }

    public partial class ChartQueryLayer : ArcGISQueryLayer
    {
        [XmlElement(ElementName = "ChartOutput")]
        public ChartOutputConfig ChartOutput { set; get; }
    }

    public partial class ChartOutputConfig
    {
        [XmlAttribute(AttributeName = "type")]
        public string ChartType { get; set; }

        [XmlAttribute(AttributeName = "orientation")]
        public string Orientation { get; set; }

        [XmlAttribute(AttributeName = "independentField")]
        public string IndependentField { get; set; }

        [XmlAttribute(AttributeName = "independentAxisTitle")]
        public string IndependentAxisTitle { get; set; }

        [XmlAttribute(AttributeName = "dependentAxisTitle")]
        public string DependentAxisTitle { get; set; }

        [XmlAttribute(AttributeName = "dependentIsPercentage")]
        public bool DependentIsPercentage { get; set; }
    }
}
