using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace ESRI.SilverlightViewer.Controls
{
    #region PopupWindow Control Class
    [TemplatePart(Name = PopupWindow._TITLE_BLOCK, Type = typeof(TextBlock))]
    [TemplatePart(Name = PopupWindow._BUTTON_CLOSE, Type = typeof(CloseButton))]
    [TemplatePart(Name = PopupWindow._CONTROL_GRIP, Type = typeof(FrameworkElement))]
    public class PopupWindow : ContentControl
    {
        protected const string _TITLE_BLOCK = "WindowTitle";
        protected const string _BUTTON_CLOSE = "CloseButton";
        protected const string _CONTROL_GRIP = "CONTROL_GRIP";

        protected const string _ARROW_UPPER_LEFT = "ArrowUpperLeft";
        protected const string _ARROW_LOWER_LEFT = "ArrowLowerLeft";
        protected const string _ARROW_UPPER_RIGHT = "ArrowUpperRight";
        protected const string _ARROW_LOWER_RIGHT = "ArrowLowerRight";

        protected const string _RESIZEBAR_EAST = "resizebarE";
        protected const string _RESIZEBAR_SOUTH = "resizebarS";

        TextBlock WindowTitleBlock = null;

        #region Define Close Event Handler and Event
        private WindowCloseEventHandler CloseHandler = null;
        public event WindowCloseEventHandler Close
        {
            add
            {
                if (CloseHandler == null || !CloseHandler.GetInvocationList().Contains(value))
                    CloseHandler += value;
            }
            remove
            {
                CloseHandler -= value;
            }
        }
        #endregion Custom Events

        /// <summary>
        /// For being used in Popup
        /// </summary>
        public PopupWindow()
        {
            this.DefaultStyleKey = typeof(PopupWindow);
        }

        #region Dependency Properties
        // Using a DependencyProperty as the backing store for Title. 
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PopupWindow), new PropertyMetadata("", new PropertyChangedCallback(OnTitleChanged)));

        // Using a DependencyProperty as the backing store for TitleFormat. 
        public static readonly DependencyProperty TitleFormatProperty = DependencyProperty.Register("TitleFormat", typeof(string), typeof(PopupWindow), new PropertyMetadata("{0}"));

        // Using a DependencyProperty as the backing store for TitleWrapping. 
        public static readonly DependencyProperty TitleWrappingProperty = DependencyProperty.Register("TitleWrapping", typeof(TextWrapping), typeof(PopupWindow), new PropertyMetadata(TextWrapping.NoWrap));

        // Using a DependencyProperty as the backing store for Title Font Color. 
        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register("TitleFontSize", typeof(double), typeof(PopupWindow), null);

        // Using a DependencyProperty as the backing store for Title Font Color. 
        public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register("TitleForeground", typeof(SolidColorBrush), typeof(PopupWindow), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        // Using a DependencyProperty as the backing store for BackgroundOpacity. 
        public static readonly DependencyProperty BackgroundOpacityProperty = DependencyProperty.Register("BackgroundOpacity", typeof(double), typeof(PopupWindow), new PropertyMetadata((double)0.75));

        // Using a DependencyProperty as the backing store for ForegroundOpacity. 
        public static readonly DependencyProperty ForegroundOpacityProperty = DependencyProperty.Register("ForegroundOpacity", typeof(double), typeof(PopupWindow), new PropertyMetadata((double)0.90));

        // Using a DependencyProperty as the backing store for ArrowDirection.
        public static readonly DependencyProperty ArrowDirectionProperty = DependencyProperty.Register("ArrowDirection", typeof(ArrowDirection), typeof(PopupWindow), new PropertyMetadata(ArrowDirection.UpperLeft));

        // Using a DependencyProperty as the backing store for ShowCloseButton.
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(PopupWindow), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsResizable.
        public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register("IsResizable", typeof(bool), typeof(PopupWindow), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for IsFloatable.
        public static readonly DependencyProperty IsFloatableProperty = DependencyProperty.Register("IsFloatable", typeof(bool), typeof(PopupWindow), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for ShowArrow.
        public static readonly DependencyProperty ShowArrowProperty = DependencyProperty.Register("ShowArrow", typeof(bool), typeof(PopupWindow), new PropertyMetadata(false));

        /// <summary>
        /// Popup Window Title
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Title Format String, e.g. "Layer: {0}"
        /// </summary>
        public string TitleFormat
        {
            get { return (string)GetValue(TitleFormatProperty); }
            set { SetValue(TitleFormatProperty, value); }
        }

        /// <summary>
        /// Specifies if title wraps when it reaches the end of the container
        /// </summary>
        public TextWrapping TitleWrapping
        {
            get { return (TextWrapping)GetValue(TitleWrappingProperty); }
            set { SetValue(TitleWrappingProperty, value); }
        }

        public double TitleFontSize
        {
            get { return (double)GetValue(TitleFontSizeProperty); }
            set { SetValue(TitleFontSizeProperty, value); }
        }

        public SolidColorBrush TitleForeground
        {
            get { return (SolidColorBrush)GetValue(TitleForegroundProperty); }
            set { SetValue(TitleForegroundProperty, value); }
        }

        public double BackgroundOpacity
        {
            get { return (double)GetValue(BackgroundOpacityProperty); }
            set { SetValue(BackgroundOpacityProperty, value); }
        }

        public double ForegroundOpacity
        {
            get { return (double)GetValue(ForegroundOpacityProperty); }
            set { SetValue(ForegroundOpacityProperty, value); }
        }

        public ArrowDirection ArrowDirection
        {
            get { return (ArrowDirection)GetValue(ArrowDirectionProperty); }
            set { SetValue(ArrowDirectionProperty, value); }
        }

        public bool ShowCloseButton
        {
            get { return (bool)GetValue(ShowCloseButtonProperty); }
            set { SetValue(ShowCloseButtonProperty, value); }
        }

        public bool IsResizable
        {
            get { return (bool)GetValue(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        public bool IsFloatable
        {
            get { return (bool)GetValue(IsFloatableProperty); }
            set { SetValue(IsFloatableProperty, value); }
        }

        public bool ShowArrow
        {
            get { return (bool)GetValue(ShowArrowProperty); }
            set { SetValue(ShowArrowProperty, value); }
        }

        protected static void OnTitleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PopupWindow me = o as PopupWindow;

            if (me != null && me.WindowTitleBlock != null)
            {
                string title = (string)e.NewValue;
                me.WindowTitleBlock.Text = string.Format(me.TitleFormat, title);
            }
        }
        #endregion Dependency Properties

        // Locate Elements and Add Event Listeners
        public override void OnApplyTemplate()
        {
            WindowTitleBlock = this.GetTemplateChild(_TITLE_BLOCK) as TextBlock;
            if (WindowTitleBlock != null)
            {
                if (TitleFontSize > 0.0) WindowTitleBlock.FontSize = TitleFontSize;
                if (TitleForeground != null) WindowTitleBlock.Foreground = TitleForeground;
                WindowTitleBlock.Text = string.Format(TitleFormat, Title);
                WindowTitleBlock.TextWrapping = TitleWrapping;
            }

            CloseButton WindowCloseButton = this.GetTemplateChild(_BUTTON_CLOSE) as CloseButton;
            if (WindowCloseButton != null)
            {
                if (this.ShowCloseButton)
                {
                    WindowCloseButton.Click += new RoutedEventHandler(WindowCloseButton_Click);
                }
                else
                {
                    WindowCloseButton.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            if (ShowArrow)
            {
                Shape ArrowUpperLeft = this.GetTemplateChild(_ARROW_UPPER_LEFT) as Shape;
                Shape ArrowLowerLeft = this.GetTemplateChild(_ARROW_LOWER_LEFT) as Shape;
                Shape ArrowUpperRight = this.GetTemplateChild(_ARROW_UPPER_RIGHT) as Shape;
                Shape ArrowLowerRight = this.GetTemplateChild(_ARROW_LOWER_RIGHT) as Shape;
                switch (this.ArrowDirection)
                {
                    case Controls.ArrowDirection.UpperLeft:
                        ArrowUpperLeft.Visibility = System.Windows.Visibility.Visible; break;
                    case Controls.ArrowDirection.LowerLeft:
                        ArrowLowerLeft.Visibility = System.Windows.Visibility.Visible; break;
                    case Controls.ArrowDirection.UpperRight:
                        ArrowUpperRight.Visibility = System.Windows.Visibility.Visible; break;
                    case Controls.ArrowDirection.LowerRight:
                        ArrowLowerRight.Visibility = System.Windows.Visibility.Visible; break;
                }
            }

            if (IsFloatable)
            {
                FrameworkElement ControlGrip = this.GetTemplateChild(_CONTROL_GRIP) as FrameworkElement;
                if (ControlGrip != null)
                {
                    ControlGrip.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDownControl);
                    ControlGrip.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUpControl);
                    ControlGrip.MouseMove += new MouseEventHandler(OnMouseMoveControl);
                }
            }

            if (IsResizable)
            {
                FrameworkElement resizebarS = GetTemplateChild(_RESIZEBAR_SOUTH) as FrameworkElement;
                FrameworkElement resizebarE = GetTemplateChild(_RESIZEBAR_EAST) as FrameworkElement;

                if (resizebarS != null && resizebarE != null)
                {
                    resizebarS.Cursor = Cursors.SizeNS;
                    resizebarE.Cursor = Cursors.SizeWE;

                    resizebarS.MouseLeftButtonDown += new MouseButtonEventHandler(Resizebar_MouseLeftButtonDown);
                    resizebarE.MouseLeftButtonDown += new MouseButtonEventHandler(Resizebar_MouseLeftButtonDown);

                    resizebarS.MouseMove += new MouseEventHandler(Resizebar_MouseMove);
                    resizebarE.MouseMove += new MouseEventHandler(Resizebar_MouseMove);

                    resizebarS.MouseLeftButtonUp += new MouseButtonEventHandler(Resizebar_MouseLeftButtonUp);
                    resizebarE.MouseLeftButtonUp += new MouseButtonEventHandler(Resizebar_MouseLeftButtonUp);
                }
            }
        }

        #region Handle Close Button Event
        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            OnClose();
        }

        /// <summary>
        /// Dispatch a Close event invoked by clicking the close button
        /// </summary>
        protected virtual void OnClose()
        {
            if (CloseHandler != null) CloseHandler(this, new RoutedEventArgs());
        }
        #endregion

        #region Variables and Event Handlers for Resizing the Window
        protected bool IsResizing = false;
        protected Point lastSizePosition;
        protected double CurrentWidth = 0.0;
        protected double CurrentHeight = 0.0;

        protected virtual void Resizebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            this.CurrentWidth = this.ActualWidth;
            this.CurrentHeight = this.ActualHeight;
            this.lastSizePosition = e.GetPosition(null);
            this.IsResizing = (sender as FrameworkElement).CaptureMouse();
        }

        protected virtual void Resizebar_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsResizing)
            {
                double delta = 0;
                double shift = 0;

                Point position = e.GetPosition(null);
                switch ((sender as FrameworkElement).Name)
                {
                    case _RESIZEBAR_SOUTH:
                        delta = position.Y - this.lastSizePosition.Y;
                        shift = this.CurrentHeight + delta;
                        if (shift > MinHeight && shift < MaxHeight) this.Height = shift;
                        break;
                    case _RESIZEBAR_EAST:
                        delta = position.X - this.lastSizePosition.X;
                        shift = this.CurrentWidth + delta;
                        if (shift > MinWidth && shift < MaxWidth) this.Width = shift;
                        break;
                }
            }
        }

        protected virtual void Resizebar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            if (this.IsResizing)
            {
                (sender as FrameworkElement).ReleaseMouseCapture();
                this.CurrentHeight = this.ActualHeight;
                this.CurrentWidth = this.ActualWidth;
                this.IsResizing = false;
            }
        }
        #endregion

        #region Variables and Event Handlers for Dragging the Window
        protected bool IsDragging = false;
        protected Point lastDragPosition;

        protected virtual void OnMouseLeftButtonDownControl(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            this.lastDragPosition = e.GetPosition(sender as FrameworkElement);
            this.IsDragging = (sender as FrameworkElement).CaptureMouse();
        }

        protected virtual void OnMouseMoveControl(object sender, MouseEventArgs e)
        {
            if (this.IsDragging)
            {
                Point position = e.GetPosition(sender as FrameworkElement);
                Canvas.SetLeft(this, Canvas.GetLeft(this) + position.X - this.lastDragPosition.X);
                Canvas.SetTop(this, Canvas.GetTop(this) + position.Y - this.lastDragPosition.Y);
            }
        }

        protected virtual void OnMouseLeftButtonUpControl(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            if (IsDragging)
            {
                (sender as FrameworkElement).ReleaseMouseCapture();
                this.IsDragging = false;
                e.Handled = true;
            }
        }
        #endregion
    }
    #endregion

    #region Static Floating Popup With a PopupWindow Plugged Inside
    public static class FloatingPopup
    {
        #region Private Member and Methods
        private static Canvas backCanvas = null;
        private static Popup popup = new Popup();

        private static void InternalShow(PopupWindow childWindow, Point position)
        {
            childWindow.Close += new WindowCloseEventHandler(Window_Close);
            Size hostSize = new Size(Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);

            if (popup.Child == null)
            {
                backCanvas = new Canvas();
                backCanvas.Background = new SolidColorBrush(Color.FromArgb(0X64, 0X00, 0X00, 0X00));
                popup.Child = backCanvas;
            }
            else
            {
                backCanvas = popup.Child as Canvas;
                backCanvas.Children.Clear();
            }

            // Add the Child Window on the Canvas 
            backCanvas.Children.Add(childWindow);

            if (position.X > 0 || position.Y > 0)
            {
                Canvas.SetTop(childWindow, position.Y);
                Canvas.SetLeft(childWindow, position.X);
            }
            else
            {
                CenterControl(childWindow, hostSize);
            }

            backCanvas.Width = hostSize.Width;
            backCanvas.Height = hostSize.Height;
            popup.IsOpen = true;
        }

        private static void Window_Close(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }

        private static void CenterControl(FrameworkElement element, Size hostSize)
        {
            if (element.ActualHeight == 0 || element.ActualHeight == double.NaN)
            {
                element.Measure(hostSize);
            }

            Canvas.SetTop(element, Math.Ceiling(hostSize.Height / 2 - element.DesiredSize.Height / 2));
            Canvas.SetLeft(element, Math.Ceiling(hostSize.Width / 2 - element.DesiredSize.Width / 2));
        }
        #endregion

        public static void Show(PopupWindow window)
        {
            InternalShow(window, new Point());
        }

        public static void Show(PopupWindow window, Point position)
        {
            InternalShow(window, position);
        }

        public static void Close()
        {
            popup.IsOpen = false;
        }
    }
    #endregion
}
