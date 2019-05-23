using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplatePart(Name = SimpleLinkButton._LINK_TEXT_PANEL, Type = typeof(Border))]
    [TemplatePart(Name = SimpleLinkButton._LINK_TEXT_BLOCK, Type = typeof(TextBlock))]
    public class SimpleLinkButton : ButtonBase
    {
        protected const string _LINK_TEXT_PANEL = "LinkTextPanel";
        protected const string _LINK_TEXT_BLOCK = "LinkTextBlock";
        protected TextBlock LinkTextBlock = null;
        protected Border LinkTextPanel = null;

        public SimpleLinkButton()
        {
            this.DefaultStyleKey = typeof(SimpleLinkButton);
        }

        #region Dependency Properties
        // Using a DependencyProperty as the backing store for Text. 
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SimpleLinkButton), new PropertyMetadata(""));

        // Using a DependencyProperty as the backing store for IsActive 
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(SimpleLinkButton), new PropertyMetadata(false, new PropertyChangedCallback(OnIsActiveChanged)));

        // Using a DependencyProperty as the backing store for the Foreground Color when it is selected. 
        public static readonly DependencyProperty ActiveTextColorProperty = DependencyProperty.Register("ActiveTextColor", typeof(Brush), typeof(SimpleLinkButton), new PropertyMetadata(new SolidColorBrush(new Color() { R = 0, G = 0, B = 0, A = 255 })));

        // Using a DependencyProperty as the backing store for the Background Color when it is selected . 
        public static readonly DependencyProperty ActiveBackColorProperty = DependencyProperty.Register("ActiveBackColor", typeof(Brush), typeof(SimpleLinkButton), new PropertyMetadata(new SolidColorBrush(new Color() { R = 204, G = 204, B = 255, A = 255 })));

        // Using a DependencyProperty as the backing store for MouseOver Foreground Color. 
        public static readonly DependencyProperty MouseOverTextColorProperty = DependencyProperty.Register("MouseOverTextColor", typeof(Brush), typeof(SimpleLinkButton), new PropertyMetadata(new SolidColorBrush(new Color() { R = 0, G = 0, B = 0, A = 255 })));

        // Using a DependencyProperty as the backing store for MouseOver Background Color. 
        public static readonly DependencyProperty MouseOverBackColorProperty = DependencyProperty.Register("MouseOverBackColor", typeof(Brush), typeof(SimpleLinkButton), new PropertyMetadata(new SolidColorBrush(new Color() { R = 204, G = 255, B = 255, A = 255 })));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public Brush ActiveTextColor
        {
            get { return (Brush)GetValue(ActiveTextColorProperty); }
            set { SetValue(ActiveTextColorProperty, value); }
        }

        public Brush ActiveBackColor
        {
            get { return (Brush)GetValue(ActiveBackColorProperty); }
            set { SetValue(ActiveBackColorProperty, value); }
        }

        public Brush MouseOverTextColor
        {
            get { return (Brush)GetValue(MouseOverTextColorProperty); }
            set { SetValue(MouseOverTextColorProperty, value); }
        }

        public Brush MouseOverBackColor
        {
            get { return (Brush)GetValue(MouseOverBackColorProperty); }
            set { SetValue(MouseOverBackColorProperty, value); }
        }
        #endregion

        #region Dependency Property Events
        protected static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool selected = (bool)e.NewValue;
            SimpleLinkButton me = d as SimpleLinkButton;

            if (me.LinkTextBlock != null && me.LinkTextPanel != null)
            {
                if (selected)
                {
                    me.LinkTextBlock.Foreground = me.ActiveTextColor;
                    me.LinkTextPanel.Background = me.ActiveBackColor;
                }
                else
                {
                    me.LinkTextBlock.Foreground = me.Foreground;
                    me.LinkTextPanel.Background = me.Background;
                }
            }
        }
        #endregion

        // Locate Elements and Add Event Listeners
        public override void OnApplyTemplate()
        {
            LinkTextBlock = this.GetTemplateChild(_LINK_TEXT_BLOCK) as TextBlock;
            if (LinkTextBlock != null && this.IsActive)
            {
                LinkTextBlock.Foreground = this.ActiveTextColor;
            }

            LinkTextPanel = this.GetTemplateChild(_LINK_TEXT_PANEL) as Border;
            if (LinkTextPanel != null && this.IsActive)
            {
                LinkTextPanel.Background = this.ActiveBackColor;
            }
        }

        #region Override Event Handlers
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (!IsActive)
            {
                LinkTextBlock.Foreground = this.MouseOverTextColor;
                LinkTextPanel.Background = this.MouseOverBackColor;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (!IsActive)
            {
                LinkTextBlock.Foreground = this.Foreground;
                LinkTextPanel.Background = this.Background;
            }
        }
        #endregion
    }
}
