using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ESRI.SilverlightViewer.Controls
{
    public sealed class ButtonGrid : Grid
    {
        private Path backPath = null;
        private Path forePath = null;

        public ButtonGrid() : base()
        {
            this.Background = new SolidColorBrush(Colors.Transparent);
            this.RenderButtonGrid(true, true);

            this.MouseEnter += new MouseEventHandler(ButtonGrid_MouseEnter);
            this.MouseLeave += new MouseEventHandler(ButtonGrid_MouseLeave);
        }

        #region Dependency Property
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ButtonGrid), new PropertyMetadata(true, new PropertyChangedCallback(OnIsEnabledChange)));
        public static readonly DependencyProperty ForeShapeFillProperty = DependencyProperty.Register("ForeShapeFill", typeof(Brush), typeof(ButtonGrid), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), new PropertyChangedCallback(OnForeShapeFillChange)));
        public static readonly DependencyProperty BackShapeFillProperty = DependencyProperty.Register("BackShapeFill", typeof(Brush), typeof(ButtonGrid), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), new PropertyChangedCallback(OnBackShapeFillChange)));
        public static readonly DependencyProperty ForeBorderBrushProperty = DependencyProperty.Register("ForeBorderBrush", typeof(Brush), typeof(ButtonGrid), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), new PropertyChangedCallback(OnForeBorderBrushChange)));
        public static readonly DependencyProperty BackBorderBrushProperty = DependencyProperty.Register("BackBorderBrush", typeof(Brush), typeof(ButtonGrid), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), new PropertyChangedCallback(OnBackBorderBrushChange)));
        public static readonly DependencyProperty ForeBorderThickProperty = DependencyProperty.Register("ForeBorderThick", typeof(double), typeof(ButtonGrid), new PropertyMetadata(2.0, new PropertyChangedCallback(OnForeBorderThickChange)));
        public static readonly DependencyProperty BackBorderThickProperty = DependencyProperty.Register("BackBorderThick", typeof(double), typeof(ButtonGrid), new PropertyMetadata(1.0, new PropertyChangedCallback(OnBackBorderThickChange)));
        public static readonly DependencyProperty ForegroundShapeProperty = DependencyProperty.Register("ForegroundShape", typeof(Geometry), typeof(ButtonGrid), new PropertyMetadata(null, new PropertyChangedCallback(OnForegroundShapeChange)));
        public static readonly DependencyProperty BackgroundShapeProperty = DependencyProperty.Register("BackgroundShape", typeof(Geometry), typeof(ButtonGrid), new PropertyMetadata(null, new PropertyChangedCallback(OnBackgroundShapeChange)));
        public static readonly DependencyProperty MouseOverBackFillProperty = DependencyProperty.Register("MouseOverBackFill", typeof(Brush), typeof(ButtonGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty MouseOverForeFillProperty = DependencyProperty.Register("MouseOverForeFill", typeof(Brush), typeof(ButtonGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty MouseOverForeBorderBrushProperty = DependencyProperty.Register("MouseOverForeBorderBrush", typeof(Brush), typeof(ButtonGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty MouseOverBackBorderBrushProperty = DependencyProperty.Register("MouseOverBackBorderBrush", typeof(Brush), typeof(ButtonGrid), new PropertyMetadata(null));

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public Brush ForeShapeFill
        {
            get { return (Brush)GetValue(ForeShapeFillProperty); }
            set { SetValue(ForeShapeFillProperty, value); }
        }

        public Brush BackShapeFill
        {
            get { return (Brush)GetValue(BackShapeFillProperty); }
            set { SetValue(BackShapeFillProperty, value); }
        }

        public Brush ForeBorderBrush
        {
            get { return (Brush)GetValue(ForeBorderBrushProperty); }
            set { SetValue(ForeBorderBrushProperty, value); }
        }

        public Brush BackBorderBrush
        {
            get { return (Brush)GetValue(BackBorderBrushProperty); }
            set { SetValue(BackBorderBrushProperty, value); }
        }

        public double ForeBorderThick
        {
            get { return (double)GetValue(ForeBorderThickProperty); }
            set { SetValue(ForeBorderThickProperty, value); }
        }

        public double BackBorderThick
        {
            get { return (double)GetValue(BackBorderThickProperty); }
            set { SetValue(BackBorderThickProperty, value); }
        }

        public Geometry ForegroundShape
        {
            get { return (Geometry)GetValue(ForegroundShapeProperty); }
            set { SetValue(ForegroundShapeProperty, value); }
        }

        public Geometry BackgroundShape
        {
            get { return (Geometry)GetValue(BackgroundShapeProperty); }
            set { SetValue(BackgroundShapeProperty, value); }
        }

        public Brush MouseOverForeFill
        {
            get { return (Brush)GetValue(MouseOverForeFillProperty); }
            set { SetValue(MouseOverForeFillProperty, value); }
        }

        public Brush MouseOverBackFill
        {
            get { return (Brush)GetValue(MouseOverBackFillProperty); }
            set { SetValue(MouseOverBackFillProperty, value); }
        }

        public Brush MouseOverForeBorderBrush
        {
            get { return (Brush)GetValue(MouseOverForeBorderBrushProperty); }
            set { SetValue(MouseOverForeBorderBrushProperty, value); }
        }

        public Brush MouseOverBackBorderBrush
        {
            get { return (Brush)GetValue(MouseOverBackBorderBrushProperty); }
            set { SetValue(MouseOverBackBorderBrushProperty, value); }
        }

        #region Dependency Property Change Events
        private static void OnIsEnabledChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null)
            {
                if ((bool)e.NewValue)
                {
                    me.IsHitTestVisible = true;
                    me.SetBackToNormal();
                }
                else
                {
                    me.IsHitTestVisible = false;
                    me.SetToBeDisabled();
                }
            }
        }

        private static void OnForeShapeFillChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null && me.forePath != null) me.forePath.Fill = e.NewValue as Brush;
        }

        private static void OnBackShapeFillChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null && me.backPath != null) me.backPath.Fill = e.NewValue as Brush;
        }

        private static void OnForeBorderBrushChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null && me.forePath != null) me.forePath.Stroke = e.NewValue as Brush;
        }

        private static void OnBackBorderBrushChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null && me.backPath != null) me.backPath.Stroke = e.NewValue as Brush;
        }

        private static void OnForeBorderThickChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null && me.forePath != null) me.forePath.StrokeThickness = (double)e.NewValue;
        }

        private static void OnBackBorderThickChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null && me.backPath != null) me.backPath.StrokeThickness = (double)e.NewValue;
        }

        private static void OnForegroundShapeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null) me.RenderButtonGrid(false, true);
        }

        private static void OnBackgroundShapeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonGrid me = d as ButtonGrid;
            if (me != null) me.RenderButtonGrid(true, false);
        }
        #endregion
        #endregion

        #region Render this UIElement
        private void RenderButtonGrid(bool isBackChange, bool isForeChange)
        {
            this.Children.Clear();

            if (BackgroundShape != null && isBackChange)
            {
                if (backPath != null)
                    backPath.Data = BackgroundShape;
                else
                    backPath = new Path() { Data = BackgroundShape, Stretch = Stretch.Fill, Stroke = this.BackBorderBrush, StrokeThickness = this.BackBorderThick, Fill = this.BackShapeFill, UseLayoutRounding = true };
            }

            if (ForegroundShape != null && isForeChange)
            {
                if (forePath != null)
                    forePath.Data = ForegroundShape;
                else
                    forePath = new Path() { Data = ForegroundShape, Stretch = Stretch.Fill, Stroke = this.ForeBorderBrush, StrokeThickness = this.ForeBorderThick, Fill = this.ForeShapeFill, Margin = new Thickness(4, 4, 4, 4) };
            }

            if (backPath != null) this.Children.Add(backPath);
            if (forePath != null) this.Children.Add(forePath);
        }
        #endregion

        #region Mouse Hover and Leave Events
        private void ButtonGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.IsEnabled)
            {
                if (forePath != null)
                {
                    forePath.Fill = this.MouseOverForeFill;
                    forePath.Stroke = this.MouseOverForeBorderBrush;
                    forePath.Opacity = 1.0;
                }

                if (backPath != null)
                {
                    backPath.Fill = this.MouseOverBackFill;
                    backPath.Stroke = this.MouseOverBackBorderBrush;
                }
            }
        }

        private void ButtonGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            SetBackToNormal();
        }

        private void SetToBeDisabled()
        {
            if (forePath != null)
            {
                forePath.Fill = this.ForeShapeFill;
                forePath.Stroke = this.ForeBorderBrush;
                forePath.Opacity = 0.5;
            }

            if (backPath != null)
            {
                backPath.Fill = this.BackShapeFill;
                backPath.Stroke = this.BackBorderBrush;
            }
        }

        private void SetBackToNormal()
        {
            if (forePath != null)
            {
                forePath.Fill = this.ForeShapeFill;
                forePath.Stroke = this.ForeBorderBrush;
                forePath.Opacity = 1.0;
            }

            if (backPath != null)
            {
                backPath.Fill = this.BackShapeFill;
                backPath.Stroke = this.BackBorderBrush;
            }
        }
        #endregion
    }
}
