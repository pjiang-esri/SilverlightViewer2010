using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Geometry;

using ESRI.SilverlightViewer.Config;
using ESRI.SilverlightViewer.Widget;
using ESRI.SilverlightViewer.Utility;

namespace ESRI.SilverlightViewer.UIWidget
{
    public partial class IncidentWidget : WidgetBase
    {
        private FeatureLayer agisFeatureLayer = null;
        private IncidentWidgetConfig widgetConfig = null;
        private Dictionary<string, List<object>> fieldValues = null;

        public IncidentWidget()
        {
            InitializeComponent();
        }

        #region Override Functions - Load Configuration and Initialize Feature Layer
        protected override void OnDownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            this.IsBusy = true;
            string xmlConfig = e.Result;
            widgetConfig = (IncidentWidgetConfig)WidgetConfigBase.Deserialize(xmlConfig, typeof(IncidentWidgetConfig));

            if (widgetConfig != null)
            {
                agisFeatureLayer = new FeatureLayer() { Url = widgetConfig.LayerUrl, ProxyUrl = widgetConfig.ProxyUrl, Token = widgetConfig.Token };
                agisFeatureLayer.ID = string.Format("feature_{0}", widgetConfig.ID.ToString());
                agisFeatureLayer.Opacity = (widgetConfig.Opacity == 0.0) ? 1.0 : widgetConfig.Opacity;
                agisFeatureLayer.Initialized += new EventHandler<EventArgs>(FeatureLayer_Initialized);
                agisFeatureLayer.InitializationFailed += new EventHandler<EventArgs>(FeatureLayer_InitializationFailed);
                agisFeatureLayer.UpdateCompleted += new EventHandler(FeatureLayer_UpdateCompleted);
                agisFeatureLayer.DisableClientCaching = true;

                if (!string.IsNullOrEmpty(widgetConfig.OutputFields))
                {
                    string[] fields = widgetConfig.OutputFields.Split(',');
                    foreach (string field in fields) { agisFeatureLayer.OutFields.Add(field); }
                }

                this.MapControl.Layers.Add(agisFeatureLayer);
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            if (agisFeatureLayer != null)
            {
                agisFeatureLayer.Visible = true;
            }
        }

        protected override void OnClose()
        {
            if (agisFeatureLayer != null)
            {
                agisFeatureLayer.Visible = false;
            }
            base.OnClose();
        }
        #endregion

        #region Feature Layer Initialization Event Handlers and Populate Filter Values
        /// <summary>
        /// Handle FeatureLayer Initialization Event
        /// </summary>
        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            FeatureLayer fLayer = sender as FeatureLayer;
            if (fLayer.LayerInfo == null)
            {
                MessageBox.Show("Initializing feature layer failed. Please check the REST service.");
                return;
            }

            // Create a Map Tip popup window for the feature layer
            fLayer.MapTip = new MapTipPopup(fLayer.LayerInfo.Fields, fLayer.OutFields, fLayer.LayerInfo.DisplayField, null) { ShowCloseButton = false, Background = this.Background };

            // Populate the values of the filter fields
            PopulateFilterValues(fLayer.LayerInfo.Fields);

            // Initialize TimeSlider for TimeExtent filter
            InitializeTimeSlider(fLayer.TimeExtent);
        }

        private void FeatureLayer_UpdateCompleted(object sender, EventArgs e)
        {
            if (agisFeatureLayer.FullExtent != null)
            {
                // Expand this extent in case only one point is selected
                this.MapControl.ZoomTo(GeometryTool.ExpandGeometryExtent(agisFeatureLayer.FullExtent, 0.0));
            }
        }

        /// <summary>
        /// Populate the Values of the Filter Fields
        /// </summary>
        private void PopulateFilterValues(List<Field> lyrFields)
        {
            int queryCount = 0;
            string[] sFields = widgetConfig.FilterFields.Split(',');
            fieldValues = new Dictionary<string, List<object>>();

            foreach (string field in sFields)
            {
                Field qField = lyrFields.First(f => f.Name.Equals(field));
                if (qField == null) continue;

                ListBoxItem listItem = new ListBoxItem() { Content = qField.Alias, Tag = qField };
                listQueryFields.Items.Add(listItem);

                QueryTask queryTask = new QueryTask(widgetConfig.LayerUrl);
                queryTask.ExecuteCompleted += (obj, arg) =>
                {
                    object value = null;
                    Field useField = arg.UserState as Field;
                    List<object> listValues = new List<object>();

                    foreach (Graphic graphic in arg.FeatureSet.Features)
                    {
                        value = graphic.Attributes[useField.Name];
           
                        if (!listValues.Contains(value))
                        {
                            listValues.Add(value);
                        }
                    }

                    queryCount++;
                    listValues.Sort();
                    fieldValues.Add(useField.Name, listValues);

                    if (queryCount == sFields.Length)
                    {
                        listQueryFields.SelectedIndex = 0;
                        this.IsBusy = false;
                    }
                };

                Query qParams = new Query();
                qParams.Where = "1=1";
                qParams.ReturnGeometry = false;
                qParams.OutFields.Add(qField.Name);
                queryTask.ExecuteAsync(qParams, qField);
            }
        }

