using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplatePart(Name = FloatingTaskbar._TASKBAR_BACK, Type = typeof(Shape))]
    [TemplatePart(Name = FloatingTaskbar._DOCK_STATION, Type = typeof(Shape))]
    public class FloatingTaskbar : FloatingControl
    {
        protected const string _TEMPLATE_GRID = "TemplateGrid";
        protected const string _TASKBAR_BACK = "TASKBAR_BACK";
        protected const string _DOCK_STATION = "DOCK_STATION";

        protected Grid TemplateGrid = null;
        protected Shape TaskbarBack = null;
        protected Shape DockStation = null;
        protected Border StateMenuPanel = null;
        protected StackPanel StateMenuStack = null;
        protected SolidColorBrush DockingShadeColor = new SolidColorBrush(Colors.Red) { Opacity = 0.25 };

        protected RadioButton RadioFloat = null;
        protected RadioButton RadioDockTop = null;
        protected RadioButton RadioDockBottom = null;

        protected bool IsDocking = false;
        protected bool IsDockable = true;
        protected bool IsInUpperPage = true;
        protected string StoryboardName = null;

        private double currentTop = 0.0;
        private double currentLeft = 0.0;
        private bool isTemplateApplied = false;

        #region Define Event Handlers and Events
        private TaskbarStateChangeEventHandler DockStateChangeHandler = null;
        public event TaskbarStateChangeEventHandler DockStateChange
        {
            add
            {
                if (DockStateChangeHandler == null || !DockStateChangeHandler.GetInvocationList().Contains(value))
                    DockStateChangeHandler += value;
            }

            remove
            {
                DockStateChangeHandler -= value;
            }
        }
        #endregion

        public FloatingTaskbar()
        {
            this.DefaultStyleKey = typeof(FloatingTaskbar);
        }

        #region Readable Only Properties
        protected double CurrentTop { get { return currentTop; } }
        protected double CurrentLeft { get { return currentLeft; } }
        protected bool IsTemplateApplied { get { return isTemplateApplied; } }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register("RadiusX", typeof(double), typeof(FloatingTaskbar), new PropertyMetadata((double)8.0));
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register("RadiusY", typeof(double), typeof(FloatingTaskbar), new PropertyMetadata((double)8.0));
        public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register("BarWidth", typeof(double), typeof(FloatingTaskbar), new PropertyMetadata((double)500.0));
        public static readonly DependencyProperty BarHeightProperty = DependencyProperty.Register("BarHeight", typeof(double), typeof(FloatingTaskbar), new PropertyMetadata((double)70.0));
        public static readonly DependencyProperty DockHeightProperty = DependencyProperty.Register("DockHeight", typeof(double), typeof(FloatingTaskbar), new PropertyMetadata((double)60.0));
        public static readonly DependencyProperty BackOpacityProperty = DependencyProperty.Register("BackOpacity", typeof(double), typeof(FloatingTaskbar), new PropertyMetadata((double)0.75));
        public static readonly DependencyProperty DockPositionProperty = DependencyProperty.Register("DockPosition", typeof(DockPosition), typeof(FloatingTaskbar), new PropertyMetadata(DockPosition.TOP));

        public double RadiusX
        {
            get { return (double)GetValue(RadiusXProperty); }
            set { SetValue(RadiusXProperty, value); }
        }

        public double RadiusY
        {
            get { return (double)GetValue(RadiusYProperty); }
            set { SetValue(RadiusYProperty, value); }
        }

        public double BarWidth
        {
            get { return (double)GetValue(BarWidthProperty); }
            set { SetValue(BarWidthProperty, value); }
        }

        public double BarHeight
        {
            get { return (double)GetValue(BarHeightProperty); }
            set { SetValue(BarHeightProperty, value); }
        }

        public double BackOpacity
        {
            get { return (double)GetValue(BackOpacityProperty); }
            set { SetValue(BackOpacityProperty, value); }
        }

        public double DockHeight
        {
            get { return (double)GetValue(DockHeightProperty); }
            set { SetValue(DockHeightProperty, value); }
        }

        [TypeConverter(typeof(DockPositionConverter))]
        public DockPosition DockPosition
        {
            get { return (DockPosition)GetValue(DockPositionProperty); }
            set { SetValue(DockPositionProperty, value); }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.CreateStoryboard();

            this.currentTop = InitialTop;
            this.currentLeft = InitialLeft;

            TaskbarBack = this.GetTemplateChild(_TASKBAR_BACK) as Shape;
            DockStation = this.GetTemplateChild(_DOCK_STATION) as Shape;
            TemplateGrid = this.GetTemplateChild(_TEMPLATE_GRID) as Grid;

            if (TemplateGrid != null && DockStation != null && TaskbarBack != null && ControlGrip != null)
            {
                DockStation.MouseLeftButtonDown += (o, e) => { this.Focus(); };

                Canvas StateMenuCanvas = new Canvas() { Margin = new Thickness(0, 0, 0, 0) };
                TemplateGrid.Children.Add(StateMenuCanvas);

                StateMenuPanel = new Border() { MaxWidth = 132, MaxHeight = 72, Visibility = Visibility.Collapsed, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(4) };
                StateMenuPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x66, 0x66, 0x99));
                StateMenuPanel.Background = new SolidColorBrush(Colors.White);
                StateMenuPanel.MouseLeave += new MouseEventHandler(OnMouseLeaveMenuPanel);
                StateMenuCanvas.Children.Add(StateMenuPanel);

                StateMenuStack = new StackPanel() { Margin = new Thickness(4, 4, 4, 4), Orientation = Orientation.Vertical, Background = new SolidColorBrush(Colors.Transparent) };
                StateMenuPanel.Child = StateMenuStack;

                RadioFloat = new RadioButton() { Margin = new Thickness(1, 1, 1, 1), GroupName = "RadioTaskbarState", Content = ResourcesUtil.GetResourceValue(ResourcesUtil.TASKBAR_FLOATING), Tag = DockPosition.NONE };
                RadioDockTop = new RadioButton() { Margin = new Thickness(1, 1, 1, 1), GroupName = "RadioTaskbarState", Content = ResourcesUtil.GetResourceValue(ResourcesUtil.TASKBAR_DOCK_TOP), Tag = DockPosition.TOP };
                RadioDockBottom = new RadioButton() { Margin = new Thickness(1, 1, 1, 1), GroupName = "RadioTaskbarState", Content = ResourcesUtil.GetResourceValue(ResourcesUtil.TASKBAR_DOCK_BOTTOM), Tag = DockPosition.BOTTOM };
                RadioFloat.Click += new RoutedEventHandler(OnStateRadioClicked);
                RadioDockTop.Click += new RoutedEventHandler(OnStateRadioClicked);
                RadioDockBottom.Click += new RoutedEventHandler(OnStateRadioClicked);

                StateMenuStack.Children.Add(RadioFloat);
                StateMenuStack.Children.Add(RadioDockTop);
                StateMenuStack.Children.Add(RadioDockBottom);
            }
            else if (TemplateGrid != null && ControlGrip == null)
            {
                TemplateGrid.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDownControl);
            }

            if (this.Effect != null)
            {
                if (DockStation != null) DockStation.Effect = this.Effect;
                if (TaskbarBack != null) TaskbarBack.Effect = this.Effect;
                this.Effect = null;
            }

            // After Canvas is loaded
            if (this.CanvasWidth > 0.0)
            {
                this.ResetTaskbarState();
            }

            this.isTemplateApplied = true;
        }

        #region Override Drag Event Handlers - Dock and Undock Taskbar
        protected override void OnMouseLeftButtonDownControl(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            // Get focus to be active
            this.Focus();

            ModifierKeys keys = Keyboard.Modifiers;
            if ((StateMenuPanel != null) && ((keys & ModifierKeys.Control) != 0))
            {
                // Show Taskbar Status Context Menu
                Point p = e.GetPosition(sender as FrameworkElement);
                StateMenuPanel.Visibility = Visibility.Visible;
                double top = (IsInUpperPage) ? (p.Y - 4) : (p.Y - StateMenuPanel.ActualHeight + 4);
                Canvas.SetTop(StateMenuPanel, top); // Add an offset
                Canvas.SetLeft(StateMenuPanel, p.X - 4); // Add an offset
            }
            else if (!IsNailed && DockPosition == DockPosition.NONE)
            {
                this.lastDragPosition = e.GetPosition(sender as FrameworkElement);
                this.IsDragging = (sender as FrameworkElement).CaptureMouse();
            }
        }

        protected override void OnMouseMoveControl(object sender, MouseEventArgs e)
        {
            if (this.IsDragging)
            {
                Point position = e.GetPosition(sender as FrameworkElement);
                this.currentTop = Canvas.GetTop(this) + position.Y - this.lastDragPosition.Y;
                this.currentLeft = Canvas.GetLeft(this) + position.X - this.lastDragPosition.X;

                Canvas.SetTop(this, this.currentTop);
                Canvas.SetLeft(this, this.currentLeft);
                RaiseStateChangeEvent(false);

                if (DockStation != null)
                {
                    double dockTop = CanvasHeight - this.currentTop - DockHeight;
                    double dockRight = this.currentLeft + BarWidth - CanvasWidth;

                    if (this.currentTop < 5 && !this.IsDocking)
                    {
                        this.IsDocking = true;
                        DockPosition = DockPosition.TOP;
                        DockStation.Fill = DockingShadeColor;
                        DockStation.Visibility = Visibility.Visible;
                        DockStation.Margin = new Thickness(-this.currentLeft, -this.currentTop, dockRight, 0);
                    }
                    else if (dockTop < BarHeight - DockHeight && !this.IsDocking)
                    {
                        this.IsDocking = true;
                        DockPosition = DockPosition.BOTTOM;
                        DockStation.Fill = DockingShadeColor;
                        DockStation.Visibility = Visibility.Visible;
                        DockStation.Margin = new Thickness(-this.currentLeft, dockTop, dockRight, 0);
                    }
                    else
                    {
                        DockPosition = DockPosition.NONE;
                        DockStation.Visibility = Visibility.Collapsed;
                        this.IsDocking = false;
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonUpControl(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            if (this.IsDragging)
            {
                (sender as FrameworkElement).ReleaseMouseCapture();
                this.IsDragging = false;
                e.Handled = true;

                if (this.IsDocking)
                {
                    RadioDockTop.IsChecked = DockPosition == DockPosition.TOP;
                    RadioDockBottom.IsChecked = DockPosition == DockPosition.BOTTOM;
                    this.UpdateStoryboard().Begin();
                }
            }
        }
        #endregion

        #region Handle Canvas Size Change Event and Reset Taskbar State
        protected override void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.OnCanvasSizeChanged(sender, e);
            if (isTemplateApplied) ResetTaskbarState();
        }

        protected virtual void ResetTaskbarState()
        {
            if (DockStation != null)
            {
                this.IsDockable = true;
                DockStation.Width = CanvasWidth;

                if (TaskbarBack != null && ControlGrip != null)
                {
                    if (DockPosition == DockPosition.NONE) UndockMe();
                    else DockMe();
                }
                else
                {
                    this.currentLeft = 0;
                    this.currentTop = (DockPosition == DockPosition.BOTTOM) ? CanvasHeight - DockHeight : 0;

                    this.IsNailed = true;
                    this.IsInUpperPage = DockPosition == DockPosition.TOP;
                    Canvas.SetZIndex(this, 1);
                    Canvas.SetTop(this, this.currentTop);
                    Canvas.SetLeft(this, this.currentLeft);
                    RaiseStateChangeEvent(true);
                }
            }
            else
            {
                this.IsDockable = false;
                this.DockPosition = DockPosition.NONE;
                this.IsInUpperPage = InitialTop < CanvasHeight / 2.0;
                RaiseStateChangeEvent(true);
            }

            this.Focus();
        }
        #endregion

        #region Context Menu Panel and Menu Event Handlers
        private void OnMouseLeaveMenuPanel(object sender, MouseEventArgs e)
        {
            StateMenuPanel.Visibility = Visibility.Collapsed;
        }

        private void OnStateRadioClicked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton)
            {
                RadioButton radio = sender as RadioButton;
                radio.ReleaseMouseCapture();
                this.IsDragging = false;

                StateMenuPanel.Visibility = Visibility.Collapsed;
                this.DockPosition = (DockPosition)radio.Tag;

                this.UpdateStoryboard().Begin();
            }
        }
        #endregion

        #region Dock and Undock Taskbar Functions
        protected virtual void DockMe()
        {
            this.currentLeft = 0;
            this.currentTop = (DockPosition == DockPosition.BOTTOM) ? CanvasHeight - DockHeight : 0;
            Canvas.SetTop(this, this.currentTop);
            Canvas.SetLeft(this, this.currentLeft);
            Canvas.SetZIndex(this, 1);
            RadioDockTop.IsChecked = DockPosition == DockPosition.TOP;
            RadioDockBottom.IsChecked = DockPosition == DockPosition.BOTTOM;
            this.IsInUpperPage = DockPosition == DockPosition.TOP;
            RaiseStateChangeEvent(true);

            ControlGrip.Height = DockHeight;
            DockStation.Fill = this.Background;
            DockStation.Margin = new Thickness(0, 0, 0, 0);
            DockStation.Visibility = Visibility.Visible;
            TaskbarBack.Visibility = Visibility.Collapsed;
            ToolTipService.SetToolTip(ControlGrip, ResourcesUtil.GetResourceValue(ResourcesUtil.TASKBAR_TOOLTIP_DOCK));
        }

        protected virtual void UndockMe()
        {
            this.currentTop = InitialTop;
            this.currentLeft = InitialLeft;
            Canvas.SetTop(this, InitialTop);
            Canvas.SetLeft(this, InitialLeft);
            Canvas.SetZIndex(this, CurrentZIndex++);
            RadioFloat.IsChecked = true;
            this.IsInUpperPage = InitialTop < CanvasHeight / 2.0;
            RaiseStateChangeEvent(true);

            ControlGrip.Height = BarHeight - 10;
            TaskbarBack.Visibility = Visibility.Visible;
            DockStation.Visibility = Visibility.Collapsed;
            ToolTipService.SetToolTip(ControlGrip, ResourcesUtil.GetResourceValue(ResourcesUtil.TASKBAR_TOOLTIP_FLOAT));
        }
        #endregion

        #region Create and Save StoryBoards and Handle Storyboard Events
        /// <summary>
        /// Create Dock and Undock Animation Storyboards
        /// </summary>
        protected virtual void CreateStoryboard()
        {
            if (this.Projection == null || !(this.Projection is PlaneProjection))
            {
                PlaneProjection projection = new PlaneProjection();
                this.Projection = projection;
            }

            StoryboardName = "TB_" + Guid.NewGuid().ToString();

            Storyboard sb = new Storyboard();
            sb.SetValue(FrameworkElement.NameProperty, StoryboardName);
            sb.Completed += new EventHandler(Storyboard_Completed);

            DoubleAnimation dbAnimT = new DoubleAnimation() { To = InitialTop, Duration = TimeSpan.FromSeconds(0.5), AutoReverse = false };
            Storyboard.SetTarget(dbAnimT, this);
            Storyboard.SetTargetProperty(dbAnimT, new PropertyPath("(Canvas.Top)"));
            sb.Children.Add(dbAnimT);

            DoubleAnimation dbAnimL = new DoubleAnimation() { To = InitialLeft, Duration = TimeSpan.FromSeconds(0.5), AutoReverse = false };
            Storyboard.SetTarget(dbAnimL, this);
            Storyboard.SetTargetProperty(dbAnimL, new PropertyPath("(Canvas.Left)"));
            sb.Children.Add(dbAnimL);

            DoubleAnimation dbAnimO = new DoubleAnimation() { To = 300, Duration = TimeSpan.FromSeconds(0.5), AutoReverse = true };
            Storyboard.SetTarget(dbAnimO, this.Projection);
            Storyboard.SetTargetProperty(dbAnimO, new PropertyPath("LocalOffsetZ"));
            sb.Children.Add(dbAnimO);

            this.Resources.Add(StoryboardName, sb);
        }

        private Storyboard UpdateStoryboard()
        {
            if (StoryboardName == null || !this.Resources.Contains(StoryboardName))
            {
                this.CreateStoryboard();
            }

            Storyboard sb = this.Resources[StoryboardName] as Storyboard;
            DoubleAnimation dbAnimT = sb.Children[0] as DoubleAnimation;
            dbAnimT.To = (this.DockPosition == DockPosition.NONE) ? InitialTop : ((this.DockPosition == DockPosition.TOP) ? 0 : CanvasHeight - DockHeight);

            DoubleAnimation dbAnimL = sb.Children[1] as DoubleAnimation;
            dbAnimL.To = (this.DockPosition == DockPosition.NONE) ? InitialLeft : 0;

            DoubleAnimation dbAnimO = sb.Children[2] as DoubleAnimation;
            dbAnimO.To = (this.DockPosition == DockPosition.NONE) ? 300 : -300;

            return sb;
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            if (this.DockPosition == DockPosition.NONE)
            {
                UndockMe();
            }
            else
            {
                DockMe();
            }
        }
        #endregion

        #region Validate PositionChange Event Trigger
        private void RaiseStateChangeEvent(bool force)
        {
            if (force)
            {
                OnDockStateChange(new TaskbarStateChangeEventArgs(DockPosition, IsInUpperPage));
            }
            else
            {
                if (this.currentTop > CanvasHeight / 2.0)
                {
                    if (IsInUpperPage)
                    {
                        IsInUpperPage = false;
                        OnDockStateChange(new TaskbarStateChangeEventArgs(DockPosition.NONE, false));
                    }
                }
                else if (!IsInUpperPage)
                {
                    IsInUpperPage = true;
                    OnDockStateChange(new TaskbarStateChangeEventArgs(DockPosition.NONE, true));
                }
            }
        }

        protected virtual void OnDockStateChange(TaskbarStateChangeEventArgs e)
        {
            if (DockStateChangeHandler != null) DockStateChangeHandler(this, e);
        }
        #endregion
    }
}
