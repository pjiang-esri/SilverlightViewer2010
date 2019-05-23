using System;
using System.Net;
using System.Windows;
using System.Xml.Serialization;

namespace SocialMediaWidget.Utility
{
    [XmlRoot(ElementName = "rsp")]
    public class FlickrResult
    {
        [XmlElement(ElementName = "photos")]
        public FlickrPhotos photoPage { get; set; }

        public static FlickrResult Deserialize(string resultXml)
        {
            FlickrResult frResult = null;
            XmlSerializer serializer = new XmlSerializer(typeof(FlickrResult));

            using (System.IO.TextReader textReader = new System.IO.StringReader(resultXml))
            {
                frResult = (FlickrResult)serializer.Deserialize(textReader);
            }

            return frResult;
        }
    }

    public class FlickrPhotos
    {
        [XmlAttribute(AttributeName = "page")]
        public int page { get; set; }

        [XmlAttribute(AttributeName = "pages")]
        public int totalPages { get; set; }

        [XmlAttribute(AttributeName = "perpage")]
        public int NumPerPage { get; set; }

        [XmlAttribute(AttributeName = "total")]
        public int total { get; set; }

        [XmlElement(ElementName = "photo")]
        public FlickrPhoto[] photos { get; set; }
    }

    public class FlickrPhoto
    {
        [XmlAttribute(AttributeName = "id")]
        public string photoID { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string title { get; set; }

        [XmlAttribute(AttributeName = "secret")]
        public string secret { get; set; }

        [XmlAttribute(AttributeName = "server")]
        public string server { get; set; }

        [XmlAttribute(AttributeName = "farm")]
        public string farm { get; set; }

        [XmlAttribute(AttributeName = "ownername")]
        public string ownerName { get; set; }

        [XmlAttribute(AttributeName = "datetaken")]
        public string dateTaken { get; set; }

        [XmlAttribute(AttributeName = "o_width")]
        public double width { get; set; }

        [XmlAttribute(AttributeName = "o_height")]
        public double height { get; set; }

        [XmlAttribute(AttributeName = "latitude")]
        public double latitude { get; set; }

        [XmlAttribute(AttributeName = "longitude")]
        public double longitude { get; set; }

        public string photoUrl
        {
            get
            {
                return "http://farm" + farm + ".static.flickr.com/" + server + "/" + photoID + "_" + secret + ".jpg";
            }
        }
    }
}
