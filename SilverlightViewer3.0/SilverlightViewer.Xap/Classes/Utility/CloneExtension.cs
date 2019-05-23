/*
 * A lot of thanks to Dominique Broux for his contribution
 * All the code in this file sources from his code posted on ArcGIS.com
 */

using System;
using System.Windows;
using System.Reflection;
//using System.Diagnostics;
using ESRI.ArcGIS.Client;

namespace ESRI.SilverlightViewer.Utility
{
    /// <summary>
    /// Clone recursively a dependency object
    /// Very limited implementation based on CLR properties
    /// Attached properties are not taken incare except specific case for this sample
    /// 
    /// Is used to clone Layers and Elements of ElementLayer
    /// </summary>
    public static class CloneExtension
    {
        /// <summary>     
        /// Extension for 'Object' that copies the properties to a target object.     
        /// </summary>     
        /// <param name="source">The source object</param>     
        /// <param name="target">The target object</param>     
        public static void CopyPropertiesTo(this object source, object target)
        {
            // If any is null throw an exception         
            if (source == null || target == null)
            {
                throw new Exception("Source or/and Destination Objects are null");
            }

            // Getting the Types of the objects     
            Type typeSource = source.GetType();
            Type typeTarget = target.GetType();

            // Loop on CLR properties (except name and parent)
            int nn = typeSource.GetProperties().GetLength(0);
            foreach (PropertyInfo sourceProperty in typeSource.GetProperties())
            {
                if (sourceProperty.Name == "Name" || sourceProperty.Name == "Parent" || !sourceProperty.CanRead || sourceProperty.GetGetMethod() == null || sourceProperty.GetIndexParameters().Length > 0)
                    continue;

                if (sourceProperty.Name == "Watermark" || sourceProperty.Name == "InputScope") // Avoid exception with these unimplemented properties
                    continue;

                if (sourceProperty.Name == "Resources")
                    continue;

                PropertyInfo targetProperty = typeTarget.GetProperty(sourceProperty.Name);
                if (targetProperty == null) { continue; }

                Object value = sourceProperty.GetValue(source, null);
                if (value == null) { continue; }

                try
                {
                    if (sourceProperty.PropertyType.GetInterface("IList", true) != null && !sourceProperty.PropertyType.IsArray)
                    {
                        var count = (int)sourceProperty.PropertyType.InvokeMember("get_Count", BindingFlags.InvokeMethod, null, sourceProperty.GetValue(source, null), null);
                        targetProperty.PropertyType.InvokeMember("Clear", BindingFlags.InvokeMethod, null, targetProperty.GetValue(target, null), null); // without this line, text can be duplicated due to inlines objects added after text is set

                        //Debug.WriteLine("IList {0} count = {1}", sourceProperty.Name, count);
                        for (int index = 0; index < count; index++)
                        {
                            //Debug.WriteLine("IList {0} item = {1}", sourceProperty.Name, index);
                            object item = sourceProperty.PropertyType.InvokeMember("get_Item", BindingFlags.InvokeMethod, null, sourceProperty.GetValue(source, null), new object[] { index });
                            targetProperty.PropertyType.InvokeMember("Add", BindingFlags.InvokeMethod, null, targetProperty.GetValue(target, null), new[] { CloneIfDO(item) });
                        }

                    }
                    else if (targetProperty.CanWrite && targetProperty.GetSetMethod() != null)
                    {
                        if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                        {
                            //Debug.WriteLine("Set property {0} value:{1}", targetProperty.Name, value.ToString());
                            targetProperty.SetValue(target, CloneIfDO(value), null);
                        }
                    }
                }
                catch (Exception e)
                {
                    //Debug.WriteLine("Exception during set property {0} : {1}", targetProperty.Name, e.Message);
                    throw new Exception(string.Format("Exception during set property {0} : {1}", targetProperty.Name, e.Message));
                }
            }
        }

        /// <summary>
        /// Clones a dependency object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source to clone.</param>
        /// <returns>A cloned object</returns>
        public static T Clone<T>(this T source) where T : DependencyObject
        {
            // Can be different from typeof(T)
            var clone = (T)Activator.CreateInstance(source.GetType());

            // Copy properties
            source.CopyPropertiesTo(clone);

            // Copy some useful attached properties (not done by reflection at this time)
            if (source is UIElement)
            {
                DependencyProperty attachedProperty = ElementLayer.EnvelopeProperty; // needed for ElementLayer
                SetDP(attachedProperty, source, clone);
            }

            return clone;
        }

        private static object CloneIfDO(object source)
        {
            return (source is DependencyObject) ? (source as DependencyObject).Clone() : source;
        }

        private static void SetDP(DependencyProperty dp, DependencyObject source, DependencyObject target)
        {
            Object value = source.GetValue(dp);

            if (value != null)
            {
                try
                {
                    target.SetValue(dp, CloneIfDO(value));
                }
                catch (Exception e)
                {
                    //Debug.WriteLine("Exception in setDP {0}", e.Message);
                    throw new Exception(string.Format("Exception in setDP {0}", e.Message));
                }
            }
        }
    }
}
