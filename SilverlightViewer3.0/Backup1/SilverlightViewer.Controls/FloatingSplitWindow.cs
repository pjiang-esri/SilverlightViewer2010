using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplatePart(Name = FloatingSplitWindow._LEFT_CONTAINER, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = FloatingSplitWindow._RIGHT_CONTAINER, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = FloatingSplitWindow._WINDOW_SPLITTER, Type = typeof(FrameworkElement))]
    public class FloatingSplitWindow: FloatingWindow
    {
        protected const string _LEFT_CONTAINER = "LeftContentPanel";
        protected const string _RIGHT_CONTAINER = "RightContentPanel";
        protected const string _WINDOW_SPLITTER = "WindowSplitter";

        protected FrameworkElement LeftContainer = null;
        protected FrameworkElement RightContainer = null;
        protected FrameworkElement WindowSplitter = null;
        protected Point lastSplitPosition;
        protected bool IsSplitting = false;

        private ColumnDefinition LeftColumn = null;
        private ColumnDefinition RightColumn = null;
        private double maxColWitdh = 0;
        private double gridLWidth0 = 0;
        private double gridRWidth0 = 0;

        public FloatingSplitWindow()
        {
            this.DefaultStyleKey = typeof(FloatingSplitWindow);
        }

        #region Dependency Properties
        public static readonly DependencyProperty LeftWindowProperty = DependencyProperty.Register("LeftWindow", typeof(FrameworkElement), typeof(FloatingSplitWindow), null);
        public static readonly DependencyProperty RightWindowProperty = DependencyProperty.Register("RightWindow", typeof(FrameworkElement), typeof(FloatingSplitWindow), null);
        public static readonly DependencyProperty SplitterBackgroundProperty = DependencyProperty.Register("SplitterBackground", typeof(SolidColorBrush), typeof(FloatingSplitWindow), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 224, G = 255, B = 255 })));

        public FrameworkElement LeftWindow
        {
            get { return (FrameworkElement)this.GetValue(LeftWindowProperty); }
            set { this.SetValue(LeftWindowProperty, value); }
        }

        public FrameworkElement RightWindow
        {
            get { return (FrameworkElement)this.GetValue(RightWindowProperty); }
            set { this.SetValue(RightWindowProperty, value); }
        }

        public SolidColorBrush SplitterBackground
        {
            get { return (SolidColorBrush)this.GetValue(SplitterBackgroundProperty); }
            set { this.SetValue(SplitterBackgroundProperty, value); }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            LeftContainer = this.GetTemplateChild(_LEFT_CONTAINER) as FrameworkElement;
            RightContainer = this.GetTemplateChild(_RIGHT_CONTAINER) as FrameworkElement;
            WindowSplitter = this.GetTemplateChild(_WINDOW_SPLITTER) as FrameworkElement;

            if (ContentPanel != null)
            {
                LeftColumn = (ContentPanel as Grid).ColumnDefinitions[0];
                RightColumn = (ContentPanel as Grid).ColumnDefinitions[2];
            }
            else { throw new Exception("Failed to get ContentPanel grid columns"); }

            if (WindowSplitter != null)
            {
                WindowSplitter.MouseLeftButtonDown += new MouseButtonEventHandler(Splitter_MouseLeftButtonDown);
                WindowSplitter.MouseLeftButtonUp += new MouseButtonEventHandler(Splitter_MouseLeftButtonUp);
                WindowSplitter.MouseMove += new MouseEventHandler(Splitter_MouseMove);
                WindowSplitter.MouseEnter += new MouseEventHandler(Splitter_MouseEnter);
                WindowSplitter.MouseLeave += new MouseEventHandler(Splitter_MouseLeave);
            }
        }

        #region Splitter Events
        void Splitter_MouseEnter(object sender, MouseEventArgs e)
        {
            (WindowSplitter as Grid).Background = new SolidColorBrush(new Color() { A = 255, R = 163, G = 204, B = 255 });
        }

        void Splitter_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsSplitting)
            {
                (WindowSplitter as Grid).Background = SplitterBackground;
            }
        }

        void Splitter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LeftContainer != null && RightContainer != null)
            {
                IsSplitting = (sender as FrameworkElement).CaptureMouse();
                lastSplitPosition = e.GetPosition(ContentContainer);
                maxColWitdh = ContentContainer.ActualWidth - 4;
                gridLWidth0 = LeftContainer.ActualWidth;
                gridRWidth0 = RightContainer.ActualWidth;
            }
        }

        void Splitter_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSplitting)
            {
                Point position = e.GetPosition(ContentContainer);
                ResetColumnWidth(position.X - lastSplitPosition.X);
            }
        }

        void Splitter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsSplitting)
            {
                (sender as FrameworkElement).ReleaseMouseCapture();
                IsSplitting = false;
            }
        }

        void ResetColumnWidth(double deltaX)
        {
            if (deltaX > 0)
            {
                double gridRWidth2 = gridRWidth0 - deltaX;
                if (gridRWidth2 > maxColWitdh) gridRWidth2 = maxColWitdh;
                LeftColumn.Width = new GridLength(1.0, GridUnitType.Star);
                RightColumn.Width = new GridLength(gridRWidth2 > 4 ? gridRWidth2 : 4);
            }
            else
            {
                double gridLWidth2 = gridLWidth0 + deltaX;
                if (gridLWidth2 > maxColWitdh) gridLWidth2 = maxColWitdh;
                LeftColumn.Width = new GridLength(gridLWidth2 > 4 ? gridLWidth2 : 4);
                RightColumn.Width = new GridLength(1.0, GridUnitType.Star);
            }
        }
        #endregion
    }
}
