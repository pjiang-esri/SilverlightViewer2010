using System;
using System.Net;
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit.Utilities;

namespace ESRI.SilverlightViewer.Utility
{
    public delegate void ArcGISLayerInfoReadyHandler(object sender, ArcGISLayerInfoEventArgs args);

    internal class ArcGISLayerInfoReader
    {
        public event ArcGISLayerInfoReadyHandler InfoReady;

        public ArcGISLayerInfoReader(string layerRestUrl)
        {
            WebClient jsonClient = new WebClient();
            jsonClient.OpenReadCompleted += new OpenReadCompletedEventHandler(WebClient_OpenReadCompleted);
            jsonClient.OpenReadAsync(new Uri(layerRestUrl + "?f=JSON", UriKind.Absolute));
        }

        private void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                Collection<Type> knowTypes = new Collection<Type>();
                knowTypes.Add(typeof(EsriFieldType));
                knowTypes.Add(typeof(EsriGeometryType));

                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(ArcGISLayerInfo), knowTypes);
                ArcGISLayerInfo layerInfo = dcjs.ReadObject(e.Result) as ArcGISLayerInfo;
                e.Result.Close();
                e.Result.Dispose();

                if (InfoReady != null)
                {
                    layerInfo.IsReady = true;
                    InfoReady(this, new ArcGISLayerInfoEventArgs(layerInfo));
                }
            }
            catch (Exception ex)
            {
                if (InfoReady != null)
                {
                    InfoReady(this, new ArcGISLayerInfoEventArgs(ex.Message));
                }
            }
        }
    }

    [DataContract]
    public class ArcGISLayerInfo
    {
        public bool IsReady { get; set; }

        public string LayerUrl { get; internal set; }

        [DataMember(Name = "id")]
        public string LayerID { get; set; }

        [DataMember(Name = "name")]
        public string LayerName { get; set; }

        [DataMember(Name = "type")]
        public string LayerType { get; set; }

        [DataMember(Name = "geometryType")]
        public string GeometryType { get; set; }

        [DataMember(Name = "displayField")]
        public string DisplayField { get; set; }

        [DataMember(Name = "objectIdField")]
        public string ObjectIdField { get; set; }

        [DataMember(Name = "typeIdField")]
        public string TypeIdField { get; set; }

        [DataMember(Name = "hasAttatchments")]
        public bool HasAttatchments { get; set; }

        [DataMember(Name = "extent")]
        public Envelope Extent { get; set; }

        [DataMember(Name = "fields")]
        public ArcGISLayerField[] Fields { get; set; }

        private ArcGISLayerInfoReadyHandler infoReadyHandler;

        public ArcGISLayerInfo(string layerUrl)
            : this(layerUrl, null)
        {
        }

        public ArcGISLayerInfo(string layerUrl, ArcGISLayerInfoReadyHandler infoReadyCallback)
        {
            this.IsReady = false;
            this.LayerUrl = layerUrl;
            this.infoReadyHandler = infoReadyCallback;
            ArcGISLayerInfoReader infoReader = new ArcGISLayerInfoReader(layerUrl);
            infoReader.InfoReady += new ArcGISLayerInfoReadyHandler(ArcGISLayerInfoReader_InfoReady);
        }

        private void ArcGISLayerInfoReader_InfoReady(object sender, ArcGISLayerInfoEventArgs args)
        {
            if (args.LayerInfo.IsReady)
            {
                args.LayerInfo.CopyPropertiesTo(this);
            }

            if (infoReadyHandler != null) infoReadyHandler(this, args);
        }
    }

    [DataContract]
    public class ArcGISLayerField
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "editable")]
        public bool Editable { get; set; }

        [DataMember(Name = "domain")]
        public ArcGISFieldDomain Domain { get; set; }
    }

    [DataContract]
    public class ArcGISFieldDomain
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "codedValues")]
        public CodedValueSources CodedValues { get; set; }
    }

    #region EsriGeometryType and EsriFieldType Enum
    public enum EsriGeometryType
    {
        esriGeometryNull = 0,
        esriGeometryPoint = 1,
        esriGeometryMultiPoint = 2,
        esriGeometryPolyline = 3,
        esriGeometryPolygon = 4,
        esriGeometryEnvelope = 5,
        esriGeometryPath = 6,
        esriGeometryAny = 7
    }

    public enum EsriFieldType
    {
        esriFieldTypeSmallInteger = 0,
        esriFieldTypeInteger = 1,
        esriFieldTypeSingle = 2,
        esriFieldTypeDouble = 3,
        esriFieldTypeString = 4,
        esriFieldTypeDate = 5,
        esriFieldTypeOID = 6,
        esriFieldTypeGeometry = 7,
        esriFieldTypeBlob = 8,
        esriFieldTypeRaster = 9,
        esriFieldTypeGUID = 10,
        esriFieldTypeGlobalID = 11
    }
    #endregion

    #region ArcGISLayerInfoEventArgs Class
    public class ArcGISLayerInfoEventArgs : System.EventArgs
    {
        private string _errorMsg = "";
        private ArcGISLayerInfo _layerInfo = null;

        public ArcGISLayerInfoEventArgs(ArcGISLayerInfo layerInfo)
        {
            this._layerInfo = layerInfo;
        }

        public ArcGISLayerInfoEventArgs(string error)
        {
            _errorMsg = error;
        }

        public string ErrorMessage { get { return _errorMsg; } }
        public ArcGISLayerInfo LayerInfo { get { return _layerInfo; } }
    }
    #endregion
}
