﻿#pragma checksum "C:\Projects\Silverlight2010\SilverlightViewer2.4\SilverlightViewer2.4\SilverlightViewer.Xap\Widgets\BookmarkWidget.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F890BE3B42AE16C58D4E806E4F8DE465"
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


namespace ESRI.SilverlightViewer.UIWidget {
    
    
    public partial class BookmarkWidget : ESRI.SilverlightViewer.Widget.WidgetBase {
        
        internal System.Windows.Controls.Border PanelBookmarkList;
        
        internal System.Windows.Controls.StackPanel StackBookmarkList;
        
        internal System.Windows.Controls.StackPanel PanelAddBookmark;
        
        internal System.Windows.Controls.TextBox txtBookmarkName;
        
        internal System.Windows.Controls.Button btnAddBookmark;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/ESRI.SilverlightViewer;component/Widgets/BookmarkWidget.xaml", System.UriKind.Relative));
            this.PanelBookmarkList = ((System.Windows.Controls.Border)(this.FindName("PanelBookmarkList")));
            this.StackBookmarkList = ((System.Windows.Controls.StackPanel)(this.FindName("StackBookmarkList")));
            this.PanelAddBookmark = ((System.Windows.Controls.StackPanel)(this.FindName("PanelAddBookmark")));
            this.txtBookmarkName = ((System.Windows.Controls.TextBox)(this.FindName("txtBookmarkName")));
            this.btnAddBookmark = ((System.Windows.Controls.Button)(this.FindName("btnAddBookmark")));
        }
    }
}

