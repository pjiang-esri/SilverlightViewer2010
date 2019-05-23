using System;
using System.Xml.Serialization;
using ESRI.ArcGIS.Client.Bing; 

namespace ESRI.SilverlightViewer.Config
{
    public partial class MapConfig
    {
        [XmlElement(typeof(Extent), ElementName = "FullExtent")]
        public Extent FullExtent { get; set; }

        [XmlElement(typeof(Extent), ElementName = "InitialExtent")]
        public Extent InitialExtent { get; set; }

        [XmlElement(ElementName = "BaseMap")]
        public BaseMapConfig BaseMap { get; set; }

        [XmlArray(ElementName = "LivingMaps")]
        [XmlArrayItem(typeof(LivingMapLayer), ElementName = "Layer")]
        public LivingMapLayer[] LivingMaps { get; set; }
    }

    public partial class BaseMapConfig
    {
        [XmlAttribute(AttributeName = "enable")]
        public ServiceSource EnableBase { get; set; }

        [XmlElement(typeof(BingBaseMap), ElementName = "BingMap")]
        public BingBaseMap BingBaseMap { get; set; }

        [XmlElement(typeof(ArcGISBaseMap), ElementName = "ArcGISMap")]
        public ArcGISBaseMap ArcGISBaseMap { get; set; }
    }

    #region BING and ArcGIS Base Map Config
    public partial class BingBaseMap
    {
        [XmlElement(ElementName = "Token")]
        public string Token { get; set; }

        [XmlElement(ElementName = "Server")]
        public ServerType ServerType { get; set; }

        [XmlArray(ElementName = "Layers")]
        [XmlArrayItem(typeof(BingBaseMapLayer), ElementName = "Layer")]
        public BingBaseMapLayer[] Layers { get; set; }
    }

    public partial class ArcGISBaseMap
    {
        [XmlArray(ElementName = "Layers")]
        [XmlArrayItem(typeof(ArcGISBaseMapLayer), ElementName = "Layer")]
        public ArcGISBaseMapLayer[] Layers { get; set; }

        [XmlElement(ElementName = "LabelLayer")]
        public ArcGISMapLayer LabelLayer { get; set; }
    }
    #endregion

    #region Map Layer Config

    // Shared Properties by 
    // BingBaseMapLayer, ArcGISBaseMapLayer, and 
    public abstract class LayerConfig
    {
        public abstract string ID { get; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "icon")]
        public string IconSource { get; set; }

        [XmlAttribute(AttributeName = "visibleInitial")]
        public bool VisibleInitial { get; set; }
    }

    public partial class BingBaseMapLayer : LayerConfig
    {
        public override string ID
        {
            get
            {
                return string.Format("bingBase_{0}", this.Title.Replace(' ', '_'));
            }
        }

        [XmlAttribute(AttributeName = "type")]
        public BingLayerType LayerType { get; set; }
    }

    public partial class ArcGISMapLayer : LayerConfig
    {
        private Guid guid = Guid.NewGuid();

        public override string ID
        {
            get
            {
                return guid.ToString("N");
            }
        }

        [XmlAttribute(AttributeName = "serviceType")]
        public ArcGISServiceType ServiceType { get; set; }

        [XmlAttribute(AttributeName = "restURL")]
        public string RESTURL { get; set; }

        [XmlAttribute(AttributeName = "proxyURL")]
        public string ProxyURL { get; set; }

        [XmlAttribute(AttributeName = "token")]
        public string Token { get; set; }
    }

    public partial class ArcGISBaseMapLayer : ArcGISMapLayer
    {
        public override string ID
        {
            get
            {
                return string.Format("esriBase_{0}", this.Title.Replace(' ', '_'));
            }
        }

        [XmlAttribute(AttributeName = "showLabel")]
        public bool ShowLabel { get; set; }
    }

    public partial class LivingMapLayer : ArcGISMapLayer
    {
        private Guid guid = Guid.NewGuid();

        public override string ID
        {
            get { return "map_" + guid.ToString("N"); }
        }

        [XmlAttribute(AttributeName = "opacityBar")]
        public bool OpacityBar { get; set; }

        [XmlAttribute(AttributeName = "toggleLayer")]
        public bool ToggleLayer { get; set; }

        [XmlAttribute(AttributeName = "visibleLayers")]
        public string VisibleLayers { get; set; }

        [XmlAttribute(AttributeName = "refreshRate")]
        public double RefreshRate { get; set; }

        [XmlAttribute(AttributeName = "opacity")]
        public double Opacity { get; set; }

        /// <summary>
        /// Feature Layer Configuration Extension
        /// </summary>
        [XmlElement(typeof(FeatureLayerExtension), ElementName = "FeatureLayerExtension")]
        public FeatureLayerExtension FeatureLayerConfig { get; set; }
    }

    public partial class FeatureLayerExtension
    {
        /// <summary>
        /// Determine if cluster point type features using FlareSymbol
        /// </summary>
        [XmlElement(typeof(bool), ElementName = "UseCluster")]
        public bool UseCluster { get; set; }

        /// <summary>
        /// A where clause to filter the features 
        /// </summary>
        [XmlElement(typeof(string), ElementName = "WhereString")]
        public string WhereString { get; set; }

        /// <summary>
        /// An envelope to be used as a spatial filter
        /// </summary>
        [XmlElement(typeof(Extent), ElementName = "EnvelopeFilter")]
        public Extent EnvelopeFilter { get; set; }

        /// <summary>
        /// Symbolize point features with an image. Leave this blank for polygons and ploylines
        /// </summary>
        [XmlElement(typeof(string), ElementName = "SymbolImage")]
        public string SymbolImage { get; set; }

        /// <summary>
        /// A list out fields separated with ','
        /// </summary>
        [XmlElement(typeof(string), ElementName = "OutFields")]
        public string OutFields { get; set; }
    }
    #endregion
}
