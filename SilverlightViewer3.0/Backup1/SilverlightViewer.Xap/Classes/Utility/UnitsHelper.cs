using System;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;

namespace ESRI.SilverlightViewer.Utility
{
    public partial class UnitsHelper
    {
        #region Units Help Functions
        // Set up "Constants"
        private const double m1 = 111132.92;    // latitude calculation term 1
        private const double m2 = -559.82;		// latitude calculation term 2
        private const double m3 = 1.175;		// latitude calculation term 3
        private const double m4 = -0.0023;		// latitude calculation term 4
        private const double p1 = 111412.84;	// longitude calculation term 1
        private const double p2 = -93.5;		// longitude calculation term 2
        private const double p3 = 0.118;		// longitude calculation term 3

        /// <summary>
        /// Convert a degree to meters at a specific latitude
        /// </summary>
        /// <param name="latitude">Location at this latitude</param>
        /// <returns>Meters along longitude and latitude</returns>
        private static double[] Degree2Meter(double latitude)
        {
            // Convert latitude to radians
            double lat = latitude * (2.0 * Math.PI) / 360.0;

            // Calculate the length of a degree of latitude and longitude in meters
            double lonlen = (p1 * Math.Cos(lat)) + (p2 * Math.Cos(3 * lat)) + (p3 * Math.Cos(5 * lat));
            double latlen = m1 + (m2 * Math.Cos(2 * lat)) + (m3 * Math.Cos(4 * lat)) + (m4 * Math.Cos(6 * lat));

            return new double[2] { lonlen, latlen };
        }

        /// <summary>
        /// Calculate the conversion from decimal degrees to other units at a specific Latitude
        /// </summary>
        /// <param name="latitude">Where the conversion happens</param>
        /// <param name="toUnit">Convert from degrees into this Units</param>
        /// <returns>The conversion value from degrees into the parameter units</returns>
        public static double[] GetConversionFromDegree(double latitude, ScaleLine.ScaleLineUnit toUnit)
        {
            double[] meters = Degree2Meter(latitude);
            double conversion = GetConversionFromMeter(toUnit);

            return new double[2] { meters[0] * conversion, meters[1] * conversion };
        }

        /// <summary>
        /// Calculate the conversion from other units to decimal degrees at a specific Latitude
        /// </summary>
        /// <param name="latitude">Where the conversion happens</param>
        /// <param name="fromUnit">The units from which to convert into degrees</param>
        /// <returns>The conversion value from the parameter units into degrees</returns>
        public static double[] GetConversionToDegree(double latitude, ScaleLine.ScaleLineUnit fromUnit)
        {
            double[] meters = Degree2Meter(latitude);
            double conversion = GetConversionToMeter(fromUnit);

            return new double[2] { conversion / meters[0], conversion / meters[1] };
        }

        /// <summary>
        /// Calculate the conversion from meters to other distance units
        /// </summary>
        /// <param name="toUnit">Convert from meters into this Units</param>
        /// <returns>The conversion value from meters into the parameter units</returns>
        public static double GetConversionFromMeter(ScaleLine.ScaleLineUnit toUnit)
        {
            // 1 meter = 3.2808334 US Feet (3.2808399 International Feet)
            double conversion = 1.0;

            switch (toUnit)
            {
                case ScaleLine.ScaleLineUnit.Meters: conversion = 1.0; break;
                case ScaleLine.ScaleLineUnit.Feet: conversion = 3.28083; break;
                case ScaleLine.ScaleLineUnit.Yards: conversion = 1.09361; break;
                case ScaleLine.ScaleLineUnit.Miles: conversion = 3.28083 / 5280; break;
                case ScaleLine.ScaleLineUnit.Inches: conversion = 3.28083 * 12.0; break;
                case ScaleLine.ScaleLineUnit.Decimeters: conversion = 10; break;
                case ScaleLine.ScaleLineUnit.Centimeters: conversion = 100; break;
                case ScaleLine.ScaleLineUnit.Millimeters: conversion = 1000; break;
                case ScaleLine.ScaleLineUnit.Kilometers: conversion = 0.001; break;
                case ScaleLine.ScaleLineUnit.NauticalMiles: conversion = 3.28083 / (5280 * 1.15077945); break;
                default: conversion = 1.0; break;
            }

            return conversion;
        }

