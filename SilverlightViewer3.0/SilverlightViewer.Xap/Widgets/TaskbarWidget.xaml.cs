using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.SilverlightViewer.Controls;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class TaskbarWidget : TaskbarBase, IDrawObject
    {
        private MapActionMode toolMode = MapActionMode.Pan;
        private DropMenuButton LinkMenuButton = null;
        private SwitchMenuButton ToolMenuButton = null;
        private SwitchMenuButton BaseMenuButton = null;
        private SimpleFillSymbol ZoomBoxFillSymbol = null;
        private PopupWindow AboutWindow = null;
        private Key pressedKey = Key.Unknown;

        public TaskbarWidget()
        {
            InitializeComponent();
        }

        #region Override OnWidgetLoaded, Hook Hotkey Handlers and Reset DrawObject Mode
        protected override void OnWidgetLoaded()
        {
            this.IsActive = true;
            this.DrawObject.Map = MapControl;
            this.CurrentPage.KeyUp += new KeyEventHandler(MapPage_KeyUp); // Capture Shift & Space Key Up
            this.CurrentPage.KeyDown += new KeyEventHandler(MapPage_KeyDown); // Capture Shift & Space Key Down
            this.DrawObject.DrawComplete += new EventHandler<DrawEventArgs>(DrawObject_DrawComplete);
            this.ZoomBoxFillSymbol = this.CurrentApp.Resources[SymbolResources.ZOOM_BOX] as SimpleFillSymbol;

            WidgetManager.Taskbar = this;
            this.InitializeWidgetMenuBars();
            this.ResetDrawObjectMode();
        }

        private void MapPage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Shift || e.Key == Key.Space)
            {
                this.pressedKey = Key.Unknown;
                WidgetBase widget = WidgetManager.FindActiveWidget();
                if (widget != null)
                {
                    widget.ResetDrawObjectMode();
                }
                else
                {
                    this.ResetDrawObjectMode();
                }
            }
        }

        private void MapPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Shift)
            {
                // Use the built-in Shift hotkey capability
                this.DrawObject.IsEnabled = false;
                this.MapControl.Cursor = Cursors.Arrow;
            }
            else if (e.Key == Key.Space)
            {
                this.pressedKey = e.Key;
                this.DrawWidget = this.GetType();
                this.DrawObject.IsEnabled = true;
                this.DrawObject.DrawMode = DrawMode.Rectangle;
                this.DrawObject.FillSymbol = ZoomBoxFillSymbol;
                this.MapControl.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Implement IDrawObject
        /// </summary>
        public void ResetDrawObjectMode()
        {
            switch (toolMode)
            {
                case MapActionMode.Pan:
                    this.DrawWidget = this.GetType();
                    this.DrawObject.IsEnabled = false;
                    if (this.MapControl != null) this.MapControl.Cursor = Cursors.Hand;
                    break;
                case MapActionMode.ZoomIn:
                case MapActionMode.Zoomout:
                    this.DrawWidget = this.GetType();
                    this.DrawObject.IsEnabled = true;
                    this.DrawObject.DrawMode = DrawMode.Rectangle;
                    this.DrawObject.FillSymbol = ZoomBoxFillSymbol;
                    if (this.MapControl != null) this.MapControl.Cursor = Cursors.Arrow;
                    break;
                case MapActionMode.Identify: // Hand over to IdentifyWidget to process
                    this.DrawWidget = typeof(IdentifyWidget);
                    this.DrawObject.IsEnabled = true;
                    this.DrawObject.DrawMode = DrawMode.Point;
                    if (this.MapControl != null) this.MapControl.Cursor = Cursors.Hand;
                    break;
            }
        }
        #endregion

        #region Override DockStateChange Event Handler - Rearrange Controls
        protected override void OnDockStateChange(TaskbarStateChangeEventArgs e)
        {
            if (e.DockState == DockPosition.NONE)
            {
                WidgetMenuButton.Visibility = Visibility.Visible;
                DockWidgetsStack.Visibility = Visibility.Collapsed;
            }
            else
            {
                WidgetMenuButton.Visibility = Visibility.Collapsed;
                DockWidgetsStack.Visibility = Visibility.Visible;
            }

            if (!IsDockable)
            {
                if (IsNailed)
                {
                    double top = InitialTop;
                    double left = InitialLeft;
                    switch (this.VerticalAlignment)
                    {
                        case VerticalAlignment.Top: top = 0; break;
                        case VerticalAlignment.Bottom: top = CanvasHeight - this.ActualHeight; break;
                        case VerticalAlignment.Center: top = (CanvasHeight - this.ActualHeight) / 2.0; break;
                    }

                    switch (this.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left: left = 0; break;
                        case HorizontalAlignment.Right: left = CanvasWidth - this.ActualWidth; break;
                        case HorizontalAlignment.Center: left = (CanvasWidth - this.ActualWidth) / 2.0; break;
                    }

                    Canvas.SetTop(this, top);
                    Canvas.SetLeft(this, left);
                }
                else
                {
                    RearrageControls(e.IsInUpperPage, e.DockState);
                }
            }
            else
            {
                if (IsNailed)
                {
                    Canvas.SetLeft(StackToolbar, CanvasWidth - 320);
                }
                else
                {
                    RearrageControls(e.IsInUpperPage, e.DockState);
                }
            }
        }

        private void RearrageControls(bool isInUpperPage, DockPosition dockState)
        {
            if (isInUpperPage)
            {
                if (dockState == DockPosition.NONE)
                {
                    Canvas.SetTop(StackTitleBar, 0);
                    Canvas.SetTop(StackToolbar, BarHeight - 20);
                    Canvas.SetLeft(StackToolbar, 75);
                    WidgetMenuButton.ContentPosition = ContentOrientation.DOWN;
                }
                else
                {
                    Canvas.SetTop(StackTitleBar, 0);
                    Canvas.SetTop(StackToolbar, DockHeight - 25);
                    Canvas.SetLeft(StackToolbar, CanvasWidth - 320);
                    if (WidgetManager.OverviewMap != null) WidgetManager.OverviewMap.Position = OverviewMapPosition.LowerRight;
                }

                if (ToolMenuButton != null) ToolMenuButton.ContentPosition = ContentOrientation.DOWN;
                if (BaseMenuButton != null) BaseMenuButton.ContentPosition = ContentOrientation.DOWN;
                if (LinkMenuButton != null) LinkMenuButton.ContentPosition = ContentOrientation.DOWN;
            }
            else
            {
                if (dockState == DockPosition.NONE)
                {
                    Canvas.SetTop(StackTitleBar, 16);
                    Canvas.SetTop(StackToolbar, -30);
                    Canvas.SetLeft(StackToolbar, 75);
                    WidgetMenuButton.ContentPosition = ContentOrientation.UP;
                }
                else
                {
                    Canvas.SetTop(StackTitleBar, 0);
                    Canvas.SetTop(StackToolbar, -25);
                    Canvas.SetLeft(StackToolbar, CanvasWidth - 320);
                    if (WidgetManager.OverviewMap != null) WidgetManager.OverviewMap.Position = OverviewMapPosition.UpperRight;
                }

                if (ToolMenuButton != null) ToolMenuButton.ContentPosition = ContentOrientation.UP;
                if (BaseMenuButton != null) BaseMenuButton.ContentPosition = ContentOrientation.UP;
                if (LinkMenuButton != null) LinkMenuButton.ContentPosition = ContentOrientation.UP;
            }
        }
        #endregion

        #region Initialize Widget Menus and Handle Widget Menu Buttons
        private void InitializeWidgetMenuBars()
        {
            if (AppConfig == null) return;

            WidgetMenuButton.Style = (this.CurrentApp.Resources.Contains("DropMenuButtonStyle")) ? (this.CurrentApp.Resources["DropMenuButtonStyle"] as Style) : (this.CurrentApp.Resources["defaultDropMenuButtonStyle"] as Style);

            foreach (WidgetConfig widget in AppConfig.WidgetsConfig)
            {
                ImageSource iconSource = new BitmapImage(new Uri(widget.IconSource, UriKind.Relative));
                ContextMenuItem menuItem = new ContextMenuItem() { Text = widget.Title, IconSource = iconSource, UseSmallIcon = true, Margin = new Thickness(1) };
                menuItem.Background = new SolidColorBrush(Colors.Transparent);
                WidgetMenuButton.MenuItems.Add(menuItem);

                HyperlinkButton linkButton = new HyperlinkButton() { Width = 40, Height = 40, Tag = widget.Title, Margin = new Thickness(1) };
                linkButton.Background = new ImageBrush() { ImageSource = iconSource, Stretch = Stretch.None };
                linkButton.Click += new RoutedEventHandler(WidgetLinkButton_Click);
                ToolTipService.SetToolTip(linkButton, widget.Title);
                DockWidgetsStack.Children.Add(linkButton);
            }
        }

        private void WidgetLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton)
            {
                string title = (string)(sender as HyperlinkButton).Tag;
                WidgetBase widget = WidgetManager.FindWidgetByTitle(title);
                if (widget != null) widget.IsOpen = true;
            }
        }

        private void WidgetMenuItem_Click(object sender, MenuItemClickEventArgs e)
        {
            if (sender is ContextMenuItem)
            {
                string title = (sender as ContextMenuItem).Text;
                WidgetBase widget = WidgetManager.FindWidgetByTitle(title);
                if (widget != null) widget.IsOpen = true;
            }
        }
        #endregion

        #region Initialize Controls - Toolbar and BaseMap Buttons
        protected override void InitializeControls()
        {
            if (AppConfig == null) return;
            if (AppConfig.TaskbarConfig == null) return;

            LogoImage.Style = (this.CurrentApp.Resources.Contains("LogoImageStyle")) ? (this.CurrentApp.Resources["LogoImageStyle"] as Style) : null;
            StackToolbar.Style = (this.CurrentApp.Resources.Contains("AppToolbarStyle")) ? (this.CurrentApp.Resources["AppToolbarStyle"] as Style) : (this.CurrentApp.Resources["defaultAppToolbarStyle"] as Style);

            // Initialize Tool Buttons
            if (AppConfig.TaskbarConfig.MapToolButtons.Length > 0)
            {
                ToolMenuButton = new SwitchMenuButton() { IsTabStop = true, MenuOpenMode = MenuOpenAction.MouseHover }; // Do not set Width and Height
                ToolMenuButton.Style = (this.CurrentApp.Resources.Contains("SwitchMenuButtonStyle")) ? (this.CurrentApp.Resources["SwitchMenuButtonStyle"] as Style) : (this.CurrentApp.Resources["defaultSwitchMenuButtonStyle"] as Style);
                ToolMenuButton.MenuButtonChange += new MenuButtonChangeEventHandler(ToolMenuButton_SelectedButtonChange);
                StackToolbar.Children.Add(ToolMenuButton);

                foreach (MapToolButton btnTool in AppConfig.TaskbarConfig.MapToolButtons)
                {
                    if (btnTool.IsDefault) this.toolMode = (MapActionMode)Enum.Parse(typeof(MapActionMode), btnTool.MapAction, true);
                    CircleMenuButton toolButton = new CircleMenuButton() { Width = 50, Height = 50, Text = btnTool.Title, Tag = btnTool.MapAction };
                    toolButton.ImageSource = new BitmapImage(new Uri(btnTool.ButtonImage, UriKind.Relative));
                    toolButton.IsSelected = btnTool.IsDefault;
                    ToolMenuButton.Items.Add(toolButton);
                }
            }

            // Initialize Base Map Toggle Button
            if (AppConfig.MapConfig.BaseMap != null)
            {
                LayerConfig[] lyrConfigs = null;
                BaseMapConfig baseConfig = AppConfig.MapConfig.BaseMap;

                if (baseConfig.EnableBase == ServiceSource.BING && baseConfig.BingBaseMap != null)
                {
                    lyrConfigs = baseConfig.BingBaseMap.Layers;
                }
                else if (baseConfig.ArcGISBaseMap != null)
                {
                    lyrConfigs = baseConfig.ArcGISBaseMap.Layers;
                }

                if ((lyrConfigs != null) && (lyrConfigs.Length > 1))
                {
                    BaseMenuButton = new SwitchMenuButton() { IsTabStop = true, MenuOpenMode = MenuOpenAction.MouseHover }; // Do not set Width and Height
                    BaseMenuButton.Style = (this.CurrentApp.Resources.Contains("SwitchMenuButtonStyle")) ? (this.CurrentApp.Resources["SwitchMenuButtonStyle"] as Style) : (this.CurrentApp.Resources["defaultSwitchMenuButtonStyle"] as Style);
                    BaseMenuButton.MenuButtonChange += new MenuButtonChangeEventHandler(BaseMenuButton_SelectedButtonChange);
                    StackToolbar.Children.Add(BaseMenuButton);

                    foreach (LayerConfig config in lyrConfigs)
                    {
                        SquareMenuButton baseButton = new SquareMenuButton() { Width = 50, Height = 50, Text = config.Title, Tag = config };
                        baseButton.ImageSource = new BitmapImage() { UriSource = new Uri(config.IconSource, UriKind.Relative) };
                        baseButton.IsSelected = config.VisibleInitial;
                        BaseMenuButton.Items.Add(baseButton);
                    }
                }
            }

            // Initialize Help Menu Button
            if (AppConfig.AppHelpMenu != null)
            {
                LinkMenuButton = new DropMenuButton() { Text = "Help", Width = 50, Height = 50, VerticalContentAlignment = System.Windows.VerticalAlignment.Bottom, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left };
                LinkMenuButton.Style = (this.CurrentApp.Resources.Contains("DropMenuButtonStyle")) ? (this.CurrentApp.Resources["DropMenuButtonStyle"] as Style) : (this.CurrentApp.Resources["defaultDropMenuButtonStyle"] as Style);
                LinkMenuButton.ImageSource = new BitmapImage(new Uri(AppConfig.AppHelpMenu.MenuIcon, UriKind.Relative));
                LinkMenuButton.MenuItemClick += new MenuItemClickEventHandler(LinkMenuButton_MenuItemClick);
                StackToolbar.Children.Add(LinkMenuButton);

                foreach (ApplicationLink link in AppConfig.AppHelpMenu.Links)
                {
                    ImageSource iconSource = new BitmapImage(new Uri(link.Icon, UriKind.Relative));
                    ContextMenuItem menuItem = new ContextMenuItem() { Text = link.Title, IconSource = iconSource, Tag = link.Url, UseSmallIcon = true, Margin = new Thickness(1) };
                    menuItem.Background = new SolidColorBrush(Colors.Transparent);
                    LinkMenuButton.MenuItems.Add(menuItem);
                }

                if (AppConfig.AppHelpMenu.About != null)
                {
                    ImageSource iconSource = new BitmapImage(new Uri(AppConfig.AppHelpMenu.About.Icon, UriKind.Relative));
                    ContextMenuItem menuItem = new ContextMenuItem() { Text = AppConfig.AppHelpMenu.About.Title, IconSource = iconSource, Tag = "About", UseSmallIcon = true, Margin = new Thickness(1) };
                    menuItem.Background = new SolidColorBrush(Colors.Transparent);
                    LinkMenuButton.MenuItems.Add(menuItem);
                }
            }
        }
        #endregion

        #region Handle Map Tool Button and Base Map Button Events
        private void ToolMenuButton_SelectedButtonChange(object sender, MenuButtonChangeEventArgs e)
        {
            if (e.NewButton != null)
            {
                toolMode = (MapActionMode)Enum.Parse(typeof(MapActionMode), (string)e.NewButton.Tag, true);
                this.ResetDrawObjectMode();
            }
        }

        private void BaseMenuButton_SelectedButtonChange(object sender, MenuButtonChangeEventArgs e)
        {
            if (e.NewButton != null && e.OldButton != null)
            {
                LayerConfig oldLayerConfig = (LayerConfig)e.OldButton.Tag;
                LayerConfig newLayerConfig = (LayerConfig)e.NewButton.Tag;

                this.MapControl.Layers[oldLayerConfig.ID].Visible = false;
                this.MapControl.Layers[newLayerConfig.ID].Visible = true;

                BaseMapConfig baseConfig = AppConfig.MapConfig.BaseMap;
                if (baseConfig.EnableBase == ServiceSource.ArcGIS && baseConfig.ArcGISBaseMap.LabelLayer != null)
                {
                    string layerID = AppConfig.MapConfig.BaseMap.ArcGISBaseMap.LabelLayer.ID;
                    this.MapControl.Layers[layerID].Visible = (newLayerConfig as ArcGISBaseMapLayer).ShowLabel;
                }

                EventCenter.DispatchBaseMapLayerChangeEvent(this, new BaseMapLayerChangeEventArgs(newLayerConfig.ID));
            }
        }

        private void LinkMenuButton_MenuItemClick(object sender, MenuItemClickEventArgs e)
        {
            string sUrl = (string)e.ItemTag;

            if (!string.IsNullOrEmpty(sUrl))
            {
                if (sUrl.Equals("About"))
                {
                    if (AboutWindow == null)
                    {
                        AboutWindow = new PopupWindow() { Title = "About", Background = this.Background, ShowArrow = false, IsResizable = false, IsFloatable = true };
                        StackPanel aboutContent = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(12, 12, 12, 12) };
                        AboutWindow.Content = aboutContent;

                        string[] contentLines = Regex.Split(AppConfig.AppHelpMenu.About.Text, @"\\n");
                        for (int i = 0; i < contentLines.Length; i++)
                        {
                            if (contentLines[i].Trim().StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
                            {
                                HyperlinkButton linkBlock = new HyperlinkButton() { NavigateUri = new Uri(contentLines[i].Trim(), UriKind.Absolute), Content = contentLines[i].Trim(), Margin = new Thickness(2), FontSize = 12.5, Foreground = new SolidColorBrush(Colors.Blue), HorizontalAlignment = System.Windows.HorizontalAlignment.Center, TargetName = "_blank" };
                                aboutContent.Children.Add(linkBlock);
                            }
                            else
                            {
                                TextBlock textBlock = new TextBlock() { Text = contentLines[i].Trim(), Margin = new Thickness(2), FontSize = 12.5, HorizontalAlignment = System.Windows.HorizontalAlignment.Center };
                                aboutContent.Children.Add(textBlock);
                            }
                        }
                    }

                    FloatingPopup.Show(AboutWindow);
                }
                else
                {
                    try
                    {
                        System.Windows.Browser.HtmlPopupWindowOptions winOptions = new System.Windows.Browser.HtmlPopupWindowOptions() { Resizeable = true, Width = 1000, Height = 800 };
                        System.Windows.Browser.HtmlWindow win = System.Windows.Browser.HtmlPage.PopupWindow(new Uri(sUrl, UriKind.Absolute), "_blank", winOptions);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        #endregion

        #region DrawObject DrawComplete Event Handler
        private void DrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (this.DrawWidget == this.GetType())
            {
                if (toolMode == MapActionMode.ZoomIn || pressedKey == Key.Shift)
                {
                    this.MapControl.ZoomTo(e.Geometry as ESRI.ArcGIS.Client.Geometry.Envelope);
                }
                else if (toolMode == MapActionMode.Zoomout || pressedKey == Key.Space)
                {
                    Envelope currentExtent = this.MapControl.Extent;

                    Envelope zoomBoxExtent = e.Geometry as Envelope;
                    MapPoint zoomBoxCenter = zoomBoxExtent.GetCenter();

                    double whRatioCurrent = currentExtent.Width / currentExtent.Height;
                    double whRatioZoomBox = zoomBoxExtent.Width / zoomBoxExtent.Height;

                    Envelope newEnv = null;

                    if (whRatioZoomBox > whRatioCurrent) // use width
                    {
                        double mapWidthPixels = this.MapControl.Width;
                        double multiplier = currentExtent.Width / zoomBoxExtent.Width;
                        double newWidthMapUnits = currentExtent.Width * multiplier;
                        newEnv = new Envelope(new MapPoint(zoomBoxCenter.X - (newWidthMapUnits / 2), zoomBoxCenter.Y), new MapPoint(zoomBoxCenter.X + (newWidthMapUnits / 2), zoomBoxCenter.Y));
                    }
                    else // use height
                    {
                        double mapHeightPixels = this.MapControl.Height;
                        double multiplier = currentExtent.Height / zoomBoxExtent.Height;
                        double newHeightMapUnits = currentExtent.Height * multiplier;
                        newEnv = new Envelope(new MapPoint(zoomBoxCenter.X, zoomBoxCenter.Y - (newHeightMapUnits / 2)), new MapPoint(zoomBoxCenter.X, zoomBoxCenter.Y + (newHeightMapUnits / 2)));
                    }

                    if (newEnv != null) this.MapControl.ZoomTo(newEnv);
                }
            }
        }
        #endregion
    }
}
