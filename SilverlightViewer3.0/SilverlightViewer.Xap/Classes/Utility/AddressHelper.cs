using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ESRI.SilverlightViewer.Utility
{
    [Obsolete("Deprecated Class. With a locator service of ArcGIS Server 10.0 or above this class is not needed anymore.")]
    public class AddressHelper
    {
        #region Variables
        private string _street1 = "";
        private string _street2 = "";
        private string _city = "";
        private string _state = "";
        private string _zipCode = "";
        private string _country = "";
        #endregion

        #region Constants
        // USA State Abbreviations
        string[] USA_STATE_ABBR = { "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM", "FL", "GA", "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK", "OR", "PW", "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI", "VA", "WA", "WV", "WI", "WY" };
        // USA State Names
        string[] USA_STATE_NAME = { "ALABAMA", "ALASKA", "AMERICAN SAMOA", "ARIZONA", "ARKANSAS", "CALIFORNIA", "COLORADO", "CONNECTICUT", "DELAWARE", "DISTRICT OF COLUMBIA", "FEDERATED STATES OF MICRONESIA", "FLORIDA", "GEORGIA", "GUAM", "HAWAII", "IDAHO", "ILLINOIS", "INDIANA", "IOWA", "KANSAS", "KENTUCKY", "LOUISIANA", "MAINE", "MARSHALL ISLANDS", "MARYLAND", "MASSACHUSETTS", "MICHIGAN", "MINNESOTA", "MISSISSIPPI", "MISSOURI", "MONTANA", "NEBRASKA", "NEVADA", "NEW HAMPSHIRE", "NEW JERSEY", "NEW MEXICO", "NEW YORK", "NORTH CAROLINA", "NORTH DAKOTA", "NORTHERN MARIANA ISLANDS", "OHIO", "OKLAHOMA", "OREGON", "PALAU", "PENNSYLVANIA", "PUERTO RICO", "RHODE ISLAND", "SOUTH CAROLINA", "SOUTH DAKOTA", "TENNESSEE", "TEXAS", "UTAH", "VERMONT", "VIRGIN ISLANDS", "VIRGINIA", "WASHINGTON", "WEST VIRGINIA", "WISCONSIN", "WYOMING" };

        // Canada Province Abbreviations
        string[] CANADA_STATE_ABBR = { "ONT.", "ON", "QUE.", "P.Q.", "QC", "N.S.", "NS", "N.B.", "NB", "MAN.", "MB", "B.C.", "BC", "P.E.I.", "PE", "SASK.", "SK", "ALTA.", "AB", "NFLD.", "LAB.", "NL" };
        // Canada Province Abbreviations
        string[] CANADA_STATE_NAME = { "ONTARIO", "QUEBEC", "NOVA SCOTIA", "NEW BRUNSWICK", "MANITOBA", "BRITISH COLUMBIA", "PRINCE EDWARD ISLAND", "SASKATCHEWAN", "ALBERTA", "NEWFOUNDLAND", "LABRADOR" };
        #endregion

        #region Constructors
        public AddressHelper()
        {
        }

        public AddressHelper(string address)
        {
            string state, zip;
            string[] info = address.Split(',');

            for (int i = 0; i < info.Length; i++)
                info[i] = info[i].Trim();

            if (info.Length == 5)
            {
                if (IsCountry(info[4]))
                {
                    this._street1 = info[0];
                    this._city = info[1];
                    this._state = info[2];
                    this._zipCode = info[3];
                    this._country = info[4];
                }
                else
                {
                    this._street1 = info[0];
                    this._street2 = info[1];
                    this._city = info[2];
                    this._state = info[3];
                    this._zipCode = info[4];
                }
            }
            else if (info.Length == 4)
            {
                if (IsCountry(info[3]))
                {
                    this._street1 = info[0];
                    this._city = info[1];
                    this._country = info[3];

                    if (IsStateZipCode(info[2], out state, out zip))
                    {
                        this._state = state;
                        this._zipCode = zip;
                    }
                }
                if (IsZipCode(info[3]))
                {
                    this._street1 = info[0];
                    this._city = info[1];
                    this._state = info[2];
                    this._zipCode = info[3];
                }
                else
                {
                    this._street1 = info[0];
                    this._street2 = info[1];
                    this._city = info[2];

                    if (IsStateZipCode(info[3], out state, out zip))
                    {
                        this._state = state;
                        this._zipCode = zip;
                    }
                }
            }
            else if (info.Length == 3)
            {
                this._street1 = info[0];
                this._city = info[1];

                if (IsStateZipCode(info[2], out state, out zip))
                {
                    this._state = state;
                    this._zipCode = zip;
                }
                else this._country = info[2];
            }
            else if (info.Length == 2)
            {
                if (IsStateZipCode(info[1], out state, out zip))
                {
                    if (Regex.IsMatch(info[0], "(\\d+\x20?-*\x20?\\w+)"))
                    {
                        this._street1 = info[0];
                    }
                    else
                    {
                        this._city = info[0];
                    }

                    this._state = state;
                    this._zipCode = zip;
                }
                else
                {
                    this._street1 = info[0];
                    this._city = info[1];
                }
            }
            else if (info.Length == 1)
            {
                if (IsStateZipCode(info[0], out state, out zip))
                {
                    this._state = state;
                    this._zipCode = zip;
                }
                else if (Regex.IsMatch(info[0], "(\\d+\x20?-*\x20?\\w+)"))
                {
                    this.Street1 = info[0];
                }
                else this._city = info[0];
            }
        }

        public AddressHelper(string street, string city, string state, string zipCode)
        {
            this._street1 = street;
            this._street2 = "";
            this._city = city;
            this._state = state;
            this._zipCode = zipCode;
        }

        public AddressHelper(string street1, string street2, string city, string state, string zipCode)
        {
            this._street1 = street1;
            this._street2 = street2;
            this._city = city;
            this._state = state;
            this._zipCode = zipCode;
        }
        #endregion

        #region Properties
        public string Street1
        {
            get
            {
                return _street1;
            }
            set
            {
                _street1 = value;
            }
        }

        public string Street2
        {
            get
            {
                return _street2;
            }
            set
            {
                _street2 = value;
            }
        }

        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                _city = value;
            }
        }

        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public string ZipCode
        {
            get
            {
                return _zipCode;
            }
            set
            {
                _zipCode = value;
            }
        }

        public string Country
        {
            get
            {
                return _country;
            }
            set
            {
                _country = value;
            }
        }

        #endregion

        #region Methods
        public string PostalAddress
        {
            get
            {
                StringBuilder postalAddress = new StringBuilder();

                if (!String.IsNullOrEmpty(_street1))
                    postalAddress.Append(_street1);

                if (!String.IsNullOrEmpty(_street2))
                {
                    if (postalAddress.Length > 0)
                        postalAddress.Append(", ");

                    postalAddress.Append(_street2);
                }

                if (!String.IsNullOrEmpty(_city))
                {
                    if (postalAddress.Length > 0)
                        postalAddress.Append(", ");

                    postalAddress.Append(_city);
                }

                if (!String.IsNullOrEmpty(_state))
                {
                    if (postalAddress.Length > 0)
                        postalAddress.Append(", ");

                    postalAddress.Append(_state);
                }

                if (!String.IsNullOrEmpty(_zipCode))
                {
                    if (postalAddress.Length > 0)
                        postalAddress.Append(", ");

                    postalAddress.Append(_zipCode);
                }

                return postalAddress.ToString();
            }
        }
        #endregion

        #region Utitlity Method
        private bool IsZipCode(string value)
        {
            if (Regex.IsMatch(value.Trim(), "\\d{5}(-\\d{4})?"))
                return true;
            else if (Regex.IsMatch(value.Trim(), "[A-Z]\\d[A-Z] \\d[A-Z]\\d"))
                return true;
            else
                return false;
        }

        private bool IsStateInUSA(string value)
        {
            if (USA_STATE_ABBR.Contains(value.ToUpper())) 
                return true;
            else if (USA_STATE_NAME.Contains(value.ToUpper())) 
                return true;
            else 
                return false;
        }

        private bool IsStateInCanada(string value)
        {
            if (CANADA_STATE_ABBR.Contains(value.ToUpper()))
                return true;
            else if (CANADA_STATE_NAME.Contains(value.ToUpper()))
                return true;
            else
                return false;
        }

        private bool IsStateZipCode(string value, out string state, out string zip)
        {
            zip = "";
            state = "";

            bool isRight = true;
            string[] values = value.Split(' ');

            Match match = Regex.Match(value.Trim(), "\\d{5}(-\\d{4})?");
            if (match.Success) // USA Zipcode
            {
                zip = match.Value;
                if (match.Index > 0)
                {
                    state = value.Substring(0, match.Index).Trim();
                }
            }
            else
            {
                match = Regex.Match(value, "[A-Z]\\d[A-Z] \\d[A-Z]\\d");
                if (match.Success) // Canada ZipCode
                {
                    zip = match.Value;
                    if (match.Index > 0)
                    {
                        state = value.Substring(0, match.Index).Trim();
                    }
                }
                else if (IsStateInUSA(value.Trim()))
                {
                    state = value.Trim();
                }
                else if (IsStateInCanada(value.Trim()))
                {
                    state = value.Trim();
                }
                else isRight = false;
            }

            return isRight;
        }

        private bool IsCountry(string value)
        {
            if ("UNITED STATES".Equals(value.ToUpper())) return true;
            else if ("USA".Equals(value.ToUpper())) return true;
            else if ("U.S.A.".Equals(value.ToUpper())) return true;
            else if ("CANADA".Equals(value.ToUpper())) return true;
            else return false;
        }
        #endregion
    }
}
