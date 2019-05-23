using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace ESRI.SilverlightViewer.Controls
{
    public abstract class FloatingControl : ContentControl
    {
        // A GRIP FRAMEWORK MUST BE ADDED IN THE SUB-CLASS TEMPLATE and NAMED WITH "CONTROL_GRIP"
        // IT IS USED TO CAPTURE MOUSE EVENTS and MAKE THE CONTROL MOVABLE
        protected const string _CONTROL_GRIP = "CONTROL_GRIP";
        protected FrameworkElement ControlGrip = null;

        protected static int CurrentZIndex = 1;
        protected static Canvas ParentCanvas = null;

        private static double canvasWidth = 0.0;
        private static double canvasHeight = 0.0;

        public FloatingControl()
        {
        }

        #region Readable Only Properties
        protected double CanvasWidth { get { return canvasWidth; } }
        protected double CanvasHeight { get { return canvasHeight; } }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(FloatingControl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsActivePropertyChanged)));
        public static readonly DependencyProperty IsNailedProperty = DependencyProperty.Register("IsNailed", typeof(bool), typeof(FloatingControl), new PropertyMetadata(false));
        public static readonly DependencyProperty InitialTopProperty = DependencyProperty.Register("InitialTop", typeof(double), typeof(FloatingControl), new PropertyMetadata((double)0.0));
        public static readonly DependencyProperty InitialLeftProperty = DependencyProperty.Register("InitialLeft", typeof(double), typeof(FloatingControl), new PropertyMetadata((double)0.0));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public bool IsNailed
        {
            get { return (bool)GetValue(IsNailedProperty); }
            set { SetValue(IsNailedProperty, value); }
        }

        public double InitialTop
        {
            get { return (double)GetValue(InitialTopProperty); }
            set { SetValue(InitialTopProperty, value); }
        }

        public double InitialLeft
        {
            get { return (double)GetValue(InitialLeftProperty); }
            set { SetValue(InitialLeftProperty, value); }
        }

        protected static void OnIsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool isActive = (bool)e.NewValue;
            FloatingControl me = d as FloatingControl;

            if (me != null)
            {
                bool isTaskbar = me is FloatingTaskbar;
                bool isDocked = (isTaskbar) ? ((me as FloatingTaskbar).DockPosition != DockPosition.NONE) : false;

                if (isActive)
                {
                    me.Opacity = 1.0;
                    if (!me.IsNailed && !isDocked) Canvas.SetZIndex(me, CurrentZIndex++);
                }
                else
                {
                    me.Opacity = 0.75;
                    Canvas.SetZIndex(me, (isTaskbar) ? 1 : 2);
                }

                me.OnIsActiveChanged(me, new IsActiveChangedEventArgs(isActive));
            }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            this.GotFocus += new RoutedEventHandler(FloatingControl_GotFocus);

            if (this.Parent is Canvas)
            {
                if (this.IsActive)
                {
                    this.Opacity = 1.0;
                    Canvas.SetZIndex(this, CurrentZIndex++);
                }
                else
                {
                    this.Opacity = 0.75;
                    Canvas.SetZIndex(this, 1);
                }

                // Set Initial Position
                Canvas.SetTop(this, InitialTop);
                Canvas.SetLeft(this, InitialLeft);

                ParentCanvas = this.Parent as Canvas;
                if (ParentCanvas != null)
                {
                    canvasWidth = ParentCanvas.ActualWidth;
                    canvasHeight = ParentCanvas.ActualHeight;
                    ParentCanvas.SizeChanged += new SizeChangedEventHandler(OnCanvasSizeChanged);
                }

                ControlGrip = this.GetTemplateChild(_CONTROL_GRIP) as FrameworkElement;
                if (ControlGrip != null)
                {
                    ControlGrip.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDownControl);
                    ControlGrip.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUpControl);
                    ControlGrip.MouseMove += new MouseEventHandler(OnMouseMoveControl);
                }
                else
                {
                    this.IsNailed = true;
                }
            }
            else if (System.Windows.Browser.HtmlPage.IsEnabled) // Not In Design Mode 
            {
                throw new Exception("The control must be a child of a Canvas");
            }
        }

        #region Focus Events Handlers
        void FloatingControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.IsActive)
            {
                this.ResetActiveWindow(true);
            }
        }
        #endregion

        #region Overrideable Canvas Size Change Event Handlers
        protected virtual void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasWidth = ParentCanvas.ActualWidth;
            canvasHeight = ParentCanvas.ActualHeight;
        }

        protected virtual void OnIsActiveChanged(object sender, IsActiveChangedEventArgs e)
        {
        }
        #endregion

        #region Variables and Drag Event Handler Abstracts
        protected bool IsDragging = false;
        protected Point lastDragPosition;

        /// <summary>
        /// Starts the dragging operation by bringing to the top the container,
        /// setting the initial drag position and indicating that a drag operation has started
        /// </summary>
        /// <param name="sender">The Grip Element - since we attached the event to it</param>
        /// <param name="e">Mouse button event arguments</param>
        protected abstract void OnMouseLeftButtonDownControl(object sender, MouseButtonEventArgs e);

        /// <summary>
        /// Drags the Container, if the indicator of drag operation is enabled (true).
        /// </summary>
        /// <param name="sender">The Grip Element - since we attached the event to it</param>
        /// <param name="e">Mouse event arguments</param>
        protected abstract void OnMouseMoveControl(object sender, MouseEventArgs e);

        /// <summary>
        /// Ends the dragging operation, and turns off the drag operation indicator
        /// </summary>
        /// <param name="sender">The Grip Element - since we attached the event to it</param>
        /// <param name="e">Mouse button event arguments</param>
        protected abstract void OnMouseLeftButtonUpControl(object sender, MouseButtonEventArgs e);

        #endregion

        #region Function Change Active Window
        /* =====================================================================
         * If current window is activated, deactivate other floating controls
         * If current window is deactivated, activate the Taskbar or an expanded 
         * window if Taskbar does not exist. Always set IsActive = false for a 
         * window before set IsActive = true for another window
         * =====================================================================*/
        protected void ResetActiveWindow(bool active)
        {
            if (ParentCanvas == null) return;

            if (active)
            {
                // Deactivate Other Windows
                if (ParentCanvas.Children.Count > 1)
                {
                    foreach (FrameworkElement child in ParentCanvas.Children)
                    {
                        if (child is FloatingControl)
                        {
                            FloatingControl win = child as FloatingControl;
                            if (win != this && win.IsActive)
                            {
                                win.IsActive = false;
                            }
                        }
                    }
                }

                this.IsActive = true;
            }
            else
            {
                this.IsActive = false;

                // Activate Taskbar if Taskbar exists
                bool found = false;
                foreach (Control child in ParentCanvas.Children)
                {
                    if (child is FloatingTaskbar)
                    {
                        FloatingTaskbar taskbar = child as FloatingTaskbar;
                        taskbar.IsActive = true;
                        taskbar.Focus();
                        found = true;
                        break;
                    }
                }

                // Activate an expanded window
                if (!found)
                {
                    foreach (Control child in ParentCanvas.Children)
                    {
                        if (child is FloatingWindow)
                        {
                            FloatingWindow win = child as FloatingWindow;
                            if (win != this && win.Visibility == Visibility.Visible && win.IsExpanded)
                            {
                                win.IsActive = true;
                                win.Focus();
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
