﻿#pragma checksum "C:\Projects\Silverlight2010\SilverlightViewer3.0\SilverlightViewer3.0\SocialMediaWidget\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7099DD51069A91850563B09223DBAC19"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
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


namespace SocialMediaWidget {
    
    
    public partial class MainPage : ESRI.SilverlightViewer.Widget.WidgetBase {
        
        internal System.Windows.Controls.StackPanel PanelSubmitSearch;
        
        internal System.Windows.Controls.RadioButton radioMediaYouTube;
        
        internal System.Windows.Controls.RadioButton radioMediaFlickr;
        
        internal System.Windows.Controls.RadioButton radioMediaTwitter;
        
        internal System.Windows.Controls.TextBlock textSearchContent;
        
        internal System.Windows.Controls.RadioButton radioSearchMapCenter;
        
        internal System.Windows.Controls.RadioButton radioSearchGeometry;
        
        internal System.Windows.Controls.ComboBox lstGraphicWidget;
        
        internal System.Windows.Controls.TextBox textKeyWord;
        
        internal System.Windows.Controls.Slider sliderSearchRadius;
        
        internal System.Windows.Controls.StackPanel panelYTTimeParam;
        
        internal System.Windows.Controls.ComboBox lstYouTubeTime;
        
        internal System.Windows.Controls.StackPanel panelFrTimeParam;
        
        internal System.Windows.Controls.DatePicker dateFlickrFrom;
        
        internal System.Windows.Controls.DatePicker dateFlickrTo;
        
        internal System.Windows.Controls.TextBlock descTwitterTime;
        
        internal System.Windows.Controls.Grid PanelSearchResult;
        
        internal ESRI.SilverlightViewer.Widget.FeaturesGrid socialMediaGrid;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/SocialMediaWidget;component/MainPage.xaml", System.UriKind.Relative));
            this.PanelSubmitSearch = ((System.Windows.Controls.StackPanel)(this.FindName("PanelSubmitSearch")));
            this.radioMediaYouTube = ((System.Windows.Controls.RadioButton)(this.FindName("radioMediaYouTube")));
            this.radioMediaFlickr = ((System.Windows.Controls.RadioButton)(this.FindName("radioMediaFlickr")));
            this.radioMediaTwitter = ((System.Windows.Controls.RadioButton)(this.FindName("radioMediaTwitter")));
            this.textSearchContent = ((System.Windows.Controls.TextBlock)(this.FindName("textSearchContent")));
            this.radioSearchMapCenter = ((System.Windows.Controls.RadioButton)(this.FindName("radioSearchMapCenter")));
            this.radioSearchGeometry = ((System.Windows.Controls.RadioButton)(this.FindName("radioSearchGeometry")));
            this.lstGraphicWidget = ((System.Windows.Controls.ComboBox)(this.FindName("lstGraphicWidget")));
            this.textKeyWord = ((System.Windows.Controls.TextBox)(this.FindName("textKeyWord")));
            this.sliderSearchRadius = ((System.Windows.Controls.Slider)(this.FindName("sliderSearchRadius")));
            this.panelYTTimeParam = ((System.Windows.Controls.StackPanel)(this.FindName("panelYTTimeParam")));
            this.lstYouTubeTime = ((System.Windows.Controls.ComboBox)(this.FindName("lstYouTubeTime")));
            this.panelFrTimeParam = ((System.Windows.Controls.StackPanel)(this.FindName("panelFrTimeParam")));
            this.dateFlickrFrom = ((System.Windows.Controls.DatePicker)(this.FindName("dateFlickrFrom")));
            this.dateFlickrTo = ((System.Windows.Controls.DatePicker)(this.FindName("dateFlickrTo")));
            this.descTwitterTime = ((System.Windows.Controls.TextBlock)(this.FindName("descTwitterTime")));
            this.PanelSearchResult = ((System.Windows.Controls.Grid)(this.FindName("PanelSearchResult")));
            this.socialMediaGrid = ((ESRI.SilverlightViewer.Widget.FeaturesGrid)(this.FindName("socialMediaGrid")));
        }
    }
}

