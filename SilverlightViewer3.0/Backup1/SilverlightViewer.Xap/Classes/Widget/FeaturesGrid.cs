using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using ESRI.SilverlightViewer.Data;
using ESRI.SilverlightViewer.Utility;
using ESRI.SilverlightViewer.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.SilverlightViewer.Widget
{
    public class FeaturesGrid : SplitGrid
    {
        #region Define SelectedItemChangeEvent Handler and Event
        private SelectedItemChangeEventHandler SelectedItemChangeHandler = null;
        public event SelectedItemChangeEventHandler SelectedItemChange
        {
            add
            {
                if (SelectedItemChangeHandler == null || !SelectedItemChangeHandler.GetInvocationList().Contains(value))
                {
                    SelectedItemChangeHandler += value;
                }
            }

            remove
            {
                SelectedItemChangeHandler -= value;
            }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty DataSourcesProperty = DependencyProperty.Register("DataSources", typeof(ObservableCollection<GeoFeatureCollection>), typeof(FeaturesGrid), new PropertyMetadata(null, new PropertyChangedCallback(OnDataSourcesChange)));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(Graphic), typeof(FeaturesGrid), null);

        /// <summary>
        /// A read only list of GeoFeatureCollection's or GeoFeatureCollection's views
        /// Please use AddDataSource/RemoveDataSource to add/remove a GeoFeatureCollection from the list
        /// </summary>
        public ObservableCollection<GeoFeatureCollection> DataSources
        {
            get { return (ObservableCollection<GeoFeatureCollection>)GetValue(DataSourcesProperty); }
            set { SetValue(DataSourcesProperty, value); }
        }

        /// <summary>
        /// Read Only Property - Get the selected item
        /// </summary>
        public Graphic SelectedItem
        {
            get { return (Graphic)GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }

        protected static void OnDataSourcesChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d != null && e.NewValue != null)
            {
                FeaturesGrid me = d as FeaturesGrid;
                ObservableCollection<GeoFeatureCollection> dataSources = e.NewValue as ObservableCollection<GeoFeatureCollection>;

                foreach (GeoFeatureCollection dataset in dataSources)
                {
                    me.BindDataset(dataset);
                }

                // Handle DataSources Collection Change Event
                dataSources.CollectionChanged += (sender, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Reset)
                    {
                        me.Clear();
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Add)
                    {
                        me.BindDataset(args.NewItems[0] as GeoFeatureCollection);
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Remove)
                    {
                        me.RemoveDataset(args.OldItems[0] as GeoFeatureCollection);
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Replace) // When use InsertAt/RemoveAt
                    {
                        me.UpdateBinding();
                    }
                };
            }
        }
        #endregion

        #region Manipulate Data Sources: Add, Remove, and Bind to the Grid
        /// <summary>
        /// Clear all binded dataset 
        /// </summary>
        public void Clear()
        {
            this.SelectedItem = null;

            if ((this.LeftWindow != null) && (this.LeftWindow is StackPanel))
            {
                (this.LeftWindow as StackPanel).Children.Clear();
            }

            if ((this.RightWindow != null) && (this.RightWindow is Grid))
            {
                (this.RightWindow as Grid).Children.Clear();
                (this.RightWindow as Grid).RowDefinitions.Clear();
                (this.RightWindow as Grid).ColumnDefinitions.Clear();
            }
        }

        /// <summary>
        /// Update content panel when a dataset view changes 
        /// </summary>
        public void UpdateBinding()
        {
            this.Clear();
            foreach (GeoFeatureCollection ds in this.DataSources)
            {
                BindDataset(ds);
            }
        }
        #endregion

        #region Bind/Remove Data Source - Create/Remove Link Stack in the Left Window
        /// <summary>
        /// Bind a dataset to the Grid
        /// </summary>
        protected virtual void BindDataset(GeoFeatureCollection dataset)
        {
            if ((dataset != null) && (dataset.Count > 0))
            {
                StackPanel leftStack = null;

                if ((this.LeftWindow != null) && (this.LeftWindow is StackPanel))
                {
                    leftStack = this.LeftWindow as StackPanel;
                }
                else
                {
                    leftStack = new StackPanel() { Orientation = Orientation.Vertical };
                    this.LeftWindow = leftStack;
                }

                StackPanel titleStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(1, 1, 1, 1) };
                StackPanel valueStack = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(15, 0, 0, 0), Tag = dataset };

                // Create a head stack containing a toggle button and a feature layer name block
                ToggleButton toggler = new ToggleButton() { Tag = valueStack, Width = 16, Height = 14, VerticalAlignment = VerticalAlignment.Top };
                toggler.Background = new SolidColorBrush(Colors.Transparent);
                toggler.Foreground = new SolidColorBrush(Colors.Gray);
                toggler.MouseOverColor = new SolidColorBrush(Colors.Blue);
                toggler.Click += new RoutedEventHandler(LeftFeatureListToggler_Click);

                TextBlock titleText = new TextBlock() { TextWrapping = TextWrapping.Wrap };
                titleText.Text = (string.IsNullOrEmpty(dataset.FeatureLayerName)) ? "Unknown Feature" : dataset.FeatureLayerName;
                if (!string.IsNullOrEmpty(dataset.DataSourceName)) titleText.Text += "\n(" + dataset.DataSourceName + ")";
                titleStack.Children.Add(toggler);
                titleStack.Children.Add(titleText);

                // Create a list stack of display field values
                string displayValue = "";
                SimpleLinkButton valueLink = null;
                foreach (Graphic feature in dataset)
                {
                    displayValue = (string)feature.Attributes[dataset.DisplayFieldName];
                    valueLink = new SimpleLinkButton() { Tag = feature, Cursor = Cursors.Hand, Margin = new Thickness(1, 1, 1, 1) };
                    valueLink.Text = (string.IsNullOrEmpty(displayValue)) ? "[Empty]" : displayValue;
                    valueLink.Click += new RoutedEventHandler(LeftFeatureLink_Click);
                    valueLink.MouseEnter += new MouseEventHandler(LeftFeatureLink_MouseEnter);
                    valueLink.MouseLeave += new MouseEventHandler(LeftFeatureLink_MouseLeave);
                    valueStack.Children.Add(valueLink);

                    //Select the first one
                    if (SelectedItem == null)
                    {
                        SelectedItem = feature;
                        leftStack.Tag = valueLink;
                        valueLink.IsActive = true;
                        FillDataInRightWindow(feature, dataset);
                        OnSelectedItemChange(new SelectedItemChangeEventArgs(feature));
                    }
                }

                leftStack.Children.Add(titleStack);
                leftStack.Children.Add(valueStack);
                this.SelectItem(dataset[0]);
            }
        }

        /// <summary>
        /// Remove a dataset from the Grid
        /// </summary>
        protected virtual void RemoveDataset(GeoFeatureCollection dataset)
        {
            if (this.LeftWindow != null && dataset != null)
            {
                StackPanel leftStack = this.LeftWindow as StackPanel;
                UIElement valueStack = leftStack.Children.FirstOrDefault(child => (child as StackPanel).Tag == dataset);

                if (valueStack != null)
                {
                    int k = leftStack.Children.IndexOf(valueStack);
                    if (k > 0) // 
                    {
                        leftStack.Children.RemoveAt(k); // Remove Value Stack
                        leftStack.Children.RemoveAt(k - 1); // Remove Title Stack
                    }
                }
            }
        }
        #endregion

        #region Event Handlers - Select Item or Hightlight Mouseover Item
        /// <summary>
        /// Select a Graphic item and show its attributes 
        /// </summary>
        /// <param name="graphic">The graphic be selected</param>
        public void SelectItem(Graphic graphic)
        {
            bool found = false;

            if (this.LeftWindow is StackPanel)
            {
                StackPanel leftStack = this.LeftWindow as StackPanel;
                SimpleLinkButton linkOld = leftStack.Tag as SimpleLinkButton;
                if (linkOld != null) linkOld.IsActive = false;

                for (int i = 0; i < leftStack.Children.Count / 2; i++)
                {
                    StackPanel titleStack = leftStack.Children[i * 2] as StackPanel;
                    StackPanel valueStack = leftStack.Children[(i * 2) + 1] as StackPanel;
                    GeoFeatureCollection dataset = valueStack.Tag as GeoFeatureCollection;

                    foreach (SimpleLinkButton link in valueStack.Children)
                    {
                        if (link.Tag == graphic)
                        {
                            leftStack.Tag = link;
                            link.IsActive = true;

                            this.SelectedItem = graphic;
                            FillDataInRightWindow(graphic, dataset);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        ToggleButton toggler = titleStack.Children[0] as ToggleButton;
                        if (valueStack.Visibility == Visibility.Visible) ToggleLeftFeatureList(toggler);
                    }
                }
            }
        }

        private void ToggleLeftFeatureList(ToggleButton toggler)
        {
            if (toggler != null)
            {
                StackPanel content = (StackPanel)toggler.Tag;
                if (content.Visibility == Visibility.Visible)
                {
                    toggler.State = ToggleButtonState.STATE_ROTATE_90;
                    content.Visibility = Visibility.Collapsed;
                }
                else
                {
                    toggler.State = ToggleButtonState.STATE_ORIGIN;
                    content.Visibility = Visibility.Visible;
                }
            }
        }

        private void LeftFeatureListToggler_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton)
            {
                ToggleLeftFeatureList(sender as ToggleButton);
            }
        }

        private void LeftFeatureLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is SimpleLinkButton)
            {
                StackPanel leftStack = this.LeftWindow as StackPanel;
                SimpleLinkButton leftLinkOld = leftStack.Tag as SimpleLinkButton;
                if (leftLinkOld != null) leftLinkOld.IsActive = false;

                SimpleLinkButton leftLinkNew = sender as SimpleLinkButton;
                leftStack.Tag = leftLinkNew;
                leftLinkNew.IsActive = true;

                StackPanel valueStack = leftLinkNew.Parent as StackPanel;
                GeoFeatureCollection dataset = valueStack.Tag as GeoFeatureCollection;

                this.SelectedItem = (Graphic)leftLinkNew.Tag;
                FillDataInRightWindow(this.SelectedItem, dataset);
                OnSelectedItemChange(new SelectedItemChangeEventArgs(this.SelectedItem));
            }
        }

        private void LeftFeatureLink_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is SimpleLinkButton)
            {
                SimpleLinkButton fLink = sender as SimpleLinkButton;
                if (!fLink.IsActive)
                {
                    Graphic feature = (Graphic)fLink.Tag;
                    if (feature != null) feature.Select();
                }
            }
        }

        private void LeftFeatureLink_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is SimpleLinkButton)
            {
                SimpleLinkButton fLink = sender as SimpleLinkButton;
                if (!fLink.IsActive)
                {
                    Graphic feature = (Graphic)fLink.Tag;
                    if (feature != null) feature.UnSelect();
                }
            }
        }

        protected virtual void FillDataInRightWindow(Graphic feature, GeoFeatureCollection dataset)
        {
            Grid rightGrid = null;
            if ((this.RightWindow != null) && (this.RightWindow is Grid))
            {
                rightGrid = this.RightWindow as Grid;
                rightGrid.Children.Clear();
                rightGrid.RowDefinitions.Clear();
                rightGrid.ColumnDefinitions.Clear();
            }
            else
            {
                rightGrid = new Grid();
                this.RightWindow = rightGrid;
            }

            if (feature == null) return;
            rightGrid.ColumnDefinitions.Add(new ColumnDefinition());
            rightGrid.ColumnDefinitions.Add(new ColumnDefinition());

            int k = 0;
            string value = "";
            TextBlock fieldBlock = null;
            TextBlock valueBlock = null;
            HyperlinkButton valueLink = null;
            Dictionary<string, string> fieldAlias = (dataset == null) ?
                (new Dictionary<string, string>()) : StringHelper.ConvertToDictionary(dataset.OutputFields, dataset.OutputLabels, ',');

            if (fieldAlias.Count == 0)
            {
                foreach (string key in feature.Attributes.Keys)
                {
                    if (!key.StartsWith("Shape")) fieldAlias.Add(key, key);
                }
            }

            foreach (string key in fieldAlias.Keys)
            {
                rightGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                fieldBlock = new TextBlock() { Text = fieldAlias[key] + ":", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(2, 1, 2, 1) };
                rightGrid.Children.Add(fieldBlock);
                Grid.SetColumn(fieldBlock, 0);
                Grid.SetRow(fieldBlock, k);

                value = (feature.Attributes.ContainsKey(key) && feature.Attributes[key] != null) ? feature.Attributes[key].ToString() : "";

                if (value.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
                {
                    valueLink = new HyperlinkButton() { FontWeight = FontWeights.Normal, Margin = new Thickness(2, 1, 2, 1) };
                    valueLink.NavigateUri = new Uri(value);
                    valueLink.TargetName = "_blank";
                    Grid.SetColumn(valueLink, 1);
                    Grid.SetRow(valueLink, k);

                    if (value.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) || value.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
                    {
                        rightGrid.RowDefinitions[k].Height = new GridLength(160);
                        valueLink.Content = new Image() { Source = new BitmapImage(new Uri(value, UriKind.Absolute)), Height = 160, Stretch = Stretch.Uniform };
                    }
                    else valueLink.Content = value;

                    rightGrid.Children.Add(valueLink);
                }
                else
                {
                    valueBlock = new TextBlock() { FontWeight = FontWeights.Normal, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(2, 1, 2, 1) };
                    Grid.SetColumn(valueBlock, 1);
                    Grid.SetRow(valueBlock, k);
                    valueBlock.Text = value;
                    valueBlock.MaxWidth = 300;
                    valueBlock.TextWrapping = TextWrapping.Wrap;
                    rightGrid.Children.Add(valueBlock);
                }

                k++;
            }
        }

        public virtual void OnSelectedItemChange(SelectedItemChangeEventArgs args)
        {
            if (SelectedItemChangeHandler != null) SelectedItemChangeHandler(this, args);
        }
        #endregion
    }
}
