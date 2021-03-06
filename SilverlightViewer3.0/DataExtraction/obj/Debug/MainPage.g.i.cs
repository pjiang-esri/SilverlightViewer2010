﻿#pragma checksum "C:\Projects\Silverlight2010\SilverlightViewer2.2.1\SilverlightViewer2.2\DataExtraction\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3E0F9EE650A9236A5A7F448EDE66DEDC"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
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


namespace DataExtraction {
    
    
    public partial class MainPage : ESRI.SilverlightViewer.Widget.WidgetBase {
        
        internal System.Windows.Controls.StackPanel PanelSubmitExtJob;
        
        internal System.Windows.Controls.ComboBox lstExtractionService;
        
        internal System.Windows.Shapes.Path SplitLine1;
        
        internal System.Windows.Controls.StackPanel DrawModeButtonStack;
        
        internal System.Windows.Controls.HyperlinkButton drawModePoly;
        
        internal System.Windows.Controls.HyperlinkButton drawModeRect;
        
        internal System.Windows.Controls.HyperlinkButton drawModeFreePoly;
        
        internal System.Windows.Controls.TextBlock txtDrawModeStatus;
        
        internal System.Windows.Shapes.Path SplitLine2;
        
        internal System.Windows.Controls.ListBox lstLayersToClip;
        
        internal System.Windows.Shapes.Path SplitLine3;
        
        internal System.Windows.Controls.ComboBox lstFeatureFormat;
        
        internal System.Windows.Shapes.Path SplitLine4;
        
        internal System.Windows.Controls.ComboBox lstRasterformat;
        
        internal System.Windows.Controls.TextBlock txtExtractionStatus;
        
        internal System.Windows.Controls.StackPanel PanelExtractResult;
        
        internal System.Windows.Controls.HyperlinkButton lnkExtractionOutput;
        
        internal System.Windows.Controls.TextBlock txtJobErrorMessage;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/DataExtraction;component/MainPage.xaml", System.UriKind.Relative));
            this.PanelSubmitExtJob = ((System.Windows.Controls.StackPanel)(this.FindName("PanelSubmitExtJob")));
            this.lstExtractionService = ((System.Windows.Controls.ComboBox)(this.FindName("lstExtractionService")));
            this.SplitLine1 = ((System.Windows.Shapes.Path)(this.FindName("SplitLine1")));
            this.DrawModeButtonStack = ((System.Windows.Controls.StackPanel)(this.FindName("DrawModeButtonStack")));
            this.drawModePoly = ((System.Windows.Controls.HyperlinkButton)(this.FindName("drawModePoly")));
            this.drawModeRect = ((System.Windows.Controls.HyperlinkButton)(this.FindName("drawModeRect")));
            this.drawModeFreePoly = ((System.Windows.Controls.HyperlinkButton)(this.FindName("drawModeFreePoly")));
            this.txtDrawModeStatus = ((System.Windows.Controls.TextBlock)(this.FindName("txtDrawModeStatus")));
            this.SplitLine2 = ((System.Windows.Shapes.Path)(this.FindName("SplitLine2")));
            this.lstLayersToClip = ((System.Windows.Controls.ListBox)(this.FindName("lstLayersToClip")));
            this.SplitLine3 = ((System.Windows.Shapes.Path)(this.FindName("SplitLine3")));
            this.lstFeatureFormat = ((System.Windows.Controls.ComboBox)(this.FindName("lstFeatureFormat")));
            this.SplitLine4 = ((System.Windows.Shapes.Path)(this.FindName("SplitLine4")));
            this.lstRasterformat = ((System.Windows.Controls.ComboBox)(this.FindName("lstRasterformat")));
            this.txtExtractionStatus = ((System.Windows.Controls.TextBlock)(this.FindName("txtExtractionStatus")));
            this.PanelExtractResult = ((System.Windows.Controls.StackPanel)(this.FindName("PanelExtractResult")));
            this.lnkExtractionOutput = ((System.Windows.Controls.HyperlinkButton)(this.FindName("lnkExtractionOutput")));
            this.txtJobErrorMessage = ((System.Windows.Controls.TextBlock)(this.FindName("txtJobErrorMessage")));
        }
    }
}

