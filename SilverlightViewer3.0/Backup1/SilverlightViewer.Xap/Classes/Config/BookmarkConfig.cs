using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class BookmarkConfig : WidgetConfigBase
    {
        [XmlAttribute(AttributeName = "storagePlace")]
        public string StoragePlace { get; set; }

        [XmlArray(ElementName = "Bookmarks")]
        [XmlArrayItem(ElementName = "Bookmark", Type = typeof(Bookmark))]
        public Bookmark[] Bookmarks { get; set; }
    }

    public partial class Bookmark
    {
        public bool UserAdded { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "extent")]
        public Extent Extent { get; set; }
    }
}
