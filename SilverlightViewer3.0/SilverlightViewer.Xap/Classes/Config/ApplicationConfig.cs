using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "AppConfig")]
    public partial class ApplicationConfig
    {
        [XmlElement(ElementName = "ApplicationLogo")]
        public string ApplicationLogo { get; set; }

        [XmlElement(ElementName = "ApplicationTitle")]
        public string ApplicationTitle { get; set; }

        [XmlElement(ElementName = "ApplicationSubTitle")]
        public string ApplicationSubTitle { get; set; }

        [XmlElement(ElementName = "ApplicationHelpMenu")]
        public HelpMenuConfig AppHelpMenu { get; set; }

        [XmlElement(ElementName = "GeometryService")]
        public string GeometryService { get; set; }

        [XmlElement(ElementName = "Map")]
        public MapConfig MapConfig { get; set; }

        [XmlElement(ElementName = "Taskbar")]
        public TaskbarConfig TaskbarConfig { get; set; }

        [XmlArray(ElementName = "Widgets")]
        [XmlArrayItem(typeof(WidgetConfig), ElementName = "Widget")]
        public WidgetConfig[] WidgetsConfig { get; set; }

        [XmlElement(ElementName = "OverviewMapConfigFile")]
        public string OverviewMapConfigFile { get; set; }

        [XmlElement(ElementName = "Theme")]
        public string Theme { get; set; }

        public ApplicationConfig()
        {
        }

        public static ApplicationConfig Deserialize(string configXml)
        {
            ApplicationConfig appConfig = null;
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationConfig));

            using (System.IO.TextReader textReader = new System.IO.StringReader(configXml))
            {
                appConfig = (ApplicationConfig)serializer.Deserialize(textReader);
            }

            return appConfig;
        }
    }

    #region Widget Config
    public partial class WidgetConfig
    {
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "xapFile")]
        public string XapFile { get; set; }

        [XmlAttribute(AttributeName = "className")]
        public string ClassName { get; set; }

        [XmlAttribute(AttributeName = "openInitial")]
        public bool OpenInitial { get; set; }

        [XmlAttribute(AttributeName = "hasGraphics")]
        public bool HasGraphics { get; set; }

        [XmlAttribute(AttributeName = "initialTop")]
        public double InitialTop { get; set; }

        [XmlAttribute(AttributeName = "initialLeft")]
        public double InitialLeft { get; set; }

        [XmlAttribute(AttributeName = "icon")]
        public string IconSource { get; set; }

        [XmlAttribute(AttributeName = "configFile")]
        public string ConfigFile { get; set; }
    }
    #endregion

    #region Help Menu Config
    public partial class HelpMenuConfig
    {
        [XmlAttribute(AttributeName = "text")]
        public string MenuText { get; set; }

        [XmlAttribute(AttributeName = "icon")]
        public string MenuIcon { get; set; }

        [XmlArray(ElementName = "Links")]
        [XmlArrayItem(typeof(ApplicationLink), ElementName = "Link")]
        public ApplicationLink[] Links { get; set; }

        [XmlElement(ElementName = "About")]
        public ApplicationLink About { get; set; }
    }

    public partial class ApplicationLink
    {
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "icon")]
        public string Icon { get; set; }

        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }

        [XmlAttribute(AttributeName = "text")]
        public string Text { get; set; }
    }
    #endregion
}
