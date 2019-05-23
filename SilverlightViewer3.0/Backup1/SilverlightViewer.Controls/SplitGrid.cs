using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplatePart(Name = SplitGrid._LEFT_CONTAINER, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SplitGrid._RIGHT_CONTAINER, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SplitGrid._WINDOW_SPLITTER, Type = typeof(FrameworkElement))]
    public class SplitGrid : ContentControl
    {
        protected const string _TEMPLATE_GRID = "TemplateGrid";
        protected const string _LEFT_CONTAINER = "LeftContentPanel";
        protected const string _RIGHT_CONTAINER = "RightContentPanel";
        protected const string _WINDOW_SPLITTER = "WindowSplitter";

        protected FrameworkElement TemplateGrid = null;
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

        public SplitGrid()
        {
            this.DefaultStyleKey = typeof(SplitGrid);
        }

        #region Dependency Properties
        public static readonly DependencyProperty LeftWindowProperty = DependencyProperty.Register("LeftWindow", typeof(FrameworkElement), typeof(SplitGrid), null);
        public static readonly DependencyProperty RightWindowProperty = DependencyProperty.Register("RightWindow", typeof(FrameworkElement), typeof(SplitGrid), null);
        public static readonly DependencyProperty SplitterBackgroundProperty = DependencyProperty.Register("SplitterBackground", typeof(SolidColorBrush), typeof(SplitGrid), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 224, G = 255, B = 255 })));
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(SplitGrid), null);
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(SplitGrid), null);

        /// <summary>
        /// Set or Get the Left Framework of the Grid
        /// </summary>
        public FrameworkElement LeftWindow
        {
            get { return (FrameworkElement)this.GetValue(LeftWindowProperty); }
            set { this.SetValue(LeftWindowProperty, value); }
        }

        /// <summary>
        /// Set or Get the Right Framework of the Grid
        /// </summary>
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

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TemplateGrid = this.GetTemplateChild(_TEMPLATE_GRID) as FrameworkElement;
            LeftContainer = this.GetTemplateChild(_LEFT_CONTAINER) as FrameworkElement;
            RightContainer = this.GetTemplateChild(_RIGHT_CONTAINER) as FrameworkElement;
            WindowSplitter = this.GetTemplateChild(_WINDOW_SPLITTER) as FrameworkElement;

            if (TemplateGrid != null)
            {
                LeftColumn = (TemplateGrid as Grid).ColumnDefinitions[0];
                RightColumn = (TemplateGrid as Grid).ColumnDefinitions[2];
            }
            else { throw new Exception("Failed to get Template grid columns"); }

            if (WindowSplitter != null)
            {
                WindowSplitter.MouseLeftButtonDown += new MouseButtonEventHandler(Splitter_MouseLeftButtonDown);
                WindowSplitter.MouseLeftButtonUp += new MouseButtonEventHandler(Splitter_MouseLeftButtonUp);
                WindowSplitter.MouseMove += new MouseEventHandler(Splitter_MouseMove);
                WindowSplitter.MouseEnter += new MouseEventHandler(Splitter_MouseEnter);
                WindowSplitter.MouseLeave += new MouseEventHandler(Splitter_MouseLeave);
            }
        }

        #region Splitter Event Handlers
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
                lastSplitPosition = e.GetPosition(this);
                maxColWitdh = this.ActualWidth - 4;
                gridLWidth0 = LeftContainer.ActualWidth;
                gridRWidth0 = RightContainer.ActualWidth;
            }
        }

        void Splitter_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSplitting)
            {
                Point position = e.GetPosition(this);
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
