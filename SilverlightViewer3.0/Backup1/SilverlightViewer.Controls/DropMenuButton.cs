using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplatePart(Name = DropMenuButton._BASE_MENU_ITEM_PANEL, Type = typeof(Panel))]
    [TemplatePart(Name = DropMenuButton._CHILD_MENU_ITEM_PANEL, Type = typeof(Panel))]
    [TemplateVisualState(GroupName = DropMenuButton._MENU_BUTTON_STATES, Name = DropMenuButton._STATE_EXPAND)]
    [TemplateVisualState(GroupName = DropMenuButton._MENU_BUTTON_STATES, Name = DropMenuButton._STATE_COLLAPSE)]
    [TemplateVisualState(GroupName = DropMenuButton._MENU_BUTTON_STATES, Name = DropMenuButton._STATE_DELAYHIDE)]
    public class DropMenuButton : Control
    {
        protected const string _STATE_EXPAND = "StateExpand";
        protected const string _STATE_COLLAPSE = "StateCollapse";
        protected const string _STATE_DELAYHIDE = "StateDelayHide";
        protected const string _MENU_BUTTON_STATES = "MenuButtonStates";

        protected const string _BASE_MENU_ITEM_PANEL = "BaseMenuItemPanel";
        protected const string _CHILD_MENU_ITEM_PANEL = "ChildMenuItemPanel";
        protected const string _MENU_ITEM_STACK_PANEL = "MenuItemStackPanel";
        protected const string _MENU_STACK_CLIP_PANEL = "MenuStackClipPanel";
        protected const string _MENU_STACK_SCROLL_BAR = "MenuStackScrollBar";
        protected const string _MENU_BOX_LEADING_PATH = "MenuBoxLeadingPath";
        protected const string _MENU_ITEM_STACK_BORDER = "MenuItemStackBorder";

        protected Path MenuBoxLeadingPath = null;
        protected Panel BaseMenuItemPanel = null;
        protected Panel ChildMenuItemPanel = null;
        protected Panel MenuStackClipPanel = null;
        protected Border MenuItemStackBorder = null;
        protected ScrollBar MenuStackScrollBar = null;
        protected StackPanel MenuItemStackPanel = null;

        private MenuItemButton BaseButton = null;
        private string CurrentState = _STATE_COLLAPSE;

        #region Define MenuItemClick Event Handler and Event
        private MenuItemClickEventHandler MenuItemClickHandler = null;
        public event MenuItemClickEventHandler MenuItemClick
        {
            add
            {
                if (MenuItemClickHandler == null || !MenuItemClickHandler.GetInvocationList().Contains(value))
                {
                    MenuItemClickHandler += value;
                }
            }
            remove
            {
                MenuItemClickHandler -= value;
            }
        }
        #endregion

        public DropMenuButton()
        {
            this.DefaultStyleKey = typeof(DropMenuButton);
            MenuItems = new ObservableCollection<ContextMenuItem>();
        }

        #region Dependency Property
        // Using a DependencyProperty as the backing store for Text
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DropMenuButton), new PropertyMetadata("", new PropertyChangedCallback(OnTextChange)));

        // Using a DependencyProperty as the backing store for MenuItems
        public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register("MenuItems", typeof(ObservableCollection<ContextMenuItem>), typeof(DropMenuButton), null);

        // Using a DependencyProperty as the backing store for IsImageOnly
        public static readonly DependencyProperty IsImageOnlyProperty = DependencyProperty.Register("IsImageOnly", typeof(bool), typeof(DropMenuButton), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for ImageSource
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(DropMenuButton), new PropertyMetadata(null, new PropertyChangedCallback(OnImageSourceChange)));

        // Using a DependencyProperty as the backing store for ImageMargin
        public static readonly DependencyProperty ImageMarginProperty = DependencyProperty.Register("ImageMargin", typeof(Double), typeof(DropMenuButton), new PropertyMetadata(5.0));

        // Using a DependencyProperty as the backing store for ButtonShape
        public static readonly DependencyProperty ButtonShapeProperty = DependencyProperty.Register("ButtonShape", typeof(MenuButtonShape), typeof(DropMenuButton), new PropertyMetadata(MenuButtonShape.Circle));

        // Using a DependencyProperty as the backing store for MenuOpenMode
        public static readonly DependencyProperty MenuOpenModeProperty = DependencyProperty.Register("MenuOpenMode", typeof(MenuOpenAction), typeof(DropMenuButton), new PropertyMetadata(MenuOpenAction.MouseClick));

        // Using a DependencyProperty as the backing store for TextVisibility
        public static readonly DependencyProperty TextVisibilityProperty = DependencyProperty.Register("TextVisibility", typeof(Visibility), typeof(DropMenuButton), new PropertyMetadata(Visibility.Visible));

        // Using a DependencyProperty as the backing store for ContentPosition
        public static readonly DependencyProperty ContentPositionProperty = DependencyProperty.Register("ContentPosition", typeof(ContentOrientation), typeof(DropMenuButton), new PropertyMetadata(ContentOrientation.DOWN, new PropertyChangedCallback(OnContentPositionChange)));

        // Using a DependencyProperty as the backing store for MenuBoxMaxHeight
        public static readonly DependencyProperty MenuBoxMaxHeightProperty = DependencyProperty.Register("MenuBoxMaxHeight", typeof(double), typeof(DropMenuButton), new PropertyMetadata(320.0, new PropertyChangedCallback(OnMenuBoxMaxHeightChange)));

        // Using a DependencyProperty as the backing store for MenuBoxCornerRadius
        public static readonly DependencyProperty MenuBoxCornerRadiusProperty = DependencyProperty.Register("MenuBoxCornerRadius", typeof(CornerRadius), typeof(DropMenuButton), null);

          // Using a DependencyProperty as the backing store for DefaultEffect
        public static readonly DependencyProperty DefaultEffectProperty = DependencyProperty.Register("DefaultEffect", typeof(Effect), typeof(DropMenuButton), null);

        // Using a DependencyProperty as the backing store for MouseOverEffect
        public static readonly DependencyProperty MouseOverEffectProperty = DependencyProperty.Register("MouseOverEffect", typeof(Effect), typeof(DropMenuButton), null);

        // Using a DependencyProperty as the backing store for MouseOver Background Color
        public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(DropMenuButton), null);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ObservableCollection<ContextMenuItem> MenuItems
        {
            get { return (ObservableCollection<ContextMenuItem>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }

        public bool IsImageOnly
        {
            get { return (bool)GetValue(IsImageOnlyProperty); }
            set { SetValue(IsImageOnlyProperty, value); }
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public Double ImageMargin
        {
            get { return (Double)GetValue(ImageMarginProperty); }
            set { SetValue(ImageMarginProperty, value); }
        }

        [TypeConverter(typeof(MenuButtonShape))]
        public MenuButtonShape ButtonShape
        {
            get { return (MenuButtonShape)GetValue(ButtonShapeProperty); }
            set { SetValue(ButtonShapeProperty, value); }
        }

        [TypeConverter(typeof(MenuOpenActionConverter))]
        public MenuOpenAction MenuOpenMode
        {
            get { return (MenuOpenAction)GetValue(MenuOpenModeProperty); }
            set { SetValue(MenuOpenModeProperty, value); }
        }

        public Visibility TextVisibility
        {
            get { return (Visibility)GetValue(TextVisibilityProperty); }
            set { SetValue(TextVisibilityProperty, value); }
        }

        [TypeConverter(typeof(ContentOrientationConverter))]
        public ContentOrientation ContentPosition
        {
            get { return (ContentOrientation)GetValue(ContentPositionProperty); }
            set { SetValue(ContentPositionProperty, value); }
        }

        public double MenuBoxMaxHeight
        {
            get { return (double)GetValue(MenuBoxMaxHeightProperty); }
            set { SetValue(MenuBoxMaxHeightProperty, value); }
        }

        public CornerRadius MenuBoxCornerRadius
        {
            get { return (CornerRadius)GetValue(MenuBoxCornerRadiusProperty); }
            set { SetValue(MenuBoxCornerRadiusProperty, value); }
        }

        public Effect DefaultEffect
        {
            get { return (Effect)GetValue(DefaultEffectProperty); }
            set { SetValue(DefaultEffectProperty, value); }
        }

        public Effect MouseOverEffect
        {
            get { return (Effect)GetValue(MouseOverEffectProperty); }
            set { SetValue(MouseOverEffectProperty, value); }
        }

        public Brush MouseOverBackground
        {
            get { return (Brush)GetValue(MouseOverBackgroundProperty); }
            set { SetValue(MouseOverBackgroundProperty, value); }
        }

        protected static void OnTextChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropMenuButton me = d as DropMenuButton;
            if (me != null && me.BaseButton != null) me.BaseButton.Text = (string)e.NewValue;
        }

        protected static void OnImageSourceChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropMenuButton me = d as DropMenuButton;
            if (me != null && me.BaseButton != null) me.BaseButton.ImageSource = (ImageSource)e.NewValue;
        }

        protected static void OnContentPositionChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropMenuButton me = d as DropMenuButton;
            if (me != null) me.InitializeChildPanel(false);
        }

        protected static void OnMenuBoxMaxHeightChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropMenuButton me = d as DropMenuButton;
            if (me != null) me.InitializeChildPanel(true);
        }

        protected void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                AddChildMenuButtons();
                InitializeChildPanel(true);
            }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.Effect != null)
            {
                this.DefaultEffect = this.Effect;
                this.Effect = null;
            }

            BaseMenuItemPanel = this.GetTemplateChild(_BASE_MENU_ITEM_PANEL) as Panel;
            if (BaseMenuItemPanel == null)
            {
                throw new Exception("Apply Template Error: Failed to get BaseMenuButtonPanel");
            }

            ChildMenuItemPanel = this.GetTemplateChild(_CHILD_MENU_ITEM_PANEL) as Panel;
            if (ChildMenuItemPanel == null)
            {
                throw new Exception("Apply Template Error: Failed to get ChildMenuButtonPanel");
            }

            MenuStackClipPanel = this.GetTemplateChild(_MENU_STACK_CLIP_PANEL) as Panel;
            if (MenuStackClipPanel == null)
            {
                throw new Exception("Apply Template Error: Failed to get MenuStackClipPanel");
            }

            MenuItemStackPanel = this.GetTemplateChild(_MENU_ITEM_STACK_PANEL) as StackPanel;
            if (MenuItemStackPanel == null)
            {
                throw new Exception("Apply Template Error: Failed to get ChildMenuItemStack");
            }

            MenuStackScrollBar = this.GetTemplateChild(_MENU_STACK_SCROLL_BAR) as ScrollBar;
            if (MenuStackScrollBar != null)
            {
                MenuStackScrollBar.Scroll += new ScrollEventHandler(MenuStackScrollBar_Scroll);
            }

            MenuBoxLeadingPath = this.GetTemplateChild(_MENU_BOX_LEADING_PATH) as Path;
            if (MenuBoxLeadingPath == null)
            {
                throw new Exception("Apply Template Error: Failed to get MenuBoxLeadingPath");
            }

            MenuItemStackBorder = this.GetTemplateChild(_MENU_ITEM_STACK_BORDER) as Border;
            if (MenuItemStackBorder == null)
            {
                throw new Exception("Apply Template Error: Failed to get ChildMenuItemBorder");
            }

            if (this.MenuItems != null)
                this.MenuItems.CollectionChanged += new NotifyCollectionChangedEventHandler(Items_CollectionChanged);

            InitializeControl();
        }

        #region Initialize Control Functions
        protected void InitializeControl()
        {
            if (BaseMenuItemPanel == null) return;
            if (ChildMenuItemPanel == null) return;

            InitializeBaseButton();
            AddChildMenuButtons();
            InitializeChildPanel(true);
        }

        private void InitializeBaseButton()
        {
            if (this.ImageSource != null)
            {
                if (this.ButtonShape == MenuButtonShape.Square)
                {
                    BaseButton = new SquareMenuButton();
                }
                else
                {
                    BaseButton = new CircleMenuButton();
                }

                BaseButton.Text = this.Text;
                BaseButton.Width = this.Width;
                BaseButton.Height = this.Height;
                BaseButton.IsImageOnly = this.IsImageOnly;
                BaseButton.ImageSource = this.ImageSource;
                BaseButton.ImageMargin = this.ImageMargin;
                BaseButton.TextPosition = this.ContentPosition;
                BaseButton.TextVisibility = this.TextVisibility;
                BaseButton.DefaultEffect = this.DefaultEffect;
                BaseButton.MouseOverEffect = this.MouseOverEffect;
                BaseButton.MouseOverBackground = this.MouseOverBackground;

                if (double.IsNaN(BaseButton.Width)) BaseButton.Width = 50;
                if (double.IsNaN(BaseButton.Height)) BaseButton.Height = 50;
                BaseButton.Click += new MenuItemClickEventHandler(BaseItem_Clicked);
                BaseButton.LostFocus += new RoutedEventHandler(BaseItem_LostFocus);
                BaseMenuItemPanel.Children.Add(BaseButton);
            }
        }

        private void InitializeChildPanel(bool isChanged)
        {
            if (BaseButton != null && ChildMenuItemPanel != null) // Template has been applied
            {
                double vm = 0.0; // vertical margin
                double hm = 0.0; // horizontal margin

                BaseButton.TextPosition = this.ContentPosition;

                Size boxSize = (isChanged || (MenuItemStackBorder.ActualWidth == 0.0) || (MenuItemStackBorder.ActualHeight == 0.0)) ?
                    MeasureMenuBoxSize() : (new Size(MenuItemStackBorder.ActualWidth, MenuItemStackBorder.ActualHeight));

                switch (this.ContentPosition)
                {
                    case ContentOrientation.DOWN:
                        vm = boxSize.Height + 5; // 5 - the length of the leading path
                        hm = boxSize.Width - BaseButton.Width;

                        switch (this.HorizontalContentAlignment)
                        {
                            case System.Windows.HorizontalAlignment.Center:
                            case System.Windows.HorizontalAlignment.Stretch:
                                ChildMenuItemPanel.Margin = new Thickness(-hm / 2.0, BaseButton.Height, -hm / 2.0, -vm);
                                MenuBoxLeadingPath.Margin = new Thickness(hm / 2.0 + BaseButton.Width / 2.0, 0, 0, 0);
                                break;
                            case System.Windows.HorizontalAlignment.Right:
                                ChildMenuItemPanel.Margin = new Thickness(0, BaseButton.Height, -hm, -vm);
                                MenuBoxLeadingPath.Margin = new Thickness(BaseButton.Width / 2.0, 0, 0, 0);
                                break;
                            case System.Windows.HorizontalAlignment.Left:
                                ChildMenuItemPanel.Margin = new Thickness(-hm, BaseButton.Height, 0, -vm);
                                MenuBoxLeadingPath.Margin = new Thickness(hm + BaseButton.Width / 2.0, 0, 0, 0);
                                break;
                        }
                        
                        MenuItemStackBorder.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                        MenuItemStackBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        (MenuBoxLeadingPath.RenderTransform as RotateTransform).Angle = 0;
                        break;

                    case ContentOrientation.LEFT:
                        hm = boxSize.Width + 5;
                        vm = boxSize.Height - BaseButton.Height;

                        switch (this.VerticalContentAlignment)
                        {
                            case System.Windows.VerticalAlignment.Center:
                            case System.Windows.VerticalAlignment.Stretch:
                                ChildMenuItemPanel.Margin = new Thickness(-hm, -vm / 2.0, BaseButton.Width, -vm / 2.0);
                                MenuBoxLeadingPath.Margin = new Thickness(boxSize.Width, vm / 2.0 + BaseButton.Height / 2.0, 0, 0);
                                break;
                            case System.Windows.VerticalAlignment.Top:
                                ChildMenuItemPanel.Margin = new Thickness(-hm, -vm, BaseButton.Width, 0);
                                MenuBoxLeadingPath.Margin = new Thickness(boxSize.Width, vm + BaseButton.Height / 2.0, 0, 0);
                                break;
                            case System.Windows.VerticalAlignment.Bottom:
                                ChildMenuItemPanel.Margin = new Thickness(-hm, 0, BaseButton.Width, -vm);
                                MenuBoxLeadingPath.Margin = new Thickness(boxSize.Width, BaseButton.Height / 2.0, 0, 0);
                                break;
                        }

                        MenuItemStackBorder.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MenuItemStackBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        (MenuBoxLeadingPath.RenderTransform as RotateTransform).Angle = -90;
                        break;

                    case ContentOrientation.RIGHT:
                        hm = boxSize.Width + 5;
                        vm = boxSize.Height - BaseButton.Height;

                        switch (this.VerticalContentAlignment)
                        {
                            case System.Windows.VerticalAlignment.Center:
                            case System.Windows.VerticalAlignment.Stretch:
                                ChildMenuItemPanel.Margin = new Thickness(BaseButton.Width, -vm / 2.0, -hm, -vm / 2.0);
                                MenuBoxLeadingPath.Margin = new Thickness(0, vm / 2.0 + BaseButton.Height / 2.0, 0, 0);
                                break;
                            case System.Windows.VerticalAlignment.Top:
                                ChildMenuItemPanel.Margin = new Thickness(BaseButton.Width, -vm, -hm, 0);
                                MenuBoxLeadingPath.Margin = new Thickness(0, vm + BaseButton.Height / 2.0, 0, 0);
                                break;
                            case System.Windows.VerticalAlignment.Bottom:
                                ChildMenuItemPanel.Margin = new Thickness(BaseButton.Width, 0, -hm, -vm);
                                MenuBoxLeadingPath.Margin = new Thickness(0, BaseButton.Height / 2.0, 0, 0);
                                break;
                        }

                        MenuItemStackBorder.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MenuItemStackBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        (MenuBoxLeadingPath.RenderTransform as RotateTransform).Angle = -90;
                        break;

                    case ContentOrientation.UP:
                        vm = boxSize.Height + 5;
                        hm = boxSize.Width - BaseButton.Width;

                        switch (this.HorizontalContentAlignment)
                        {
                            case System.Windows.HorizontalAlignment.Center:
                            case System.Windows.HorizontalAlignment.Stretch:
                                ChildMenuItemPanel.Margin = new Thickness(-hm / 2.0, -vm, -hm / 2.0, BaseButton.Height);
                                MenuBoxLeadingPath.Margin = new Thickness(hm / 2.0 + BaseButton.Width / 2.0, boxSize.Height, 0, 0);
                                break;
                            case System.Windows.HorizontalAlignment.Right:
                                ChildMenuItemPanel.Margin = new Thickness(0, -vm, -hm, BaseButton.Height);
                                MenuBoxLeadingPath.Margin = new Thickness(BaseButton.Width / 2.0, boxSize.Height, 0, 0);
                                break;
                            case System.Windows.HorizontalAlignment.Left:
                                ChildMenuItemPanel.Margin = new Thickness(-hm, -vm, 0, BaseButton.Height);
                                MenuBoxLeadingPath.Margin = new Thickness(hm + BaseButton.Width / 2.0, boxSize.Height, 0, 0);
                                break;
                        }

                        MenuItemStackBorder.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MenuItemStackBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        (MenuBoxLeadingPath.RenderTransform as RotateTransform).Angle = 0;
                        break;
                }
            }
        }

        private void AddChildMenuButtons()
        {
            if (MenuItems == null) return;
            if (MenuItems.Count == 0) return;

            MenuItemStackPanel.Children.Clear();
            foreach (ContextMenuItem item in MenuItems)
            {
                item.Click += new MenuItemClickEventHandler(MenuItem_Clicked);
                MenuItemStackPanel.Children.Add(item);
            }
        }

        private Size MeasureMenuBoxSize()
        {
            MenuItemStackPanel.Measure(new Size(400, 100 * MenuItems.Count));

            double stackW = MenuItemStackPanel.DesiredSize.Width;
            double stackH = MenuItemStackPanel.DesiredSize.Height;
            if (MenuItemStackPanel.Margin.Right > 0) { stackW -= MenuItemStackPanel.Margin.Right; }

            double boxW = stackW + this.BorderThickness.Left * 2 + 4; // MenuStackClipPanel's Margin = 2 * 2
            double boxH = stackH + this.BorderThickness.Top * 2 + 4;

            if (!double.IsNaN(MenuBoxMaxHeight) && MenuBoxMaxHeight > 0.0 && boxH > MenuBoxMaxHeight)
            {
                // ScrollBar's Width = 18
                MenuStackClipPanel.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, stackW + 18, stackH) };
                MenuItemStackPanel.Margin = new Thickness(0, 0, 18, 0);
                MenuStackScrollBar.Maximum = boxH - MenuBoxMaxHeight;
                MenuStackScrollBar.SmallChange = stackH * 0.75 / MenuItems.Count;
                MenuStackScrollBar.Visibility = System.Windows.Visibility.Visible;
                MenuStackScrollBar.ViewportSize = MenuBoxMaxHeight;
                return new Size(boxW + 18, MenuBoxMaxHeight);
            }
            else
            {
                MenuStackClipPanel.Clip = null;
                MenuStackScrollBar.Visibility = System.Windows.Visibility.Collapsed;
                return new Size(boxW, boxH);
            }
        }
        #endregion

        #region Handle Base Item and Menu Item Events
        void BaseItem_LostFocus(object sender, RoutedEventArgs e)
        {
            bool isChild = IsRelated(FocusManager.GetFocusedElement(), ChildMenuItemPanel);

            //If a child menu item does not get focus, hide child items
            if (!isChild && CurrentState == _STATE_EXPAND)
            {
                CurrentState = _STATE_DELAYHIDE;
                VisualStateManager.GoToState(this, _STATE_DELAYHIDE, true);
            }
        }

        void BaseItem_MouseEnter(object sender, MouseEventArgs e)
        {
            MenuItemButton item = sender as MenuItemButton;
            if (CurrentState != _STATE_EXPAND)
            {
                item.Focus();
                CurrentState = _STATE_EXPAND;
                VisualStateManager.GoToState(this, _STATE_EXPAND, true);
            }
        }

        void BaseItem_Clicked(object sender, MenuItemClickEventArgs e)
        {
            MenuItemButton item = sender as MenuItemButton;
            if (CurrentState != _STATE_EXPAND)
            {
                CurrentState = _STATE_EXPAND;
                VisualStateManager.GoToState(this, _STATE_EXPAND, true);
            }

            if (this.MenuOpenMode == MenuOpenAction.MouseHover)
            {
                this.BaseButton.MouseEnter += new MouseEventHandler(BaseItem_MouseEnter);
                this.BaseButton.Click -= BaseItem_Clicked;
            }
        }

        void MenuItem_Clicked(object sender, MenuItemClickEventArgs e)
        {
            ContextMenuItem item = sender as ContextMenuItem;

            if (item != null)
            {
                OnMenuItemClick(item, e);
                CurrentState = _STATE_DELAYHIDE;
                VisualStateManager.GoToState(this, _STATE_DELAYHIDE, false);
            }
        }

        void MenuStackScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            MenuItemStackPanel.Margin = new Thickness(0, -e.NewValue, 18, 0);
        }

        protected virtual void OnMenuItemClick(ContextMenuItem sender, MenuItemClickEventArgs e)
        {
            if (MenuItemClickHandler != null) MenuItemClickHandler(sender, e);
        }
        #endregion

        #region Check the focused item is related to a specific container
        private bool IsRelated(object child, object parent)
        {
            return (child == parent) || ((child is FrameworkElement) && IsRelated(((FrameworkElement)child).Parent, parent));
        }
        #endregion
    }
}
