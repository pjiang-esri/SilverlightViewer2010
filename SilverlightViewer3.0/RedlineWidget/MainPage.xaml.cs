using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel; //gre for checkboxes

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;
using RedlineWidget.Config;

namespace RedlineWidget
{
    public partial class MainPage : WidgetBase
    {
        // Config file information
        private RedlineConfig _widgetConfig = null;

        // collection of checkboxes that represent the layers in the Selected QueryGroup
        private ObservableCollection<GraphicCheckbox> _graphicCheckBoxes = new ObservableCollection<GraphicCheckbox>();

        // collection of radiobuttons that define Drawmode
        private ObservableCollection<DrawModeRadioButton> _drawModeRadiobuttons = new ObservableCollection<DrawModeRadioButton>();

        // collection of radiobuttons that define Color
        private ObservableCollection<RadioButton> _colorRadiobuttons = new ObservableCollection<RadioButton>();

        // collection of radiobuttons that define Size
        private ObservableCollection<RadioButton> _sizeRadiobuttons = new ObservableCollection<RadioButton>();

        // collection of radiobuttons that define Width
        private ObservableCollection<RadioButton> _widthRadiobuttons = new ObservableCollection<RadioButton>();

        private bool _isDrawingText = false;
        private double _anchorX = 0;
        private double _anchorY = 0;

        private bool _isDrawingMeasured = false;
        private string _drawMeasureMode = string.Empty;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.DrawObject.DrawComplete += new EventHandler<DrawEventArgs>(DrawObject_DrawComplete);
        }

        #region Override Functions - Load Widget Configuration
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string xmlConfig = e.Result;
            _widgetConfig = (RedlineConfig)RedlineConfig.Deserialize(xmlConfig, typeof(RedlineConfig));

            if (_widgetConfig == null)
            {
                MessageBox.Show("Missing Config File", this.GetType().ToString(), MessageBoxButton.OK);
                return;
            }

            foreach (RedlineMeasurementUnits units in _widgetConfig.LengthUnits)
            {
                //cboLengthUnits.Items.Add(new ComboBoxItem() { Content = units.Name, Tag = units });
                cboWidthUnits.Items.Add(new ComboBoxItem() { Content = units.Name, Tag = units });
                cboHeightUnits.Items.Add(new ComboBoxItem() { Content = units.Name, Tag = units });
                cboLengthUnits.Items.Add(new ComboBoxItem() { Content = units.Name, Tag = units });
            }

            //cboLengthUnits.SelectedIndex = 0;
            cboWidthUnits.SelectedIndex = 0;
            cboHeightUnits.SelectedIndex = 0;
            cboLengthUnits.SelectedIndex = 0;

            //DRAWMODE
            _drawModeRadiobuttons.Add(new DrawModeRadioButton() { Name = "Point", Checked = true });
            _drawModeRadiobuttons.Add(new DrawModeRadioButton() { Name = "Line", Checked = false });
            _drawModeRadiobuttons.Add(new DrawModeRadioButton() { Name = "Polygon", Checked = false });
            _drawModeRadiobuttons.Add(new DrawModeRadioButton() { Name = "Circle", Checked = false });
            _drawModeRadiobuttons.Add(new DrawModeRadioButton() { Name = "Triangle", Checked = false });
            _drawModeRadiobuttons.Add(new DrawModeRadioButton() { Name = "Rectangle", Checked = false });
            _drawModeRadiobuttons.Add(new DrawModeRadioButton() { Name = "Text", Checked = false });

            //_drawModeRadiobuttons.Add(new DrawModeRadioButton() { Name = "Freehand", Checked = false });

            // Bind ObservableCollections to DrawMode Radiobuttons
            lbDrawMode.ItemsSource = _drawModeRadiobuttons;

            cboColor.ItemsSource = new ColorList().List;
            cboColor.SelectedIndex = 1;
            cboColor_M.ItemsSource = cboColor.ItemsSource;
            cboColor_M.SelectedIndex = cboColor.SelectedIndex;

            cboFill.ItemsSource = new ColorList().List;
            cboFill.SelectedIndex = 0;
            cboFill_M.ItemsSource = cboFill.ItemsSource;
            cboFill_M.SelectedIndex = cboFill.SelectedIndex;

            cboSize.ItemsSource = new String[] { "6", "8", "10", "12", "14", "16", "18" };
            cboSize.SelectedIndex = 2;

