using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;  
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Bing.GeocodeService;
using ESRI.SilverlightViewer.Config;

namespace ESRI.SilverlightViewer.Utility
{
    public class GeocodeTool
    {
        public delegate void GeocodeResultReady(object sender, GeocodeResultEventArgs args);
        public event GeocodeResultReady ResultReady;

        private int outSRWKID;
        private bool isReverse = false;
        private string geoServiceUrl = "";
        private LocatorConfig locatorConfig;
        private List<LocationCandidate> lastResults;

        /// <summary>
        /// Locator Configuration
        /// </summary>
        public LocatorConfig Locator
        {
            get { return locatorConfig; }
            set { locatorConfig = value; }
        }

        /// <summary>
        /// Geometry Service URL
        /// </summary>
        public string GeometryServiceURL
        {
            get { return geoServiceUrl; }
            set { geoServiceUrl = value; }
        }

        /// <summary>
        /// Output Spatial Reference WKID (the Map WKID)
        /// </summary>
        public int OutSRWKID
        {
            get { return outSRWKID; }
            set { outSRWKID = value; }
        }

        public GeocodeTool(LocatorConfig widgetConfig, string geometryService, int outWKID)
        {
            outSRWKID = outWKID;
            geoServiceUrl = geometryService;
            locatorConfig = widgetConfig;
        }

        #region Geocode an address using BING locator or ArcGIS locator
        /// <summary>
        /// Deprecated Medthod - Used for an ArcGIS locator service before verison 10
        /// </summary>
        /// <param name="address">Street name with house number</param>
        /// <param name="city">City name</param>
        /// <param name="state">State or Province</param>
        /// <param name="zipCode">ZipCode or Zone</param>
        /// <param name="country">Optional, default is USA</param>
        [Obsolete("This method is deprecated, please use the mothed using a single-line address parameter")] 
        public void GeocodeAddress(string address, string city, string state, string zipCode, string country)
        {
            if (locatorConfig.EnableLocator == ServiceSource.BING)
            {
                ESRI.ArcGIS.Client.Bing.Geocoder bingLocator = new ESRI.ArcGIS.Client.Bing.Geocoder(locatorConfig.BingLocator.Token, locatorConfig.BingLocator.ServerType);
                address = (string.IsNullOrEmpty(address)) ? "" : address + ",";
                city = (string.IsNullOrEmpty(city)) ? "" : city + ",";
                string formatAddress = string.Format("{0} {1} {2} {3}", address, city, state, zipCode, country);
                if (!string.IsNullOrEmpty(country)) formatAddress += "," + country;
                bingLocator.Geocode(formatAddress, new EventHandler<ESRI.ArcGIS.Client.Bing.GeocodeService.GeocodeCompletedEventArgs>(BingLocator_GeocodeCompleted));
            }
            else if (locatorConfig.ArcGISLocator != null)
            {
                ESRI.ArcGIS.Client.Tasks.Locator locatorTask = new ESRI.ArcGIS.Client.Tasks.Locator(locatorConfig.ArcGISLocator.RESTURL);
                locatorTask.AddressToLocationsCompleted += new EventHandler<ESRI.ArcGIS.Client.Tasks.AddressToLocationsEventArgs>(ArcGISLocator_GeocodeCompleted);
                locatorTask.Failed += new EventHandler<TaskFailedEventArgs>(ArcGISLocatorTask_Failed);
                AddressToLocationsParameters addressParams = new AddressToLocationsParameters();
                ArcGISLocatorParams paramFields = locatorConfig.ArcGISLocator.ParameterFields;

                if (!string.IsNullOrEmpty(paramFields.AddressField)) addressParams.Address.Add(paramFields.AddressField, address);
                if (!string.IsNullOrEmpty(paramFields.CityField)) addressParams.Address.Add(paramFields.CityField, city);
                if (!string.IsNullOrEmpty(paramFields.StateField)) addressParams.Address.Add(paramFields.StateField, state);
                if (!string.IsNullOrEmpty(paramFields.ZipField)) addressParams.Address.Add(paramFields.ZipField, zipCode);
                if (!string.IsNullOrEmpty(paramFields.CountryField)) addressParams.Address.Add(paramFields.CountryField, country);

                locatorTask.AddressToLocationsAsync(addressParams);
            }
        }

