using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;

using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class MeasureWidget : WidgetBase
    {
        private const string MODE_AREA = "AREA";
        private const string MODE_COORD = "COORD";
        private const string MODE_LENGTH = "LENGTH";

        private string measurementMode = "";
        private MeasurementConfig widgetConfig = null;
        private GeometryService geometryService = null;
        private ScaleLine.ScaleLineUnit resultUnits = ScaleLine.ScaleLineUnit.Meters;

        public MeasureWidget()
        {
            InitializeComponent();
        }

        #region Override Function - Load Configuration and Clear Graphics
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            this.widgetConfig = (MeasurementConfig)MeasurementConfig.Deserialize(e.Result, typeof(MeasurementConfig));
            this.DrawObject.DrawComplete += new EventHandler<DrawEventArgs>(DrawObject_DrawComplete);

            geometryService = new GeometryService(this.AppConfig.GeometryService);
            geometryService.ProjectCompleted += new EventHandler<GraphicsEventArgs>(GeometryService_ProjectCompleted);
            geometryService.LengthsCompleted += (o, args) => { ShowMeasurementResult(true, (ScaleLine.ScaleLineUnit)args.UserState); };
            geometryService.AreasAndLengthsCompleted += (o, args) => { ShowMeasurementResult(true, (ScaleLine.ScaleLineUnit)args.UserState); };
            geometryService.Failed += (o, args) => { MessageBox.Show(args.Error.Message); };

            foreach (MeasurementUnits units in widgetConfig.LengthUnits)
            {
                this.ListLengthUnits.Items.Add(new ComboBoxItem() { Content = units.Name, Tag = units });
            }

            foreach (MeasurementUnits units in widgetConfig.AreaUnits)
            {
                this.ListAreaUnits.Items.Add(new ComboBoxItem() { Content = units.Name, Tag = units });
            }

            this.ListLengthUnits.SelectedIndex = 0;
            this.ListAreaUnits.SelectedIndex = 0;
        }

        protected override void OnClose()
        {
            this.ClearGraphics(-1);
        }
        #endregion

        #region Override Function - Reset DrawObject Mode
        /// <summary>
        /// Override ResetDrawObjectMode - IDrawObject has been removed
        /// </summary>
        public override void ResetDrawObjectMode()
        {
            if (!string.IsNullOrEmpty(measurementMode))
            {
                this.DrawWidget = this.GetType();
                this.DrawObject.IsEnabled = true;
                this.MapControl.Cursor = Cursors.Stylus;

                if (measurementMode == MODE_COORD)
                {
                    this.DrawObject.DrawMode = DrawMode.Point;
                }
                else
                {
                    if (CheckUseFreehand.IsChecked.Value)
                    {
                        this.DrawObject.DrawMode = DrawMode.Freehand;
                        this.DrawObject.LineSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_LINE] as LineSymbol;
                    }
                    else
                    {
                        switch (measurementMode)
                        {
                            case MODE_LENGTH:
                                this.DrawObject.DrawMode = DrawMode.Polyline;
                                this.DrawObject.LineSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_LINE] as LineSymbol; break;
                            case MODE_AREA:
                                this.DrawObject.DrawMode = DrawMode.Polygon;
                                this.DrawObject.FillSymbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol; break;
                        }
                    }
                }
            }
            else
            {
                WidgetManager.ResetDrawObject();
            }
        }
        #endregion

        #region Handle Radio Events to Choose a Measurement Tool
        private void MeasurementRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton)
            {
                RadioButton radio = sender as RadioButton;
                measurementMode = ((string)radio.Tag).ToUpper();

                TextLengthResult.Text = "";
                TextAreaResult.Text = "";

                if (measurementMode == MODE_COORD)
                {
                    CheckUseFreehand.IsEnabled = false;
                    ListAreaUnits.Visibility = Visibility.Collapsed;
                    ListLengthUnits.Visibility = Visibility.Collapsed;
                    TextAreaTitle.Visibility = Visibility.Visible;
                    TextAreaResult.Visibility = Visibility.Visible;
                    TextResultTitle.Text = "Point Coordinates";
                    TextLengthTitle.Text = "X:";
                    TextAreaTitle.Text = "Y:";
                }
                else
                {
                    CheckUseFreehand.IsEnabled = true;

                    if (measurementMode == MODE_LENGTH)
                    {
                        ListAreaUnits.Visibility = Visibility.Collapsed;
                        TextAreaTitle.Visibility = Visibility.Collapsed;
                        TextAreaResult.Visibility = Visibility.Collapsed;
                        ListLengthUnits.Visibility = Visibility.Visible;
                        TextResultTitle.Text = "Length Measurement Result";
                        TextLengthTitle.Text = "Total Length:";
                    }
                    else
                    {
                        ListLengthUnits.Visibility = Visibility.Visible;
                        ListAreaUnits.Visibility = Visibility.Visible;
                        TextAreaTitle.Visibility = Visibility.Visible;
                        TextAreaResult.Visibility = Visibility.Visible;
                        TextResultTitle.Text = "Area Measurement Result";
                        TextLengthTitle.Text = "Parimeter:";
                        TextAreaTitle.Text = "Total Area:";
                    }
                }

                this.ResetDrawObjectMode();
            }
        }

        private void CheckUseFreehand_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUseFreehand.IsChecked.Value)
            {
                IconMeasureLength.Source = new BitmapImage(new Uri("../Images/icons/i_draw_freeline.png", UriKind.Relative));
                IconMeasureArea.Source = new BitmapImage(new Uri("../Images/icons/i_draw_freepoly.png", UriKind.Relative));
            }
            else
            {
                IconMeasureLength.Source = new BitmapImage(new Uri("../Images/icons/i_draw_line.png", UriKind.Relative));
                IconMeasureArea.Source = new BitmapImage(new Uri("../Images/icons/i_draw_poly.png", UriKind.Relative));
            }

            this.ResetDrawObjectMode();
        }
        #endregion

        #region Handle DrawObject Event and Do Measurement
        private void DrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (this.DrawWidget == this.GetType() && e.Geometry != null)
            {
                if (this.measurementMode == MODE_COORD)
                {
                    MapPoint point = e.Geometry as MapPoint;

                    TextLengthResult.Text = string.Format("{0}", Math.Round(point.X, 6));
                    TextAreaResult.Text = string.Format("{0}", Math.Round(point.Y, 6));
                    Graphic dotGraphic = new Graphic() { Geometry = point, Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_MARKER] as MarkerSymbol };

                    //this.ClearGraphics(-1);
                    AddGraphic(dotGraphic);

                    if (CheckResultOnMap.IsChecked.Value)
                    {
                        string label = string.Format("X: {0}\nY: {1}", TextLengthResult.Text, TextAreaResult.Text);
                        Graphic lblGraphic = new Graphic() { Geometry = point, Symbol = new TextSymbol() { Text = label, OffsetX = 0, OffsetY = 30, FontSize = 11.0, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xFF)) } };
                        AddGraphic(lblGraphic);
                    }
                }
                else
                {
                    this.IsBusy = true;
                    Graphic graphic = null;
                    if (this.DrawObject.DrawMode == DrawMode.Freehand && this.measurementMode == MODE_AREA)
                    {
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
                        graphic.Geometry = e.Geometry;
                        switch (e.Geometry.GetType().Name)
                        {
                            case "MapPoint":
                                graphic.Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_MARKER] as MarkerSymbol; break;
                            case "Polyline":
                                graphic.Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_LINE] as LineSymbol; break;
                            case "Polygon":
                                graphic.Symbol = this.CurrentApp.Resources[SymbolResources.DRAW_FILL] as FillSymbol; break;
                        }
                    }

                    if (graphic != null)
                    {
                        //this.ClearGraphics(-1);
                        this.AddGraphic(graphic);
                        DoMesurement(graphic);
                    }
                }
            }
        }

        private void DoMesurement(Graphic graphic)
        {
            List<Graphic> graphicList = new List<Graphic>();
            graphicList.Add(graphic);

            if (widgetConfig.AlwaysProject || this.CurrentPage.MapUnits == ScaleLine.ScaleLineUnit.DecimalDegrees)
            {
                geometryService.ProjectAsync(graphicList, new SpatialReference(widgetConfig.ProjectToWKID));
            }
            else
            {
                switch (this.measurementMode)
                {
                    case MODE_LENGTH:
                        geometryService.LengthsAsync(graphicList, this.CurrentPage.MapUnits);
                        break;
                    case MODE_AREA:
                        geometryService.AreasAndLengthsAsync(graphicList, this.CurrentPage.MapUnits);
                        break;
                }
            }
        }
        #endregion

        #region Handle GeometryService Event and do Measurement
        private void GeometryService_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            ScaleLine.ScaleLineUnit units = UnitsHelper.GetUnitsByWKID(widgetConfig.ProjectToWKID);

            switch (this.measurementMode)
            {
                case MODE_LENGTH:
                    geometryService.LengthsAsync(e.Results, units);
                    break;
                case MODE_AREA:
                    geometryService.AreasAndLengthsAsync(e.Results, units);
                    break;
            }
        }
        #endregion
         
        #region Methods to Show Mearurement Results
        private void UnitsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowMeasurementResult(false, ScaleLine.ScaleLineUnit.Undefined);
        }

        private void ShowMeasurementResult(bool showLabel, ScaleLine.ScaleLineUnit outUnits)
        {
            showLabel = showLabel && CheckResultOnMap.IsChecked.Value;
            if (outUnits != ScaleLine.ScaleLineUnit.Undefined) resultUnits = outUnits;

            if (measurementMode == MODE_LENGTH && geometryService.LengthsLastResult != null)
            {
                // Length in meters
                double length = geometryService.LengthsLastResult[0] * UnitsHelper.GetConversionToMeter(resultUnits);

                // Length in selected units
                MeasurementUnits lenUnits = (ListLengthUnits.SelectedItem as ComboBoxItem).Tag as MeasurementUnits;
                TextLengthResult.Text = string.Format("{0}", Math.Round(length * lenUnits.Conversion, 3));

                if (showLabel && this.GraphicsLayer.Graphics.Count > 0)
                {
                    int k = this.GraphicsLayer.Graphics.Count - 1;
                    ESRI.ArcGIS.Client.Geometry.Geometry geometry = this.GraphicsLayer.Graphics[k].Geometry;
                    string label = string.Format("Length: {0} {1}", TextLengthResult.Text, lenUnits.Abbreviation);
                    Graphic lblGraphic = new Graphic() { Geometry = geometry.Extent.GetCenter(), Symbol = new TextSymbol() { Text = label, OffsetX = 30, OffsetY = 15, FontSize = 12.0, FontFamily = new FontFamily("Arial Black"), Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xFF)) } };
                    AddGraphic(lblGraphic);
                }
            }
            else if (measurementMode == MODE_AREA && geometryService.AreasAndLengthsLastResult != null)
            {
                // Length in meters
                double length = geometryService.AreasAndLengthsLastResult.Lengths[0] * UnitsHelper.GetConversionToMeter(resultUnits);
                // Area in square meters
                double area = Math.Abs(geometryService.AreasAndLengthsLastResult.Areas[0]) * Math.Pow(UnitsHelper.GetConversionToMeter(resultUnits), 2);

                // Length in selected units
                MeasurementUnits lenUnits = (ListLengthUnits.SelectedItem as ComboBoxItem).Tag as MeasurementUnits;
                TextLengthResult.Text = string.Format("{0}", Math.Round(length * lenUnits.Conversion, 3));

                // Area in selected units
                MeasurementUnits areaUnits = (ListAreaUnits.SelectedItem as ComboBoxItem).Tag as MeasurementUnits;
                TextAreaResult.Text = string.Format("{0}", Math.Round(area * areaUnits.Conversion, 3));

                if (showLabel && this.GraphicsLayer.Graphics.Count > 0)
                {
                    int k = this.GraphicsLayer.Graphics.Count - 1;
                    ESRI.ArcGIS.Client.Geometry.Geometry geometry = this.GraphicsLayer.Graphics[k].Geometry;
                    string label = string.Format("Parimeter: {0} {1}\nArea: {2} {3}", TextLengthResult.Text, lenUnits.Abbreviation, TextAreaResult.Text, areaUnits.Abbreviation);
                    Graphic lblGraphic = new Graphic() { Geometry = geometry.Extent.GetCenter(), Symbol = new TextSymbol() { Text = label, OffsetX = 50, OffsetY = 15, FontSize = 12.0, FontFamily = new FontFamily("Arial Black"), Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x99)) } };
                    AddGraphic(lblGraphic);
                }
            }

            this.IsBusy = false;
        }
        #endregion
    }
}
