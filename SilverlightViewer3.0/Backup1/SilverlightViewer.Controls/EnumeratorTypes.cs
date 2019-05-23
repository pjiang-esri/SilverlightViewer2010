using System;
using System.Globalization;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ESRI.SilverlightViewer.Controls
{
    #region BusySignalState Enum and Converter
    public enum BusySignalState
    {
        STATE_HIDE = 0,
        STATE_BUSY = 1
    }

    public class BusySignalStateConverter : TypeConverter
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
                    return (BusySignalState)Enum.Parse(typeof(BusySignalState), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type BusySignalState.", (string)value));
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

    #region Taskbar DockStation Enum and Converter
    public enum DockPosition
    {
        NONE = 0,
        [XmlEnum(Name = "Top")]
        TOP = 1,
        [XmlEnum(Name = "Bottom")]
        BOTTOM = 2
    }

    public class DockPositionConverter : TypeConverter
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
                    return (DockPosition)Enum.Parse(typeof(DockPosition), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type DockPosition.", (string)value));
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

    #region ToggleButtonState Enum and Converter
    public enum ToggleButtonState
    {
        STATE_ORIGIN = 0,
        STATE_ROTATE90 = 1,
        STATE_ROTATE180 = 2,
        STATE_ROTATE270 = 3,
        STATE_ROTATE_90 = 4,
        STATE_ROTATE_180 = 5,
        STATE_ROTATE_270 = 6,
        STATE_SHOW = 7,
        STATE_HIDE = 8
    }

    public class ToggleButtonStateConverter : TypeConverter
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
                    return (ToggleButtonState)Enum.Parse(typeof(ToggleButtonState), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type ToggleButtonState.", (string)value));
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

    #region CloseButtonState Enum and Converter
    public enum CloseButtonState
    {
        STATE_ORIGIN = 0,
        STATE_ROTATE360 = 1,
        STATE_SHOW = 2,
        STATE_HIDE = 3
    }

    public class CloseButtonStateConverter : TypeConverter
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
                    return (CloseButtonState)Enum.Parse(typeof(CloseButtonState), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type CloseButtonStates.", (string)value));
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

    #region ContentOrientation Enum and Converter
    public enum ContentOrientation
    {
        DOWN = 0, // Default value
        LEFT = 1,
        RIGHT = 2,
        UP = 3
    }

    public class ContentOrientationConverter : TypeConverter
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
                    return (ContentOrientation)Enum.Parse(typeof(ContentOrientation), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type MenuButtonOrientation.", (string)value));
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

    #region MenuButtonShape Enum and Converter
    public enum MenuButtonShape
    {
        Circle = 0, // default
        Square = 1
    }

    public class MenuButtonShapeConverter : TypeConverter
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
                    return (MenuButtonShape)Enum.Parse(typeof(MenuButtonShape), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type MenuButtonShape.", (string)value));
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

    #region MenuButton MenuOpenAction Enum and Converter
    public enum MenuOpenAction
    {
        MouseHover = 0, // default
        MouseClick = 1
    }

    public class MenuOpenActionConverter : TypeConverter
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
                    return (MenuOpenAction)Enum.Parse(typeof(MenuOpenAction), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type MenuOpenAction.", (string)value));
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

    #region PopupWindow ArrowDirection Enum and Converter
    public enum ArrowDirection
    {
        UpperLeft = 0,
        LowerLeft = 1,
        UpperRight = 2,
        LowerRight = 3
    }

    public class PopupWindowArrowConverter : TypeConverter
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
                    return (ArrowDirection)Enum.Parse(typeof(ArrowDirection), (string)value, true);
                }
                catch
                {
                    throw new InvalidCastException(string.Format("Failed to cast %1 to Enum type PopupWindow ArrowDirection.", (string)value));
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