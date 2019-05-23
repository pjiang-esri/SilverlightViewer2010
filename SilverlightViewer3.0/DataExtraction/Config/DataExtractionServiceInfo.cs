using System;
using System.Net;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using ESRI.SilverlightViewer.Utility;

namespace DataExtraction.Utility
{
    public delegate void GPSServiceInfoReadyHandler(object sender, GPServiceInfoEventArgs args);

    internal class DataExtractionServiceInfoReader
    {
        public event GPSServiceInfoReadyHandler InfoReady;

        public DataExtractionServiceInfoReader(string serviceUrl)
        {
            WebClient jsonClient = new WebClient();
            jsonClient.OpenReadCompleted += new OpenReadCompletedEventHandler(WebClient_OpenReadCompleted);
            jsonClient.OpenReadAsync(new Uri(serviceUrl + "?f=JSON", UriKind.Absolute));
        }

        private void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(DataExtractionServiceInfo));
                DataExtractionServiceInfo serviceInfo = dcjs.ReadObject(e.Result) as DataExtractionServiceInfo;
                e.Result.Close();
                e.Result.Dispose();

                if (InfoReady != null)
                {
                    serviceInfo.IsReady = true;
                    InfoReady(this, new GPServiceInfoEventArgs(serviceInfo));
                }
            }
            catch (Exception ex)
            {
                if (InfoReady != null)
                {
                    InfoReady(this, new GPServiceInfoEventArgs(ex.Message));
                }
            }
        }
    }

    [DataContract]
    public class DataExtractionServiceInfo
    {
        private GPSServiceInfoReadyHandler infoReadyHandler = null;

        public DataExtractionServiceInfo(string serviceUrl)
            : this(serviceUrl, null)
        {
        }

        public DataExtractionServiceInfo(string serviceUrl, GPSServiceInfoReadyHandler infoReadyCallback)
        {
            this.IsReady = false;
            this.ServiceUrl = serviceUrl;
            this.infoReadyHandler = infoReadyCallback;
            DataExtractionServiceInfoReader infoReader = new DataExtractionServiceInfoReader(serviceUrl);
            infoReader.InfoReady += new GPSServiceInfoReadyHandler(GPServiceInfoReader_InfoReady);
        }

        private void GPServiceInfoReader_InfoReady(object sender, GPServiceInfoEventArgs args)
        {
            if (args.ServiceInfo.IsReady)
            {
                args.ServiceInfo.CopyPropertiesTo(this);
            }

            if (infoReadyHandler != null) infoReadyHandler(this, args);
        }

        public bool IsReady { get; set; }

        public string ServiceUrl { get; internal set; }

        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "displayName")]
        public string DisplayName { set; get; }

        [DataMember(Name = "category")]
        public string Category { set; get; }

        [DataMember(Name = "executionType")]
        public string ExecutionType { set; get; }

        [DataMember(Name = "parameters")]
        public GPServiceParameter[] Parameters { set; get; }
    }

    [DataContract]
    public class GPServiceParameter
    {
        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "dataType")]
        public string DataType { set; get; }

        [DataMember(Name = "displayName")]
        public string DisplayName { set; get; }

        [DataMember(Name = "direction")]
        public string ParameterDirection { set; get; }

        [DataMember(Name = "defaultValue")]
        public object DefaultValue { set; get; }

        [DataMember(Name = "parameterType")]
        public string ParameterType { set; get; }

        [DataMember(Name = "category")]
        public string Category { set; get; }

        [DataMember(Name = "choiceList")]
        public string[] ChoiceList { set; get; }
    }

    #region Enum Types and Enum Converter
    [DataContract]
    public enum EsriGPExecutionType
    {
        [EnumMember(Value = "esriExecutionTypeSynchronous")]
        esriExecutionTypeSynchronous = 0,
        [EnumMember(Value = "esriExecutionTypeAsynchronous")]
        esriExecutionTypeAsynchronous = 1
    }

    [DataContract]
    public enum EsriGPParameterDirection
    {
        [EnumMember(Value = "esriGPParameterDirectionInput")]
        esriGPParameterDirectionInput = 0,
        [EnumMember(Value = "esriGPParameterDirectionOutput")]
        esriGPParameterDirectionOutput = 1
    }

    [DataContract]
    public enum EsriGPParameterType
    {
        [EnumMember(Value = "esriGPParameterTypeDerived")]
        esriGPParameterTypeDerived = 0,
        [EnumMember(Value = "esriGPParameterTypeOptional")]
        esriGPParameterTypeOptional = 1,
        [EnumMember(Value = "esriGPParameterTypeRequired")]
        esriGPParameterTypeRequired = 2
    }
    #endregion

    #region ServiceInfoEventArgs Class
    public class GPServiceInfoEventArgs : System.EventArgs
    {
        private string _errorMsg = "";
        private DataExtractionServiceInfo _serviceInfo = null;

        public GPServiceInfoEventArgs(DataExtractionServiceInfo serviceInfo)
        {
            this._serviceInfo = serviceInfo;
        }

        public GPServiceInfoEventArgs(string errorMsg)
        {
            this._errorMsg = errorMsg;
        }

        public string ErrorMessage { get { return _errorMsg; } }
        public DataExtractionServiceInfo ServiceInfo { get { return _serviceInfo; } }
    }
    #endregion
}
