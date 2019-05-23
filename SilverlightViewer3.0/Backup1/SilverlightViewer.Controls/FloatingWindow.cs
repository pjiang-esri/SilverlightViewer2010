// Author: Ping Jiang (pjiang@esri.com)

// COPYRIGHT 2009 ESRI
// TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
// Unpublished material - all rights reserved under the
// Copyright Laws of the United States.

// For additional information, contact:
// Environmental Systems Research Institute, Inc.
// Attn: Contracts Dept
// 380 New York Street
// Redlands, California, USA 92373

// Email: contracts@esri.com
// Website: http://www.esri.com

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;

namespace ESRI.SilverlightViewer.Controls
{

    [TemplatePart(Name = FloatingWindow._WINDOW_ICON, Type = typeof(Image))]
    [TemplatePart(Name = FloatingWindow._TITLE_BLOCK, Type = typeof(TextBlock))]
    [TemplatePart(Name = FloatingWindow._TITLE_PANEL, Type = typeof(StackPanel))]
    [TemplatePart(Name = FloatingWindow._HEADER_PANEL, Type = typeof(StackPanel))]
    [TemplatePart(Name = FloatingWindow._BUTTON_CLOSE, Type = typeof(CloseButton))]
    [TemplatePart(Name = FloatingWindow._BUTTON_TOGGLE, Type = typeof(ToggleButton))]
    [TemplatePart(Name = FloatingWindow._CONTENT_PANEL, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = FloatingWindow._WATERMARK_TEXT, Type = typeof(TextBlock))]
    [TemplatePart(Name = FloatingWindow._BUSY_ANIMATION, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = FloatingWindow._CONTENT_CONTAINER, Type = typeof(FrameworkElement))]
    [TemplateVisualState(GroupName = FloatingWindow._WINDOW_STATES, Name = FloatingWindow._STATE_EXPAND)]
    [TemplateVisualState(GroupName = FloatingWindow._WINDOW_STATES, Name = FloatingWindow._STATE_COLLAPSE)]
    public class FloatingWindow : FloatingControl
    {
        protected const string _WINDOW_STATES = "WindowStates";
        protected const string _STATE_EXPAND = "StateExpand";
        protected const string _STATE_COLLAPSE = "StateCollapse";
        protected const string _BUSY_ANIMATION = "BusyAnimation";
        protected const string _WATERMARK_TEXT = "WatermarkText";

        protected const string _WINDOW_ICON = "WindowIcon";
        protected const string _TITLE_PANEL = "TitlePanel";
        protected const string _TITLE_BLOCK = "WindowTitle";
        protected const string _HEADER_PANEL = "HeaderPanel";
        protected const string _BUTTON_PANEL = "ButtonPanel";
        protected const string _BUTTON_CLOSE = "CloseButton";
        protected const string _BUTTON_TOGGLE = "ToggleButton";
        protected const string _CONTENT_PANEL = "ContentPanel";
        protected const string _CONTENT_CONTAINER = "ContentContainer";

        protected const string _RESIZEBAR_NORTH = "resizebarN";
        protected const string _RESIZEBAR_SOUTH = "resizebarS";
        protected const string _RESIZEBAR_WEST = "resizebarW";
        protected const string _RESIZEBAR_EAST = "resizebarE";

        protected Image IconImage;
        protected TextBlock TitleBlock;
        protected StackPanel TitlePanel;
        protected StackPanel HeaderPanel;
        protected CloseButton WindowCloseButton;
        protected ToggleButton WindowToggleButton;
        protected FrameworkElement ContentContainer;
        protected FrameworkElement ContentPanel;
        protected BusySignal BusyAnimation;
        protected TextBlock WatermarkText;
   
        protected bool IsToggling = false;
        protected bool isResizing = false;
        protected string OpenStoryboardName = null;
        protected string CloseStoryboardName = null;
        protected string RollUpStoryboardName = null;
        protected string RollDownStoryboardName = null;

        // Private variables for readable only properties
        private double currentTop = Double.NaN;
        private double currentLeft = Double.NaN;
        private double currentWidth = Double.NaN;
        private double currentHeight = Double.NaN;
        private bool isTemplateApplied = false;

