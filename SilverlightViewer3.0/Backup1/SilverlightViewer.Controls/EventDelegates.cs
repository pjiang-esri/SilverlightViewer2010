using System;
using System.Windows;
using System.Windows.Controls;
//using ESRI.ArcGIS.Client;

/// ===========================================================
/// All event handler delegates and event arguments are defined
/// for the controls in this assembly in this class file
/// ===========================================================
namespace ESRI.SilverlightViewer.Controls
{
    // Used in ContextMenuItem and MenuItemButton
    public delegate void MenuItemClickEventHandler(object sender, MenuItemClickEventArgs e);

    // Used in FloatingTaskbar
    public delegate void TaskbarStateChangeEventHandler(object sender, TaskbarStateChangeEventArgs e);

    // Used in FloatingWindow, FloatingRollWindow, and FloatingSplitWindow
    public delegate void WindowOpenEventHandler(object sender, RoutedEventArgs e);
    public delegate void WindowCloseEventHandler(object sender, RoutedEventArgs e);
    public delegate void WindowToggleEventHandler(object sender, WindowToggleEventArgs e);

    // Used in SwitchMenuButton
    public delegate void MenuButtonChangeEventHandler(object sender, MenuButtonChangeEventArgs e);

    //Used in SpatialDataGrid
    //public delegate void DataGridSelectionChangeEventHandler(object sender, SelectionChangedEventArgs e);
    //public delegate void MouseOverDataGridRowEventHandler(object sender, MouseOverRowEventArgs e);

    /*******************************************************
     * Event Arguments
     *******************************************************/

    // Used in ContextMenuItem and MenuItemButton
    #region MenuItemClick Event Argument
    public class MenuItemClickEventArgs
    {
        object _itemTag = null;

        public MenuItemClickEventArgs()
        {
        }

        public MenuItemClickEventArgs(object itemTag)
        {
            this._itemTag = itemTag;
        }

        public object ItemTag
        {
            get { return this._itemTag; }
        }
    }
    #endregion

    // Used by FloatingControl and its inheritances
    #region IsActiveChanged Event Argument
    public class IsActiveChangedEventArgs
    {
        public bool IsActive { get; set; }

        public IsActiveChangedEventArgs() { }

        public IsActiveChangedEventArgs(bool active)
        {
            this.IsActive = active;
        }
    }
    #endregion

    // Used in FloatingTaskbar
    #region PositionChangeEventArgs Event Argument
    public class TaskbarStateChangeEventArgs
    {
        bool _IsInUpper = true;
        DockPosition _DockState = DockPosition.TOP;

        public TaskbarStateChangeEventArgs(DockPosition dockState, bool isInUpper)
        {
            this._IsInUpper = isInUpper;
            this._DockState = dockState;
        }

        public DockPosition DockState
        {
            get { return this._DockState; }
        }

        public bool IsInUpperPage
        {
            get { return this._IsInUpper; }
        }
    }
    #endregion

    // Used in FloatingWindow, FloatingRollWindow, and FloatingSplitWindow
    #region Window Toggle Event Argument
    public class WindowToggleEventArgs
    {
        public bool Expanded { get; set; }

        public WindowToggleEventArgs() { }

        public WindowToggleEventArgs(bool expanded)
        {
            this.Expanded = expanded;
        }
    }
    #endregion

    // Used in SwitchMenuButton
    #region  MenuButtonChange Event Argument
    public class MenuButtonChangeEventArgs
    {
        MenuItemButton _OldButton = null;
        MenuItemButton _NewButton = null;

        public MenuButtonChangeEventArgs()
        {
        }

        public MenuButtonChangeEventArgs(MenuItemButton oldButton, MenuItemButton newButton)
        {
            this._OldButton = oldButton;
            this._NewButton = newButton;
        }

        public MenuItemButton OldButton
        {
            get { return this._OldButton; }
        }

        public MenuItemButton NewButton
        {
            get { return this._NewButton; }
        }
    }
    #endregion

    //Used in SpatialdataGrid
    #region MoverOver SpatialDataGrid Row Event Argument
    //public class MouseOverRowEventArgs
    //{ 
    //    Graphic _graphic = null;

    //    public MouseOverRowEventArgs(Graphic graphic)
    //    {
    //        this._graphic = graphic;
    //    }

    //    public Graphic Graphic
    //    {
    //        get { return _graphic; }
    //    }
    //}
    #endregion
}
