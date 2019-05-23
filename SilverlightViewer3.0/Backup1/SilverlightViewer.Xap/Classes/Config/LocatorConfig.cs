using System;
using System.Xml.Serialization;
using ESRI.ArcGIS.Client.Bing;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class LocatorConfig : WidgetConfigBase
    {
        [XmlAttribute(AttributeName = "enable")]
        public ServiceSource EnableLocator { get; set; }

        [XmlElement(typeof(BingLocator), ElementName = "BingLocator")]
        public BingLocator BingLocator { get; set; }

        [XmlElement(typeof(ArcGISLocator), ElementName = "ArcGISLocator")]
        public ArcGISLocator ArcGISLocator { get; set; }

        [XmlElement(typeof(LocationSymbol), ElementName = "LocationSymbol")]
        public LocationSymbol LocationSymbol { get; set; }
    }

    public partial class BingLocator
    {
        [XmlElement(ElementName = "Token")]
        public string Token { get; set; }

        [XmlElement(ElementName = "Server")]
        public ServerType ServerType { get; set; }
    }

    public partial class ArcGISLocator
    {
        [XmlElement(ElementName = "RESTURL")]
        public string RESTURL { get; set; }

        [XmlElement(ElementName = "ParameterFields")]
        public ArcGISLocatorParams ParameterFields { get; set; }

        [XmlElement(ElementName = "SpatialReference")]
        public int SRWKID { get; set; }

        [XmlElement(ElementName = "ReverseSearchDistance")]
        public double SearchDistance { get; set; }

        [XmlElement(ElementName = "ReturnFirstCandidate")]
        public bool ReturnFirst { get; set; }
    }

    public partial class ArcGISLocatorParams
    {
        [XmlElement(ElementName = "Address")]
        public string AddressField { get; set; }

        [XmlElement(ElementName = "City")]
        public string CityField { get; set; }

        [XmlElement(ElementName = "State")]
        public string StateField { get; set; }

        [XmlElement(ElementName = "ZipCode")]
        public string ZipField { get; set; }

        [XmlElement(ElementName = "Country")]
        public string CountryField { get; set; }
    }

    public partial class LocationSymbol
    {
        [XmlAttribute(AttributeName = "symbolImage")]
        public string ImageSource { get; set; }

        [XmlAttribute(AttributeName = "offsetX")]
        public double OffsetX { get; set; }

        [XmlAttribute(AttributeName = "offsetY")]
        public double OffsetY { get; set; }
    }
}
