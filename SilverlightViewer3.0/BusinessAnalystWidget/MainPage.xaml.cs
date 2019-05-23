using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.BAO;
using ESRI.ArcGIS.Client.BACore;
using ESRI.ArcGIS.Client.BAO.Tasks;
using ESRI.ArcGIS.Client.BAO.Tasks.Reports;
using ESRI.ArcGIS.Client.BAO.Tasks.Authentication;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Controls;
using BusinessAnalystWidget.Config;

namespace BusinessAnalystWidget
{
    public partial class MainPage : WidgetBase
    {
        private BusinessAnalystConfig widgetConfig = null;
        private GeometryService geometryService = null;
        private string locationName = "";
        private string addressValue = "";

        public MainPage()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Configuration and Clear Graphics
        protected override void OnWidgetLoaded()
        {
            base.OnWidgetLoaded();

            LoadWidgetWithSelection();

            EventCenter.WidgetSelectedGraphicChange += new WidgetSelectedGraphicChangeEventHandler(OnWidgetSelectedGraphicChange);
        }

        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (BusinessAnalystConfig)BusinessAnalystConfig.Deserialize(xmlConfig, typeof(BusinessAnalystConfig));

            if (widgetConfig != null)
            {
                if (!string.IsNullOrEmpty(this.AppConfig.GeometryService))
                {
                    geometryService = new GeometryService(this.AppConfig.GeometryService);
                    geometryService.BufferCompleted += new EventHandler<GraphicsEventArgs>(GeometryService_BufferCompleted);
                }
            }
        }

        protected override void OnClose()
        {
            ClearGraphics(0);
            base.OnClose();
        }
        #endregion

        #region Listen to the SelectedGraphicChange event of other widgets
        private void OnWidgetSelectedGraphicChange(object sender, SelectedItemChangeEventArgs args)
        {
            if (sender != this)
            {
                LoadWidgetWithSelection();
            }
        }

        private void LoadWidgetWithSelection()
        {
            lstGraphicWidget.Items.Clear();

            foreach (WidgetBase widget in WidgetManager.Widgets)
            {
                if (widget.Title != this.Title && widget.HasGraphics && widget.SelectedGraphic != null)
                {
                    lstGraphicWidget.Items.Add(widget.Title);
                }
            }
        }
        #endregion

        #region Prepare Parameters for Demographic Analysis Using BAO API
        private void SubmitReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (lstGraphicWidget.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a widget with selected graphic", "Message", MessageBoxButton.OK);
                return;
            }

            double bufferDistance = 0;
            if (!double.TryParse(txtBufferDistance.Text, out bufferDistance))
            {
                MessageBox.Show("Buffer distance is invalid. Please input a valid number", "Message", MessageBoxButton.OK);
                return;
            }

            string widgetTitle = (string)lstGraphicWidget.SelectedItem;
            WidgetBase widget = WidgetManager.FindWidgetByTitle(widgetTitle);

            Graphic centerGraphic = widget.SelectedGraphic;
            GeoFeatureCollection dataset = widget.FindFeatureSetWithSelection();
            string displayField = (dataset != null) ? dataset.DisplayFieldName : "";
            string featureLayer = (dataset != null) ? dataset.FeatureLayerName : "";

            if (centerGraphic.Attributes.ContainsKey("Address")) //Locator
            {
                addressValue = (string)centerGraphic.Attributes["Address"];
            }
            else if (centerGraphic.Attributes.ContainsKey(displayField))
            {
                addressValue = (string)centerGraphic.Attributes[displayField];
            }

