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
    [TemplatePart(Name = ToggleButton._BUTTON_SHAPE, Type = typeof(Shape))]
    [TemplatePart(Name = ToggleButton._BUTTON_PANEL, Type = typeof(Border))]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_SHOW)]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_HIDE)]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_ORIGIN)]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_ROTATE90)]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_ROTATE180)]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_ROTATE270)]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_ROTATE_90)]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_ROTATE_180)]
    [TemplateVisualState(GroupName = ToggleButton._BUTTON_STATES, Name = ToggleButton._STATE_ROTATE_270)]
    public class ToggleButton : ButtonBase
    {
        protected const string _BUTTON_PANEL = "ButtonPanel";
        protected const string _BUTTON_SHAPE = "ButtonShape";
        protected const string _BUTTON_STATES = "ToggleButtonStates";

        protected const string _STATE_SHOW = "ShowState";
        protected const string _STATE_HIDE = "HideState";
        protected const string _STATE_ORIGIN = "BackOrigin";
        protected const string _STATE_ROTATE90 = "Rotate90";
        protected const string _STATE_ROTATE180 = "Rotate180";
        protected const string _STATE_ROTATE270 = "Rotate270";
        protected const string _STATE_ROTATE_90 = "Rotate-90";
        protected const string _STATE_ROTATE_180 = "Rotate-180";
        protected const string _STATE_ROTATE_270 = "Rotate-270";

        protected Shape ButtonShape = null;
        protected Border ButtonPanel = null;
        protected Effect DefaultEffect = null;

        public ToggleButton()
        {
            this.DefaultStyleKey = typeof(ToggleButton);
        }

        #region Dependency Properties
        // Using a DependencyProperty as the backing store for State. 
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(ToggleButtonState), typeof(ToggleButton), new PropertyMetadata(ToggleButtonState.STATE_ORIGIN, new PropertyChangedCallback(OnStateChanged)));

        // Using a DependencyProperty as the backing store for MouseOverColor. 
        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(Brush), typeof(ToggleButton), new PropertyMetadata(new SolidColorBrush(new Color() { R = 255, G = 255, B = 0, A = 255 })));

        // Using a DependencyProperty as the backing store for MouseOverEffect
        public static readonly DependencyProperty MouseOverEffectProperty = DependencyProperty.Register("MouseOverEffect", typeof(Effect), typeof(ToggleButton), new PropertyMetadata(new DropShadowEffect() { Color = new Color() { R = 255, G = 255, B = 0, A = 255 }, BlurRadius = 24, ShadowDepth = 0 }));

        [TypeConverter(typeof(ToggleButtonStateConverter))]
        public ToggleButtonState State
        {
            get { return (ToggleButtonState)GetValue(StateProperty); }
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
            ToggleButton me = d as ToggleButton;
            me.State = (ToggleButtonState)e.NewValue;
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
                ButtonShape.Width = size - 4.0;
                ButtonShape.Height = size - 4.0;
            }

            RotateTransform transform = this.GetTemplateChild("RotateButtonTransform") as RotateTransform;
            transform.CenterX = (size - 4.0) * 0.5;
            transform.CenterY = (size - 4.0) * 0.3;
            ApplyState();
        }

        private void ApplyState()
        {
            if (this.ButtonShape != null)
            {
                switch (this.State)
                {
                    case ToggleButtonState.STATE_SHOW:
                        VisualStateManager.GoToState(this, _STATE_SHOW, true);
                        break;
                    case ToggleButtonState.STATE_HIDE:
                        VisualStateManager.GoToState(this, _STATE_HIDE, true);
                        break;
                    case ToggleButtonState.STATE_ORIGIN:
                        VisualStateManager.GoToState(this, _STATE_ORIGIN, true);
                        break;
                    case ToggleButtonState.STATE_ROTATE90:
                        VisualStateManager.GoToState(this, _STATE_ROTATE90, false);
                        break;
                    case ToggleButtonState.STATE_ROTATE180:
                        VisualStateManager.GoToState(this, _STATE_ROTATE180, false);
                        break;
                    case ToggleButtonState.STATE_ROTATE270:
                        VisualStateManager.GoToState(this, _STATE_ROTATE270, false);
                        break;
                    case ToggleButtonState.STATE_ROTATE_90:
                        VisualStateManager.GoToState(this, _STATE_ROTATE_90, false);
                        break;
                    case ToggleButtonState.STATE_ROTATE_180:
                        VisualStateManager.GoToState(this, _STATE_ROTATE_180, false);
                        break;
                    case ToggleButtonState.STATE_ROTATE_270:
                        VisualStateManager.GoToState(this, _STATE_ROTATE_270, false);
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
                ButtonShape.Fill = MouseOverColor;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.Effect = DefaultEffect;

            if (ButtonShape != null)
                ButtonShape.Fill = this.Foreground;
        }
        #endregion
    }
}