            cboWidth.ItemsSource = new String[] { "1", "2", "3", "4", "5" };
            cboWidth.SelectedIndex = 0;
            cboWidth_M.ItemsSource = cboWidth.ItemsSource;
            cboWidth_M.SelectedIndex = cboWidth.SelectedIndex;

            // Bind ObservableCollections to Graphic CheckBoxes
            lbGraphics.ItemsSource = _graphicCheckBoxes;
        }

        public override void ClearGraphics(int newTab)
        {
            base.ClearGraphics(newTab);
            _graphicCheckBoxes.Clear();
        }

        protected override void OnClose()
        {
            this.ClearGraphics(-1);
        }

        #endregion

        #region Override ResetDrawObjectMode
        public override void ResetDrawObjectMode()
        {
            this.DrawWidget = this.GetType();
            this.DrawObject.IsEnabled = true;
            this.DrawObject.DrawMode = DrawMode.Point;
            this.MapControl.Cursor = Cursors.Arrow;
        }
        #endregion

        #region Handle DrawObject DrawComplete Event
        private void DrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (this.DrawWidget != this.GetType()) return;

            try
            {
                Graphic graphic = null;
                String prefix = "";

                MapPoint mapPoint;

                Envelope envelope = null;
                Polygon polygon = new Polygon();
                ESRI.ArcGIS.Client.Geometry.PointCollection pointCollection = new ESRI.ArcGIS.Client.Geometry.PointCollection();

                if (e.Geometry == null)
                {
                    MessageBox.Show("Click and Drag the mouse to define the shape.");
                    return;
                }

                if (this.DrawObject.DrawMode == DrawMode.Point)
                {
                    prefix = "Point-";
                    mapPoint = e.Geometry as MapPoint;
                    graphic = new Graphic()
                    {
                        Geometry = mapPoint,
                        Symbol = this.ChooseSymbol(e.Geometry.GetType().Name)
                    };

                    double[] dimension = this.GetTextDimension(graphic);
                    if (dimension != null)
                    {
                        //MessageBox.Show("Width = " + dimension[0].ToString() + "\nHeight = " + dimension[1].ToString() + "\n" + _anchorX.ToString() + " " + _anchorY.ToString());
                    }

                    if (_isDrawingText == true)
                    {
                        prefix = "Text-";
                        // need to get text size into mapunits
                        mapPoint.X = mapPoint.X - dimension[0] * _anchorX; // x is subtract
                        mapPoint.Y = mapPoint.Y + dimension[1] * _anchorY; // y is add
                        graphic = new Graphic()
                        {
                            Geometry = mapPoint,
                            Symbol = this.ChooseSymbol(e.Geometry.GetType().Name)
                        };
                    }

                    if (_isDrawingMeasured == true)
                    {
                        prefix = "Measured-";
                        graphic = this.CreateMeasuredGraphic(mapPoint);
                        if (graphic == null)
                        {
                            return;
                        }

                        Symbol symb = this.ChooseSymbol(graphic.Geometry.GetType().Name, true);
                        graphic.Symbol = symb;
                    }

                }
                else if (this.DrawObject.DrawMode == DrawMode.Polyline)
                {
                    prefix = "Line-";
                    graphic = new Graphic()
                    {
                        Geometry = e.Geometry as Polyline,
                        Symbol = this.ChooseSymbol(e.Geometry.GetType().Name)
                    };
                }
                else if (this.DrawObject.DrawMode == DrawMode.Polygon)
                {
                    prefix = "Polygon-";
                    graphic = new Graphic()
                    {
                        Geometry = e.Geometry as Polygon,
                        Symbol = this.ChooseSymbol(e.Geometry.GetType().Name)
                    };
                }
                else if (this.DrawObject.DrawMode == DrawMode.Circle)
                {
                    prefix = "Circle-";
                    graphic = new Graphic()
                    {
                        Geometry = e.Geometry as Polygon,
                        Symbol = this.ChooseSymbol(e.Geometry.GetType().Name)
                    };
                }
                else if (this.DrawObject.DrawMode == DrawMode.Triangle)
                {
                    prefix = "Triangle-";
                    graphic = new Graphic()
                    {
                        Geometry = e.Geometry as Polygon,
                        Symbol = this.ChooseSymbol(e.Geometry.GetType().Name)
                    };
                }
                else if (this.DrawObject.DrawMode == DrawMode.Rectangle)
                {
                    prefix = "Rectangle-";
                    envelope = e.Geometry as Envelope;
                    pointCollection.Add(new MapPoint(envelope.XMin, envelope.YMin));
                    pointCollection.Add(new MapPoint(envelope.XMin, envelope.YMax));
                    pointCollection.Add(new MapPoint(envelope.XMax, envelope.YMax));
                    pointCollection.Add(new MapPoint(envelope.XMax, envelope.YMin));
                    pointCollection.Add(new MapPoint(envelope.XMin, envelope.YMin));
                    polygon.Rings.Add(pointCollection);
                    graphic = new Graphic()
                    {
                        Geometry = polygon,
                        Symbol = this.ChooseSymbol("Polygon")
                    };
                }
                else if (this.DrawObject.DrawMode == DrawMode.Freehand)
                {
                    prefix = "Freehand-";
                }

                String id = prefix + DateTime.Now.ToString("HH:mm:ss");

                if (graphic != null)
                {
                    graphic.Attributes.Add("Id", id);
                    _graphicCheckBoxes.Add(new GraphicCheckbox() { Name = id, Checked = false });
                    this.AddGraphic(graphic);
                    this.DrawObject.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        #endregion

        #region UI Element Event Handlers
        private void rdDrawMode_Click(object sender, RoutedEventArgs e)
        {
            _isDrawingText = false;
            _isDrawingMeasured = false;
            _drawMeasureMode = string.Empty;

            String drawMode = (sender as RadioButton).Content.ToString();

            if (drawMode.Equals("Measured Line", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                drawMode = "Point";
                _isDrawingMeasured = true;
                _drawMeasureMode = "Measured Line";
                gridMeasured.Visibility = System.Windows.Visibility.Visible;
                gridAzimuthed.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (drawMode.Equals("Azimuthed Line", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                drawMode = "Point";
                _isDrawingMeasured = true;
                _drawMeasureMode = "Azimuthed Line";
                gridMeasured.Visibility = System.Windows.Visibility.Collapsed;
                gridAzimuthed.Visibility = System.Windows.Visibility.Visible;
            }

            if (drawMode.Equals("Measured Rectangle", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                drawMode = "Point";
                _isDrawingMeasured = true;
                _drawMeasureMode = "Measured Rectangle";
                gridMeasured.Visibility = System.Windows.Visibility.Visible;
                gridAzimuthed.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (String.Equals(drawMode, "Text", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                drawMode = "Point";
                _isDrawingText = true;
            }

            if (_isDrawingText == true)
            {
                txtString.IsEnabled = true;
            }
            else
            {
                txtString.IsEnabled = false;
            }

            if (String.Equals(drawMode, "Line", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                drawMode = "Polyline";
            }


            this.DrawObject.DrawMode = (DrawMode)Enum.Parse(typeof(DrawMode), drawMode, false);

            // if point use size, else use width
            cboWidth.IsEnabled = (this.DrawObject.DrawMode != DrawMode.Point);
            cboSize.IsEnabled = (this.DrawObject.DrawMode == DrawMode.Point);
        }

        private void cbGraphic_Click(object sender, RoutedEventArgs e)
        {
            Graphic graphic;
            String id;

            try
            {
                id = (sender as CheckBox).Content.ToString();
                graphic = this.FindGraphicById(id, true);
                if (graphic != null)
                {
                    graphic.Select();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            this.graphicsLayer.Graphics.Clear();
            _graphicCheckBoxes.Clear();
        }

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            List<GraphicCheckbox> lstDelete = new List<GraphicCheckbox>();

            Graphic graphic = null;
            try
            {
                foreach (GraphicCheckbox gcb in _graphicCheckBoxes)
                {
                    if (gcb.Checked == true)
                    {
                        graphic = this.FindGraphicById(gcb.Name);
                        if (graphic != null)
                        {
                            this.GraphicsLayer.Graphics.Remove(graphic);
                            lstDelete.Add(gcb);
                        }
                    }
                }

                foreach (GraphicCheckbox gcb in lstDelete)
                {
                    _graphicCheckBoxes.Remove(gcb);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

        private void tbAnchor_Click(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // restore old colors - primitive
            foreach (TextBox tb in gridAnchor.Children)
            {
                switch (tb.Tag.ToString())
                {
                    case "0":
                    case "2":
                    case "6":
                    case "8":
                        tb.Background = new SolidColorBrush() { Color = Colors.Magenta };
                        break;
                    case "1":
                    case "3":
                    case "5":
                    case "7":
                        tb.Background = new SolidColorBrush() { Color = Colors.Blue }; break;
                    case "4":
                        tb.Background = new SolidColorBrush() { Color = Colors.Orange }; break;
                    default:
                        break;
                }
            }

            textBox.Background = new SolidColorBrush() { Color = Colors.Yellow };

            switch (textBox.Tag.ToString())
            {
                case "0":
                    _anchorX = 0;
                    _anchorY = 0;
                    break;
                case "1":
                    _anchorX = 0.5;
                    _anchorY = 0;
                    break;
                case "2":
                    _anchorX = 1.0;
                    _anchorY = 0;
                    break;

                case "3":
                    _anchorX = 0;
                    _anchorY = 0.5;
                    break;
                case "4":
                    _anchorX = 0.5;
                    _anchorY = 0.5;
                    break;
                case "5":
                    _anchorX = 1.0;
                    _anchorY = 0.5;
                    break;

                case "6":
                    _anchorX = 0;
                    _anchorY = 1.0;
                    break;
                case "7":
                    _anchorX = 0.5;
                    _anchorY = 1.0;
                    break;
                case "8":
                    _anchorX = 1.0;
                    _anchorY = 1.0;
                    break;
                default:
                    break;
            }
        }

        //private void rdMeasureType_Click(object sender, RoutedEventArgs e)
        //{
        //  // Click Handler for Actions Radio Buttons
        //  string content = (e.OriginalSource as RadioButton).Content.ToString();
        //  if (content.Equals("Line", StringComparison.InvariantCultureIgnoreCase) == true)
        //  {
        //    grLength.Height = new GridLength(20);
        //    grWidth.Height = new GridLength(0);
        //    grHeight.Height = new GridLength(0);
        //  }
        //  else
        //  {
        //    grLength.Height = new GridLength(0);
        //    grWidth.Height = new GridLength(20);
        //    grHeight.Height = new GridLength(20);
        //  }
        //}

        //private void btnMeasureApply_Click(object sender, RoutedEventArgs e)
        //{
        //  this._isDrawingMeasured = true;
        //}

        #endregion

        #region Privates for Symbology

        private Symbol ChooseSymbol(string geoType, bool isMeasured = false)
        {
            try
            {

                Symbol symbol = null;

                ColorObj cObj;
                if (isMeasured == false)
                {
                    cObj = (ColorObj)cboColor.SelectedItem;
                }
                else
                {
                    cObj = (ColorObj)cboColor_M.SelectedItem;
                }

                ColorObj cFill;
                if (isMeasured == false)
                {
                    cFill = (ColorObj)cboFill.SelectedItem;
                }
                else
                {
                    cFill = (ColorObj)cboFill_M.SelectedItem;
                }

                int width = 1;
                if (isMeasured == false)
                {
                    width = int.Parse(cboWidth.Items[cboWidth.SelectedIndex].ToString());
                }
                else
                {
                    width = int.Parse(cboWidth_M.Items[cboWidth_M.SelectedIndex].ToString());
                }


                switch (geoType)
                {
                    case "Point":
                    case "MapPoint":
                    case "MultiPoint":
                        if (_isDrawingText == false)
                        {
                            symbol = new SimpleMarkerSymbol()
                            {
                                Size = int.Parse(cboSize.Items[cboSize.SelectedIndex].ToString()),
                                Color = new System.Windows.Media.SolidColorBrush(cObj.cColor),
                                Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
                            };
                        }
                        else
                        {
                            symbol = new TextSymbol()
                            {
                                FontSize = int.Parse(cboSize.Items[cboSize.SelectedIndex].ToString()),
                                Foreground = new System.Windows.Media.SolidColorBrush(cObj.cColor),
                                Text = txtString.Text
                            };
                        }

                        break;

                    case "Polyline":
                        symbol = new SimpleLineSymbol()
                        {
                            Width = width,
                            Color = new System.Windows.Media.SolidColorBrush(cObj.cColor),
                            Style = SimpleLineSymbol.LineStyle.Solid
                        };
                        break;

                    case "Polygon":
                        symbol = new SimpleFillSymbol()
                        {
                            BorderBrush = new System.Windows.Media.SolidColorBrush(cObj.cColor),
                            BorderThickness = width,
                            Fill = new System.Windows.Media.SolidColorBrush(cFill.cColor)
                        };

                        break;
                }

                return symbol;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                return null;
            }

        }

        private Graphic FindGraphicById(String id, bool unselectALL = false)
        {

            if (unselectALL == true)
            {
                foreach (Graphic grp in this.GraphicsLayer.Graphics)
                {
                    grp.UnSelect();
                }
            }

            foreach (Graphic grp in this.GraphicsLayer.Graphics)
            {
                if (grp.Attributes["Id"].ToString() == id)
                {
                    return grp;
                }
            }
            return null;
        }

        private double[] GetTextDimension(Graphic graphic)
        {
            TextSymbol ts = null;

            if (graphic.Symbol is TextSymbol)
            {
                // use a Textbox to obtain size of text in pixels
                ts = graphic.Symbol as ESRI.ArcGIS.Client.Symbols.TextSymbol;
                TextBlock tb = new TextBlock();
                tb.FontFamily = ts.FontFamily;
                tb.FontSize = ts.FontSize;
                tb.Text = ts.Text;

                double textPixelWidth = tb.ActualWidth;
                double textPixelHeight = tb.ActualHeight;

                // now compute the dimension in MapUnits
                double mapPixelWidth = this.MapControl.ActualWidth;
                double mapPixelHeight = this.MapControl.ActualHeight;

                // get the MapPoint as the Center of screen
                MapPoint mapCenter = this.MapControl.ScreenToMap(new Point(mapPixelWidth / 2, mapPixelHeight / 2));
                // get the MapPoint center plus textWidth
                MapPoint mapOffsetX = this.MapControl.ScreenToMap(new Point(mapPixelWidth / 2 + textPixelWidth, mapPixelHeight / 2));
                // get the MapPoint center plus textHeight
                MapPoint mapOffsetY = this.MapControl.ScreenToMap(new Point(mapPixelWidth / 2, mapPixelHeight / 2 + textPixelHeight));

                // return difference between center and offset points, X and Y
                return new double[] { Math.Abs(mapCenter.X - mapOffsetX.X), Math.Abs(mapCenter.Y - mapOffsetY.Y) };
            }
            else
            {
                return null;
            }
        }

        private Graphic CreateMeasuredGraphic(MapPoint mapPoint)
        {
            try
            {
                double dOut;
                double dX = 0;
                double dY = 0;
                double dL = 0;
                double dA = 0;
                double dAX = 0;
                double dAY = 0;

                RedlineMeasurementUnits rmuX = null;
                RedlineMeasurementUnits rmuY = null;
                RedlineMeasurementUnits rmuL = null;
                Polyline polyline = null;
                Polygon polygon = null;

                Graphic reply = new Graphic();
                ESRI.ArcGIS.Client.Geometry.PointCollection ptColl = new ESRI.ArcGIS.Client.Geometry.PointCollection();

                if (double.TryParse(txtWidthM.Text, out dOut) == false)
                {
                    throw new Exception("Measure Width must be Numeric");
                }
                rmuX = (cboWidthUnits.SelectedItem as ComboBoxItem).Tag as RedlineMeasurementUnits;
                dX = dOut / rmuX.Conversion;

                if (double.TryParse(txtHeightM.Text, out dOut) == false)
                {
                    throw new Exception("Measure Height must be Numeric");
                }
                rmuY = (cboHeightUnits.SelectedItem as ComboBoxItem).Tag as RedlineMeasurementUnits;
                dY = dOut / rmuY.Conversion;

                if (double.TryParse(txtLengthA.Text, out dOut) == false)
                {
                    throw new Exception("Length must be Numeric");
                }
                rmuL = (cboLengthUnits.SelectedItem as ComboBoxItem).Tag as RedlineMeasurementUnits;
                dL = dOut / rmuL.Conversion;

                if (double.TryParse(txtAzimuth.Text, out dOut) == false)
                {
                    throw new Exception("Azimuth must be Numeric");
                }
                dA = dOut;


                if (_drawMeasureMode == "Measured Line")
                {
                    polyline = new Polyline();
                    ptColl.Add(new MapPoint(mapPoint.X, mapPoint.Y, this.MapControl.SpatialReference));
                    ptColl.Add(new MapPoint(mapPoint.X + dX, mapPoint.Y + dY, this.MapControl.SpatialReference));
                    polyline.Paths.Add(ptColl);
                    reply.Geometry = polyline;
                }
                else if (_drawMeasureMode == "Azimuthed Line")
                {
                    polyline = new Polyline();
                    ptColl.Add(new MapPoint(mapPoint.X, mapPoint.Y, this.MapControl.SpatialReference));
                    dAX = mapPoint.X + Math.Cos(dA * (Math.PI / 180.0)) * dL;
                    dAY = mapPoint.Y + Math.Sin(dA * (Math.PI / 180.0)) * dL;

                    //txtAzimuth.Text = (double.Parse(txtAzimuth.Text) + 10.0).ToString();

                    ptColl.Add(new MapPoint(dAX, dAY, this.MapControl.SpatialReference));
                    polyline.Paths.Add(ptColl);
                    reply.Geometry = polyline;
                }
                else
                {
                    polygon = new Polygon();
                    ptColl.Add(new MapPoint(mapPoint.X, mapPoint.Y, this.MapControl.SpatialReference));
                    ptColl.Add(new MapPoint(mapPoint.X + dX, mapPoint.Y, this.MapControl.SpatialReference));
                    ptColl.Add(new MapPoint(mapPoint.X + dX, mapPoint.Y + dY, this.MapControl.SpatialReference));
                    ptColl.Add(new MapPoint(mapPoint.X, mapPoint.Y + dY, this.MapControl.SpatialReference));
                    ptColl.Add(new MapPoint(mapPoint.X, mapPoint.Y, this.MapControl.SpatialReference));
                    polygon.Rings.Add(ptColl);
                    reply.Geometry = polygon;
                }
                return reply;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

        }

        #endregion

        #region Event Handlers to Set and Highlight Selected Graphic

        protected override void OnGraphicsLayerMouseUp(object sender, GraphicMouseButtonEventArgs e)
        {
            HighlightSelectedGraphic(e.Graphic);

            // Run the base's handler at last
            base.OnGraphicsLayerMouseUp(sender, e);
        }

        private void QueryResultGrid_SelectedItemChange(object sender, SelectedItemChangeEventArgs args)
        {
            Graphic feature = args.Feature;
            if (feature != null)
            {
                /*
                 * If filter the results with changing map extent, remove this block
                 * Otherwise, add this block to pan the map to the selected graphic.
                 * But do not do it at the first load when the map just zooms to the 
                 * results, otherwise the map will be hanging without response.
                 * (See MapControl_ExtentChanged handler)
                 */

                HighlightSelectedGraphic(feature);
            }
        }

        private void HighlightSelectedGraphic(Graphic newGraphic)
        {
            string geoType = "";

            if (this.SelectedGraphic != null)
            {
                geoType = this.SelectedGraphic.Geometry.GetType().Name;
                this.SelectedGraphic.Symbol = ChooseSymbol(geoType);
                this.SelectedGraphic.Selected = false;
            }

            this.SelectedGraphic = newGraphic;
            geoType = this.SelectedGraphic.Geometry.GetType().Name;
            this.SelectedGraphic.Symbol = ChooseSymbol(geoType);
            this.SelectedGraphic.Selected = true;
        }

        #endregion
    }

    //---------------------------------------------------------------------------
    public class GraphicCheckbox
    {
        public bool Checked { get; set; }
        public String Name { get; set; }
    }

    //---------------------------------------------------------------------------
    public class DrawModeRadioButton
    {
        public bool Checked { get; set; }
        public String Name { get; set; }
    }

    //---------------------------------------------------------------------------
    public class ColorList
    {
        public List<ColorObj> List = new List<ColorObj>();

        public ColorList()
        {
            List.Add(new ColorObj(Colors.Transparent, "None"));
            List.Add(new ColorObj(Colors.Black, "Black"));
            List.Add(new ColorObj(Colors.Blue, "Blue"));
            List.Add(new ColorObj(Colors.Brown, "Brown"));
            List.Add(new ColorObj(Colors.Cyan, "Cyan"));
            List.Add(new ColorObj(Colors.DarkGray, "DarkGray"));
            List.Add(new ColorObj(Colors.Gray, "Gray"));
            List.Add(new ColorObj(Colors.Green, "Green"));
            List.Add(new ColorObj(Colors.LightGray, "LightGray"));
            List.Add(new ColorObj(Colors.Magenta, "Magenta"));
            List.Add(new ColorObj(Colors.Orange, "Orange"));
            List.Add(new ColorObj(Colors.Purple, "Purple"));
            List.Add(new ColorObj(Colors.Red, "Red"));
            List.Add(new ColorObj(Colors.White, "White"));
            List.Add(new ColorObj(Colors.Yellow, "Yellow"));
        }
    }

    //---------------------------------------------------------------------------
    public class ColorObj
    {
        public Color cColor { get; set; }
        public String Name { get; set; }

        public ColorObj(Color color, String name)
        {
            this.cColor = color;
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
