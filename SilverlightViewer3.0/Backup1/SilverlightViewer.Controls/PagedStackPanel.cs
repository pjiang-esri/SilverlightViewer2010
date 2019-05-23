using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace ESRI.SilverlightViewer.Controls
{
    public class PagedStackPanel : Panel
    {
        private const int NAVIGATOR_SIZE = 15;

        private int ViewFrom = 0;
        private int ViewLast = 0;
        private int PageSize = 0;
        private bool IsAllShow = true;
        private bool IsLastShow = false;
        private Size UsableSize = Size.Empty;

        // Page Navigation Buttons
        protected ToggleButton PageNextButton = null;
        protected ToggleButton PageBackButton = null;

        public PagedStackPanel() : base()
        {
            this.Loaded += new RoutedEventHandler(ScrollStackPanel_Loaded);

            PageNextButton = new ToggleButton() { Width = NAVIGATOR_SIZE, Height = NAVIGATOR_SIZE, Foreground = this.Foreground, Background = this.Background };
            PageBackButton = new ToggleButton() { Width = NAVIGATOR_SIZE, Height = NAVIGATOR_SIZE, Foreground = this.Foreground, Background = this.Background };

            PageNextButton.Click += new RoutedEventHandler(PageNextButton_Click);
            PageBackButton.Click += new RoutedEventHandler(PageBackButton_Click);

            this.Children.Add(PageBackButton);
            this.Children.Add(PageNextButton);
        }

        #region Dependency Property
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(PagedStackPanel), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x99, 0xAA, 0x00, 0x00))));
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(PagedStackPanel), new PropertyMetadata(Orientation.Horizontal));

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        #endregion

        #region Add MouseMove Handler at Loaded
        void ScrollStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageNextButton != null)
            {
                ToolTipService.SetToolTip(PageNextButton, ResourcesUtil.GetResourceValue(ResourcesUtil.WIDGETBAR_NAVIGATE_NEXT));
                PageNextButton.State = (this.Orientation == Orientation.Horizontal) ? ToggleButtonState.STATE_ROTATE_90 : ToggleButtonState.STATE_ORIGIN;
            }

            if (PageBackButton != null)
            {
                ToolTipService.SetToolTip(PageBackButton, ResourcesUtil.GetResourceValue(ResourcesUtil.WIDGETBAR_NAVIGATE_BACK));
                PageBackButton.State = (this.Orientation == Orientation.Horizontal) ? ToggleButtonState.STATE_ROTATE90 : ToggleButtonState.STATE_ROTATE180;
            }
        }
        #endregion

        #region Override MeasureOverride and ArrangeOverride
        protected override Size MeasureOverride(Size availableSize)
        {
            if (UsableSize.IsEmpty) UsableSize = availableSize; // Save Available Size
            if (Children.Count < 3) return new Size(0, 0);

            int k = 0;
            int count = Children.Count - 2;
            Size needSize = new Size(0, 0);
            Size viewSize = new Size(0, 0);
            bool isLast = false;
            bool isFull = false;

            foreach (FrameworkElement child in Children)
            {
                if (!(child is ToggleButton))
                {
                    if (k >= ViewFrom && !isFull)
                    {
                        child.Measure(new Size(availableSize.Width, availableSize.Height));

                        if (this.Orientation == Orientation.Horizontal)
                        {
                            needSize.Height = Math.Max(needSize.Height, child.DesiredSize.Height);
                            isLast = (needSize.Width + child.DesiredSize.Width <= availableSize.Width) && (k == count - 1);
                            isFull = (!isLast && (needSize.Width + child.DesiredSize.Width + NAVIGATOR_SIZE * 2 > availableSize.Width));

                            if (!isFull)
                            {
                                needSize.Width += child.DesiredSize.Width;
                                ViewLast = k + 1;
                            }
                        }
                        else
                        {
                            needSize.Width = Math.Max(needSize.Width, child.DesiredSize.Width);
                            isLast = (needSize.Height + child.DesiredSize.Height <= availableSize.Height) && (k == count - 1);
                            isFull = (!isLast && (needSize.Height + child.DesiredSize.Height + NAVIGATOR_SIZE * 2 > availableSize.Height));

                            if (!isFull)
                            {
                                needSize.Height += child.DesiredSize.Height;
                                ViewLast = k + 1;
                            }
                        }
                    }

                    k++;
                }
            }

            if (this.Orientation == Orientation.Horizontal)
            {
                viewSize.Width = needSize.Width + NAVIGATOR_SIZE * 2;
                viewSize.Height = Math.Min(needSize.Height, availableSize.Height);
            }
            else
            {
                viewSize.Width = Math.Min(needSize.Width, availableSize.Width);
                viewSize.Height = needSize.Height + NAVIGATOR_SIZE * 2;
            }

            IsAllShow = (ViewLast - ViewFrom == count);
            IsLastShow = (ViewLast == count);
            if (!IsLastShow) PageSize = ViewLast - ViewFrom;

            return viewSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count < 3) return finalSize;

            int k = 0;
            double offset = 0;

            if (Orientation == Orientation.Horizontal)
            {
                if (!IsAllShow && ViewFrom > 0)
                {
                    PageBackButton.Arrange(new Rect(0, (finalSize.Height - NAVIGATOR_SIZE) / 2.0, NAVIGATOR_SIZE, NAVIGATOR_SIZE));
                    offset += NAVIGATOR_SIZE;
                }
                else
                {
                    PageBackButton.Arrange(new Rect(0, 0, 0, 0));
                }

                foreach (FrameworkElement child in this.Children)
                {
                    if (!(child is ToggleButton))
                    {
                        if (k >= ViewFrom && k < ViewLast)
                        {
                            child.Arrange(new Rect(offset, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                            offset += child.DesiredSize.Width;
                        }
                        else
                        {
                            child.Arrange(new Rect(0, 0, 0, 0));
                        }

                        k++;
                    }
                }

                if (!IsAllShow && !IsLastShow)
                {
                    PageNextButton.Arrange(new Rect(offset, (finalSize.Height - NAVIGATOR_SIZE) / 2.0, NAVIGATOR_SIZE, NAVIGATOR_SIZE));
                }
                else
                {
                    PageNextButton.Arrange(new Rect(0, 0, 0, 0));
                }
            }
            else
            {
                if (!IsAllShow && ViewFrom > 0)
                {
                    PageBackButton.Arrange(new Rect((finalSize.Width - NAVIGATOR_SIZE) / 2.0, 0, NAVIGATOR_SIZE, NAVIGATOR_SIZE));
                    offset += NAVIGATOR_SIZE;
                }
                else
                {
                    PageBackButton.Arrange(new Rect(0, 0, 0, 0));
                }

                foreach (FrameworkElement child in this.Children)
                {
                    if (!(child is ToggleButton))
                    {
                        if (k >= ViewFrom && k < ViewLast)
                        {
                            child.Arrange(new Rect(0, offset, child.DesiredSize.Width, child.DesiredSize.Height));
                            offset += child.DesiredSize.Height;
                        }
                        else
                        {
                            child.Arrange(new Rect(0, 0, 0, 0));
                        }

                        k++;
                    }
                }

                if (!IsAllShow && !IsLastShow)
                {
                    PageNextButton.Arrange(new Rect((finalSize.Width - NAVIGATOR_SIZE) / 2.0, offset, NAVIGATOR_SIZE, NAVIGATOR_SIZE));
                }
                else
                {
                    PageNextButton.Arrange(new Rect(0, 0, 0, 0));
                }
            }

            return finalSize;
        }
        #endregion

        #region Handle Forward/Backward Button Event - Navigate Buttons
        private void PageBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewFrom == 0) return;

            ViewFrom = ((ViewFrom - PageSize) < 0) ? 0 : (ViewFrom - PageSize);
            ArrangeOverride(MeasureOverride(UsableSize));
        }

        private void PageNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsLastShow) return;

            ViewFrom = ViewLast;
            ArrangeOverride(MeasureOverride(UsableSize));
        }
        #endregion
    }
}
