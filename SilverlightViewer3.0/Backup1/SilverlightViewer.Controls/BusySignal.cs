using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace ESRI.SilverlightViewer.Controls
{
    [TemplateVisualState(GroupName = BusySignal._SIGNAL_STATES, Name = BusySignal._STATE_BUSY)]
    [TemplateVisualState(GroupName = BusySignal._SIGNAL_STATES, Name = BusySignal._STATE_HIDE)]
    public class BusySignal : Control
    {
        protected const string _TEMPLATE_GRID = "TemplateGrid";
        protected const string _SIGNAL_STATES = "BusySignalStates";
        protected const string _STATE_BUSY = "StateBusy";
        protected const string _STATE_HIDE = "StateHide";

        protected Grid TemplateGrid = null;

        public BusySignal()
        {
            this.DefaultStyleKey = typeof(BusySignal);
        }

        #region Dependent Properties
        // Using a DependencyProperty as the backing store for Color. 
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(BusySignal), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        // Using a DependencyProperty as the backing store for State. 
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(BusySignalState), typeof(BusySignal), new PropertyMetadata(BusySignalState.STATE_HIDE, new PropertyChangedCallback(OnStateChanged)));

        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        [TypeConverter(typeof(BusySignalStateConverter))]
        public BusySignalState State
        {
            get { return (BusySignalState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        protected static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BusySignal me = d as BusySignal;
            me.State = (BusySignalState)e.NewValue;
            me.ApplyState();
        }
        #endregion

        public override void OnApplyTemplate()
        {
            TemplateGrid = this.GetTemplateChild(_TEMPLATE_GRID) as Grid;
            ApplyState();
        }

        private void ApplyState()
        {
            // After Template is applied 
            if (TemplateGrid != null)
            {
                switch (this.State)
                {
                    case BusySignalState.STATE_BUSY:
                        VisualStateManager.GoToState(this, _STATE_BUSY, true);
                        break;
                    case BusySignalState.STATE_HIDE:
                        VisualStateManager.GoToState(this, _STATE_HIDE, true);
                        break;
                }
            }
        }
    }
}
