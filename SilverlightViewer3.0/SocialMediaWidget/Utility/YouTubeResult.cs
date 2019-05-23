using System;
using System.Net;
using System.Windows;
using System.Xml.Serialization;

namespace SocialMediaWidget.Utility
{
    [XmlRoot(ElementName = "feed", Namespace = Namespaces.NS_ATOM)]
    public class YouTubeResult
    {
        [XmlElement(Type = typeof(YouTubeItem), ElementName = "entry")]
        public YouTubeItem[] items { get; set; }

        public static YouTubeResult Deserialize(string resultXml)
        {
            YouTubeResult ytResult = null;
            XmlSerializer serializer = new XmlSerializer(typeof(YouTubeResult));

            using (System.IO.TextReader textReader = new System.IO.StringReader(resultXml))
            {
                ytResult = (YouTubeResult)serializer.Deserialize(textReader);
            }

            return ytResult;
        }
    }

    public class YouTubeItem
    {
        [XmlElement(ElementName = "group", Namespace = Namespaces.NS_MEDIA)]
        public YouTubeMedia media { get; set; }

        [XmlElement(ElementName = "where", Namespace = Namespaces.NS_GEORSS)]
        public RSSGeometry geometry { get; set; }
    }

    public class YouTubeMedia
    {
        [XmlElement(ElementName = "videoid", Namespace = Namespaces.NS_YT)]
        public string videoID { get; set; }

        [XmlElement(ElementName = "title", Namespace = Namespaces.NS_MEDIA)]
        public string title { get; set; }

        [XmlElement(ElementName = "uploaded", Namespace = Namespaces.NS_YT)]
        public string publishDate { get; set; }

        [XmlElement(ElementName = "description", Namespace = Namespaces.NS_MEDIA)]
        public string description { get; set; }

        [XmlElement(ElementName = "aspectRatio", Namespace = Namespaces.NS_YT)]
        public string aspectRatio { get; set; }

        [XmlElement(ElementName = "content", Namespace = Namespaces.NS_MEDIA)]
        public YouTubeContent[] content { get; set; }

        [XmlElement(ElementName = "thumbnail", Namespace = Namespaces.NS_MEDIA)]
        public YouTubeThumbnail[] thumbnail { get; set; }
    }

    public class YouTubeContent
    {
        [XmlAttribute(AttributeName = "type")]
        public string type { get; set; }

        [XmlAttribute(AttributeName = "url")]
        public string source { get; set; }

        [XmlAttribute(AttributeName = "format", Namespace = Namespaces.NS_YT)]
        public string format { get; set; }
    }

    public class YouTubeThumbnail
    {
        [XmlAttribute(AttributeName = "width")]
        public int width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int height { get; set; }

        [XmlAttribute(AttributeName = "url")]
        public string url { get; set; }
    }

    public class RSSGeometry
    {
        [XmlElement(ElementName = "Point", Namespace = Namespaces.NS_GML)]
        public RSSPoint rssPoint { get; set; }
    }

    public class RSSPoint
    {
        private double _x, _y;
        private string _coords;

        [XmlElement(ElementName = "pos", Namespace = Namespaces.NS_GML)]
        public string position
        {
            get { return _coords; }

            set
            {
                _coords = value;
                if (!string.IsNullOrEmpty(_coords))
                {
                    string[] coords = _coords.Split(' ');
                    if (coords.Length > 1)
                    {
                        _y = double.Parse(coords[0]);
                        _x = double.Parse(coords[1]);
                    }
                }
            }
        }

        public double x { get { return _x; } set { _x = value; } }

        public double y { get { return _y; } set { _y = value; } }
    }
}