        /// <summary>
        /// Calculate the conversion from other distance units to meters
        /// </summary>
        /// <param name="fromUnit">The units from which to convert into meters</param>
        /// <returns>The conversion value from the parameter units into meters</returns>
        public static double GetConversionToMeter(ScaleLine.ScaleLineUnit fromUnit)
        {
            double conversion = 1.0;

            switch (fromUnit)
            {
                case ScaleLine.ScaleLineUnit.Meters: conversion = 1.0; break;
                case ScaleLine.ScaleLineUnit.Feet: conversion = 1.0 / 3.28083; break;
                case ScaleLine.ScaleLineUnit.Yards: conversion = 1.0 / 1.09361; break;
                case ScaleLine.ScaleLineUnit.Miles: conversion = 5280 / 3.28083; break;
                case ScaleLine.ScaleLineUnit.Inches: conversion = 1.0 / (3.28083 * 12.0); break;
                case ScaleLine.ScaleLineUnit.Decimeters: conversion = 0.1; break;
                case ScaleLine.ScaleLineUnit.Centimeters: conversion = 0.01; break;
                case ScaleLine.ScaleLineUnit.Millimeters: conversion = 0.001; break;
                case ScaleLine.ScaleLineUnit.Kilometers: conversion = 1000.0; break;
                case ScaleLine.ScaleLineUnit.NauticalMiles: conversion = (5280 * 1.15077945) / 3.28083; break;
                default: conversion = 1.0; break;
            }

            return conversion;
        }

        /// <summary>
        /// Get Units by Spatial Reference WKID - Not available for all WKID(s)
        /// </summary>
        /// <param name="wkid">Spatial Reference WKID</param>
        /// <returns>Units</returns>
        public static ScaleLine.ScaleLineUnit GetUnitsByWKID(int wkid)
        {
            ScaleLine.ScaleLineUnit units = ScaleLine.ScaleLineUnit.Meters;

            if (wkid > 4000 && wkid < 5000) units = ScaleLine.ScaleLineUnit.DecimalDegrees;          // Current 4001 - 4904
            else if (wkid > 37000 && wkid < 37500) units = ScaleLine.ScaleLineUnit.DecimalDegrees;   // Current 37001 - 37260
            else if (wkid > 103999 && wkid < 105000) units = ScaleLine.ScaleLineUnit.DecimalDegrees; // Current 104000 - 104970
            else if (wkid >= 2222 && wkid <= 2289) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 2759 && wkid <= 2930) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 2964 && wkid <= 2968) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 3417 && wkid <= 3438) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 3441 && wkid <= 3446) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 3453 && wkid <= 3459) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 3560 && wkid <= 3570) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 3734 && wkid <= 3760) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid == 3991 || wkid == 3992) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 26801 && wkid <= 26813) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 32164 && wkid <= 32167) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 32664 && wkid <= 32667) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid == 102219 || wkid == 102220) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 102461 && wkid <= 102468) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 102629 && wkid <= 102766) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 103400 && wkid <= 103471) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 103700 && wkid <= 103793) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid >= 103900 && wkid <= 103971) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid == 2314 || wkid == 2992 || wkid == 2994 || wkid == 3089 || wkid == 3091) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid == 3359 || wkid == 3361 || wkid == 3363 || wkid == 3365 || wkid == 3404) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid == 65062) units = ScaleLine.ScaleLineUnit.Feet;
            else if (wkid == 102068 || wkid == 102069) units = ScaleLine.ScaleLineUnit.Kilometers;
            else units = ScaleLine.ScaleLineUnit.Meters;

            return units;
        }
        #endregion
    }

    #region Units Enum
    //public enum MapUnits
    //{
    //    Undefined = 0,
    //    DecimalDegrees = 1,
    //    Inches = 2,
    //    Feet = 3,
    //    Yards = 4,
    //    Miles = 5,
    //    NauticalMiles = 6,
    //    Millimeters = 7,
    //    Centimeters = 8,
    //    Decimeters = 9,
    //    Meters = 10,
    //    Kilometers = 11
    //}
    #endregion
}
