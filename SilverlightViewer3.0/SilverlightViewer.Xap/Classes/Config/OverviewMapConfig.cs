using System;
using System.Globalization;
using System.ComponentModel;
using System.Xml.Serialization;
using ESRI.SilverlightViewer.Config;

namespace ESRI.SilverlightViewer.Config
{
    [XmlRoot(ElementName = "WidgetConfig")]
    public partial class OverviewMapConfig : WidgetConfigBase
    {
        [XmlAttribute(AttributeName = "width")]
        public double Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public double Height { get; set; }

        [XmlAttribute(AttributeName = "openInitial")]
        public bool OpenInitial { get; set; }

        [XmlAttribute(AttributeName = "position")]
        public OverviewMapPosition Position { get; set; }

        [XmlElement(typeof(Extent), ElementName = "MaximumExtent")]
        public Extent MaximumExtent { get; set; }

        [XmlElement(typeof(ArcGISBaseMapLayer), ElementName = "MapLayer")]
        public ArcGISBaseMapLayer MapLayer { get; set; }
    }

    #region OverviewMap Position Enum && Enum Converter
    public enum OverviewMapPosition
    {
        Undefined = 0,
        [XmlEnum(Name = "LowerRight")]
        LowerRight = 1,
        [XmlEnum(Name = "UpperRight")]
        UpperRight = 2,
        [XmlEnum(Name = "LowerLeft")]
        LowerLeft = 3,
        [XmlEnum(Name = "UpperLeft")]
        UpperLeft = 4
    }

    public class OverviewMapPositionConverter : TypeConverter
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
                    return (OverviewMapPosition)Enum.Parse(typeof(OverviewMapPosition), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type OverviewMapPosition.", (string)value));
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
