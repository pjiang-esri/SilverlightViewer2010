using System;
using System.Windows;
using System.Globalization;
using System.ComponentModel;
using System.Xml.Serialization;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.SilverlightViewer.Utility;

namespace ESRI.SilverlightViewer.Config
{
    #region Extent Class - Custom Envelope Type
    public class Extent
    {
        [XmlAttribute(DataType = "double", AttributeName = "xmin")]
        public double xmin { get; set; }

        [XmlAttribute(DataType = "double", AttributeName = "ymin")]
        public double ymin { get; set; }

        [XmlAttribute(DataType = "double", AttributeName = "xmax")]
        public double xmax { get; set; }

        [XmlAttribute(DataType = "double", AttributeName = "ymax")]
        public double ymax { get; set; }

        [XmlAttribute(DataType = "int", AttributeName = "spatialReference")]
        public int spatialReference { get; set; }

        #region Convert to Envelope (ESRI Geometry Type)
        /// <summary>
        /// Convert Extent to Envelope in the same spatial reference
        /// </summary>
        /// <returns>An envelope in the same spatial reference</returns>
        public Envelope ToEnvelope()
        {
            Envelope envelope = new Envelope(this.xmin, this.ymin, this.xmax, this.ymax);
            envelope.SpatialReference = new SpatialReference(this.spatialReference);

            return envelope;
        }

        /// <summary>
        /// Convert extent to Envelope between Mercator(102100) and Geographic(4326) spatial references
        /// If it is other spatial reference, use GeometryTool to project the envelope
        /// </summary>
        /// <param name="outSRWKID"></param>
        public Envelope ToEnvelope(int outSRWKID)
        {
            Envelope envelope = null;

            if (GeometryTool.IsWebMercatorSR(outSRWKID) && GeometryTool.IsGeographicSR(this.spatialReference))
            {
                MapPoint mPoint1 = new MapPoint(this.xmin, this.ymin, new SpatialReference(4326));
                MapPoint mPoint2 = new MapPoint(this.xmax, this.ymax, new SpatialReference(4326));
                envelope = new Envelope(mPoint1.GeographicToWebMercator(), mPoint2.GeographicToWebMercator());
            }
            else if (GeometryTool.IsWebMercatorSR(this.spatialReference) && GeometryTool.IsGeographicSR(outSRWKID))
            {
                MapPoint mPoint1 = new MapPoint(this.xmin, this.ymin, new SpatialReference(this.spatialReference));
                MapPoint mPoint2 = new MapPoint(this.xmax, this.ymax, new SpatialReference(this.spatialReference));
                envelope = new Envelope(mPoint1.WebMercatorToGeographic(), mPoint2.WebMercatorToGeographic());
            }
            else if (this.spatialReference == outSRWKID)
            {
                envelope = ToEnvelope();
            }

            return envelope;
        }
        #endregion
    }
    #endregion

    #region Map Service and Layer Enum Types
    public enum ServiceSource
    {
        [XmlEnum(Name = "Unknown")]
        Unknown = 0,
        [XmlEnum(Name = "BING")]
        BING = 1,
        [XmlEnum(Name = "ArcGIS")]
        ArcGIS = 2
    }

    public enum ArcGISServiceType
    {
        [XmlEnum(Name = "Unknown")]
        Unknown = 0,
        [XmlEnum(Name = "Cached")]
        Cached = 1,
        [XmlEnum(Name = "Dynamic")]
        Dynamic = 2,
        [XmlEnum(Name = "Image")]
        Image = 3,
        [XmlEnum(Name = "Feature")]
        Feature = 4
    }

    public enum BingLayerType
    {
        [XmlEnum(Name = "Unknown")]
        Unknown = 0,
        [XmlEnum(Name = "Road")]
        Road = 1,
        [XmlEnum(Name = "Aerial")]
        Aerial = 2,
        [XmlEnum(Name = "AerialWithLabels")]
        AerialWithLabels = 3
    }
    #endregion

    #region MapActionMode Enum and Enum Converter
    public enum MapActionMode
    {
        [XmlEnum(Name = "Pan")]
        Pan = 0,
        [XmlEnum(Name = "ZoomIn")]
        ZoomIn = 1,
        [XmlEnum(Name = "ZoomOut")]
        Zoomout = 2,
        [XmlEnum(Name = "Identify")]
        Identify = 3,
        [XmlEnum(Name = "FullMap")]
        FullMap = 4,
        [XmlEnum(Name = "ZoomBack")]
        ZoomBack = 5,
        [XmlEnum(Name = "ZoomNext")]
        ZoomNext = 6
    }

    public class MapActionModeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType.Equals(typeof(String)))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }


        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType.Equals(typeof(String)))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            if (value is String)
            {
                try
                {
                    return (MapActionMode)Enum.Parse(typeof(MapActionMode), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type MapActionMode.", (string)value));
                }
            }
            else
                return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, System.Type destinationType)
        {
            if (destinationType.Equals(typeof(String)))
                return value.ToString();
            else
                return base.ConvertTo(context, culture, value, destinationType);

        }
    }
    #endregion
}
