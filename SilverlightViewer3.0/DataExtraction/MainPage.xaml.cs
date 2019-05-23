using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Collections.Generic;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

using ESRI.SilverlightViewer;
using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;
using DataExtraction.Config;
using DataExtraction.Utility;

namespace DataExtraction
{
    public partial class MainPage : WidgetBase
    {
        private int infoReadycount = 0;
        private bool clipByExtent = false;
        private bool exposeLayers = false;
        private bool needGeneralized = false;
        private string drawMode = "Rectangle"; // Default Draw Mode
        private List<GPString> layersToClip = null;
        private DataExtractionConfig widgetConfig = null;
        private GeometryService geometryService = null;
        private Geoprocessor gpService = null;

        public MainPage()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Configuration and Clear Graphics
        protected override void OnWidgetLoaded()
        {
            base.OnWidgetLoaded();

            this.gpService = new Geoprocessor();
            this.gpService.Failed += new EventHandler<TaskFailedEventArgs>(GPService_Failed);
            this.gpService.JobCompleted += new EventHandler<JobInfoEventArgs>(GPService_JobCompleted); //Asynchronous GP service
            this.gpService.ExecuteCompleted += new EventHandler<GPExecuteCompleteEventArgs>(GPService_ExecuteCompleted); //Synchronous GP service
            this.gpService.GetResultDataCompleted += new EventHandler<GPParameterEventArgs>(GPService_GetResultDataCompleted);

            this.geometryService = new GeometryService(this.AppConfig.GeometryService);
            this.geometryService.GeneralizeCompleted += new EventHandler<GraphicsEventArgs>(geometryService_GeneralizeCompleted);
            this.DrawObject.DrawComplete += new EventHandler<DrawEventArgs>(DrawObject_DrawComplete);
            this.layersToClip = new List<GPString>();
        }

        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (DataExtractionConfig)DataExtractionConfig.Deserialize(xmlConfig, typeof(DataExtractionConfig));

            if (widgetConfig != null)
            {
                this.IsBusy = true;
                this.clipByExtent = widgetConfig.AOISelectionMethod.Equals("extent", StringComparison.CurrentCultureIgnoreCase);

                if (clipByExtent)
                {
                    DrawModeButtonStack.Visibility = System.Windows.Visibility.Collapsed;
                    txtDrawModeStatus.Text = "The map extent is used as the area of interest";
                }

                foreach (DataExtractionService service in widgetConfig.DataExtractionServices)
                {
                    DataExtractionServiceInfo serviceInfo = new DataExtractionServiceInfo(service.restURL, OnDataExtractionServiceInfoReady);
                    ComboBoxItem serviceItem = new ComboBoxItem() { Tag = serviceInfo, Content = service.title };
                    lstExtractionService.Items.Add(serviceItem);
                }
            }
        }

        private void OnDataExtractionServiceInfoReady(object sender, DataExtraction.Utility.GPServiceInfoEventArgs args)
        {
            if (args.ServiceInfo != null)
            {
                infoReadycount++;
                if (infoReadycount == widgetConfig.DataExtractionServices.Length) this.IsBusy = false;
            }
        }

        public override void ClearGraphics(int newTab)
        {
            base.ClearGraphics(newTab);
            needGeneralized = false;
            txtJobErrorMessage.Visibility = Visibility.Collapsed;
        }

        protected override void OnClose()
        {
            ClearGraphics(0);
            base.OnClose();
        }
        #endregion

        #region Override Function - Reset DrawObject Mode
        protected override void OnSelectedContentChange(int newIndex)
        {
            base.OnSelectedContentChange(newIndex);
            this.ResetDrawObjectMode();
        }

