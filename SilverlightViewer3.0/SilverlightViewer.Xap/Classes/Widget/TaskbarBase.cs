using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client;
using ESRI.SilverlightViewer.Controls;
using ESRI.SilverlightViewer.Config;

namespace ESRI.SilverlightViewer.Widget
{
    public class TaskbarBase : FloatingTaskbar
    {
        public TaskbarBase()
        {
        }

        #region Readonly Properties
        /// <summary>
        /// Get the current Application
        /// </summary>
        public MapApp CurrentApp
        {
            get { return Application.Current as MapApp; }
        }

        /// <summary>
        /// Get the main Map Page of the Application  
        /// </summary>
        public MapPage CurrentPage
        {
            get { return (CurrentApp == null) ? null : CurrentApp.RootVisual as MapPage; }
        }

        /// <summary>
        /// Get the current Application's configuration
        /// </summary>
        public ApplicationConfig AppConfig
        {
            get { return (CurrentApp == null) ? null : CurrentApp.AppConfig; }
        }

        /// <summary>
        /// Get the DrawObject from the Map Page, which is shared by all widgets
        /// </summary>
        public Draw DrawObject
        {
            get { return CurrentPage.DrawObject; }
        }

        /// <summary>
        /// Get and Set which widget has the control on the DrawObject
        /// </summary>
        public Type DrawWidget
        {
            get { return CurrentPage.DrawWidget; }
            set { CurrentPage.DrawWidget = value; }
        }

        /// <summary>
        /// Get the map's sptatial reference WKID
        /// </summary>
        public int MapSRWKID
        {
            get { return CurrentPage.MapSRWKID; }
        }
        #endregion

        #region Normal Properties
        /// <summary>
        /// Set and Get the Application's Title
        /// </summary>
        public string AppTitle { get; set; }

        /// <summary>
        /// Set and Get the Application's Sub-Title
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// Set and Get the Application's Logo Image Source URI
        /// </summary>
        public string LogoSource { get; set; }

        /// <summary>
        /// Get ans Set the Map control of the Application
        /// </summary>
        public ESRI.ArcGIS.Client.Map MapControl { get; set; }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            InitializeProperties();
            InitializeControls();

            // Make nessary changes by overriding OnWidgetLoaded
            OnWidgetLoaded();
        }

        protected virtual void InitializeProperties()
        {
            if (AppConfig != null)
            {
                this.AppTitle = AppConfig.ApplicationTitle;
                this.SubTitle = AppConfig.ApplicationSubTitle;
                this.LogoSource = AppConfig.ApplicationLogo;

                if (AppConfig.TaskbarConfig != null)
                {
                    this.BarWidth = AppConfig.TaskbarConfig.BarWidth;
                    this.BarHeight = AppConfig.TaskbarConfig.BarHeight;
                    this.DockHeight = AppConfig.TaskbarConfig.DockHeight;
                    this.InitialTop = AppConfig.TaskbarConfig.InitialTop;
                    this.InitialLeft = AppConfig.TaskbarConfig.InitialLeft;
                    this.DockPosition = AppConfig.TaskbarConfig.DockPosition;
                }
            }
        }

        protected virtual void InitializeControls() { }

        protected virtual void OnWidgetLoaded() { }
    }
}