        #region Define Open, Close and Toggle Event Handlers and Events
        private WindowOpenEventHandler OpenHandler = null;
        public event WindowOpenEventHandler Open
        {
            add
            {
                if (OpenHandler == null || !OpenHandler.GetInvocationList().Contains(value))
                    OpenHandler += value;
            }
            remove
            {
                OpenHandler -= value;
            }
        }

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

        private WindowToggleEventHandler ToggleHandler = null;
        public event WindowToggleEventHandler Toggle
        {
            add
            {
                if (ToggleHandler == null || !ToggleHandler.GetInvocationList().Contains(value))
                    ToggleHandler += value;
            }
            remove
            {
                ToggleHandler -= value;
            }
        }
        #endregion Custom Events

        public FloatingWindow()
        {
            this.DefaultStyleKey = typeof(FloatingWindow);
        }

        #region Readable Only Properties
        protected double CurrentTop { get { return currentTop; } }
        protected double CurrentLeft { get { return currentLeft; } }
        protected double CurrentWidth { get { return currentWidth; } }
        protected double CurrentHeight { get { return currentHeight; } }
        protected double ScrollVerticalOffset { get { return (ContentPanel as ScrollViewer).VerticalOffset; } }
        protected double ScrollHorizontalOffset { get { return (ContentPanel as ScrollViewer).HorizontalOffset; } }
        protected bool IsTemplateApplied { get { return isTemplateApplied; } }
        #endregion

        #region Dependency Properties
        // Using a DependencyProperty as the backing store for Title. 
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(FloatingWindow), null);

        // Using a DependencyProperty as the backing store for RadiusX. 
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register("RadiusX", typeof(double), typeof(FloatingWindow), new PropertyMetadata((double)8.0));

        // Using a DependencyProperty as the backing store for RadiusY. 
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register("RadiusY", typeof(double), typeof(FloatingWindow), new PropertyMetadata((double)8.0));

        // Using a DependencyProperty as the backing store for Window Icon ImageSource. 
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(FloatingWindow), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for Title Font Color. 
        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register("TitleFontSize", typeof(double), typeof(FloatingWindow), null);

        // Using a DependencyProperty as the backing store for Title Font Color. 
        public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register("TitleForeground", typeof(SolidColorBrush), typeof(FloatingWindow), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        // Using a DependencyProperty as the backing store for ContentBackground Color. 
        public static readonly DependencyProperty ContentBackgroundProperty = DependencyProperty.Register("ContentBackground", typeof(SolidColorBrush), typeof(FloatingWindow), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        // Using a DependencyProperty as the backing store for BackgroundOpacity. 
        public static readonly DependencyProperty BackgroundOpacityProperty = DependencyProperty.Register("BackgroundOpacity", typeof(double), typeof(FloatingWindow), new PropertyMetadata((double)0.75));

        // Using a DependencyProperty as the backing store for ForegroundOpacity. 
        public static readonly DependencyProperty ForegroundOpacityProperty = DependencyProperty.Register("ForegroundOpacity", typeof(double), typeof(FloatingWindow), new PropertyMetadata((double)0.90));

        // Using a DependencyProperty as the backing store for IsBusy.
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(FloatingWindow), new PropertyMetadata(false, new PropertyChangedCallback(OnIsBusyChanged)));

        // Using a DependencyProperty as the backing store for IsOpen.
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(FloatingWindow), new PropertyMetadata(true, new PropertyChangedCallback(OnIsOpenChanged)));

