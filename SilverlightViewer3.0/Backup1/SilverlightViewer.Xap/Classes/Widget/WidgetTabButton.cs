using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace ESRI.SilverlightViewer.Widget
{
    public class WidgetTabButton : DependencyObject
    {
        #region Properties
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(WidgetTabButton), new PropertyMetadata(""));
        public static readonly DependencyProperty ButtonImageProperty = DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(WidgetTabButton), new PropertyMetadata(null));
        public static readonly DependencyProperty ContentPanelProperty = DependencyProperty.Register("ContentPanel", typeof(string), typeof(WidgetTabButton), new PropertyMetadata(""));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string ContentPanel
        {
            get { return (string)GetValue(ContentPanelProperty); }
            set { SetValue(ContentPanelProperty, value); }
        }

        public ImageSource ButtonImage
        {
            get { return (ImageSource)GetValue(ButtonImageProperty); }
            set { SetValue(ButtonImageProperty, value); }
        }
        #endregion
    }
}