        /// <summary>
        /// Override ResetDrawObjectMode - IDrawObject has been removed
        /// </summary>
        public override void ResetDrawObjectMode()
        {
            if (this.SelectedTabIndex == 0 && !clipByExtent)
            {
                this.DrawWidget = this.GetType();
                this.DrawObject.IsEnabled = true;
                this.MapControl.Cursor = Cursors.Arrow;

                switch (drawMode)
                {
                    case "Polygon":
                        this.DrawObject.DrawMode = DrawMode.Polygon;
                        this.DrawObject.FillSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol; break;
                    case "Freepoly":
                        this.DrawObject.DrawMode = DrawMode.Freehand;
                        this.DrawObject.LineSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_LINE] as LineSymbol; break;
                    case "Rectangle":
                        this.DrawObject.DrawMode = DrawMode.Rectangle;
                        this.DrawObject.FillSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol; break;
                    default:
                        this.DrawObject.IsEnabled = false; break;
                }
            }
            else
            {
                WidgetManager.ResetDrawObject();
            }
        }
        #endregion

        #region Select Extraction Service and Layers to Clip
        private void ExtrationService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem serviceItem = e.AddedItems[0] as ComboBoxItem;
            DataExtractionServiceInfo serviceInfo = serviceItem.Tag as DataExtractionServiceInfo;
            if (serviceInfo != null && serviceInfo.IsReady)
            {
                gpService.Url = serviceInfo.ServiceUrl;
                EsriGPExecutionType exeType = (EsriGPExecutionType)Enum.Parse(typeof(EsriGPExecutionType), serviceInfo.ExecutionType, true);

                foreach (GPServiceParameter gpParam in serviceInfo.Parameters)
                {
                    if (gpParam.Name == "Layers_to_Clip")
                    {
                        List<CheckBox> checkList = new List<CheckBox>();
                        foreach (string choice in gpParam.ChoiceList)
                        {
                            CheckBox check = new CheckBox() { Content = choice };
                            check.Click += ExtractionLayerCheckBox_Click;
                            checkList.Add(check);
                        }

                        lstLayersToClip.ItemsSource = checkList;
                        exposeLayers = true;
                    }
                    else if (gpParam.Name == "Feature_Format")
                    {
                        lstFeatureFormat.ItemsSource = gpParam.ChoiceList;
                        lstFeatureFormat.SelectedIndex = 0;
                    }
                    else if (gpParam.Name == "Raster_Format")
                    {
                        lstRasterformat.ItemsSource = gpParam.ChoiceList;
                        lstRasterformat.SelectedIndex = 0;
                    }
                }
            }
        }

        private void ExtractionLayerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            layersToClip.Clear();
            foreach (CheckBox check in lstLayersToClip.Items)
            {
                if (check.IsChecked.Value)
                {
                    layersToClip.Add(new GPString((string)check.Content, (string)check.Content));
                }
            }
        }
        #endregion

        #region Handle DrawObject and Create Area of Interest
        private void GeometryDrawMode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton)
            {
                foreach (HyperlinkButton button in DrawModeButtonStack.Children)
                {
                    if (button.Effect != null) { button.Effect = null; break; }
                }

                HyperlinkButton linkButton = sender as HyperlinkButton;
                linkButton.Effect = new DropShadowEffect() { Color = System.Windows.Media.Colors.Cyan, BlurRadius = 40, ShadowDepth = 0 };

                this.needGeneralized = false;
                this.drawMode = (string)linkButton.Tag;
                this.ResetDrawObjectMode();

                if (drawMode.StartsWith("Freepoly"))
                {
                    txtDrawModeStatus.Text = "Select by drawing a free polygon on the map.";
                }
                else
                {
                    txtDrawModeStatus.Text = string.Format("Select by drawing a {0} on the map.", this.drawMode.ToLower());
                }
            }
        }

        private void DrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (this.DrawWidget == this.GetType())
            {
                Graphic graphic = null;

                if (this.drawMode == "Freepoly")
                {
                    // Geometry needs to be simplified
                    needGeneralized = true;
                    // Create a Graphic with the newly closed Polygon
                    graphic = new ESRI.ArcGIS.Client.Graphic()
                    {
                        Geometry = GeometryTool.FreehandToPolygon(e.Geometry as Polyline),
                        Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol
                    };
                }
                else
                {
                    // Create a Graphic with the drawn geometry
                    graphic = new ESRI.ArcGIS.Client.Graphic();
                    graphic.Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol;
                    switch (e.Geometry.GetType().Name)
                    {
                        case "Envelope":
                            graphic.Geometry = GeometryTool.EnvelopeToPolygon(e.Geometry as Envelope); break;
                        case "Polygon":
                            graphic.Geometry = e.Geometry; break;
                    }
                }

                if (graphic != null)
                {
                    this.AddGraphic(graphic);
                }
            }
        }
        #endregion

        #region Create GP Params and Start Extraction
        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            if (clipByExtent)
            {
                GraphicCollection clipGraphics = new GraphicCollection();
                clipGraphics.Add(new Graphic() { Geometry = GeometryTool.EnvelopeToPolygon(this.MapControl.Extent) });
                CreateGPParamsAndExtract(clipGraphics);
            }
            else
            {
                if (needGeneralized)
                {
                    GeneralizeParameters genParam = new GeneralizeParameters() { DeviationUnit = LinearUnit.Meter, MaxDeviation = 10 };
                    geometryService.GeneralizeAsync(this.GraphicsLayer.Graphics, genParam);
                }
                else
                {
                    CreateGPParamsAndExtract(this.GraphicsLayer.Graphics);
                }
            }
        }

        private void geometryService_GeneralizeCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.Results != null && e.Results.Count > 0)
            {
                GraphicCollection clipGraphics = new GraphicCollection();
                foreach (Graphic graphic in e.Results)
                {
                    clipGraphics.Add(graphic);
                }

                CreateGPParamsAndExtract(clipGraphics);
            }
        }

        private void CreateGPParamsAndExtract(GraphicCollection clipGraphics)
        {
            FeatureSet clipFS = new FeatureSet();
            clipFS.GeometryType = GeometryType.Polygon;
            clipFS.SpatialReference = this.MapControl.SpatialReference;
            foreach (Graphic graphic in clipGraphics)
            {
                clipFS.Features.Add(graphic);
            }

            if (clipFS.Features.Count > 0)
            {
                List<GPParameter> gpParams = CreateGPParams(clipFS);

                if (gpParams.Count > 0)
                {
                    gpService.ProcessSpatialReference = this.MapControl.SpatialReference;
                    gpService.OutputSpatialReference = this.MapControl.SpatialReference;

                    this.IsBusy = true;
                    txtExtractionStatus.Visibility = Visibility.Visible;
                    gpService.SubmitJobAsync(gpParams);
                }
            }
        }

        private List<GPParameter> CreateGPParams(FeatureSet clipFS)
        {
            List<GPParameter> gpParams = new List<GPParameter>();
            
            if (exposeLayers)
            {
                if (layersToClip.Count > 0)
                {
                    gpParams.Add(new GPFeatureRecordSetLayer("Area_of_Interest", clipFS));
                    gpParams.Add(new GPMultiValue<GPString>("Layers_to_Clip", layersToClip));
                    gpParams.Add(new GPString("Feature_Format", (string)lstFeatureFormat.SelectedValue));
                    gpParams.Add(new GPString("Raster_Format", (string)lstRasterformat.SelectedValue));
                }
                else
                {
                    MessageBox.Show("Please select layers to clip");
                }
            }
            else
            {
                gpParams.Add(new GPFeatureRecordSetLayer("Area_of_Interest", clipFS));
                gpParams.Add(new GPString("Feature_Format", (string)lstFeatureFormat.SelectedValue));
                gpParams.Add(new GPString("Raster_Format", (string)lstRasterformat.SelectedValue));
            }

            return gpParams;
        }
        #endregion

        #region Precess Extraction GP Service Results
        private void GPService_ExecuteCompleted(object sender, GPExecuteCompleteEventArgs e)
        {
            this.IsBusy = false;
            this.ToggleWidgetContent(1);
            txtExtractionStatus.Visibility = Visibility.Collapsed;
            foreach (GPParameter gpParam in e.Results.OutParameters)
            {
                if (gpParam is GPDataFile)
                {
                    string dataUrl = (gpParam as GPDataFile).Url;
                    lnkExtractionOutput.NavigateUri = new Uri(dataUrl, UriKind.Absolute);
                    break;
                }
            }
        }

        private void GPService_JobCompleted(object sender, JobInfoEventArgs e)
        {
            this.IsBusy = false;
            this.ToggleWidgetContent(1);
            txtExtractionStatus.Visibility = Visibility.Collapsed;
            
            if (e.JobInfo.JobStatus == esriJobStatus.esriJobSucceeded)
            {
                lnkExtractionOutput.Visibility = Visibility.Visible;
                txtJobErrorMessage.Visibility = Visibility.Collapsed;
                gpService.GetResultDataAsync(e.JobInfo.JobId, "Output_Zip_File"); // parameterName
            }
            else
            {
                txtJobErrorMessage.Text = e.JobInfo.Messages[0].Description;
                txtJobErrorMessage.Visibility = Visibility.Visible;
                lnkExtractionOutput.Visibility = Visibility.Collapsed;
            }
        }

        private void GPService_GetResultDataCompleted(object sender, GPParameterEventArgs e)
        {
            if (e.Parameter is GPDataFile)
            {
                lnkExtractionOutput.Visibility = Visibility.Visible;
                txtJobErrorMessage.Visibility = Visibility.Collapsed;
                string dataUrl = (e.Parameter as GPDataFile).Url;
                lnkExtractionOutput.NavigateUri = new Uri(dataUrl, UriKind.Absolute);
            }
        }

        private void GPService_Failed(object sender, TaskFailedEventArgs e)
        {
            this.IsBusy = false;
            this.ToggleWidgetContent(1);
            txtJobErrorMessage.Text = e.Error.Message;
            txtJobErrorMessage.Visibility = Visibility.Visible;
            lnkExtractionOutput.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
