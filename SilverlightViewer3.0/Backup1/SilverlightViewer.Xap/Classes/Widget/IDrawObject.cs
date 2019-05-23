namespace ESRI.SilverlightViewer.Widget
{
    /* =============================================================================
     * This interface is phasing out. For most widgets (EditWidget is an exception)
     * overriding OnIsActiveChanged is not a necessity any more since this version,
     * since an overriding is provided in class WidgetBase. But overriding virtual 
     * function ResetDrawObjectMode defined in class WidgetBase is required if your
     * widget uses DrawObject. You may please take LocatorWidget and MeasureWidget 
     * as exmaples to learn how to override the function - ResetDrawObjectMode
     * 
     * OnIsActiveChanged is overrided in WidgetBase.cs like this:
      
            if (e.IsActive)
            {
                this.ResetDrawObjectMode();
            }
            else
            {
                WidgetManager.ResetDrawObject();
            }
     =============================================================================== */
    /// <summary>
    /// Deprecated - Overriding virtual function ResetDrawObjectMode defined 
    /// in class WidgetBase is alternative of implementing this interface
    /// </summary>
    public interface IDrawObject
    {
        void ResetDrawObjectMode();
    }
}
