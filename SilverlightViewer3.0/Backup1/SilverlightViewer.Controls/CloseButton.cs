using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplatePart(Name = CloseButton._BUTTON_SHAPE, Type = typeof(Shape))]
    [TemplatePart(Name = CloseButton._BUTTON_PANEL, Type = typeof(Border))]
    [TemplateVisualState(GroupName = CloseButton._BUTTON_STATES, Name = CloseButton._STATE_SHOW)]
    [TemplateVisualState(GroupName = CloseButton._BUTTON_STATES, Name = CloseButton._STATE_HIDE)]
    [TemplateVisualState(GroupName = CloseButton._BUTTON_STATES, Name = CloseButton._STATE_ORIGIN)]
    [TemplateVisualState(GroupName = CloseButton._BUTTON_STATES, Name = CloseButton._STATE_ROTATE360)]
    public class CloseButton : ButtonBase
    {
        protected Shape ButtonShape = null;
        protected Border ButtonPanel = null;
        protected Effect DefaultEffect = null;

        protected const string _BUTTON_PANEL = "ButtonPanel";
        protected const string _BUTTON_SHAPE = "ButtonShape";
        protected const string _BUTTON_STATES = "CloseButtonStates";

        protected const string _STATE_SHOW = "ShowState";
        protected const string _STATE_HIDE = "HideState";
        protected const string _STATE_ORIGIN = "BackOrigin";
        protected const string _STATE_ROTATE360 = "Rotate360";

        public CloseButton()
        {
            this.DefaultStyleKey = typeof(CloseButton);
        }

        #region Dependency Properties
        // Using a DependencyProperty as the backing store for State. 
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(CloseButtonState), typeof(CloseButton), new PropertyMetadata(CloseButtonState.STATE_ORIGIN, new PropertyChangedCallback(OnStateChanged)));
  
        // Using a DependencyProperty as the backing store for MouseOverColor. 
        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(Brush), typeof(CloseButton), new PropertyMetadata(new SolidColorBrush(new Color() { R = 255, G = 255, B = 0, A = 255 })));

        // Using a DependencyProperty as the backing store for MouseOverEffect
        public static readonly DependencyProperty MouseOverEffectProperty = DependencyProperty.Register("MouseOverEffect", typeof(Effect), typeof(CloseButton), new PropertyMetadata(new DropShadowEffect() { Color = new Color() { R = 255, G = 255, B = 0, A = 255 }, BlurRadius = 24, ShadowDepth = 0 }));

        [TypeConverter(typeof(CloseButtonStateConverter))]
        public CloseButtonState State
        {
            get { return (CloseButtonState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public Brush MouseOverColor
        {
            get { return (Brush)GetValue(MouseOverColorProperty); }
            set { SetValue(MouseOverColorProperty, value); }
        }

        public Effect MouseOverEffect
        {
            get { return (Effect)GetValue(MouseOverEffectProperty); }
            set { SetValue(MouseOverEffectProperty, value); }
        }

        protected static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CloseButton me = d as CloseButton;
            me.State = (CloseButtonState)e.NewValue;
            me.ApplyState();
        }
        #endregion

        // Locate Elements and Add Event Listeners
        public override void OnApplyTemplate()
        {
            this.Width = double.IsNaN(this.Width) ? this.MinWidth : this.Width;
            this.Height = double.IsNaN(this.Height) ? this.MinHeight : this.Height;
            this.DefaultEffect = this.Effect; // Make a copy;

            double size = (this.Width > this.Height) ? this.Height : this.Width;
            ButtonPanel = this.GetTemplateChild(_BUTTON_PANEL) as Border;

            ButtonShape = this.GetTemplateChild(_BUTTON_SHAPE) as Shape;
            if (ButtonShape != null)
            {
                ButtonShape.Width = size - 6.0;
                ButtonShape.Height = size - 6.0;
            }

            RotateTransform transform = this.GetTemplateChild("RotateButtonTransform") as RotateTransform;
            transform.CenterX = (size - 6.0) / 2.0;
            transform.CenterY = (size - 6.0) / 2.0;
            ApplyState();
        }

        private void ApplyState()
        {
            if (this.ButtonShape != null)
            {
                switch (this.State)
                {
                    case CloseButtonState.STATE_SHOW:
                        VisualStateManager.GoToState(this, _STATE_SHOW, true);
                        break;
                    case CloseButtonState.STATE_HIDE:
                        VisualStateManager.GoToState(this, _STATE_HIDE, true);
                        break;
                    case CloseButtonState.STATE_ORIGIN:
                        VisualStateManager.GoToState(this, _STATE_ORIGIN, true);
                        break;
                    case CloseButtonState.STATE_ROTATE360:
                        VisualStateManager.GoToState(this, _STATE_ROTATE360, true);
                        break;
                }
            }
        }

        #region Override Event Handlers
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.Effect = MouseOverEffect;

            if (ButtonShape != null) 
                ButtonShape.Stroke = MouseOverColor;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.Effect = DefaultEffect;

            if (ButtonShape != null) 
                ButtonShape.Stroke = this.Foreground;
        }
        #endregion
    }
}
