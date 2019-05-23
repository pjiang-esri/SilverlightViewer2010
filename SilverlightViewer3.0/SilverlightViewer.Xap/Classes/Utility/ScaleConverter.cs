using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.SilverlightViewer.Utility
{
    public sealed class ScaleConverter : IValueConverter
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);
            SolidColorBrush grayBrush = new SolidColorBrush(Colors.DarkGray);

            if (parameter != null)
            {
                double[] scales = (double[])parameter;
                double minScale = scales[0];
                double maxScale = scales[1];
                double scale = (double)value;

                if (minScale == 0.0 && maxScale == 0.0)
                {
                    //return true;
                    return blackBrush;
                }
                else if (minScale > 0 && scale < minScale)
                {
                    //return ((maxScale > 0) ? (scale > maxScale) : true);
                    return (maxScale > 0) ? ((scale > maxScale) ? blackBrush : grayBrush) : blackBrush;
                }
                else if (maxScale > 0 && scale > maxScale)
                {
                    //return ((minScale > 0) ? (scale < minScale) : true);
                    return (minScale > 0) ? ((scale < minScale) ? blackBrush : grayBrush) : blackBrush;
                }

                // return false;
                return grayBrush;
            }
            else
            {
                //return true;
                return blackBrush;
            }
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)0.0;
        }
    }
}
