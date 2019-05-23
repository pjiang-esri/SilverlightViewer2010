using System;
using System.Net;
using System.Windows;
using System.Xml.Serialization;

namespace SocialMediaWidget.Utility
{
    [XmlRoot(ElementName = "feed", Namespace = Namespaces.NS_ATOM)]
    public class TwitterResult
    {
        [XmlElement(Type = typeof(TweetItem), ElementName = "entry")]
        public TweetItem[] items { get; set; }

        public static TwitterResult Deserialize(string resultXml)
        {
            TwitterResult twResult = null;
            XmlSerializer serializer = new XmlSerializer(typeof(TwitterResult));

            using (System.IO.TextReader textReader = new System.IO.StringReader(resultXml))
            {
                twResult = (TwitterResult)serializer.Deserialize(textReader);
            }

            return twResult;
        }
    }

    public class TweetItem
    {
        private string _location;

        [XmlElement(ElementName = "title")]
        public string title { get; set; }

        [XmlElement(ElementName = "content")]
        public string content { get; set; }

        [XmlElement(ElementName = "published")]
        public string publishDate { get; set; }

        [XmlElement(ElementName = "author")]
        public TweetAuthor author { get; set; }

        [XmlElement(ElementName = "link")]
        public AuthorLink[] authorLinks { get; set; }

        [XmlElement(ElementName = "geo", Namespace = Namespaces.NS_TWITTER)]
        public GeoRSSPoint geoLocation { get; set; }

        [XmlElement(ElementName = "location", Namespace = Namespaces.NS_GOOGLE)]
        public string googleLocation
        {
            get { return _location; }
            set
            {
                _location = value;
                if (_location.StartsWith("ÜT:") && (geoLocation.position == null))
                {
                    geoLocation.position = _location.Substring(4);
                }
            }
        }
    }

    public class TweetAuthor
    {
        [XmlElement(ElementName = "name")]
        public string authorName { get; set; }

        [XmlElement(ElementName = "uri")]
        public string authorUri { get; set; }
    }

    public class AuthorLink
    {
        [XmlAttribute(AttributeName = "type")]
        public string linkType { get; set; }

        [XmlAttribute(AttributeName = "href")]
        public string linkUrl { get; set; }
    }

    public class GeoRSSPoint
    {
        private double _x, _y;
        private string _coords;

        [XmlElement(ElementName = "point", Namespace = Namespaces.NS_GEORSS)]
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
