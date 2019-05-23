using System;
using System.Xml.Serialization;
using ESRI.SilverlightViewer.Config;

namespace SocialMediaWidget.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class SocialMediaConfig : WidgetConfigBase
    {
        /// <summary>
        /// Window (Javascript or Browser) used to play YouTube videos
        /// </summary>
        [XmlElement(ElementName = "YouTubePlayerWindow")]
        public string YouTubePlayerWindow { get; set; }

        /// <summary>
        /// Quality to play YouTube videos
        /// </summary>
        [XmlElement(ElementName = "YouTubePlayeSize")]
        public string YouTubePlayerSize { get; set; }

        /// <summary>
        /// Maximum Search Radius in Kilometers
        /// </summary>
        [XmlElement(ElementName = "MaximumSearchRadius")]
        public double MaximumSearchRadius { get; set; }

        /// <summary>
        /// Default Search Radius in Kilometers
        /// </summary>
        [XmlElement(ElementName = "DefaultSearchRadius")]
        public double DefaultSearchRadius { get; set; }
    }
}
