using System;
using System.Xml.Serialization;
using ESRI.SilverlightViewer.Config;

namespace RedlineWidget.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class RedlineConfig : WidgetConfigBase
    {
        [XmlElement(typeof(RedlineMeasurementUnits), ElementName = "Units")]
        public RedlineMeasurementUnits[] LengthUnits { get; set; }
    }

    public class RedlineMeasurementUnits
    {
        [XmlText(typeof(string))]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "abbreviation")]
        public string Abbreviation { get; set; }

        /// <summary>
        /// Convert to Meter or Square Meter
        /// </summary>
        [XmlAttribute(AttributeName = "conversion")]
        public double Conversion { get; set; }
    }
}