        /// <summary>
        /// Geocoding a single line address - using a locator service of ArcGIS Server above 10.0
        /// </summary>
        /// <param name="singleLineAddress">Single Line Address</param>
        public void GeocodeAddress(string singleLineAddress)
        {
            if (locatorConfig.EnableLocator == ServiceSource.BING)
            {
                ESRI.ArcGIS.Client.Bing.Geocoder bingLocator = new ESRI.ArcGIS.Client.Bing.Geocoder(locatorConfig.BingLocator.Token, locatorConfig.BingLocator.ServerType);
                bingLocator.Geocode(singleLineAddress, new EventHandler<ESRI.ArcGIS.Client.Bing.GeocodeService.GeocodeCompletedEventArgs>(BingLocator_GeocodeCompleted));
            }
            else if (locatorConfig.ArcGISLocator != null)
            {
                ESRI.ArcGIS.Client.Tasks.Locator locatorTask = new ESRI.ArcGIS.Client.Tasks.Locator(locatorConfig.ArcGISLocator.RESTURL);
                locatorTask.AddressToLocationsCompleted += new EventHandler<ESRI.ArcGIS.Client.Tasks.AddressToLocationsEventArgs>(ArcGISLocator_GeocodeCompleted);
                locatorTask.Failed += new EventHandler<TaskFailedEventArgs>(ArcGISLocatorTask_Failed);
                AddressToLocationsParameters addressParams = new AddressToLocationsParameters();
                addressParams.Address.Add("SingleLine", singleLineAddress);

                locatorTask.AddressToLocationsAsync(addressParams);
            }
        }
        #endregion

        #region Reverse geocode a point using BING locator or ArcGIS locator
        /// <summary>
        /// Reverse geocode - Find a point's address
        /// </summary>
        /// <param name="mapPoint">A point with its Spatial Reference matching the locator's Spatial Reference</param>
        /// <param name="searchDistance">Search distance around the point (ignored by BING locator)</param>
        public void ReverseGeocode(MapPoint mapPoint)
        {
            if (locatorConfig.EnableLocator == ServiceSource.BING)
            {
                ESRI.ArcGIS.Client.Bing.Geocoder BingLocator = new ESRI.ArcGIS.Client.Bing.Geocoder(locatorConfig.BingLocator.Token, locatorConfig.BingLocator.ServerType);
                BingLocator.ReverseGeocode(mapPoint, new EventHandler<ESRI.ArcGIS.Client.Bing.GeocodeService.ReverseGeocodeCompletedEventArgs>(BingLocator_ReverseGeocodeCompleted));
            }
            else if (locatorConfig.ArcGISLocator != null)
            {
                ESRI.ArcGIS.Client.Tasks.Locator locatorTask = new ESRI.ArcGIS.Client.Tasks.Locator(locatorConfig.ArcGISLocator.RESTURL);
                locatorTask.LocationToAddressCompleted += new EventHandler<ESRI.ArcGIS.Client.Tasks.AddressEventArgs>(ArcGISLocator_ReverseGeocodeCompleted);
                locatorTask.Failed += new EventHandler<TaskFailedEventArgs>(ArcGISLocatorTask_Failed);
                locatorTask.LocationToAddressAsync(mapPoint, locatorConfig.ArcGISLocator.SearchDistance);
            }
        }
        #endregion

        #region Process ArcGIS Locator Results
        private void ArcGISLocator_GeocodeCompleted(object sender, ESRI.ArcGIS.Client.Tasks.AddressToLocationsEventArgs args)
        {
            List<LocationCandidate> results = new List<LocationCandidate>();
            if (args.Results != null && args.Results.Count > 0)
            {
                foreach (AddressCandidate result in args.Results)
                {
                    results.Add(new LocationCandidate(result.Score, result.Address, result.Location, locatorConfig.LocationSymbol.ImageSource));
                    if (locatorConfig.ArcGISLocator.ReturnFirst) break;
                }
            }

            OnResultReady(results, locatorConfig.ArcGISLocator.SRWKID, false);
        }

