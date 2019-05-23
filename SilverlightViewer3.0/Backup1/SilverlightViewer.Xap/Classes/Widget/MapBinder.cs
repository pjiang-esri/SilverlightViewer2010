using System;
using System.Linq;
using System.Windows;
using System.Reflection;
using ESRI.SilverlightViewer.UIWidget;

namespace ESRI.SilverlightViewer.Widget
{
    public class MapBindingProperties
    {
        public string SourceElement { get; set; }
        public string TargetProperty { get; set; }
    }

    public static class MapBinder
    {
        public static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached("Binding", typeof(MapBindingProperties), typeof(MapBinder), new PropertyMetadata(null, OnBinding));

        public static MapBindingProperties GetBinding(DependencyObject obj)
        {
            return (MapBindingProperties)obj.GetValue(BindingProperty);
        }

        public static void SetBinding(DependencyObject obj, MapBindingProperties value)
        {
            obj.SetValue(BindingProperty, value);
        }

        private static void OnBinding(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement targetElement = obj as FrameworkElement;
            targetElement.Loaded += new RoutedEventHandler(TargetElement_Loaded);
        }

        private static void TargetElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            if (sender is WidgetBase)
            {
                WidgetBase targetElement = sender as WidgetBase;

                // Get the value of our attached property
                MapBindingProperties bindingProperties = GetBinding(targetElement);

                // Find the Map control on the Page
                if (targetElement.CurrentPage != null)
                {
                    ESRI.ArcGIS.Client.Map sourceElement = targetElement.CurrentPage.FindName(bindingProperties.SourceElement) as ESRI.ArcGIS.Client.Map;
                    targetElement.MapControl = sourceElement;

                    //string targetPropertyName = bindingProperties.TargetProperty + "Property";
                    //FieldInfo[] targetFields = targetElement.GetType().GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    //FieldInfo targetPropertyField = targetFields.First(f => f.Name == targetPropertyName);
                    //DependencyProperty targetDependencyProperty = targetPropertyField.GetValue(null) as DependencyProperty;

                    // Bind the Map to the target property
                    //targetElement.SetValue(targetDependencyProperty, sourceElement);
                }
            }
            else if (sender is TaskbarBase)
            {
                TaskbarBase targetElement = sender as TaskbarBase;
                MapBindingProperties bindingProperties = GetBinding(targetElement);

                // Find the Map control on the Page
                if (targetElement.CurrentPage != null)
                {
                    ESRI.ArcGIS.Client.Map sourceElement = targetElement.CurrentPage.FindName(bindingProperties.SourceElement) as ESRI.ArcGIS.Client.Map;
                    targetElement.MapControl = sourceElement;
                }
            }
            else if (sender is OverviewMapWidget)
            {
                OverviewMapWidget targetElement = sender as OverviewMapWidget;
                MapBindingProperties bindingProperties = GetBinding(targetElement);

                // Find the Map control on the Page
                if (targetElement.CurrentPage != null)
                {
                    ESRI.ArcGIS.Client.Map sourceElement = targetElement.CurrentPage.FindName(bindingProperties.SourceElement) as ESRI.ArcGIS.Client.Map;
                    targetElement.MapControl = sourceElement;
                }
            }
        }
    }
}
