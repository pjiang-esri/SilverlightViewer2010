using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

using ESRI.SilverlightViewer;
using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;
using ChartingWidget.Config;

namespace ChartingWidget
{
    public partial class MainPage : WidgetBase
    {
        private const int MAXIMUM_RETURN = 10;

        private QueryTool queryTool = null;
        private ElementLayer elementLayer = null;
        private ChartingConfig widgetConfig = null;

        public MainPage()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Config and Clear ElementLayer
        protected override void OnWidgetLoaded()
        {
            base.OnWidgetLoaded();

            queryTool = new QueryTool();
            queryTool.ResultReady += new QueryTool.QueryResultReady(Query_ResultReady);

            elementLayer = new ElementLayer();
            this.MapControl.Layers.Add(elementLayer);
        }

        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            widgetConfig = (ChartingConfig)ChartingConfig.Deserialize(xmlConfig, typeof(ChartingConfig));

            if (widgetConfig != null && widgetConfig.ChartLayers != null)
            {
                ChartQueryLayer[] chartLayers = widgetConfig.ChartLayers;

                for (int i = 0; i < chartLayers.Length; i++)
                {
                    ArcGISLayerInfo layerInfo = new ArcGISLayerInfo(chartLayers[i].RESTURL);
                    lstAttQueryLayer.Items.Add(new ComboBoxItem() { Content = chartLayers[i].Title, Tag = layerInfo });
                }
            }
        }

        /// <summary>
        /// Clear Charts from the ElementLayer 
        /// </summary>
        /// <param name="newTab">The index of the tab to display</param>
        public override void ClearGraphics(int newTab)
        {
            base.ClearGraphics(newTab);
            elementLayer.Children.Clear();
            LeftResultStack.Children.Clear();
            RightChartStack.Children.Clear();
        }

        protected override void OnClose()
        {
            this.ClearGraphics(0);
            base.OnClose();
        }
        #endregion

        #region UI Element Event Handlers - Select features by an attribute value
        private void SearchLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtAttQueryWhere.Text = "ST_ABBREV='KY' and NAME LIKE 'J%'";
            lstAttQueryField.Items.Clear();

            ComboBoxItem item = lstAttQueryLayer.SelectedItem as ComboBoxItem;
            ArcGISLayerInfo layerInfo = item.Tag as ArcGISLayerInfo;
            ChartQueryLayer queryLayer = widgetConfig.ChartLayers[lstAttQueryLayer.SelectedIndex];

