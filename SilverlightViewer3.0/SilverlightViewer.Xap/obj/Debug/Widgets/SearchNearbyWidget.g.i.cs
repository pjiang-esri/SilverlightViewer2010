﻿#pragma checksum "C:\Projects\Silverlight2010\SilverlightViewer3.0\SilverlightViewer3.0\SilverlightViewer.Xap\Widgets\SearchNearbyWidget.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F0B1C13A75A3E417E9E7389433C98175"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ESRI.SilverlightViewer.Widget;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace ESRI.SilverlightViewer.UIWidget {
    
    
    public partial class SearchNearbyWidget : ESRI.SilverlightViewer.Widget.WidgetBase {
        
        internal System.Windows.Controls.StackPanel PanelSearchNearby;
        
        internal System.Windows.Controls.ComboBox lstSearchLayer;
        
        internal System.Windows.Controls.RadioButton RadioBufferCenter;
        
        internal System.Windows.Controls.RadioButton RadioBufferGeometry;
        
        internal System.Windows.Controls.ComboBox lstGraphicWidget;
        
        internal System.Windows.Controls.TextBox txtBufferDistance;
        
        internal System.Windows.Controls.ComboBox lstBufferUnits;
        
        internal System.Windows.Controls.Button btnSubmitSearch;
        
        internal System.Windows.Controls.Grid PanelSearchResult;
        
        internal System.Windows.Controls.TextBlock SearchResultMessage;
        
        internal ESRI.SilverlightViewer.Widget.FeaturesGrid SearchResultGrid;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/ESRI.SilverlightViewer;component/Widgets/SearchNearbyWidget.xaml", System.UriKind.Relative));
            this.PanelSearchNearby = ((System.Windows.Controls.StackPanel)(this.FindName("PanelSearchNearby")));
            this.lstSearchLayer = ((System.Windows.Controls.ComboBox)(this.FindName("lstSearchLayer")));
            this.RadioBufferCenter = ((System.Windows.Controls.RadioButton)(this.FindName("RadioBufferCenter")));
            this.RadioBufferGeometry = ((System.Windows.Controls.RadioButton)(this.FindName("RadioBufferGeometry")));
            this.lstGraphicWidget = ((System.Windows.Controls.ComboBox)(this.FindName("lstGraphicWidget")));
            this.txtBufferDistance = ((System.Windows.Controls.TextBox)(this.FindName("txtBufferDistance")));
            this.lstBufferUnits = ((System.Windows.Controls.ComboBox)(this.FindName("lstBufferUnits")));
            this.btnSubmitSearch = ((System.Windows.Controls.Button)(this.FindName("btnSubmitSearch")));
            this.PanelSearchResult = ((System.Windows.Controls.Grid)(this.FindName("PanelSearchResult")));
            this.SearchResultMessage = ((System.Windows.Controls.TextBlock)(this.FindName("SearchResultMessage")));
            this.SearchResultGrid = ((ESRI.SilverlightViewer.Widget.FeaturesGrid)(this.FindName("SearchResultGrid")));
        }
    }
}