        private void ArcGISLocator_ReverseGeocodeCompleted(object sender, ESRI.ArcGIS.Client.Tasks.AddressEventArgs args)
        {
            List<LocationCandidate> results = new List<LocationCandidate>();
            if (args.Address != null)
            {
                string value = "";
                string formatAddress = "";
                Dictionary<string, object> values = args.Address.Attributes;
                ArcGISLocatorParams fields = locatorConfig.ArcGISLocator.ParameterFields;

                if (fields.AddressField != null && values.ContainsKey(fields.AddressField))
                {
                    value = (string)values[fields.AddressField];
                    if (!string.IsNullOrEmpty(value)) formatAddress += value.Trim() + ", ";
                }

                if (fields.CityField != null && values.ContainsKey(fields.CityField))
                {
                    value = (string)values[fields.CityField];
                    if (!string.IsNullOrEmpty(value)) formatAddress += value.Trim();
                }

                if (fields.StateField != null && values.ContainsKey(fields.StateField))
                {
                    value = (string)values[fields.StateField];
                    if (!string.IsNullOrEmpty(value)) formatAddress += ", " + value.Trim() + " ";
                }

                if (fields.ZipField != null && values.ContainsKey(fields.ZipField))
                {
                    value = (string)values[fields.ZipField];
                    if (!string.IsNullOrEmpty(value)) formatAddress += value.Trim();
                }

                if (fields.CountryField != null && values.ContainsKey(fields.CountryField))
                {
                    value = (string)values[fields.CountryField];
                    if (!string.IsNullOrEmpty(value)) formatAddress += ", " + value.Trim();
                }

                if (formatAddress == "") formatAddress = "Address is unavailable";
                LocationCandidate candidate = new LocationCandidate(100, formatAddress, args.Address.Location, locatorConfig.LocationSymbol.ImageSource);
                results.Add(candidate);
            }

            if (results.Count > 0)
            {
                int srWKID = results[0].Location.SpatialReference.WKID;
                OnResultReady(results, srWKID, true); // With ArcGIS Server 10.01 later, reverse geocoding results have the same SR with the input point 
            }
            else if (ResultReady != null)
            {
                ResultReady(this, new GeocodeResultEventArgs(results, true));
            }
        }
        #endregion

        #region Process BING Locator Results
        private void BingLocator_GeocodeCompleted(object sender, ESRI.ArcGIS.Client.Bing.GeocodeService.GeocodeCompletedEventArgs args)
        {
            if (args.Result != null)
            {
                GeocodeResponse response = args.Result;
                OnResultReady(ConvertBingGeocodeResults(response), 4326, false);
            }
            else
            {
                if (ResultReady != null) ResultReady(this, new GeocodeResultEventArgs(args.Error.Message));
            }
        }

        private void BingLocator_ReverseGeocodeCompleted(object sender, ESRI.ArcGIS.Client.Bing.GeocodeService.ReverseGeocodeCompletedEventArgs args)
        {
            if (args.Result != null)
            {
                GeocodeResponse response = args.Result;
                OnResultReady(ConvertBingGeocodeResults(response), 4326, true);
            }
            else
            {
                if (ResultReady != null) ResultReady(this, new GeocodeResultEventArgs(args.Error.Message));
            }
        }

        private List<LocationCandidate> ConvertBingGeocodeResults(GeocodeResponse response)
        {
            List<LocationCandidate> results = new List<LocationCandidate>();

            if (response != null && response.Results.Count > 0)
            {
                MapPoint location = null;
                LocationCandidate candidate = null;

                foreach (GeocodeResult result in response.Results)
                {
                    location = new MapPoint(result.Locations[0].Longitude, result.Locations[0].Latitude, new SpatialReference(4326));
                    candidate = new LocationCandidate(100, result.DisplayName, location, locatorConfig.LocationSymbol.ImageSource);
                    results.Add(candidate);
                }
            }

            return results;
        }
        #endregion