            if (bufferDistance > 0)
            {
                this.IsBusy = true;
                string bufUnits = (string)(lstBufferUnits.SelectedItem as ComboBoxItem).Content;
                locationName = string.Format("{0} {1} around {2}", txtBufferDistance.Text, bufUnits.ToLower(), featureLayer);
                CreateBufferZone(bufferDistance, centerGraphic, bufUnits, locationName);
            }
            else if (centerGraphic.Geometry is Polygon)
            {
                this.IsBusy = true;
                centerGraphic.Geometry.SpatialReference = new SpatialReference(this.MapSRWKID);
                locationName = string.Format("Within {0}", featureLayer);
                centerGraphic.Attributes.Add("AREA_ID", locationName);
                AuthenticateUser(centerGraphic);
            }
        }

        private void CreateBufferZone(double bufferDistance, Graphic graphic, string units, string locationName)
        {
            if (geometryService != null)
            {
                int bufferSRWKID = widgetConfig.ProjectToWKID;
                BufferParameters bufParams = new BufferParameters();
                bufParams.BufferSpatialReference = new SpatialReference((bufferSRWKID == 0) ? 102003 : bufferSRWKID);
                bufParams.OutSpatialReference = new SpatialReference(this.MapSRWKID);
                bufParams.Distances.Add(bufferDistance);
                bufParams.UnionResults = true;

                // Make the geometry's Spatial Reference consistent with Map's
                // QueryTask returns 3857 instead of 102100 when BING layers are used in the base map 
                graphic.Geometry.SpatialReference = new SpatialReference(this.MapSRWKID);

                if (RadioBufferCenter.IsChecked.Value && !(graphic.Geometry is MapPoint))
                {
                    MapPoint center = graphic.Geometry.Extent.GetCenter();
                    center.SpatialReference = new SpatialReference(this.MapSRWKID);
                    Graphic graphic1 = new Graphic();
                    graphic1.Geometry = center;

                    bufParams.Features.Add(graphic1);
                }
                else
                {
                    bufParams.Features.Add(graphic);
                }

                switch (units)
                {
                    case "Feet": bufParams.Unit = LinearUnit.Foot; break;
                    case "Yards": bufParams.Unit = LinearUnit.SurveyYard; break;
                    case "Miles": bufParams.Unit = LinearUnit.StatuteMile; break;
                    case "Meters": bufParams.Unit = LinearUnit.Meter; break;
                    default: bufParams.Unit = LinearUnit.StatuteMile; break;
                }

                geometryService.BufferAsync(bufParams);
            }
            else
            {
                MessageBox.Show("Geometry Service must be configurated to create a buffer zone.");
            }
        }

        private void GeometryService_BufferCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.Results.Count > 0)
            {
                this.ClearGraphics(-1);
                Graphic graphic = e.Results[0];
                graphic.Attributes.Add("AREA_ID", locationName);
                graphic.Symbol = this.CurrentApp.Resources[ESRI.SilverlightViewer.SymbolResources.SIMPLE_FILL] as SimpleFillSymbol;
                this.AddGraphic(graphic);
                this.ZoomToGraphics();

                AuthenticateUser(graphic);
            }
        }
        #endregion

        #region Authorize BAO User and Create Demographic Report Using BAO API
        private void AuthenticateUser(Graphic graphic)
        {
            AuthenticationTask authTask = new AuthenticationTask();

            //  Wire up async event handlers
            authTask.Failed += new EventHandler<TaskFailedEventArgs>(BAOTask_Failed);

            //  Execute this code block when the async response returns successfully
            authTask.ExecuteCompleted += (object sender, AuthenticationEventArgs args) =>
            {
                if (graphic != null)
                {
                    FeatureSet fset = new FeatureSet();
                    fset.Features.Add(graphic);
                    fset.GeometryType = GeometryType.Polygon;
                    fset.SpatialReference = graphic.Geometry.SpatialReference;
                    CreateSummaryReports(fset, args.Token);
                }
            };

            if (widgetConfig != null)
            {
                authTask.ExecuteAsync(widgetConfig.Username, widgetConfig.Password, "User Authorization");
            }
        }

        void BAOTask_Failed(object sender, TaskFailedEventArgs args)
        {
            this.IsBusy = false;
            BAReportMessage.Visibility = Visibility.Visible;
            BAReportMessage.Text = string.Format("{0}: {1}", args.UserState, args.Error.Message);
            ToggleWidgetContent(1);
        }

        /// <summary>
        /// Submit the Summary Reports service request.
        /// </summary>
        /// <param name="inputFS">Input FeatureSet representing the area to analyze</param>
        public void CreateSummaryReports(FeatureSet inputFS, string token)
        {
            //  Package FeatureSet as a Boundaries object
            Boundaries analysisBoundaries = new Boundaries(inputFS);

            //  We only need the output report.  We don't need the output geometry
            //  because we already have it on the client.

            SummaryReportsParameters summaryReportsParams = new SummaryReportsParameters();

            summaryReportsParams.Boundaries = analysisBoundaries;

            //  See GetReportTemplatesTask for a list of reports names and their IDs.
            ComboBoxItem selectedComboBoxItem = ReportNameComboBox.SelectedItem as ComboBoxItem;
            string templateID = selectedComboBoxItem.Tag as string;

            if (templateID != "")
            {
                //  One or more reports can be specified
                ReportOptions reportOptions = new ReportOptions();
                reportOptions.Format = widgetConfig.ReportFormat;
                reportOptions.TemplateName = templateID;

                //  Customizing report headers
                reportOptions.Header.Subtitle = "Generated with ESRI Business Analyst Online on " + DateTime.Now.ToString();
                reportOptions.Header.CustomHeaders.Add("Address", string.Format("{0} '{1}'", locationName, addressValue));
                summaryReportsParams.ReportOptions.Add(reportOptions);
                summaryReportsParams.OutputTypes.GetReport = true;
            }

            SummaryReportsTask summaryReportsTask = new SummaryReportsTask();
            summaryReportsTask.Token = token;

            //  Wire up the event handlers and submit the async request
            summaryReportsTask.Failed += new EventHandler<TaskFailedEventArgs>(BAOTask_Failed);

            //  Execute this code block when the async response returns successfully
            summaryReportsTask.ExecuteCompleted += (object task_sender, SummaryReportsEventArgs task_args) =>
            {
                this.IsBusy = false;
                this.ToggleWidgetContent(1);
                MapControl.IsEnabled = true;
                BAReportMessage.Visibility = Visibility.Visible;

                //  Grab the output report URL, if available, for the task arguments and wire up
                //  the status display to double as a report hyperlink.
                string reportURL = (task_args.TaskResultOutput.Reports != null && task_args.TaskResultOutput.Reports.Length > 0) ? task_args.TaskResultOutput.Reports[0].Url : null;
                if (reportURL != null)
                {
                    //  Update UI status
                    BAReportMessage.Text = "Thanks for choosing ESRI BAO!\n" + selectedComboBoxItem.Content as string + " analysis is completed.";
                    ReportDownloadLink.NavigateUri = new Uri(reportURL);
                }
                else
                {
                    BAReportMessage.Text = "Sorry! Analysis Complete, but failed to create a report";
                }
            };

            //  Disable map
            MapControl.IsEnabled = false;

            //  Submit the asynchronous request
            summaryReportsTask.ExecuteAsync(summaryReportsParams, "Create Report");
        }
        #endregion
    }
}
