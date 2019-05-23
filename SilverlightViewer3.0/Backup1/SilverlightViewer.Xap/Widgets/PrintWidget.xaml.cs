/*
 * A lot of thanks to Dominique Broux for his contribution to this widget.
 * Most code in this widget sources from his code posted on ArcGIS.com
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Browser;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Printing;
using System.Diagnostics;
using System.Threading;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Generic;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;

using ESRI.ArcGIS.Client.Printing;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class PrintWidget : WidgetBase
    {
        // Keep trying while a page is being prepared
        // Try at most 7 times before a page is ready
        const int MAXIMUM_TRY = 7;

        private int tryCount = 0;
        private int totalPages = 1;
        private int currentPage = 0;
        private bool isPrinting = false;
        private bool isPageReady = false;
        private bool isProcessing = false;
        private bool isWidgetLoaded = false;
        private bool isAsynPrintService = false;
        private PrintWidgetConfig widgetConfig = null;
        private PrintTask printTask = null;

        public PrintWidget()
        {
            InitializeComponent();
        }

        #region Intialize Widget and Printable Contents
        protected override void OnWidgetLoaded()
        {
            base.OnWidgetLoaded();
            isWidgetLoaded = true;

            // Add JavaScript Block to Invoke GetPrintContent in MapApp
            HtmlElement pageHead = HtmlPage.Document.GetElementsByTagName("head")[0] as HtmlElement;
            HtmlElement scriptBlock = HtmlPage.Document.CreateElement("Script");
            scriptBlock.SetAttribute("type", "text/javascript");
            scriptBlock.SetProperty("text", CreatePageScript());
            pageHead.AppendChild(scriptBlock);

            //Initialize Map-Print Page Contents
            printMapPage.ViewerMap = this.MapControl;
            printMapPage.LegendItems = GetMapLayerLegendItems();
            printMapPage.OverviewMapLayer = WidgetManager.OverviewMap.myOverviewMap.Layer;
            printMapPage.PageTitle = this.AppConfig.ApplicationTitle;
            this.txtPrintMapTitle.Text = this.AppConfig.ApplicationTitle;
            this.txtExportMapTitle.Text = this.AppConfig.ApplicationTitle;

            //Initialize Data-Print Contents
            LoadWidgetWithFeatures();
            ClearIsolatedStorageFile();

            // Hook up events
            this.printMapPage.MapReady += new EventHandler<EventArgs>(PrintMapPage_MapReady);
            EventCenter.WidgetFeatureSetsChange += new WidgetFeatureSetsChangeEventHandler(OnWidgetFeatureSetsChange);
        }

        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (PrintWidgetConfig)PrintWidgetConfig.Deserialize(xmlConfig, typeof(PrintWidgetConfig));

            if (widgetConfig.ExportMapTaskUrl != "")
            {
                printTask = new PrintTask(widgetConfig.ExportMapTaskUrl);
                printTask.GetServiceInfoCompleted += new EventHandler<ServiceInfoEventArgs>(PrintTask_GetServiceInfoCompleted);
                printTask.GetServiceInfoAsync();
            }
            else
            {
                this.HeaderPanel.Children[0].Visibility = System.Windows.Visibility.Collapsed;
                this.PanelExportMap.Visibility = System.Windows.Visibility.Collapsed;
                this.ToggleWidgetContent(1);
            }
        }

        private string CreatePageScript()
        {
            string script = @"var isolatedFile = '';
                function getPrintContent() {
                    if (isolatedFile != '') {
                        var pluginObj = document.getElementById('" + HtmlPage.Plugin.Id + @"');
                        var scriptObj = pluginObj.content." + HtmlPage.Plugin.Id + @";
                        return scriptObj.GetPrintContent(isolatedFile); 
                    }
                    else return '';
                }";

            return script;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            if (isWidgetLoaded) UpdateMapContent();
            PrintingProgressPanel.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Event and Functions - Update Data-Print Page Content
        private void OnWidgetFeatureSetsChange(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CreatePrintButton.IsEnabled = false;
            LoadWidgetWithFeatures();
        }

        private void LoadWidgetWithFeatures()
        {
            StackWidgetCheck.Children.Clear();

            foreach (WidgetBase widget in WidgetManager.Widgets)
            {
                if (widget.FeatureSets.Count > 0)
                {
                    CheckBox check = new CheckBox() { Content = widget.Title, Tag = widget, Margin = new Thickness(4) };
                    check.Click += new RoutedEventHandler(WidgetCheckBox_Click);
                    StackWidgetCheck.Children.Add(check);
                }
            }
        }

        private void WidgetCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool enable = false;

            foreach (CheckBox check in StackWidgetCheck.Children)
            {
                if (check.IsChecked.Value)
                {
                    enable = true;
                    break;
                }
            }

            CreatePrintButton.IsEnabled = enable;
        }
        #endregion

        #region Manage IolatedStageFile and Create Printable Content
        private void ClearIsolatedStorageFile()
        {
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.DirectoryExists("SilverlightViewer"))
            {
                string[] fileNames = isf.GetFileNames("SilverlightViewer/*");
                foreach (string fileName in fileNames)
                {
                    try
                    {
                        isf.DeleteFile(string.Format("SilverlightViewer/{0}", fileName));
                    }
                    catch { }
                }
            }
            else
            {
                try
                {
                    isf.CreateDirectory("SilverlightViewer");
                }
                catch (IsolatedStorageException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void CreatePrintButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                string FileName = string.Format("SilverlightViewer/Print_{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm"));
                string contentTable = "";

                using (Stream isoStream = new IsolatedStorageFileStream(FileName, FileMode.Create, FileAccess.Write, isf))
                {
                    TextWriter htmlWriter = new StreamWriter(isoStream);
                    foreach (CheckBox check in StackWidgetCheck.Children)
                    {
                        if (check.IsChecked.Value)
                        {
                            contentTable = CreateContentTable(check.Tag as WidgetBase);
                            htmlWriter.WriteLine(contentTable);
                        }
                    }

                    htmlWriter.Close();
                }

                Uri docUri = HtmlPage.Document.DocumentUri;
                int length = docUri.LocalPath.IndexOf('/', 1);
                string appPath = (length > 0) ? docUri.LocalPath.Substring(0, length) : "";
                string printPageUri = string.Format("{0}://{1}:{2}{3}/print.htm", docUri.Scheme, docUri.DnsSafeHost, docUri.Port, appPath);

                HtmlPage.Window.Eval("isolatedFile = '" + FileName + "';");
                HtmlPopupWindowOptions winOptions = new HtmlPopupWindowOptions() { Resizeable = true, Scrollbars = true, Toolbar = true, Width = 600, Height = 700, Top = 5 };
                HtmlWindow win = HtmlPage.PopupWindow(new Uri(printPageUri, UriKind.Absolute), "_blank", winOptions);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string CreateContentTable(WidgetBase widget)
        {
            int k = 0;
            StringBuilder tableBuilder = new StringBuilder();

            foreach (GeoFeatureCollection fset in widget.FeatureSets)
            {
                tableBuilder.Append("<table border=1 style=\"width:100%\">");
                tableBuilder.Append(string.Format("<tr><td colspan=2 class=\"TableTitle\">{0}: {1}</td></tr>", widget.Title, fset.FeatureLayerName));

                k = 0;
                foreach (Graphic graphic in fset)
                {
                    tableBuilder.Append((k % 2 == 0) ? "<tr><td><table class=\"ValueTable\">" : "<td><table class=\"ValueTable\">");
                    foreach (string key in graphic.Attributes.Keys)
                    {
                        if (!key.StartsWith("Shape"))
                        {
                            tableBuilder.Append(string.Format("<tr><td><b>{0}: </b></td><td>{1}</td></tr>", key, graphic.Attributes[key]));
                        }
                    }

                    tableBuilder.Append((k % 2 == 0) ? "</table></td>" : "</table></td></tr>");
                    k++;
                }

                tableBuilder.Append("</table><br/>");
            }

            return tableBuilder.ToString();
        }
        #endregion

        #region Functions - Update and Print Map-Print Page Content
        private void UpdateMapContent()
        {
            // Refresh map and legend 
            printMapPage.CloneMap();
            printMapPage.LegendItems = GetMapLayerLegendItems();
        }

        private void UpdateMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UpdateMapContent();
        }

        private void PrintMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create new a new PrintDocument object 
            PrintDocument printDoc = new PrintDocument();

            printDoc.BeginPrint += new EventHandler<BeginPrintEventArgs>(BeginPrint);
            printDoc.PrintPage += new EventHandler<PrintPageEventArgs>(PrintPage);
            printDoc.EndPrint += new EventHandler<EndPrintEventArgs>(EndPrint);

            // Give the document a name that's displayed in the printer queue    
            printDoc.Print("Silverlight Viewer - Print Map");
        }

        private void BeginPrint(object sender, BeginPrintEventArgs e)
        {
            tryCount = 0;
            currentPage = 0;
            isPrinting = false;
            isProcessing = false;
            PrintingPageText.Text = "Start";
            PrintingProgressPanel.Visibility = Visibility.Visible;
            Debug.WriteLine("Start to print");
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            e.PageVisual = null;

            if (!isPrinting)
            {
                isPrinting = true;
                totalPages = printMapPage.PreparePages(e.PrintableArea);
                PrintingProgress.Maximum = totalPages * MAXIMUM_TRY;
                PrintingPageText.Text = "Resizing Map";
            }

            if (printMapPage.IsMapResizing)
            {
                tryCount++;
                isPageReady = false;
                e.HasMorePages = true;

                Debug.WriteLine("Please wait - Resizing the map");
                Thread.Sleep(1000 + 200 * (tryCount + 1)); // sleep to give a chance to load layers before the maximum tries
            }
            else
            {
                if (!isPageReady)
                {
                    if (isProcessing) // Waiting for loaded layers
                    {
                        tryCount++;
                        if (tryCount > MAXIMUM_TRY)
                        {
                            Debug.WriteLine("Last try : Print as it is");
                            isPageReady = true;
                            isProcessing = false;
                            e.HasMorePages = true;
                            return;
                        }
                        else
                        {
                            // Wait for page is loaded
                            // Force to retry later
                            PrintingProgress.Value = currentPage * tryCount;
                            Debug.WriteLine("Still in progress. Has tried {0} times", tryCount);
                            Thread.Sleep(1000 + 200 * (tryCount + 1)); // sleep to give a chance to load layers before the maximum tries
                            e.HasMorePages = true;
                            return;
                        }
                    }
                    else
                    {
                        currentPage++;
                        if (currentPage <= totalPages)
                        {
                            Debug.WriteLine("Prepare page {0}", currentPage);
                            PrintingPageText.Text = string.Format("Prepare page {0}", currentPage);
                            printMapPage.SetPrintExtent(currentPage); // Prepare next page
                            e.HasMorePages = true; // Recall this function to print other pages
                            isProcessing = true;  // Keep trying until next page is ready
                            tryCount = 0;
                            return;
                        }
                        else
                        {
                            // No more valid page
                            e.HasMorePages = false;
                            Thread.Sleep(500); // without this, it turns out that sometimes the print never ends (EndPrint not getting called). Happens often first print of the session.
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Printing page {0}", currentPage);
                    PrintingPageText.Text = string.Format("{0} / {1} {2}", currentPage, totalPages, (totalPages > 1) ? "Pages" : "Page");
                    PrintingProgress.Value = currentPage * MAXIMUM_TRY;
                    e.HasMorePages = currentPage < totalPages;
                    e.PageVisual = this.printMapPage;
                    isProcessing = false;
                    isPageReady = false;
                    return;
                }
            }
        }

        private void EndPrint(object sender, EndPrintEventArgs e)
        {
            isPrinting = false;
            PrintingPageText.Text = "Done!";
            PrintingProgress.Value = PrintingProgress.Maximum;
            //PrintingProgressPanel.Visibility = Visibility.Collapsed;
            Debug.WriteLine("All pages are printed");
        }
        #endregion

        #region Event Handlers - Change Map-Print Page Contents
        private void PrintMapPage_MapReady(object sender, EventArgs e)
        {
            isPageReady = true;
        }

        private void CheckFitToPage_Click(object sender, RoutedEventArgs e)
        {
            printMapPage.FitIntoPage = checkFitToPage.IsChecked.Value;
        }

        private void CheckMaintainScale_Click(object sender, RoutedEventArgs e)
        {
            printMapPage.MaintainScale = checkKeepScale.IsChecked.Value;
        }

        private void CheckRotateMap_Click(object sender, RoutedEventArgs e)
        {
            printMapPage.RotateMap = checkRotateMap.IsChecked.Value;
        }

        private void CheckPrintLegend_Click(object sender, RoutedEventArgs e)
        {
            printMapPage.PrintLegend = checkPrintLegend.IsChecked.Value;
        }

        private void CheckPrintOverview_Click(object sender, RoutedEventArgs e)
        {
            printMapPage.PrintOverview = checkPrintOverview.IsChecked.Value;
        }

        private void PrintMapTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            printMapPage.PageTitle = txtPrintMapTitle.Text;
        }
        #endregion

        #region Update Legend Items
        private List<LegendItemInfo> GetMapLayerLegendItems()
        {
            List<LegendItemInfo> legendItems = new List<LegendItemInfo>();
            TOCWidget tocWidget = WidgetManager.FindWidgetByType(typeof(TOCWidget)) as TOCWidget;

            if (tocWidget != null)
            {
                // Map Service Nodes
                foreach (object item in tocWidget.MapContentTree.Items)
                {
                    if (item is TreeViewItem)
                    {
                        TreeViewItem treeNode = item as TreeViewItem;
                        CheckBox check = treeNode.Header as CheckBox;
                        if (check.IsChecked.Value)
                        {
                            if (treeNode.ItemsSource != null)
                            {
                                foreach (LegendItemInfo legItem in (IEnumerable<LegendItemInfo>)treeNode.ItemsSource)
                                {
                                    legendItems.Add(legItem);
                                }
                            }
                            else if (treeNode.HasItems)
                            {
                                GetSubLayerLegendiItems(treeNode, legendItems);
                            }
                        }
                    }
                }
            }

            return legendItems;
        }

        private void GetSubLayerLegendiItems(TreeViewItem treeNode, List<LegendItemInfo> legendItems)
        {
            bool isChecked = true; // Layers in a Cached Service is always true
            string layerName = "";
            string itemLabel = "";

            if (treeNode.Header is CheckBox)
            {
                CheckBox checkBox = treeNode.Header as CheckBox;
                isChecked = checkBox.IsChecked.Value && checkBox.IsEnabled;
                layerName = checkBox.Content as string;
            }
            else if (treeNode.Header is RadioButton)
            {
                RadioButton radioBox = treeNode.Header as RadioButton;
                isChecked = radioBox.IsChecked.Value && radioBox.IsEnabled;
                layerName = radioBox.Content as string;
            }

            if (isChecked)
            {
                if (treeNode.ItemsSource != null)
                {
                    foreach (LegendItemInfo legItem in (List<LegendItemInfo>)treeNode.ItemsSource)
                    {
                        if (legItem.ImageSource != null)
                        {
                            itemLabel = (((List<LegendItemInfo>)treeNode.ItemsSource).Count == 1) ? layerName : legItem.Label;
                            legendItems.Add(new LegendItemInfo() { ImageSource = legItem.ImageSource, Label = itemLabel });
                        }
                    }
                }
                else if (treeNode.HasItems)
                {
                    foreach (object item in treeNode.Items)
                    {
                        if (item is TreeViewItem)
                        {
                            TreeViewItem itemNode = item as TreeViewItem;
                            GetSubLayerLegendiItems(itemNode, legendItems);
                        }
                    }
                }
            }
        }
        #endregion

        #region ArcGIS 10.1 Print Service Functions
        private void PrintTask_GetServiceInfoCompleted(object sender, ServiceInfoEventArgs e)
        {
            if (e.ServiceInfo != null)
            {
                isAsynPrintService = e.ServiceInfo.IsServiceAsynchronous;

                if (e.ServiceInfo.LayoutTemplates != null)
                {
                    foreach (string template in e.ServiceInfo.LayoutTemplates)
                    {
                        boxLayoutTemplates.Items.Add(template);
                    }

                    boxLayoutTemplates.SelectedIndex = 0;
                }

                if (e.ServiceInfo.Formats != null)
                {
                    foreach (string format in e.ServiceInfo.Formats)
                    {
                        boxExportFormats.Items.Add(format);
                    }

                    boxExportFormats.SelectedIndex = 0;
                }

                if (isAsynPrintService)
                {
                    printTask.JobCompleted += new EventHandler<PrintJobEventArgs>(PrintTask_JobCompleted);
                }
                else
                {
                    printTask.ExecuteCompleted += new EventHandler<PrintEventArgs>(PrintTask_ExecuteCompleted);
                }
            }
        }

        private void ExportMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsBusy = true;
            linkExportResult.Visibility = System.Windows.Visibility.Collapsed;
            PrintParameters printParams = new PrintParameters(this.MapControl);
            printParams.Format = (string)boxExportFormats.SelectedItem;

            ExportOptions expOptions = new ExportOptions() { Dpi = 96 };
            expOptions.OutputSize = new Size(this.MapControl.ActualWidth, this.MapControl.ActualHeight);

            LegendOptions legendOptions = new LegendOptions();
            List<LegendLayer> legendLayers = new List<LegendLayer>();
            foreach (LivingMapLayer layer in this.AppConfig.MapConfig.LivingMaps)
            {
                if (this.MapControl.Layers[layer.ID] is ArcGISDynamicMapServiceLayer)
                {
                    int[] visibleLayers = (this.MapControl.Layers[layer.ID] as ArcGISDynamicMapServiceLayer).VisibleLayers;
                    List<object> layerIDs = new List<object>();
                    foreach (int lyrID in visibleLayers) layerIDs.Add(lyrID);
                    legendLayers.Add(new LegendLayer() { LayerId = layer.ID, SubLayerIds = layerIDs });
                }
                else
                {
                    legendLayers.Add(new LegendLayer() { LayerId = layer.ID });
                }
            }

            legendOptions.LegendLayers = legendLayers;
            ScaleBarOptions scaleOptions = new ScaleBarOptions();
            string metricUnits = (boxMetricUnits.SelectedItem as ComboBoxItem).Content as string;
            string nonmetricUnits = (boxNonmetricUnits.SelectedItem as ComboBoxItem).Content as string;
            scaleOptions.MetricUnit = (ScaleBarOptions.MetricUnits)Enum.Parse(typeof(ScaleBarOptions.MetricUnits), metricUnits, true);
            scaleOptions.MetricLabel = txtMetricLabel.Text;
            scaleOptions.NonMetricUnit = (ScaleBarOptions.NonMetricUnits)Enum.Parse(typeof(ScaleBarOptions.NonMetricUnits), nonmetricUnits, true);
            scaleOptions.NonMetricLabel = txtNonmetricLabel.Text;

            LayoutOptions layoutOptions = new LayoutOptions() { LegendOptions = legendOptions, ScaleBarOptions = scaleOptions };
            layoutOptions.Title = txtExportMapTitle.Text;
            layoutOptions.Copyright = txtCopyright.Text;

            printParams.ExportOptions = expOptions;
            printParams.LayoutOptions = layoutOptions;
            printParams.LayoutTemplate = (string)boxLayoutTemplates.SelectedItem;

            if (isAsynPrintService)
            {
                printTask.SubmitJobAsync(printParams);
            }
            else
            {
                printTask.ExecuteAsync(printParams);
            }
        }

        private void NonmetricUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (boxNonmetricUnits != null && boxNonmetricUnits.SelectedItem != null)
            {
                txtNonmetricLabel.Text = (boxNonmetricUnits.SelectedItem as ComboBoxItem).Content as string;
            }
        }

        private void MetricUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (boxMetricUnits != null && boxMetricUnits.SelectedItem != null)
            {
                txtMetricLabel.Text = (boxMetricUnits.SelectedItem as ComboBoxItem).Content as string;
            }
        }

        private void PrintTask_ExecuteCompleted(object sender, PrintEventArgs e)
        {
            this.IsBusy = false;
            if (e.PrintResult != null)
            {
                linkExportResult.NavigateUri = e.PrintResult.Url;
                linkExportResult.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void PrintTask_JobCompleted(object sender, PrintJobEventArgs e)
        {
            this.IsBusy = false;
            if (e.PrintResult != null)
            {
                linkExportResult.NavigateUri = e.PrintResult.Url;
                linkExportResult.Visibility = System.Windows.Visibility.Visible;
            }
        }
        #endregion
    }
}
