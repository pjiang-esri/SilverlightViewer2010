using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplatePart(Name = SwitchMenuButton._BASE_MENU_ITEM_PANEL, Type = typeof(Panel))]
    [TemplatePart(Name = SwitchMenuButton._CHILD_MENU_ITEM_PANEL, Type = typeof(Panel))]
    [TemplateVisualState(GroupName = SwitchMenuButton._MENU_BUTTON_STATES, Name = SwitchMenuButton._STATE_EXPAND)]
    [TemplateVisualState(GroupName = SwitchMenuButton._MENU_BUTTON_STATES, Name = SwitchMenuButton._STATE_COLLAPSE)]
    [TemplateVisualState(GroupName = SwitchMenuButton._MENU_BUTTON_STATES, Name = SwitchMenuButton._STATE_DELAYHIDE)]
    public class SwitchMenuButton : Control
    {
        protected const string _STATE_EXPAND = "StateExpand";
        protected const string _STATE_COLLAPSE = "StateCollapse";
        protected const string _STATE_DELAYHIDE = "StateDelayHide";
        protected const string _MENU_BUTTON_STATES = "MenuButtonStates";

        protected const string _BASE_MENU_ITEM_PANEL = "BaseMenuItemPanel";
        protected const string _CHILD_MENU_ITEM_PANEL = "ChildMenuItemPanel";

        protected Panel BaseMenuItemPanel = null;
        protected Panel ChildMenuItemPanel = null;

        private const double BUTTON_SPACE = 8;
        private double ChildPanelWidth = 0.0;
        private double ChildPanelHeight = 0.0;
        private MenuItemButton ClickedItem = null;

        private string CurrentState = _STATE_COLLAPSE;
        private bool isHoverModeSet = false;

        #region Define BaseButtonClickEvent Handler and Event
        private MenuItemClickEventHandler BaseButtonClickHandler = null;
        public event MenuItemClickEventHandler BaseButtonClick
        {
            add
            {
                if (BaseButtonClickHandler == null || !BaseButtonClickHandler.GetInvocationList().Contains(value))
                    BaseButtonClickHandler += value;
            }

            remove
            {
                BaseButtonClickHandler -= value;
            }
        }
        #endregion

        #region Define SelectedButtonChangeEvent Handler and Event
        private MenuButtonChangeEventHandler MenuButtonChangeHandler = null;
        public event MenuButtonChangeEventHandler MenuButtonChange
        {
            add
            {
                if (MenuButtonChangeHandler == null || !MenuButtonChangeHandler.GetInvocationList().Contains(value))
                {
                    MenuButtonChangeHandler += value;
                }
            }
            remove
            {
                MenuButtonChangeHandler -= value;
            }
        }
        #endregion

        public SwitchMenuButton()
        {
            this.DefaultStyleKey = typeof(SwitchMenuButton);
            Items = new ObservableCollection<MenuItemButton>();
        }

        #region Dependency Property
        // Using a DependencyProperty as the backing store for Items
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(ObservableCollection<MenuItemButton>), typeof(SwitchMenuButton), null);

        // Using a DependencyProperty as the backing store for MenuOpenMode
        public static readonly DependencyProperty MenuOpenModeProperty = DependencyProperty.Register("MenuOpenMode", typeof(MenuOpenAction), typeof(SwitchMenuButton), new PropertyMetadata(MenuOpenAction.MouseClick));

        // Using a DependencyProperty as the backing store for SelectedButton
        public static readonly DependencyProperty SelectedButtonProperty = DependencyProperty.Register("SelectedButton", typeof(MenuItemButton), typeof(SwitchMenuButton), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for ContentPosition
        public static readonly DependencyProperty ContentPositionProperty = DependencyProperty.Register("ContentPosition", typeof(ContentOrientation), typeof(SwitchMenuButton), new PropertyMetadata(ContentOrientation.DOWN, new PropertyChangedCallback(OnContentPositionChange)));

        // Using a DependencyProperty as the backing store for DefaultEffect
        public static readonly DependencyProperty DefaultEffectProperty = DependencyProperty.Register("DefaultEffect", typeof(Effect), typeof(SwitchMenuButton), null);

        // Using a DependencyProperty as the backing store for SelectedEffect
        public static readonly DependencyProperty SelectedEffectProperty = DependencyProperty.Register("SelectedEffect", typeof(Effect), typeof(SwitchMenuButton), null);

        // Using a DependencyProperty as the backing store for MouseOverEffect
        public static readonly DependencyProperty MouseOverEffectProperty = DependencyProperty.Register("MouseOverEffect", typeof(Effect), typeof(SwitchMenuButton), null);

        // Using a DependencyProperty as the backing store for MouseOver Background Color
        public static readonly DependencyProperty SelectedBackgroundProperty = DependencyProperty.Register("SelectedBackground", typeof(Brush), typeof(SwitchMenuButton), null);

        // Using a DependencyProperty as the backing store for MouseOver Background Color
        public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(SwitchMenuButton), null);

        public ObservableCollection<MenuItemButton> Items
        {
            get { return (ObservableCollection<MenuItemButton>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        [TypeConverter(typeof(MenuOpenActionConverter))]
        public MenuOpenAction MenuOpenMode
        {
            get { return (MenuOpenAction)GetValue(MenuOpenModeProperty); }
            set { SetValue(MenuOpenModeProperty, value); }
        }

        public MenuItemButton SelectedButton
        {
            get { return (MenuItemButton)GetValue(SelectedButtonProperty); }
            private set { SetValue(SelectedButtonProperty, value); }
        }

        [TypeConverter(typeof(ContentOrientationConverter))]
        public ContentOrientation ContentPosition
        {
            get { return (ContentOrientation)GetValue(ContentPositionProperty); }
            set { SetValue(ContentPositionProperty, value); }
        }

        public Effect DefaultEffect
        {
            get { return (Effect)GetValue(DefaultEffectProperty); }
            set { SetValue(DefaultEffectProperty, value); }
        }

        public Effect SelectedEffect
        {
            get { return (Effect)GetValue(SelectedEffectProperty); }
            set { SetValue(SelectedEffectProperty, value); }
        }

        public Effect MouseOverEffect
        {
            get { return (Effect)GetValue(MouseOverEffectProperty); }
            set { SetValue(MouseOverEffectProperty, value); }
        }

        public Brush SelectedBackground
        {
            get { return (Brush)GetValue(SelectedBackgroundProperty); }
            set { SetValue(SelectedBackgroundProperty, value); }
        }

        public Brush MouseOverBackground
        {
            get { return (Brush)GetValue(MouseOverBackgroundProperty); }
            set { SetValue(MouseOverBackgroundProperty, value); }
        }

        protected static void OnContentPositionChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SwitchMenuButton me = d as SwitchMenuButton;
            if (me != null) me.InitializeControl();
        }

        protected void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (MenuItemButton item in e.NewItems)
                {
                    item.IsSelected = false;
                }

                InitializeControl();
            }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!Double.IsNaN(this.Width))
            {
                this.Width = Double.NaN;
            }

            if (!Double.IsNaN(this.Height))
            {
                this.Height = Double.NaN;
            }

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

            if (this.Items != null)
                this.Items.CollectionChanged += new NotifyCollectionChangedEventHandler(Items_CollectionChanged);

            InitializeControl();
        }

        #region Initialize Control Functions
        protected void InitializeControl()
        {
            if (this.Items.Count == 0) return;
            if (BaseMenuItemPanel == null) return;
            if (ChildMenuItemPanel == null) return;

            SetSelectedMenuItem();
            MeasureChildPanelSize();
            AddChildMenuButtons();
            InitializeChildPanel();
        }

        private void SetSelectedMenuItem()
        {
            if (this.SelectedButton != null)
            {
                this.SelectedButton.TextPosition = this.ContentPosition;
                return;
            }

            MenuItemButton selectedItem = null;
            if (Items != null && Items.Count > 0)
            {
                foreach (MenuItemButton item in Items)
                {
                    if (item.IsSelected)
                    {
                        if (selectedItem == null)
                            selectedItem = item;
                        else
                            item.IsSelected = false;
                    }
                }

                if (selectedItem == null)
                {
                    Items[0].IsSelected = true;
                    selectedItem = Items[0];
                }
            }

            if (selectedItem != null)
            {
                if (double.IsNaN(selectedItem.Width)) selectedItem.Width = 50;
                if (double.IsNaN(selectedItem.Height)) selectedItem.Height = 50;
                selectedItem.Click += new MenuItemClickEventHandler(BaseItem_Clicked);
                selectedItem.LostFocus += new RoutedEventHandler(BaseItem_LostFocus);
                selectedItem.TextPosition = this.ContentPosition;
                BaseMenuItemPanel.Children.Add(selectedItem);
                this.SelectedButton = selectedItem;
            }
        }

        //Measure Child Menu Item Panel Size
        private void MeasureChildPanelSize()
        {
            if (this.Items.Count > 1 && SelectedButton != null)
            {
                int total = this.Items.Count - 1;
                switch (this.ContentPosition)
                {
                    case ContentOrientation.UP:
                    case ContentOrientation.DOWN:
                        ChildPanelWidth = total * (SelectedButton.Width + BUTTON_SPACE) - BUTTON_SPACE;
                        ChildPanelHeight = (total > 2) ? Math.Floor(total / 2.0) * BUTTON_SPACE * 2 + SelectedButton.Height * 1.5 : SelectedButton.Height * 2.0;
                        break;
                    case ContentOrientation.LEFT:
                    case ContentOrientation.RIGHT:
                        ChildPanelWidth = (total > 2) ? Math.Floor(total / 2.0) * BUTTON_SPACE * 2 + SelectedButton.Width * 1.5 : SelectedButton.Width * 2.0;
                        ChildPanelHeight = total * (SelectedButton.Height + BUTTON_SPACE) - BUTTON_SPACE;
                        break;
                }
            }
        }

        private void AddChildMenuButtons()
        {
            if (Items == null) return;
            if (Items.Count < 2) return;

            double bml = 0; // Left margin of button
            double bmt = 0; // Top margin of button
            double bmr = 0; // Right margin of button
            double bmb = 0; // Bottom margin of button
            double w2 = 25; // Half of button width
            double h2 = 25; // Half of button height

            int count = 0;
            int total = Items.Count - 1; // Count out the Selected Item 

            VerticalAlignment valign = VerticalAlignment.Top;
            HorizontalAlignment halign = HorizontalAlignment.Left;

            Point BasePoint = new Point();
            Point ItemPoint = new Point();

            ChildMenuItemPanel.Children.Clear();

            foreach (MenuItemButton item in Items)
            {
                //Synchronize items' properties with properties of this
                item.Foreground = this.Foreground;
                item.Background = this.Background;
                item.BorderBrush = this.BorderBrush;
                item.BorderThickness = this.BorderThickness;

                if (this.DefaultEffect != null) item.DefaultEffect = this.DefaultEffect;
                if (this.SelectedEffect != null) item.MouseOverEffect = this.SelectedEffect;
                if (this.MouseOverEffect != null) item.MouseOverEffect = this.MouseOverEffect;
                if (this.SelectedBackground != null) item.SelectedBackground = this.SelectedBackground;
                if (this.MouseOverBackground != null) item.MouseOverBackground = this.MouseOverBackground;

                if (!item.IsSelected)
                {
                    if (double.IsNaN(item.Width)) item.Width = 50;
                    if (double.IsNaN(item.Height)) item.Height = 50;
                    bmr = bmb = bmb = bmr = 0;
                    h2 = item.Height / 2.0;
                    w2 = item.Width / 2.0;

                    switch (this.ContentPosition)
                    {
                        case ContentOrientation.DOWN:
                            valign = VerticalAlignment.Top;
                            halign = HorizontalAlignment.Left;
                            bml = (item.Width + BUTTON_SPACE) * count;
                            bmt = (total > 2) ? ((total - count - 1) * count * BUTTON_SPACE * 2 + h2) : item.Height;
                            BasePoint = new Point(ChildPanelWidth / 2.0, 0.0);
                            break;
                        case ContentOrientation.LEFT:
                            valign = VerticalAlignment.Top;
                            halign = HorizontalAlignment.Right;
                            bmt = (item.Height + BUTTON_SPACE) * count;
                            bmr = (total > 2) ? ((total - count - 1) * count * BUTTON_SPACE * 2 + w2) : item.Width;
                            bml = ChildPanelWidth - bmr - item.Width;
                            BasePoint = new Point(ChildPanelWidth, ChildPanelHeight / 2.0);
                            break;
                        case ContentOrientation.RIGHT:
                            valign = VerticalAlignment.Top;
                            halign = HorizontalAlignment.Left;
                            bmt = (item.Height + BUTTON_SPACE) * count;
                            bml = (total > 2) ? ((total - count - 1) * count * BUTTON_SPACE * 2 + w2) : item.Width;
                            BasePoint = new Point(0, ChildPanelHeight / 2.0);
                            break;
                        case ContentOrientation.UP:
                            valign = VerticalAlignment.Bottom;
                            halign = HorizontalAlignment.Left;
                            bml = (item.Width + BUTTON_SPACE) * count;
                            bmb = (total > 2) ? ((total - count - 1) * count * BUTTON_SPACE * 2 + h2) : item.Height;
                            bmt = ChildPanelHeight - bmb - item.Height;
                            BasePoint = new Point(ChildPanelWidth / 2.0, ChildPanelHeight);
                            break;
                    }

                    ItemPoint = new Point(bml + w2, bmt + h2);
                    Path lead = CreateLeadingLine(BasePoint, ItemPoint);
                    lead.Margin = new Thickness(0, 0, 0, 0);
                    lead.VerticalAlignment = VerticalAlignment.Stretch;
                    lead.HorizontalAlignment = HorizontalAlignment.Stretch;

                    item.Margin = new Thickness(bml, bmt, bmr, bmb);
                    item.VerticalAlignment = valign;
                    item.HorizontalAlignment = halign;
                    item.TextPosition = this.ContentPosition;
                    item.RenderTransform = new TranslateTransform() { X = 0, Y = 0 };
                    item.Click += new MenuItemClickEventHandler(MenuItem_Clicked);

                    ChildMenuItemPanel.Children.Add(lead);
                    ChildMenuItemPanel.Children.Add(item);
                    count++;
                }
            }
        }

        private Path CreateLeadingLine(Point startPoint, Point endPoint)
        {
            /* Create a cursive line */
            //LineSegment line = new LineSegment() { Point = endPoint };
            //PathFigure figure = new PathFigure() { StartPoint = startPoint, IsClosed = false, IsFilled = false };
            //figure.Segments.Add(line);

            //PathGeometry data = new PathGeometry();
            //data.Figures.Add(figure);

            /* Create a straight line */
            LineGeometry data = new LineGeometry() { StartPoint = startPoint, EndPoint = endPoint };
            DropShadowEffect effect = new DropShadowEffect() { Color = Colors.Black, ShadowDepth = 0 };
            Path lead = new Path() { Data = data, Stroke = new SolidColorBrush(Colors.White), Effect = effect };

            return lead;
        }

        //Reset Child Item Panel Margin
        private void InitializeChildPanel()
        {
            if (SelectedButton != null)
            {
                switch (this.ContentPosition)
                {
                    case ContentOrientation.DOWN:
                        ChildMenuItemPanel.Margin = new Thickness(-ChildPanelWidth / 2.0, 0, -ChildPanelWidth / 2.0, SelectedButton.Height / 2.0 - ChildPanelHeight);
                        ChildMenuItemPanel.HorizontalAlignment = HorizontalAlignment.Center;
                        ChildMenuItemPanel.VerticalAlignment = VerticalAlignment.Bottom;
                        break;
                    case ContentOrientation.LEFT:
                        ChildMenuItemPanel.Margin = new Thickness(SelectedButton.Width / 2.0 - ChildPanelWidth, -ChildPanelHeight / 2.0, 0, -ChildPanelHeight / 2.0);
                        ChildMenuItemPanel.HorizontalAlignment = HorizontalAlignment.Left;
                        ChildMenuItemPanel.VerticalAlignment = VerticalAlignment.Center;
                        break;
                    case ContentOrientation.RIGHT:
                        ChildMenuItemPanel.Margin = new Thickness(0, -ChildPanelHeight / 2.0, SelectedButton.Width / 2.0 - ChildPanelWidth, -ChildPanelHeight / 2.0);
                        ChildMenuItemPanel.HorizontalAlignment = HorizontalAlignment.Right;
                        ChildMenuItemPanel.VerticalAlignment = VerticalAlignment.Center;
                        break;
                    case ContentOrientation.UP:
                        ChildMenuItemPanel.Margin = new Thickness(-ChildPanelWidth / 2.0, SelectedButton.Height / 2.0 - ChildPanelHeight, -ChildPanelWidth / 2.0, 0);
                        ChildMenuItemPanel.HorizontalAlignment = HorizontalAlignment.Center;
                        ChildMenuItemPanel.VerticalAlignment = VerticalAlignment.Top;
                        break;
                }
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
            if (item.IsSelected && CurrentState != _STATE_EXPAND)
            {
                item.Focus();
                CurrentState = _STATE_EXPAND;
                VisualStateManager.GoToState(this, _STATE_EXPAND, true);
            }
        }

        void BaseItem_Clicked(object sender, MenuItemClickEventArgs e)
        {
            MenuItemButton item = sender as MenuItemButton;
            if (item.IsSelected && CurrentState != _STATE_EXPAND)
            {
                CurrentState = _STATE_EXPAND;
                VisualStateManager.GoToState(this, _STATE_EXPAND, true);
            }

            if (this.MenuOpenMode == MenuOpenAction.MouseHover && !isHoverModeSet)
            {
                this.SelectedButton.MouseEnter += new MouseEventHandler(BaseItem_MouseEnter);
                isHoverModeSet = true;
            }

            // Added to meet ViewPoint-GIS's need
            OnBaseButtonClicked(new MenuItemClickEventArgs(item.Tag));
        }

        void MenuItem_Clicked(object sender, MenuItemClickEventArgs e)
        {
            MenuItemButton item = sender as MenuItemButton;
            if (!item.IsSelected)
            {
                ClickedItem = item;
                double x0 = (this.ContentPosition == ContentOrientation.RIGHT) ? 0 : ((this.ContentPosition == ContentOrientation.LEFT) ? ChildPanelWidth : (ChildPanelWidth / 2.0));
                double y0 = (this.ContentPosition == ContentOrientation.DOWN) ? 0 : ((this.ContentPosition == ContentOrientation.UP) ? ChildPanelHeight : (ChildPanelHeight / 2.0));
                double dx = x0 - (item.Margin.Left + item.Width / 2.0);
                double dy = y0 - (item.Margin.Top + item.Height / 2.0);

                DoubleAnimationUsingKeyFrames animationX = new DoubleAnimationUsingKeyFrames();
                animationX.KeyFrames.Add(new SplineDoubleKeyFrame() { KeyTime = TimeSpan.FromMilliseconds(0), Value = 0 });
                animationX.KeyFrames.Add(new SplineDoubleKeyFrame() { KeyTime = TimeSpan.FromMilliseconds(300), Value = dx });

                DoubleAnimationUsingKeyFrames animationY = new DoubleAnimationUsingKeyFrames();
                animationY.KeyFrames.Add(new SplineDoubleKeyFrame() { KeyTime = TimeSpan.FromMilliseconds(0), Value = 0 });
                animationY.KeyFrames.Add(new SplineDoubleKeyFrame() { KeyTime = TimeSpan.FromMilliseconds(300), Value = dy });

                Storyboard sbExchange = new Storyboard();
                sbExchange.Completed += new EventHandler(StoryboardExchange_Completed);
                Storyboard.SetTargetProperty(animationX, new PropertyPath("X"));
                Storyboard.SetTarget(animationX, item.RenderTransform);
                Storyboard.SetTargetProperty(animationY, new PropertyPath("Y"));
                Storyboard.SetTarget(animationY, item.RenderTransform);
                sbExchange.Children.Add(animationX);
                sbExchange.Children.Add(animationY);
                sbExchange.Begin();
            }
        }

        void StoryboardExchange_Completed(object sender, EventArgs e)
        {
            CurrentState = _STATE_COLLAPSE;
            VisualStateManager.GoToState(this, _STATE_COLLAPSE, false);

            if (ClickedItem != null)
            {
                TranslateTransform transform = ClickedItem.RenderTransform as TranslateTransform;
                transform.X = 0;
                transform.Y = 0;

                // Switch Content
                object tag = SelectedButton.Tag;
                string text = SelectedButton.Text;
                ImageSource imgsrc = SelectedButton.ImageSource;

                SelectedButton.ImageSource = ClickedItem.ImageSource;
                SelectedButton.Text = ClickedItem.Text;
                SelectedButton.Tag = ClickedItem.Tag;

                ClickedItem.ImageSource = imgsrc;
                ClickedItem.Text = text;
                ClickedItem.Tag = tag;

                OnMenuButtonChanged(new MenuButtonChangeEventArgs(ClickedItem, SelectedButton));
            }
        }

        protected virtual void OnBaseButtonClicked(MenuItemClickEventArgs args)
        {
            if (BaseButtonClickHandler != null) BaseButtonClickHandler(this, args);
        }

        protected virtual void OnMenuButtonChanged(MenuButtonChangeEventArgs args)
        {
            if (MenuButtonChangeHandler != null) MenuButtonChangeHandler(this, args);
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
