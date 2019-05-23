using System;
using System.Windows;
using System.Resources;
using System.Globalization;

namespace ESRI.SilverlightViewer.Controls
{
    internal class ResourcesUtil
    {
        public const string TASKBAR_FLOATING = "TASKBAR_FLOATING";
        public const string TASKBAR_DOCK_TOP = "TASKBAR_DOCK_TOP";
        public const string TASKBAR_DOCK_BOTTOM = "TASKBAR_DOCK_BOTTOM";
        public const string TASKBAR_TOOLTIP_DOCK = "TASKBAR_TOOLTIP_DOCK";
        public const string TASKBAR_TOOLTIP_FLOAT = "TASKBAR_TOOLTIP_FLOAT";
        public const string WIDGETBAR_NAVIGATE_NEXT = "WIDGETBAR_NAVIGATE_NEXT";
        public const string WIDGETBAR_NAVIGATE_BACK = "WIDGETBAR_NAVIGATE_BACK";

        public static string GetResourceValue(string resourceName)
        {
            if (System.Windows.Browser.HtmlPage.IsEnabled) // Not In Design Mode 
            {
                string assemblyName = Deployment.Current.EntryPointAssembly;
                Type mainPageType = Application.Current.RootVisual.GetType();
                ResourceManager rm = new ResourceManager(string.Format("{0}.Resources", assemblyName), mainPageType.Assembly);

                return rm.GetString(resourceName);
            }
            else return "";
        }
    }
}