        /// <summary>
        /// Initialize the TimeExtent Filter Slider
        /// </summary>
        /// <param name="lyrTimeExtent">Feature Layer Time Extent</param>
        private void InitializeTimeSlider(TimeExtent lyrTimeExtent)
        {
            if (widgetConfig.UseTimeInfo)
            {
                timeExtentSlider.MinimumValue = lyrTimeExtent.Start;
                timeExtentSlider.MaximumValue = lyrTimeExtent.End;

                List<DateTime> intervals = new List<DateTime>();
                DateTime delta = lyrTimeExtent.Start;

                while (delta < lyrTimeExtent.End)
                {
                    intervals.Add(delta);
                    delta = delta.AddMonths(widgetConfig.TimeInterval);
                }

                intervals.Add(lyrTimeExtent.End);
                timeExtentSlider.Intervals = intervals;

                if (!string.IsNullOrEmpty(widgetConfig.InitInterval))
                {
                    if (widgetConfig.InitInterval.Equals("Last", StringComparison.CurrentCultureIgnoreCase))
                    {
                        timeExtentSlider.Value = new TimeExtent(intervals[intervals.Count - 2], intervals.Last());
                    }
                    else if (widgetConfig.InitInterval.Equals("First", StringComparison.CurrentCultureIgnoreCase))
                    {
                        timeExtentSlider.Value = new TimeExtent(intervals[0], intervals[1]);
                    }
                }
            }
            else
            {
                borderTimeSlider.Visibility = Visibility.Collapsed;
            }
        }

        private void TimeExtentSlider_ValueChanged(object sender, TimeSlider.ValueChangedEventArgs e)
        {
            this.MapControl.TimeExtent = e.NewValue;
        }

        /// <summary>
        /// Handle FeatureLayer InitializationFailed Event
        /// </summary>
        private void FeatureLayer_InitializationFailed(object sender, EventArgs e)
        {
            if (sender is Layer)
            {
                MessageBox.Show("Failed to initialize the feature layer: " + (sender as Layer).ID);
            }
        }
        #endregion

        #region Handle Events and Apply Filter
        private void ListQueryFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem item = e.AddedItems[0] as ListBoxItem;
            Field fieldInfo = item.Tag as Field;

            if (fieldInfo != null)
            {
                listFieldValues.ItemsSource = fieldValues[fieldInfo.Name];

                if (agisFeatureLayer != null && string.IsNullOrEmpty(agisFeatureLayer.Where))
                {
                    listFieldValues.SelectAll();
                }
            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            listFieldValues.SelectAll();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            listFieldValues.SelectedItems.Clear();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (listFieldValues.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select filter values");
                return;
            }

            ListBoxItem fieldItem = listQueryFields.SelectedItem as ListBoxItem;
            Field queryField = fieldItem.Tag as Field;

            string sWhere = "";
            string sqlValues = "";
            List<double> dblValues = new List<double>();

            if (queryField.Type == Field.FieldType.Single || queryField.Type == Field.FieldType.Double
                    || queryField.Type == Field.FieldType.Integer || queryField.Type == Field.FieldType.SmallInteger)
            {
                foreach (object value in listFieldValues.SelectedItems)
                {
                    dblValues.Add((value == null) ? 0.0 : (double)value);
                }

                sWhere = (dblValues.Count == 1) ?
                    string.Format("{0}={1}", queryField.Name, dblValues[0]) :
                    string.Format("{0}>={1} AND {0}<={2}", queryField.Name, dblValues.Min(), dblValues.Max());
            }
            else if (queryField.Type == Field.FieldType.String)
            {
                foreach (object value in listFieldValues.SelectedItems)
                {
                    sqlValues += string.Format("'{0}',", value);
                }

                if (listFieldValues.SelectedItems.Count < listFieldValues.Items.Count)
                {
                    sqlValues = sqlValues.Remove(sqlValues.Length - 1);
                    sWhere = (listFieldValues.SelectedItems.Count > 1) ?
                        string.Format("{0} IN ({1})", queryField.Name, sqlValues) :
                        string.Format("{0} = {1}", queryField.Name, sqlValues);
                }
                else sWhere = ""; // All is selected
            }

            if (agisFeatureLayer != null)
            {
                agisFeatureLayer.Where = sWhere;
                agisFeatureLayer.Update();
                
            }
        }
        #endregion
    }
}
