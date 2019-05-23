using System;
using System.Xml.Serialization;
using ESRI.SilverlightViewer.Config;

namespace BusinessAnalystWidget.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class BusinessAnalystConfig : WidgetConfigBase
    {
        [XmlElement(ElementName = "Username")]
        public string Username { get; set; }

        [XmlElement(ElementName = "Password")]
        public string Password { get; set; }

        [XmlElement(ElementName = "ProjectToWKID")]
        public int ProjectToWKID { get; set; }

        [XmlElement(ElementName = "ReportFormat")]
        public string ReportFormat { get; set; }
    }
}