            if (layerInfo.IsReady)
            {
                string fieldType = "";

                if (!string.IsNullOrEmpty(queryLayer.QueryFields))
                {
                    string[] queryFields = queryLayer.QueryFields.Split(',');
                    foreach (ArcGISLayerField field in layerInfo.Fields)
                    {
                        if (queryFields.Contains(field.Name, StringComparer.CurrentCultureIgnoreCase))
                        {
                            fieldType = field.Type.Substring(13); // "esriFieldType".Length
                            lstAttQueryField.Items.Add(new ListBoxItem() { Content = string.Format("{0} ({1})", field.Name, fieldType), Tag = field, Height = 20 });
                        }
                    }
                }
                else
                {
                    foreach (ArcGISLayerField field in layerInfo.Fields)
                    {
                        fieldType = field.Type.Substring(13); // "esriFieldType".Length
                        if (!fieldType.Equals("Geometry") && !fieldType.Equals("Raster") && !fieldType.Equals("Blob"))
                        {
                            lstAttQueryField.Items.Add(new ListBoxItem() { Content = string.Format("{0} ({1})", field.Name, fieldType), Tag = field, Height = 20 });
                        }
                    }
                }
            }
        }

        private void SearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            ListBoxItem item = e.AddedItems[0] as ListBoxItem;

            if (item.Tag != null)
            {
                ArcGISLayerField queryField = item.Tag as ArcGISLayerField;
                txtAttQueryWhere.Text = txtAttQueryWhere.Text.Insert(txtAttQueryWhere.SelectionStart, queryField.Name + " ");
                txtAttQueryWhere.SelectionStart = txtAttQueryWhere.Text.Length;
                txtAttQueryWhere.Focus();

                EsriFieldType fieldType = (EsriFieldType)Enum.Parse(typeof(EsriFieldType), queryField.Type, false);
                switch (fieldType)
                {
                    case EsriFieldType.esriFieldTypeOID:
                    case EsriFieldType.esriFieldTypeGUID:
                    case EsriFieldType.esriFieldTypeGlobalID:
                    case EsriFieldType.esriFieldTypeSmallInteger:
                    case EsriFieldType.esriFieldTypeInteger:
                    case EsriFieldType.esriFieldTypeSingle:
                    case EsriFieldType.esriFieldTypeDouble:
                        btnOperIs.IsEnabled = false;
                        btnOperNot.IsEnabled = false;
                        btnOperNULL.IsEnabled = false;
                        btnOperLike.IsEnabled = false;
                        break;
                    default:
                        btnOperIs.IsEnabled = true;
                        btnOperNot.IsEnabled = true;
                        btnOperNULL.IsEnabled = true;
                        btnOperLike.IsEnabled = true;
                        break;
                }
            }
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string sqlText = txtAttQueryWhere.Text;

            if (button.Content.Equals("Like"))
            {
                txtAttQueryWhere.Text = sqlText.Insert(txtAttQueryWhere.SelectionStart, "LIKE '%[value]%' ");
            }
            else
            {
                txtAttQueryWhere.Text = sqlText.Insert(txtAttQueryWhere.SelectionStart, button.Content.ToString().ToUpper() + " ");
            }

            txtAttQueryWhere.SelectionStart = txtAttQueryWhere.Text.Length;
            txtAttQueryWhere.Focus();
        }

        private void WhereClearButton_Click(object sender, RoutedEventArgs e)
        {
            txtAttQueryWhere.Text = "";
            lstAttQueryField.SelectedIndex = -1;
        }

        private void SubmitQueryButton_Click(object sender, RoutedEventArgs e)
        {
            string sWhere = txtAttQueryWhere.Text.Trim();
            ChartQueryLayer queryLayer = widgetConfig.ChartLayers[lstAttQueryLayer.SelectedIndex];

            if (!string.IsNullOrEmpty(sWhere))
            {
                this.IsBusy = true;
                queryTool.QueryLayer = queryLayer;
                queryTool.LayerInfo = (lstAttQueryLayer.SelectedItem as ComboBoxItem).Tag as ArcGISLayerInfo;
                queryTool.Search(sWhere, queryLayer.ChartOutput.IndependentField, this.MapSRWKID);
            }
        }
        #endregion

        #region Process Query Results after QueryTask is completed
        private void Query_ResultReady(object sender, QueryResultEventArgs args)
        {
            this.ClearGraphics(1); // Toggle to Results Panel
            this.FeatureSets.Clear();

            if (args.Results != null && args.Results.Features.Count > 0)
            {
                ChartResultMessage.Visibility = Visibility.Collapsed;

                FeatureSet fset = args.Results;
                this.GraphicsTipTemplate = args.QueryLayer.MapTipTemplate;
                CreateSeriesChart(fset, args.QueryLayer as ChartQueryLayer);
                this.FeatureSets.Add(new GeoFeatureCollection(fset, args.QueryLayer.Title));
                BindDataToGrid(fset);
            }
            else
            {
                ChartResultMessage.Visibility = Visibility.Visible;
                ChartResultMessage.Text = string.Format("Sorry! {0}", (string.IsNullOrEmpty(args.ErrorMsg)) ? "No features are found." : args.ErrorMsg);
            }

            this.IsBusy = false;
        }

        private void BindDataToGrid(FeatureSet fset)
        {
            int limit = Math.Min(MAXIMUM_RETURN, fset.Features.Count);
            SimpleLinkButton valueLink = null;
            string displayValue = "";

            for (int i = 0; i < limit; i++)
            {
                Graphic graphic = fset.Features[i];
                displayValue = (string)graphic.Attributes[fset.DisplayFieldName];
                valueLink = new SimpleLinkButton() { Tag = graphic, Cursor = Cursors.Hand, Margin = new Thickness(1, 1, 1, 1) };
                valueLink.Text = (string.IsNullOrEmpty(displayValue)) ? "[Empty]" : displayValue;
                valueLink.Click += new RoutedEventHandler(LeftFeatureLink_Click);
                LeftResultStack.Children.Add(valueLink);
            }
        }

        private void LeftFeatureLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is SimpleLinkButton)
            {
                StackPanel leftStack = ChartResultGrid.LeftWindow as StackPanel;
                SimpleLinkButton leftLinkOld = leftStack.Tag as SimpleLinkButton;
                if (leftLinkOld != null) leftLinkOld.IsActive = false;

                SimpleLinkButton leftLinkNew = sender as SimpleLinkButton;
                leftStack.Tag = leftLinkNew;
                leftLinkNew.IsActive = true;

                Graphic graphic = (Graphic)leftLinkNew.Tag;
                graphic.Symbol = ChooseSymbol(graphic.Geometry.GetType().Name);
                CreateElementChart(graphic, leftLinkNew.Text);
                this.GraphicsLayer.Graphics.Clear();
                this.AddGraphic(graphic);
            }
        }

        private Symbol ChooseSymbol(string geoType)
        {
            Symbol symbol = null;

            switch (geoType)
            {
                case "Point":
                case "MapPoint":
                case "MultiPoint": symbol = this.CurrentApp.Resources[SymbolResources.SIMPLE_MARKER] as SimpleMarkerSymbol; break;
                case "Polyline": symbol = this.CurrentApp.Resources[SymbolResources.SIMPLE_LINE] as SimpleLineSymbol; break;
                case "Polygon": symbol = this.CurrentApp.Resources[SymbolResources.SIMPLE_FILL] as SimpleFillSymbol; break;
            }

            return symbol;
        }
        #endregion

        #region Create Mutiple Series Chart - Data series are grouped by independent value
        /// <summary>
        /// Create a mutiple series Bar Chart - Data are grouped by independent value
        /// This function is not used in this widget
        /// </summary>
        private void CreateGroupBarChart(FeatureSet fset, string[] outFields, string[] outLabels, string title, string independentField, string independentAxisTitle, string dependentAxisTitle)
        {
            int index = 0;
            int limit = Math.Min(MAXIMUM_RETURN, fset.Features.Count);
            double chartValue = 0.0;
            string chartKey = "";

            Chart chart = new Chart() { Title = title, MinWidth = 500, MinHeight = 300, MaxWidth = 600, MaxHeight = 500 };
            chart.BorderBrush = new SolidColorBrush(Colors.Transparent);

            RangeAxis dependentAxis = new LinearAxis() { Orientation = AxisOrientation.X, Location = AxisLocation.Top, ShowGridLines = true, Title = dependentAxisTitle };
            CategoryAxis independentAxis = new CategoryAxis() { Orientation = AxisOrientation.Y, Location = AxisLocation.Left, Title = independentAxisTitle, Margin = new Thickness(0, 0, 2, 0) };

            foreach (string field in outFields)
            {
                Collection<KeyValuePair<string, double>> chartData = new Collection<KeyValuePair<string, double>>();

                for (int i = 0; i < limit; i++)
                {
                    Graphic graphic = fset.Features[i];
                    chartKey = (string)graphic.Attributes[independentField];
                    // The purpose of this line is to make labels shorter, you have to remove it if you use other data
                    //chartKey = (chartKey == null) ? "Unknown" : chartKey.Remove(chartKey.Length - "County".Length - 1);

                    chartValue = (graphic.Attributes[field] == null) ? 0.0 : double.Parse(graphic.Attributes[field].ToString());
                    KeyValuePair<string, double> kv = new KeyValuePair<string, double>(chartKey, chartValue);
                    chartData.Add(kv);
                }

                BarSeries barSeries = new BarSeries();
                barSeries.Title = outLabels[index];
                barSeries.ItemsSource = chartData;
                barSeries.IndependentValuePath = "Key";
                barSeries.DependentValuePath = "Value";
                barSeries.IndependentAxis = independentAxis;
                barSeries.DependentRangeAxis = dependentAxis;
                chart.Series.Add(barSeries);
                index++;
            }

            RightChartStack.Children.Add(chart);
        }

        /// <summary>
        /// Create a mutiple series Column Chart - Data are grouped by independent value
        /// This function is not used in this widget
        /// </summary>
        private void CreateGroupColumnChart(FeatureSet fset, string[] outFields, string[] outLabels, string title, string independentField, string independentAxisTitle, string dependentAxisTitle)
        {
            int index = 0;
            int limit = Math.Min(MAXIMUM_RETURN, fset.Features.Count);
            double chartValue = 0.0;
            string chartKey = "";

            Chart chart = new Chart() { Title = title, MinWidth = 500, MinHeight = 300, MaxWidth = 600, MaxHeight = 500 };
            chart.BorderBrush = new SolidColorBrush(Colors.Transparent);
           
            RangeAxis dependentAxis = new LinearAxis() { Orientation = AxisOrientation.Y, Location = AxisLocation.Left, ShowGridLines = true, Title = dependentAxisTitle };
            CategoryAxis independentAxis = new CategoryAxis() { Orientation = AxisOrientation.X, Location = AxisLocation.Bottom, Title = independentAxisTitle, Margin = new Thickness(0, 0, 2, 0) };
            independentAxis.AxisLabelStyle = this.Resources["ChartXAxisLabelStyle"] as Style;

            foreach (string field in outFields)
            {
                Collection<KeyValuePair<string, double>> chartData = new Collection<KeyValuePair<string, double>>();

                for (int i = 0; i < limit; i++)
                {
                    Graphic graphic = fset.Features[i];
                    chartKey = (string)graphic.Attributes[independentField];
                    // The purpose of this line is to make labels shorter, you have to remove it if you use other data
                    //chartKey = (chartKey == null) ? "Unknown" : chartKey.Remove(chartKey.Length - "County".Length - 1);

                    chartValue = (graphic.Attributes[field] == null) ? 0.0 : double.Parse(graphic.Attributes[field].ToString());
                    KeyValuePair<string, double> kv = new KeyValuePair<string, double>(chartKey, chartValue);
                    chartData.Add(kv);
                }

                ColumnSeries colSeries = new ColumnSeries();
                colSeries.Title = outLabels[index];
                colSeries.ItemsSource = chartData;
                colSeries.IndependentValuePath = "Key";
                colSeries.DependentValuePath = "Value";
                colSeries.IndependentAxis = independentAxis;
                colSeries.DependentRangeAxis = dependentAxis;
                chart.Series.Add(colSeries);
                index++;
            }

            RightChartStack.Children.Add(chart);
        }
        #endregion

        #region Create Series Chart for each independent and output it to the Result Window
        private void CreateSeriesChart(FeatureSet fset, ChartQueryLayer chartLayer)
        {
            if (chartLayer.OutputFields != null)
            {
                string[] outFields = chartLayer.OutputFields.Split(',');
                string[] outLabels = (string.IsNullOrEmpty(chartLayer.OutputLabels)) ? outFields : chartLayer.OutputLabels.Split(',');

                bool isPercentDependent = chartLayer.ChartOutput.DependentIsPercentage;
                string dependentAxisTitle = chartLayer.ChartOutput.DependentAxisTitle;
                string independentAxisTitle = chartLayer.ChartOutput.IndependentAxisTitle;
                string independentField = chartLayer.ChartOutput.IndependentField;

                switch (chartLayer.ChartOutput.ChartType)
                {
                    case "Bar":
                        CreateBarSeriesChart(fset, "Bar", outFields, outLabels, chartLayer.Title, independentField, dependentAxisTitle);
                        //CreateGroupBarChart(fset, outFields, outLabels, chartLayer.Title, independentField, independentAxisTitle, dependentAxisTitle);
                        break;
                    case "Column":
                        //CreateBarSeriesChart(fset, "Column", outFields, outLabels, chartLayer.Title, independentField, dependentAxisTitle);
                        CreateGroupColumnChart(fset, outFields, outLabels, chartLayer.Title, independentField, independentAxisTitle, dependentAxisTitle);
                        break;
                    case "Pie":
                        CreatePieSeriesChart(fset, outFields, outLabels, chartLayer.Title, independentField, dependentAxisTitle, isPercentDependent);
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please enter Output Fields for the widget in the configuration file");
            }
        }

 
        /// <summary>
        /// Create a Bar or Column Series Chart for all independent values - One chart for one independent value
        /// </summary>
        private void CreateBarSeriesChart(FeatureSet fset, string chartType, string[] outFields, string[] outLabels, string title, string independentField, string dependentAxisTitle)
        {
            int limit = Math.Min(MAXIMUM_RETURN, fset.Features.Count);

            for (int i = 0; i < limit; i++)
            {
                Graphic graphic = fset.Features[i];
                string chartTitle = string.Format("{0} - {1}", graphic.Attributes[independentField], title);
                Chart chart = CreateSingleBarChart(graphic, chartType, outFields, outLabels, chartTitle, dependentAxisTitle);
                RightChartStack.Children.Add(chart);
            }
        }

        /// <summary>
        /// Create Pie Series Charts for all independent values - One chart for one independent value
        /// </summary
        private void CreatePieSeriesChart(FeatureSet fset, string[] outFields, string[] outLabels, string title, string independentField, string dependentAxisTitle, bool isPercentage)
        {
            int limit = Math.Min(MAXIMUM_RETURN, fset.Features.Count);

            for (int i = 0; i < limit; i++)
            {
                Graphic graphic = fset.Features[i];
                string chartTitle = string.Format("{0} - {1}", graphic.Attributes[independentField], title);
                Chart chart = CreateSinglePieChart(graphic, outFields, outLabels, chartTitle, dependentAxisTitle, isPercentage);
                RightChartStack.Children.Add(chart);
            }
        }
        #endregion

        #region Create a single Chart for an independent and output to a map element layer
        /// <summary>
        /// Create element chart and add it into an Elememt Layer in the Map
        /// </summary>
        private void CreateElementChart(Graphic graphic, string displayValue)
        {
            ChartQueryLayer chartLayer = queryTool.QueryLayer as ChartQueryLayer;
            string[] outFields = chartLayer.OutputFields.Split(',');
            string[] outLabels = chartLayer.OutputLabels.Split(',');

            string title = string.Format("{0} - {1}", displayValue, chartLayer.Title);
            bool isPercentDependent = chartLayer.ChartOutput.DependentIsPercentage;
            string dependentAxisTitle = chartLayer.ChartOutput.DependentAxisTitle;

            Chart chart = null;
            switch (chartLayer.ChartOutput.ChartType)
            {
                case "Bar":
                case "Column":
                    chart = CreateSingleBarChart(graphic, chartLayer.ChartOutput.ChartType, outFields, outLabels, title, dependentAxisTitle);
                    break;
                case "Pie":
                    chart = CreateSinglePieChart(graphic, outFields, outLabels, title, dependentAxisTitle, isPercentDependent);
                    chart.Background = new SolidColorBrush(Color.FromArgb(204, 240, 240, 255));
                    break;
            }

            Envelope ext = GeometryTool.ExpandGeometryExtent(graphic.Geometry, 0.10);
            ElementLayer.SetEnvelope(chart, ext);
            elementLayer.Children.Clear();
            elementLayer.Children.Add(chart);
            this.MapControl.ZoomTo(ext);
        }

        /// <summary>
        /// Create Individual Bar or Column Chart
        /// </summary>
        private Chart CreateSingleBarChart(Graphic graphic, string chartType, string[] outFields, string[] outLabels, string title, string dependentAxisTitle)
        {
            double chartValue = 0.0;
            Collection<double> chartData = new Collection<double>();

            foreach (string field in outFields)
            {
                chartValue = (graphic.Attributes[field] == null) ? 0.0 : double.Parse(graphic.Attributes[field].ToString());
                chartData.Add(chartValue);
            }

            Chart chart = new Chart() { Title = title, MinWidth = 300, MinHeight = 200, MaxWidth = 400, MaxHeight = 300 };
            chart.Background = new SolidColorBrush(Color.FromArgb(204, 240, 240, 255));
            chart.BorderBrush = new SolidColorBrush(Colors.LightGray);

            switch (chartType)
            {
                case "Bar":
                    BarSeries barSeries = new BarSeries();
                    barSeries.Title = dependentAxisTitle;
                    barSeries.ItemsSource = chartData;
                    barSeries.DependentRangeAxis = new LinearAxis() { Orientation = AxisOrientation.X, Location = AxisLocation.Top, ShowGridLines = true, Title = dependentAxisTitle };
                    chart.Series.Add(barSeries);
                    break;
                case "Column":
                    ColumnSeries colSeries = new ColumnSeries();
                    colSeries.Title = dependentAxisTitle;
                    colSeries.ItemsSource = chartData;
                    colSeries.DependentRangeAxis = new LinearAxis() { Orientation = AxisOrientation.Y, Location = AxisLocation.Left, ShowGridLines = true, Title = dependentAxisTitle };
                    chart.Series.Add(colSeries);
                    break;
            }

            int index = 1;
            foreach (string label in outLabels)
            {
                chart.Series[0].LegendItems.Add(string.Format("{0} - {1}", index++, label));
            }

            return chart;
        }

        /// <summary>
        /// Create Individual Pie Chart
        /// </summary>
        private Chart CreateSinglePieChart(Graphic graphic, string[] outFields, string[] outLabels, string title, string dependentAxisTitle, bool isPercentage)
        {
            Chart chart = new Chart() { Title = title, MinWidth = 300, MinHeight = 200, MaxWidth = 400, MaxHeight = 300 };
            chart.BorderBrush = new SolidColorBrush(Colors.Transparent);

            int index = 0;
            double chartValue = 0.0;
            double totalValue = 0.0;

            Collection<KeyValuePair<string, double>> chartData = new Collection<KeyValuePair<string, double>>();

            foreach (string field in outFields)
            {
                chartValue = (graphic.Attributes[field] == null) ? 0.0 : double.Parse(graphic.Attributes[field].ToString());
                KeyValuePair<string, double> kv = new KeyValuePair<string, double>(outLabels[index], chartValue);
                totalValue += chartValue;
                chartData.Add(kv);
                index++;
            }

            if (isPercentage && totalValue < 100.0)
            {
                KeyValuePair<string, double> kv = new KeyValuePair<string, double>("Other", 100.0 - totalValue);
                chartData.Add(kv);
            }

            PieSeries pieSeries = new PieSeries();
            pieSeries.ItemsSource = chartData;
            pieSeries.IndependentValuePath = "Key";
            pieSeries.DependentValuePath = "Value";
            pieSeries.Title = dependentAxisTitle;
            chart.Series.Add(pieSeries);

            return chart;
        }
        #endregion
    }
}