        // Using a DependencyProperty as the backing store for IsExpanded. This enables animation...
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(FloatingWindow), new PropertyMetadata(true, new PropertyChangedCallback(OnIsExpandedChanged)));

        // Using a DependencyProperty as the backing store for IsResizable.
        public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register("IsResizable", typeof(bool), typeof(FloatingWindow), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for DisableAnimation.
        public static readonly DependencyProperty DisableAnimationProperty = DependencyProperty.Register("DisableAnimation", typeof(bool), typeof(FloatingWindow), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for AllowOutOfWindow.
        public static readonly DependencyProperty AllowOutOfWindowProperty = DependencyProperty.Register("AllowOutOfWindow", typeof(bool), typeof(FloatingWindow), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for VerticalScrollBarVisibility.
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(FloatingWindow), null);

        // Using a DependencyProperty as the backing store for HorizontalScrollBarVisibility.
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(FloatingWindow), null);

        /// <summary>
        /// Sets and gets the title for the window
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Sets and gets the circle radius of round window corner in X direction
        /// </summary>
        public double RadiusX
        {
            get { return (double)GetValue(RadiusXProperty); }
            set { SetValue(RadiusXProperty, value); }
        }

        /// <summary>
        /// Sets and gets the circle radius of round window corner in Y direction
        /// </summary>
        public double RadiusY
        {
            get { return (double)GetValue(RadiusYProperty); }
            set { SetValue(RadiusYProperty, value); }
        }

        /// <summary>
        /// Sets and gets the source of the icon for the window
        /// </summary>
        public ImageSource IconSource
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        /// <summary>
        /// Sets and gets a size value for the title font 
        /// </summary>
        public double TitleFontSize
        {
            get { return (double)GetValue(TitleFontSizeProperty); }
            set { SetValue(TitleFontSizeProperty, value); }
        }

        /// <summary>
        /// Sets and gets the title backgorund color
        /// </summary>
        public SolidColorBrush TitleForeground
        {
            get { return (SolidColorBrush)GetValue(TitleForegroundProperty); }
            set { SetValue(TitleForegroundProperty, value); }
        }

        /// <summary>
        /// Sets and gets the content background color
        /// </summary>
        public SolidColorBrush ContentBackground
        {
            get { return (SolidColorBrush)GetValue(ContentBackgroundProperty); }
            set { SetValue(ContentBackgroundProperty, value); }
        }

        /// <summary>
        /// Sets and gets a value for the window background opacity (0.0 - Transparent, 1.0 - Opaque) 
        /// </summary>
        public double BackgroundOpacity
        {
            get { return (double)GetValue(BackgroundOpacityProperty); }
            set { SetValue(BackgroundOpacityProperty, value); }
        }

        /// <summary>
        /// Sets and gets a value for the content opacity (0.0 - Transparent, 1.0 - Opaque) 
        /// </summary>
        public double ForegroundOpacity
        {
            get { return (double)GetValue(ForegroundOpacityProperty); }
            set { SetValue(ForegroundOpacityProperty, value); }
        }

        /// <summary>
        /// Sets and gets the visibility of the vertical scroll bar for content: Auto(default), Disabled, Hidden, or Visible
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Sets and gets the visibility of the horizontal scroll bar for content: Auto(default), Disabled, Hidden, or Visible
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Sets and gets the window busy state value (true - show busy indicator)
        /// </summary>
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        /// <summary>
        /// Sets and gets the window state value - Open or Close;
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Sets and gets the window state value - Expanded or Collapsed
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        /// <summary>
        /// Sets and gets a value to indicate if the window is resizable
        /// </summary>
        public bool IsResizable
        {
            get { return (bool)GetValue(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        /// <summary>
        /// Sets and gets a value to disable or enable the open and close animations of the window
        /// </summary>
        public bool DisableAnimation
        {
            get { return (bool)GetValue(DisableAnimationProperty); }
            set { SetValue(DisableAnimationProperty, value); }
        }

        /// <summary>
        /// Sets and gets a value to allow or disallow the child window to float out of the main window
        /// </summary>
        public bool AllowOutOfWindow
        {
            get { return (bool)GetValue(AllowOutOfWindowProperty); }
            set { SetValue(AllowOutOfWindowProperty, value); }
        }

        protected static void OnIsBusyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            bool isBusy = (bool)e.NewValue;
            FloatingWindow me = o as FloatingWindow;

            if (me != null && me.BusyAnimation != null)
            {
                me.BusyAnimation.State = isBusy ? BusySignalState.STATE_BUSY : BusySignalState.STATE_HIDE;
            }
        }

        protected static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            bool isOpen = (bool)e.NewValue;
            FloatingWindow me = o as FloatingWindow;

            if (me != null)
            {
                if (isOpen) { me.OpenMe(); }
                else { me.CloseMe(); }
            }
        }

        protected static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool expanded = (bool)e.NewValue;
            FloatingWindow me = d as FloatingWindow;

            if (me != null)
            {
                if (expanded)
                {
                    me.Width = me.currentWidth;
                    me.Height = me.currentHeight;
                    me.WindowToggleButton.State = ToggleButtonState.STATE_ORIGIN;
                    VisualStateManager.GoToState(me, _STATE_EXPAND, true);
                    if (me.RollDownStoryboardName != null)
                    {
                        Storyboard sb = me.Resources[me.RollDownStoryboardName] as Storyboard;
                        if (sb != null) { me.IsToggling = true; sb.Begin(); }
                    }
                }
                else
                {
                    me.WindowToggleButton.State = ToggleButtonState.STATE_ROTATE270;
                    VisualStateManager.GoToState(me, _STATE_COLLAPSE, true);
                    if (me.RollUpStoryboardName != null)
                    {
                        Storyboard sb = me.Resources[me.RollUpStoryboardName] as Storyboard;
                        if (sb != null) { me.IsToggling = true; sb.Begin(); }
                    }
                }
            }
        }
        #endregion Dependency Properties

        // Locate Elements and Add Event Listeners
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TitlePanel = this.GetTemplateChild(_TITLE_PANEL) as StackPanel;
            if (TitlePanel == null)
            {
                throw new Exception("Apply Template Error: Failed to get the Title Panel");
            }

            HeaderPanel = this.GetTemplateChild(_HEADER_PANEL) as StackPanel;
            if (HeaderPanel == null)
            {
                throw new Exception("Apply Template Error: Failed to get the Header Panel");
            }

            BusyAnimation = this.GetTemplateChild(_BUSY_ANIMATION) as BusySignal;
            if (BusyAnimation != null)
            {
                BusyAnimation.State = IsBusy ? BusySignalState.STATE_BUSY : BusySignalState.STATE_HIDE;
            }

            WatermarkText = this.GetTemplateChild(_WATERMARK_TEXT) as TextBlock;
            if (WatermarkText != null)
            {
                WatermarkText.Visibility = Visibility.Collapsed;
            }

            ContentContainer = this.GetTemplateChild(_CONTENT_CONTAINER) as FrameworkElement;
            if (ContentContainer != null)
            {
                ContentContainer.SizeChanged += new SizeChangedEventHandler(ContentContainer_SizeChanged);
                if (!IsExpanded) ContentContainer.Visibility = Visibility.Collapsed;
            }

            ContentPanel = this.GetTemplateChild(_CONTENT_PANEL) as FrameworkElement;
            if (ContentPanel != null)
            {
                ContentPanel.SizeChanged += new SizeChangedEventHandler(ContentPanel_SizeChanged);
            }

            IconImage = this.GetTemplateChild(_WINDOW_ICON) as Image;
            if (IconImage != null)
            {
                if (IconSource != null) IconImage.Source = IconSource;
                else IconImage.Visibility = Visibility.Collapsed;
            }

            TitleBlock = this.GetTemplateChild(_TITLE_BLOCK) as TextBlock;
            if (TitleBlock != null)
            {
                if (TitleFontSize > 0.0) TitleBlock.FontSize = TitleFontSize;
                if (TitleForeground != null) TitleBlock.Foreground = TitleForeground;
            }

            WindowToggleButton = this.GetTemplateChild(_BUTTON_TOGGLE) as ToggleButton;
            WindowCloseButton = this.GetTemplateChild(_BUTTON_CLOSE) as CloseButton;

            if (WindowToggleButton != null)
            {
                WindowToggleButton.Click += new RoutedEventHandler(WindowToggleButton_Click);
            }

            if (WindowCloseButton != null)
            {
                WindowCloseButton.Click += new RoutedEventHandler(WindowCloseButton_Click);
            }

            if (IsResizable)
            {
                FrameworkElement resizebarN = GetTemplateChild(_RESIZEBAR_NORTH) as FrameworkElement;
                FrameworkElement resizebarS = GetTemplateChild(_RESIZEBAR_SOUTH) as FrameworkElement;
                FrameworkElement resizebarW = GetTemplateChild(_RESIZEBAR_WEST) as FrameworkElement;
                FrameworkElement resizebarE = GetTemplateChild(_RESIZEBAR_EAST) as FrameworkElement;

                if (resizebarN != null && resizebarS != null && resizebarW != null && resizebarE != null)
                {
                    resizebarN.Cursor = Cursors.SizeNS;
                    resizebarS.Cursor = Cursors.SizeNS;
                    resizebarW.Cursor = Cursors.SizeWE;
                    resizebarE.Cursor = Cursors.SizeWE;

                    resizebarN.MouseLeftButtonDown += new MouseButtonEventHandler(Resizebar_MouseLeftButtonDown);
                    resizebarS.MouseLeftButtonDown += new MouseButtonEventHandler(Resizebar_MouseLeftButtonDown);
                    resizebarW.MouseLeftButtonDown += new MouseButtonEventHandler(Resizebar_MouseLeftButtonDown);
                    resizebarE.MouseLeftButtonDown += new MouseButtonEventHandler(Resizebar_MouseLeftButtonDown);

                    resizebarN.MouseMove += new MouseEventHandler(Resizebar_MouseMove);
                    resizebarS.MouseMove += new MouseEventHandler(Resizebar_MouseMove);
                    resizebarW.MouseMove += new MouseEventHandler(Resizebar_MouseMove);
                    resizebarE.MouseMove += new MouseEventHandler(Resizebar_MouseMove);

                    resizebarN.MouseLeftButtonUp += new MouseButtonEventHandler(Resizebar_MouseLeftButtonUp);
                    resizebarS.MouseLeftButtonUp += new MouseButtonEventHandler(Resizebar_MouseLeftButtonUp);
                    resizebarW.MouseLeftButtonUp += new MouseButtonEventHandler(Resizebar_MouseLeftButtonUp);
                    resizebarE.MouseLeftButtonUp += new MouseButtonEventHandler(Resizebar_MouseLeftButtonUp);
                }
            }

            if (this.IsExpanded && this.DisableAnimation) this.Focus();
            this.isTemplateApplied = true;
            this.OnOpen();
        }

        #region Return container dimension
        public double ContainerWidth
        {
            get { return ContentPanel.ActualWidth - 4; }
        }

        public double ContainerHeight
        {
            get { return ContentPanel.ActualHeight - 4; }
        }
        #endregion

        #region Open and Close Functions
        /// <summary>
        /// Open the window. If the window is loaded at the first time, call OnWidgetLoad, or call OnOpen and dispatch an Open event
        /// </summary>
        private void OpenMe()
        {
            if (this.Visibility == Visibility.Visible) return;

            if (!DisableAnimation)
            {
                // Create Open and Close Animation Storyboards if they are not available in Resources
                if (OpenStoryboardName == null || !this.Resources.Contains(OpenStoryboardName))
                {
                    // The projection property values must be initialized
                    CreateOpenCloseStoryboard(true);
                }

                this.Visibility = Visibility.Visible;
                ((Storyboard)this.Resources[OpenStoryboardName]).Begin();
            }
            else
            {
                this.Visibility = Visibility.Visible;
                if (this.IsExpanded && this.isTemplateApplied) this.Focus();
            }

            if (isTemplateApplied) OnOpen();
        }

        /// <summary>
        /// Close the Window and dispatch a Close event
        /// </summary>
        private void CloseMe()
        {
            if (this.Visibility == Visibility.Collapsed) return;

            if (isTemplateApplied)
            {
                if (!DisableAnimation)
                {
                    // Create Open and Close Animation Storyboards if they are not available in Resources
                    if (CloseStoryboardName == null || !this.Resources.Contains(CloseStoryboardName))
                    {
                        // The projection property values must be zero
                        CreateOpenCloseStoryboard(false);
                    }

                    if (this.IsActive) this.ResetActiveWindow(false);
                    ((Storyboard)this.Resources[CloseStoryboardName]).Begin();
                }
                else
                {
                    this.Visibility = Visibility.Collapsed;
                }

                OnClose();
            }
            else { this.Visibility = Visibility.Collapsed; }
        }
        #endregion

        #region Handle Toggle and Close Button Events
        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        private void WindowToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsExpanded = !this.IsExpanded;

            if (this.IsExpanded) this.Focus();
            else if (this.IsActive) this.ResetActiveWindow(false);

            OnToggle(new WindowToggleEventArgs(this.IsExpanded));
        }

        /// <summary>
        /// Dispatch an Open event invoked by re-open the window (not first load)
        /// </summary>
        protected virtual void OnOpen()
        {
            if (OpenHandler != null) OpenHandler(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Dispatch a Close event invoked by clicking the close button
        /// </summary>
        protected virtual void OnClose()
        {
            if (CloseHandler != null) CloseHandler(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Dispatch a Toggle event invoked by collapsing or expanding the window
        /// </summary>
        /// <param name="e">WindowToggleEventArgs</param>
        protected virtual void OnToggle(WindowToggleEventArgs e)
        {
            if (ToggleHandler != null) ToggleHandler(this, e);
        }
        #endregion

        #region Handle Container Size Change Events
        protected virtual void ContentContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Panel container = sender as Panel;
            if (container != null)
            {
                if (container.Clip == null)
                {
                    RectangleGeometry clip = new RectangleGeometry();
                    clip.Rect = new Rect(0, 0, container.ActualWidth, container.ActualHeight);
                    container.Clip = clip;
                }
                else
                {
                    RectangleGeometry clip = container.Clip as RectangleGeometry;
                    clip.Rect = new Rect(0, 0, container.ActualWidth, container.ActualHeight);
                }
            }
        }

        protected virtual void ContentPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement content = sender as FrameworkElement;
            if (content != null)
            {
                if (RollUpStoryboardName == null || RollDownStoryboardName == null || content.RenderTransform == null)
                {
                    string sGUID = Guid.NewGuid().ToString();
                    TranslateTransform transform = new TranslateTransform() { Y = 0 };
                    content.RenderTransform = transform;

                    RollUpStoryboardName = "Up_" + sGUID;
                    RollDownStoryboardName = "Down_" + sGUID;

                    CreateToggleStoryboard(transform, RollUpStoryboardName, ContainerHeight, 0);
                    CreateToggleStoryboard(transform, RollDownStoryboardName, 0, ContainerHeight);
                }
                else UpdateToggleStoryboard(ContainerHeight);
            }
        }
        #endregion

        #region Create and Save StoryBoards and Handle Storyboard Events
        /// <summary>
        /// Create Open and Close Animation Storyboards
        /// </summary>
        protected virtual void CreateOpenCloseStoryboard(bool initialized)
        {
            Random RandMaker = new Random();
            int axis = RandMaker.Next(-1, 1);
            int direct = RandMaker.Next(-1, 1);
            double offset = axis * direct * 600;
            if (offset == 0) offset = -600;

            if (this.Projection == null || !(this.Projection is PlaneProjection))
            {
                PlaneProjection projection = new PlaneProjection();
                this.Projection = projection;

                if (initialized)
                {
                    switch (axis)
                    {
                        case -1: projection.RotationX = 270; break;
                        case 0: projection.RotationZ = 270; break;
                        case 1: projection.RotationY = 270; break;
                    }

                    switch (direct)
                    {
                        case -1: projection.LocalOffsetX = offset; break;
                        case 0: projection.LocalOffsetZ = offset; break;
                        case 1: projection.LocalOffsetY = offset; break;
                    }
                }
            }

            string sGUID = Guid.NewGuid().ToString();
            CloseStoryboardName = "Close_" + sGUID;
            OpenStoryboardName = "Open_" + sGUID;

            SaveOpenCloseStoryboard(OpenStoryboardName, axis, direct, 0, 0);
            SaveOpenCloseStoryboard(CloseStoryboardName, axis, direct, 270, offset);
        }

        protected virtual void SaveOpenCloseStoryboard(string name, int axis, int direct, double rotate, double offset)
        {
            Storyboard sb = new Storyboard();
            if (name == OpenStoryboardName) sb.Completed += new EventHandler(OpenAnimation_Completed);
            if (name == CloseStoryboardName) sb.Completed += new EventHandler(CloseAnimation_Completed);

            string rotationAxis = "RotationX";
            switch (axis)
            {
                case -1: rotationAxis = "RotationX"; break;
                case 0: rotationAxis = "RotationZ"; break;
                case 1: rotationAxis = "RotationY"; break;
            }

            DoubleAnimation dbAnimR = new DoubleAnimation() { To = rotate, Duration = TimeSpan.FromSeconds(0.75), AutoReverse = false };
            Storyboard.SetTarget(dbAnimR, this.Projection);
            Storyboard.SetTargetProperty(dbAnimR, new PropertyPath(rotationAxis));
            sb.Children.Add(dbAnimR);

            string offsetDirect = "LocalOffsetX";
            switch (direct)
            {
                case -1: offsetDirect = "LocalOffsetX"; break;
                case 0: offsetDirect = "LocalOffsetZ"; break;
                case 1: offsetDirect = "LocalOffsetY"; break;
            }

            DoubleAnimation dbAnimO = new DoubleAnimation() { To = offset, Duration = TimeSpan.FromSeconds(0.75), AutoReverse = false };
            Storyboard.SetTarget(dbAnimO, this.Projection);
            Storyboard.SetTargetProperty(dbAnimO, new PropertyPath(offsetDirect));
            sb.Children.Add(dbAnimO);

            this.Resources.Add(name, sb);
        }

        /// <summary>
        /// Create Toggle Storyboard for Expanding and Collapsing the Window
        /// </summary>
        /// <param name="transform">TranslateTransform</param>
        /// <param name="sbName">Storyboard name used as Key in Resource Dictionary</param>
        /// <param name="fromValue">From value for Transform's Y Property</param>
        /// <param name="toValue">To value for Transform's Y Property</param>
        protected virtual void CreateToggleStoryboard(TranslateTransform transform, string sbName, double fromValue, double toValue)
        {
            Storyboard sb = new Storyboard();
            sb.Completed += new EventHandler(ToggleAnimation_Completed);

            DoubleAnimationUsingKeyFrames dbAnimKF = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(dbAnimKF, transform);
            Storyboard.SetTargetProperty(dbAnimKF, new PropertyPath("Y"));
            dbAnimKF.BeginTime = TimeSpan.FromSeconds(0.0);

            SplineDoubleKeyFrame keyFrame = new SplineDoubleKeyFrame();
            KeySpline spline = new KeySpline();
            spline.ControlPoint1 = new Point(0, 0);
            spline.ControlPoint2 = new Point(0, 1);
            keyFrame.KeySpline = spline;
            keyFrame.KeyTime = TimeSpan.FromSeconds(0.75);
            keyFrame.Value = -fromValue;
            dbAnimKF.KeyFrames.Add(keyFrame);
            sb.Children.Add(dbAnimKF);
            this.Resources.Add(sbName, sb);
        }

        protected virtual void UpdateToggleStoryboard(double value)
        {
            // Update Rollup Storyboard Only
            if (RollUpStoryboardName != null && Resources.Contains(RollUpStoryboardName))
            {
                Storyboard sb = Resources[RollUpStoryboardName] as Storyboard;
                DoubleAnimationUsingKeyFrames dbAnimKF = sb.Children[0] as DoubleAnimationUsingKeyFrames;
                SplineDoubleKeyFrame keyFrame = dbAnimKF.KeyFrames[0] as SplineDoubleKeyFrame;
                keyFrame.Value = -value;
            }
        }

        protected virtual void ToggleAnimation_Completed(object sender, EventArgs e)
        {
            this.IsToggling = false;
            if (!this.IsExpanded)
            {
                this.Width = Double.NaN;
                this.Height = Double.NaN;
            }
        }

        protected virtual void OpenAnimation_Completed(object sender, EventArgs e)
        {
            if (this.IsExpanded) this.Focus();
        }

        protected virtual void CloseAnimation_Completed(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Variables and Event Handlers for Resizing the Window
        protected Point lastSizePosition;

        protected virtual void Resizebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            // Get focus to be active
            if (this.IsExpanded) this.Focus();

            // Only when the window is expanded
            if (this.IsExpanded)
            {
                this.currentTop = Canvas.GetTop(this);
                this.currentLeft = Canvas.GetLeft(this);
                this.currentWidth = this.ActualWidth;
                this.currentHeight = this.ActualHeight;
                this.lastSizePosition = e.GetPosition(null);
                this.isResizing = (sender as FrameworkElement).CaptureMouse();
            }
        }

        protected virtual void Resizebar_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isResizing)
            {
                double delta = 0;
                double shift = 0;

                Point position = e.GetPosition(null);
                switch ((sender as FrameworkElement).Name)
                {
                    case _RESIZEBAR_NORTH:
                        delta = position.Y - this.lastSizePosition.Y;
                        shift = this.currentHeight - delta;

                        if (shift > MinHeight && shift < MaxHeight)
                        {
                            Canvas.SetTop(this, this.currentTop + delta);
                            this.Height = shift;
                        }
                        break;
                    case _RESIZEBAR_SOUTH:
                        delta = position.Y - this.lastSizePosition.Y;
                        shift = this.currentHeight + delta;
                        if (shift > MinHeight && shift < MaxHeight) this.Height = shift;
                        break;
                    case _RESIZEBAR_WEST:
                        delta = position.X - this.lastSizePosition.X;
                        shift = this.currentWidth - delta;
                        if (shift > MinWidth && shift < MaxWidth)
                        {
                            Canvas.SetLeft(this, this.currentLeft + delta);
                            this.Width = shift;
                        }
                        break;
                    case _RESIZEBAR_EAST:
                        delta = position.X - this.lastSizePosition.X;
                        shift = this.currentWidth + delta;
                        if (shift > MinWidth && shift < MaxWidth) this.Width = shift;
                        break;
                }
            }
        }

        protected virtual void Resizebar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            if (this.isResizing)
            {
                (sender as FrameworkElement).ReleaseMouseCapture();
                this.currentHeight = this.ActualHeight;
                this.currentWidth = this.ActualWidth;
                this.currentLeft = Canvas.GetLeft(this);
                this.currentTop = Canvas.GetTop(this);
                this.isResizing = false;
                e.Handled = true;
            }
        }
        #endregion

        #region Override Drag Event Handlers
        protected override void OnMouseLeftButtonDownControl(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            // Get focus to be active
            if (this.IsExpanded) this.Focus();

            // If not nailed, remember the beginning position and start dragging
            if (!IsNailed)
            {
                this.lastDragPosition = e.GetPosition(sender as FrameworkElement);
                this.IsDragging = (sender as FrameworkElement).CaptureMouse();
                //if (this.IsDragging) this.WatermarkText.Visibility = Visibility.Visible;
            }
        }

        protected override void OnMouseMoveControl(object sender, MouseEventArgs e)
        {
            if (this.IsDragging)
            {
                Point position = e.GetPosition(sender as FrameworkElement);
                double newTop = Canvas.GetTop(this) + position.Y - this.lastDragPosition.Y;
                double newLeft = Canvas.GetLeft(this) + position.X - this.lastDragPosition.X;

                if (this.AllowOutOfWindow)
                {
                    Canvas.SetTop(this, newTop);
                    Canvas.SetLeft(this, newLeft);
                }
                else
                {
                    if (newTop > -5 && (newTop < this.CanvasHeight - this.ActualHeight + 5 || position.Y < this.lastDragPosition.Y)) Canvas.SetTop(this, newTop);
                    if (newLeft > -5 && (newLeft < this.CanvasWidth - this.ActualWidth + 5 || position.X < this.lastDragPosition.X)) Canvas.SetLeft(this, newLeft);
                }

                //this.WatermarkText.Text = string.Format("{0}, {1}", Canvas.GetLeft(this), Canvas.GetTop(this));
            }
        }

        protected override void OnMouseLeftButtonUpControl(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            if (IsDragging)
            {
                //this.WatermarkText.Visibility = Visibility.Collapsed;
                (sender as FrameworkElement).ReleaseMouseCapture();
                this.currentLeft = Canvas.GetLeft(this);
                this.currentTop = Canvas.GetTop(this);
                this.IsDragging = false;
                e.Handled = true;
            }
        }
        #endregion
    }
}