        #region Finalize results to the map's Spatial Reference and Dispatch ResultReady event
        protected virtual void OnResultReady(List<LocationCandidate> candidates, int srWKID, bool reverse)
        {
            if (srWKID == this.outSRWKID)
            {
                if (ResultReady != null) ResultReady(this, new GeocodeResultEventArgs(candidates, reverse));
            }
            else if (GeometryTool.IsGeographicSR(srWKID) && GeometryTool.IsWebMercatorSR(this.outSRWKID))
            {
                foreach (LocationCandidate candidate in candidates)
                {
                    candidate.Location.SpatialReference = new SpatialReference(srWKID);
                    candidate.Location = candidate.Location.GeographicToWebMercator();
                }

                if (ResultReady != null) ResultReady(this, new GeocodeResultEventArgs(candidates, reverse));
            }
            else if (GeometryTool.IsWebMercatorSR(srWKID) && GeometryTool.IsGeographicSR(this.outSRWKID))
            {
                foreach (LocationCandidate candidate in candidates)
                {
                    candidate.Location.SpatialReference = new SpatialReference(srWKID);
                    candidate.Location = candidate.Location.WebMercatorToGeographic();
                }

                if (ResultReady != null) ResultReady(this, new GeocodeResultEventArgs(candidates, reverse));
            }
            else
            {
                List<Graphic> locGraphics = new List<Graphic>();
                foreach (LocationCandidate candidate in candidates)
                {
                    Graphic graphic = new Graphic();
                    candidate.Location.SpatialReference = new SpatialReference(srWKID);
                    graphic.Geometry = candidate.Location;
                    locGraphics.Add(graphic);
                }

                isReverse = reverse;
                lastResults = candidates; // Save to pass 
                GeometryService geometryService = new GeometryService(geoServiceUrl);
                geometryService.ProjectCompleted += new EventHandler<GraphicsEventArgs>(GeometryService_ProjectCompleted);
                geometryService.Failed += new EventHandler<TaskFailedEventArgs>(GeometryService_Failed);
                // Use a UserToken to avoid it execute other projection complete handlers
                geometryService.ProjectAsync(locGraphics, new SpatialReference(this.outSRWKID));
            }
        }

        private void GeometryService_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            int k = 0;
            List<LocationCandidate> candidates = new List<LocationCandidate>();
            foreach (Graphic graphic in e.Results)
            {
                lastResults[k].Location = graphic.Geometry as MapPoint;
                k++;
            }

            if (ResultReady != null) ResultReady(this, new GeocodeResultEventArgs(lastResults, isReverse));
        }
        #endregion

        #region Event Handlers on Service Request Fail
        private void ArcGISLocatorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            if (ResultReady != null) ResultReady(this, new GeocodeResultEventArgs(e.Error.Message));
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            if (ResultReady != null) ResultReady(this, new GeocodeResultEventArgs(e.Error.Message));
        }
        #endregion
    }

    #region LocationCandidate Class
    public class LocationCandidate
    {
        public LocationCandidate() { }

        public LocationCandidate(int score, string address, MapPoint location, string symbol)
        {
            this.Score = score;
            this.Address = address;
            this.Location = location;
            this.SymbolImage = symbol;
        }

        public int Score { get; set; }
        public string Address { get; set; }
        public MapPoint Location { get; set; }
        public string SymbolImage { get; set; }
    }
    #endregion

    #region GeocodeResultEventArgs Class
    public class GeocodeResultEventArgs : System.EventArgs
    {
        private bool _isReverse = false;
        private List<LocationCandidate> _candidates;
        private string _errorMsg = "";

        public GeocodeResultEventArgs(string error)
        {
            _errorMsg = error;
            _candidates = new List<LocationCandidate>();
        }

        public GeocodeResultEventArgs(List<LocationCandidate> candidates, bool reverse)
        {
            _candidates = candidates;
            _isReverse = reverse;
        }

        public bool IsReverseGeocode { get { return _isReverse; } }
        public List<LocationCandidate> GeocodeResults { get { return _candidates; } }
        public string ErrorMessage { get { return _errorMsg; } }
    }
    #endregion
}
