using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplatePart(Name = ContextMenuItem._TEMPLATE_PANEL, Type = typeof(Panel))]
    [TemplatePart(Name = ContextMenuItem._MENU_BUTTON_ICON, Type = typeof(Image))]
    [TemplatePart(Name = ContextMenuItem._MENU_BUTTON_TEXT, Type = typeof(TextBlock))]
    public class ContextMenuItem : ButtonBase
    {
        protected const string _TEMPLATE_PANEL = "TemplatePanel";
        protected const string _MENU_BUTTON_TEXT = "MenuButtonText";
        protected const string _MENU_BUTTON_ICON = "MenuButtonIcon";

        protected Panel TemplatePanel = null;
        protected Image MenuButtonIcon = null;
        protected TextBlock MenuButtonText = null;

        #region Define Click Event
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

        public ContextMenuItem()
        {
            this.DefaultStyleKey = typeof(ContextMenuItem);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(OnIsEnabledChanged);
        }

        #region Dependency Properties
        // Using a DependencyProperty as the backing store for Text
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ContextMenuItem), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for ImageSource
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(ContextMenuItem), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for ImageSource
        public static readonly DependencyProperty UseSmallIconProperty = DependencyProperty.Register("UseSmallIcon", typeof(bool), typeof(ContextMenuItem), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for MouseOver Background Color
        public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(ContextMenuItem), new PropertyMetadata(new SolidColorBrush(new Color() { R = 204, G = 255, B = 240, A = 255 })));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ImageSource IconSource
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        /// <summary>
        /// If set to true, stretch icon size to 24 x 24
        /// </summary>
        public bool UseSmallIcon
        {
            get { return (bool)GetValue(UseSmallIconProperty); }
            set { SetValue(UseSmallIconProperty, value); }
        }

        public Brush MouseOverBackground
        {
            get { return (Brush)GetValue(MouseOverBackgroundProperty); }
            set { SetValue(MouseOverBackgroundProperty, value); }
        }
        #endregion

        #region Handle IsEnabledChanged Event
        protected virtual void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                this.OpacityMask = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            else
                this.OpacityMask = new SolidColorBrush(Color.FromArgb(0x66, 0x99, 0x99, 0x99));
        }
        #endregion

        #region Override OnApplyTemplate Function
        // Locate Elements and Add Event Listeners
        public override void OnApplyTemplate()
        {
            TemplatePanel = this.GetTemplateChild(_TEMPLATE_PANEL) as Panel;
            MenuButtonText = this.GetTemplateChild(_MENU_BUTTON_TEXT) as TextBlock;
            MenuButtonIcon = this.GetTemplateChild(_MENU_BUTTON_ICON) as Image;

            if (this.TemplatePanel != null)
            {
                this.TemplatePanel.Background = this.Background;
            }

            if (MenuButtonIcon != null && UseSmallIcon)
            {
                MenuButtonIcon.Width = 24;
                MenuButtonIcon.Height = 24;
                MenuButtonIcon.Stretch = Stretch.UniformToFill;
            }
        }
        #endregion

        #region Override Mouse Events
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (this.MouseOverBackground != null)
            {
                TemplatePanel.Background = this.MouseOverBackground;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            TemplatePanel.Background = this.Background;
        }

        protected override void OnClick()
        {
            if (ClickHandler != null) ClickHandler(this, new MenuItemClickEventArgs(this.Tag));
        }
        #endregion
    }
}
