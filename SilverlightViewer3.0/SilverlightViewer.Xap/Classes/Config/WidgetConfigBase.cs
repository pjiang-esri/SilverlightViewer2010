using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    public abstract partial class WidgetConfigBase
    {
        public static WidgetConfigBase Deserialize(string configXml, Type widgetType)
        {
            WidgetConfigBase widgetConfig = null;
            XmlSerializer serializer = new XmlSerializer(widgetType);

            using (System.IO.TextReader textReader = new System.IO.StringReader(configXml))
            {
                widgetConfig = (WidgetConfigBase)serializer.Deserialize(textReader);
            }

            return widgetConfig;
        }
    }
}
