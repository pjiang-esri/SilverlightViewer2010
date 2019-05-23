using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace ESRI.SilverlightViewer.Controls
{
    public abstract class MenuItemButton : ButtonBase
    {
        protected const string _MENU_TEXT_BACK = "MenuTextBack";
        protected const string _MENU_TEXT_BLOCK = "MenuTextBlock";
        protected const string _MENU_BUTTON_IMAGE = "MenuButtonImage";
        protected const string _MENU_BUTTON_SHAPE = "MenuButtonShape";

        protected Image MenuButtonImage = null;
        protected Shape MenuButtonShape = null;
        protected Border MenuTextBack = null;
        protected TextBlock MenuTextBlock = null;

        #region Define Click Handler and Event
        private MenuItemClickEventHandler ClickHandler = null;
        public new event MenuItemClickEventHandler Click
        {
            add
            {
                if (ClickHandler == null || !ClickHandler.GetInvocationList().Contains(value))
                    ClickHandler += value;
            }

            remove
            {
                ClickHandler -= value;
            }
        }
        #endregion

        public MenuItemButton()
        {
        }

        #region Dependency Properties
        // Using a DependencyProperty as the backing store for Text
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MenuItemButton), new PropertyMetadata(""));

        // Using a DependencyProperty as the backing store for IsSelected
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(MenuItemButton), new PropertyMetadata(false, new PropertyChangedCallback(OnIsSelectedChanged)));

        // Using a DependencyProperty as the backing store for IsImageOnly
        public static readonly DependencyProperty IsImageOnlyProperty = DependencyProperty.Register("IsImageOnly", typeof(bool), typeof(MenuItemButton), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for ImageSource
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(MenuItemButton), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for ImageMargin
        public static readonly DependencyProperty ImageMarginProperty = DependencyProperty.Register("ImageMargin", typeof(Double), typeof(MenuItemButton), new PropertyMetadata(5.0));

        // Using a DependencyProperty as the backing store for TextPosition
        public static readonly DependencyProperty TextPositionProperty = DependencyProperty.Register("TextPosition", typeof(ContentOrientation), typeof(MenuItemButton), new PropertyMetadata(ContentOrientation.DOWN));

        // Using a DependencyProperty as the backing store for TextVisibility
        public static readonly DependencyProperty TextVisibilityProperty = DependencyProperty.Register("TextVisibility", typeof(Visibility), typeof(MenuItemButton), new PropertyMetadata(Visibility.Visible));

        // Using a DependencyProperty as the backing store for DefaultEffect
        public static readonly DependencyProperty DefaultEffectProperty = DependencyProperty.Register("DefaultEffect", typeof(Effect), typeof(MenuItemButton), null);

        // Using a DependencyProperty as the backing store for SelectedEffect
        public static readonly DependencyProperty SelectedEffectProperty = DependencyProperty.Register("SelectedEffect", typeof(Effect), typeof(MenuItemButton), null);

        // Using a DependencyProperty as the backing store for MouseOverEffect
        public static readonly DependencyProperty MouseOverEffectProperty = DependencyProperty.Register("MouseOverEffect", typeof(Effect), typeof(MenuItemButton), null);

        // Using a DependencyProperty as the backing store for MouseOver Background Color
        public static readonly DependencyProperty SelectedBackgroundProperty = DependencyProperty.Register("SelectedBackground", typeof(Brush), typeof(MenuItemButton), null);

        // Using a DependencyProperty as the backing store for MouseOver Background Color
        public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(MenuItemButton), null);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
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

        [TypeConverter(typeof(ContentOrientationConverter))]
        public ContentOrientation TextPosition
        {
            get { return (ContentOrientation)GetValue(TextPositionProperty); }
            set { SetValue(TextPositionProperty, value); }
        }

        public Visibility TextVisibility
        {
            get { return (Visibility)GetValue(TextVisibilityProperty); }
            set { SetValue(TextVisibilityProperty, value); }
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

        protected static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MenuItemButton me = d as MenuItemButton;
            bool selected = (bool)e.NewValue;

            if (me.MenuButtonShape != null)
            {
                if (selected)
                {
                    if (me.MouseOverBackground != null)
                        me.MenuButtonShape.Fill = me.SelectedBackground;

                    if (me.SelectedEffect != null)
                    {
                        if (me.IsImageOnly)
                            me.MenuButtonImage.Effect = me.SelectedEffect;
                        else
                            me.MenuButtonShape.Effect = me.SelectedEffect;

                        if (me.SelectedEffect is DropShadowEffect)
                            (me.MenuTextBlock.Effect as DropShadowEffect).Color = (me.SelectedEffect as DropShadowEffect).Color;
                    }
                }
                else
                {
                    if (me.MouseOverBackground != null)
                        me.MenuButtonShape.Fill = me.Background;

                    if (me.SelectedEffect != null)
                    {
                        if (me.IsImageOnly)
                            me.MenuButtonImage.Effect = me.DefaultEffect;
                        else
                            me.MenuButtonShape.Effect = me.DefaultEffect;

                        (me.MenuTextBlock.Effect as DropShadowEffect).Color = Colors.White;
                    }
                }
            }
        }
        #endregion

        // Locate Elements and Add Event Listeners
        public override void OnApplyTemplate()
        {
            if (this.Effect != null)
            {
                this.DefaultEffect = this.Effect;
                this.Effect = null;
            }

            if (double.IsNaN(this.Width))
            {
                if (this.ImageSource != null)
                    this.Width = (this.ImageSource as BitmapSource).PixelWidth + ImageMargin * 2.0;
                else
                    this.Width = 50;
            }

            if (double.IsNaN(this.Height))
            {
                if (this.ImageSource != null)
                    this.Height = (this.ImageSource as BitmapSource).PixelHeight + ImageMargin * 2.0;
                else
                    this.Height = 50;
            }

            MenuButtonImage = this.GetTemplateChild(_MENU_BUTTON_IMAGE) as Image;
            MenuButtonShape = this.GetTemplateChild(_MENU_BUTTON_SHAPE) as Shape;
            MenuTextBlock = this.GetTemplateChild(_MENU_TEXT_BLOCK) as TextBlock;
            MenuTextBack = this.GetTemplateChild(_MENU_TEXT_BACK) as Border;

            if (MenuButtonImage != null)
            {
                MenuButtonImage.Margin = new Thickness(ImageMargin);
                if (IsImageOnly && MenuButtonShape != null)
                {
                    MenuButtonImage.Effect = this.DefaultEffect;
                    MenuButtonShape.Visibility = Visibility.Collapsed;
                }
            }

            if (MenuTextBlock != null)
            {
                MenuTextBlock.LayoutUpdated += new EventHandler(MenuTextBlock_LayoutUpdated);
            }
        }

        #region Update Layout
        private void MenuTextBlock_LayoutUpdated(object sender, EventArgs e)
        {
            AdjustTextPosition();
        }

        private void AdjustTextPosition()
        {
            if (MenuTextBack != null && !string.IsNullOrEmpty(this.Text))
            {
                switch (this.TextPosition)
                {
                    case ContentOrientation.DOWN:
                        MenuTextBack.Margin = new Thickness(-10, 0, -10, -20);
                        MenuTextBack.VerticalAlignment = VerticalAlignment.Bottom;
                        MenuTextBack.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                    case ContentOrientation.LEFT:
                        MenuTextBack.Margin = new Thickness(-MenuTextBack.ActualWidth, 0, 0, 0);
                        MenuTextBack.VerticalAlignment = VerticalAlignment.Bottom;
                        MenuTextBack.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case ContentOrientation.RIGHT:
                        MenuTextBack.Margin = new Thickness(0, 0, -MenuTextBack.ActualWidth, 0);
                        MenuTextBack.VerticalAlignment = VerticalAlignment.Bottom;
                        MenuTextBack.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                    case ContentOrientation.UP:
                        MenuTextBack.Margin = new Thickness(-10, -20, -10, 0);
                        MenuTextBack.VerticalAlignment = VerticalAlignment.Top;
                        MenuTextBack.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                }
            }
        }
        #endregion

        #region Override Mouse Events
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (this.MenuButtonShape != null && MenuButtonImage != null)
            {
                if (this.MouseOverEffect != null)
                {
                    if (this.IsImageOnly)
                        this.MenuButtonImage.Effect = this.MouseOverEffect;
                    else
                    {
                        this.MenuButtonShape.Effect = this.MouseOverEffect;
                        this.MenuButtonShape.Fill = (this.MouseOverBackground != null) ? this.MouseOverBackground : this.Background;
                    }
                    (this.MenuTextBack.Effect as DropShadowEffect).Color = (this.MouseOverEffect as DropShadowEffect).Color;
                }
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (this.MenuButtonShape != null && MenuButtonImage != null)
            {
                if (this.IsImageOnly)
                    this.MenuButtonImage.Effect = this.DefaultEffect;
                else
                {
                    this.MenuButtonShape.Fill = this.Background;
                    this.MenuButtonShape.Effect = this.DefaultEffect;
                }

                (this.MenuTextBack.Effect as DropShadowEffect).Color = Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF);
            }
        }

        protected override void OnClick()
        {
            this.Focus();
            if (ClickHandler != null) ClickHandler(this, new MenuItemClickEventArgs(this.Tag));
        }
        #endregion
    }

    public class SquareMenuButton : MenuItemButton
    {
        public SquareMenuButton()
        {
            this.DefaultStyleKey = typeof(SquareMenuButton);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }

    public class CircleMenuButton : MenuItemButton
    {
        public CircleMenuButton()
        {
            this.DefaultStyleKey = typeof(CircleMenuButton);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
