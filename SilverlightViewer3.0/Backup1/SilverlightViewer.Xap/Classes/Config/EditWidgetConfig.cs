using System;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class EditWidgetConfig : WidgetConfigBase
    {
        [XmlAttribute(AttributeName = "continuousAction")]
        public bool ContinuousAction { get; set; }

        [XmlAttribute(AttributeName = "useFreehand")]
        public bool UseFreehand { get; set; }

        [XmlAttribute(AttributeName = "autoSelect")]
        public bool AutoSelect { get; set; }

        [XmlAttribute(AttributeName = "autoSave")]
        public bool AutoSave { get; set; }

        [XmlElement(ElementName = "DefaultAction")]
        public string DefaultAction { get; set; }

        [XmlArray(ElementName = "EditableLayers")]
        [XmlArrayItem(typeof(EditFeatureLayer), ElementName = "Layer")]
        public EditFeatureLayer[] EditableLayers { get; set; }
    }

    public partial class EditFeatureLayer
    {
        private Guid _guid = Guid.NewGuid();

        public Guid ID { get { return _guid; } }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "opacity")]
        public double Opacity { get; set; }

        [XmlAttribute(AttributeName = "visibleInitial")]
        public bool VisibleInitial { get; set; }

        [XmlAttribute(AttributeName = "outFields")]
        public string OutFields { get; set; }

        [XmlAttribute(AttributeName = "restURL")]
        public string RESTURL { get; set; }

        [XmlAttribute(AttributeName = "proxyURL")]
        public string ProxyURL { get; set; }

        [XmlAttribute(AttributeName = "token")]
        public string Token { get; set; }
    }
}
