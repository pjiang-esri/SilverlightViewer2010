using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public class PrintWidgetConfig : WidgetConfigBase
    {
        [XmlElement(typeof(string), ElementName = "ExportMapTaskUrl")]
        public string ExportMapTaskUrl { get; set; }
    }
}
