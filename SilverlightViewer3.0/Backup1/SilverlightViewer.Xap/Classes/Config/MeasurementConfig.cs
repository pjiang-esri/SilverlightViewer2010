using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public class MeasurementConfig : WidgetConfigBase
    {
        [XmlElement(ElementName = "AlwaysProject")]
        public bool AlwaysProject { get; set; }

        [XmlElement(ElementName = "ProjectToWKID")]
        public int ProjectToWKID { get; set; }

        [XmlArray(ElementName = "LengthUnits")]
        [XmlArrayItem(typeof(MeasurementUnits), ElementName = "Units")]
        public MeasurementUnits[] LengthUnits { get; set; }

        [XmlArray(ElementName = "AreaUnits")]
        [XmlArrayItem(typeof(MeasurementUnits), ElementName = "Units")]
        public MeasurementUnits[] AreaUnits { get; set; }
    }

    public class MeasurementUnits
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
