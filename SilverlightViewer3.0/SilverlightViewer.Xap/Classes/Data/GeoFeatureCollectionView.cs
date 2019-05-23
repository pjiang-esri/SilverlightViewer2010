using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.SilverlightViewer.Data
{
    /// <summary>
    /// GeoFeatureCollectionView - an ICollectionView implementation
    /// </summary>
    public class GeoFeatureCollectionView : GeoFeatureCollection, ICollectionView
    {
        protected int _currentPosition = -1;
        protected bool _suppressCollectionChanged = false;
        protected CultureInfo _culture = null;
        protected Graphic _currentItem = null;
        protected Envelope _filterEnvelope = null;
        protected Predicate<object> _filter = null;
        protected Predicate<object> _geometryFilter = null;
        protected GeoFeatureCollection _sourceCollection = null;
        protected SortDescriptionGeoFeatureCollection _sortDescriptions;

        #region Events
        /// <summary>
        /// Occurs after the current item has been changed.
        /// </summary>
        public event EventHandler CurrentChanged;

        /// <summary>
        /// Occurs before the current item changes.
        /// </summary>
        public event CurrentChangingEventHandler CurrentChanging;
        #endregion

        #region Constructors
        public GeoFeatureCollectionView()
        {
        }

        public GeoFeatureCollectionView(GeoFeatureCollection source)
        {
            this._sourceCollection = source;
            this.DisplayFieldName = source.DisplayFieldName;
            this.FeatureLayerName = source.FeatureLayerName;
            this.DataSourceName = source.DataSourceName;
            this.HyperlinkField = source.HyperlinkField;
            this.OutputFields = source.OutputFields;
            this.OutputLabels = source.OutputLabels;
            this.Refresh();
        }
        #endregion

        #region Virtual methods
        /// <summary>
        /// Gets the item at the specified zero-based index in the view, after the source collection is filtered, sorted, and paged.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual Graphic GetItemAt(int index)
        {
            if ((index >= 0) && (index < this.Count))
            {
                return this[index];
            }

            return null;
        }
        #endregion

        #region Override ObservableCollection Members
        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            this.OnCurrentChanging();
            base.ClearItems();
        }

        /// <summary>
        /// Override OnCollectionChanged to suppress collection change event
        /// </summary>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressCollectionChanged) return;
            base.OnCollectionChanged(e);
        }
        #endregion

        #region Implements ICollectionView Members
        /// <summary>
        /// Gets a value that indicates whether this view supports filtering by way of the <see cref="P:System.ComponentModel.ICollectionView.Filter"/> property.
        /// </summary>
        /// <returns>true if this view supports filtering; otherwise, false.</returns>
        public bool CanFilter
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value that indicates whether this view supports grouping by way of the <see cref="P:System.ComponentModel.ICollectionView.GroupDescriptions"/> property.
        /// </summary>
        /// <returns>true if this view supports grouping; otherwise, false.</returns>
        public bool CanGroup
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value that indicates whether this view supports sorting by way of the <see cref="P:System.ComponentModel.ICollectionView.SortDescriptions"/> property.
        /// </summary>
        /// <returns>true if this view supports sorting; otherwise, false.</returns>
        public bool CanSort
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates whether the specified item belongs to this collection view.
        /// </summary>
        /// <returns>true if the item belongs to this collection view; otherwise, false.</returns>
        public bool Contains(object item)
        {
            if (!IsValidType(item))
            {
                return false;
            }

            return this.Contains((Graphic)item);
        }

        /// <summary>
        /// Determines whether the specified item is of valid type
        /// </summary>
        /// <returns>true if the specified item is of valid type; otherwise, false.</returns>
        private bool IsValidType(object item)
        {
            return item is Graphic;
        }

        /// <summary>
        /// Gets or sets the cultural information for any operations of the view that may differ by culture, such as sorting.
        /// </summary>
        /// <returns>The culture information to use during culture-sensitive operations.</returns>
        public System.Globalization.CultureInfo Culture
        {
            get
            {
                return this._culture;
            }
            set
            {
                if (this._culture != value)
                {
                    this._culture = value;
                    this.OnPropertyChanged("Culture");
                }
            }
        }

        /// <summary>
        /// Gets the current item in the view.
        /// </summary>
        /// <returns>The current item in the view or null if there is no current item.</returns>
        public object CurrentItem
        {
            get { return this._currentItem; }
        }

        /// <summary>
        /// Gets the ordinal position of the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view.
        /// </summary>
        /// <returns>The ordinal position of the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view.</returns>
        public int CurrentPosition
        {
            get { return this._currentPosition; }
        }

        /// <summary>
        /// Enters a defer cycle that you can use to merge changes to the view and delay automatic refresh.
        /// </summary>
        /// <returns>
        /// The typical usage is to create a using scope with an implementation of this method 
        /// and then include multiple view-changing calls within the scope. 
        /// The implementation should delay automatic refresh until after the using scope exits.
        /// </returns>
        public IDisposable DeferRefresh()
        {
            return new DeferRefreshHelper(() => Refresh());
        }

        /// <summary>
        /// Gets or sets a callback function that is used to determine whether an item is appropriate for inclusion in the view.
        /// </summary>
        /// <returns>A method that is used to determine whether an item is appropriate for inclusion in the view.</returns>
        public Predicate<object> Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets or sets an envelope filter that is used to determine whether an item is appropriate for inclusion in the view by its geometry.
        /// </summary>
        public Envelope FilterEnvelope
        {
            get
            {
                return _filterEnvelope;
            }
            set
            {
                _filterEnvelope = value;

                if (_filterEnvelope == null)
                    _geometryFilter = null;
                else if (_geometryFilter == null)
                    _geometryFilter = new Predicate<object>(FilterByGeometry);
            }
        }

        /// <summary>
        /// Gets or sets a callback function that is used to determine whether an item is within the filter envelope.
        /// Users set a value to FilterEnvelope rather than create a callback function, but a sub class can customize it.
        /// </summary>
        /// <returns>A method that is used to determine whether an item is within the filter envelope.</returns>
        protected Predicate<object> GeometryFilter
        {
            get { return _geometryFilter; }
            set { _geometryFilter = value; }
        }

        /// <summary>
        /// Gets a collection of <see cref="T:System.ComponentModel.GroupDescription"/> objects that describe how the items in the collection are grouped in the view.
        /// </summary>
        /// <returns>A collection of objects that describe how the items in the collection are grouped in the view. </returns>
        public ObservableCollection<GroupDescription> GroupDescriptions
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the top-level groups.
        /// </summary>
        /// <returns>A read-only collection of the top-level groups or null if there are no groups.</returns>
        public ReadOnlyObservableCollection<object> Groups
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> of the view is beyond the end of the collection.
        /// </summary>
        /// <returns>true if the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> of the view is beyond the end of the collection; otherwise, false.</returns>
        public bool IsCurrentAfterLast
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return (this.CurrentPosition >= this.Count);
                }
                return true;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> of the view is beyond the start of the collection.
        /// </summary>
        /// <returns>true if the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> of the view is beyond the start of the collection; otherwise, false.</returns>
        public bool IsCurrentBeforeFirst
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return (this.CurrentPosition < 0);
                }
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is current in sync.
        /// </summary>
        /// <returns>true if this instance is current in sync; otherwise, false.</returns>
        protected bool IsCurrentInSync
        {
            get
            {
                if (this.IsCurrentInView)
                {
                    return (this.GetItemAt(this.CurrentPosition) == this.CurrentItem);
                }

                return (this.CurrentItem == null);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is current in view.
        /// </summary>
        /// <returns>true if this instance is current in view; otherwise, false.</returns>
        private bool IsCurrentInView
        {
            get
            {
                return ((0 <= this.CurrentPosition) && (this.CurrentPosition < this.Count));
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the view is empty.
        /// </summary>
        /// <returns>true if the view is empty; otherwise, false.</returns>
        public bool IsEmpty
        {
            get
            {
                return (this.Count == 0);
            }
        }

        /// <summary>
        /// Sets the specified item in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
        /// </summary>
        /// <param name="item">The item to set as the current item.</param>
        /// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.</returns>
        public bool MoveCurrentTo(object item)
        {
            if (!IsValidType(item))
            {
                return false;
            }

            if (object.Equals(this.CurrentItem, item) && ((item != null) || this.IsCurrentInView))
            {
                return this.IsCurrentInView;
            }

            int index = this.IndexOf((Graphic)item);
            return this.MoveCurrentToPosition(index);
        }

        /// <summary>
        /// Sets the first item in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
        /// </summary>
        /// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.</returns>
        public bool MoveCurrentToFirst()
        {
            return this.MoveCurrentToPosition(0);
        }

        /// <summary>
        /// Sets the last item in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
        /// </summary>
        /// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.</returns>
        public bool MoveCurrentToLast()
        {
            return this.MoveCurrentToPosition(this.Count - 1);
        }

        /// <summary>
        /// Sets the item after the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
        /// </summary>
        /// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.</returns>
        public bool MoveCurrentToNext()
        {
            return ((this.CurrentPosition < this.Count) && this.MoveCurrentToPosition(this.CurrentPosition + 1));
        }

        /// <summary>
        /// Sets the item before the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view to the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
        /// </summary>
        /// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.</returns>
        public bool MoveCurrentToPrevious()
        {
            return ((this.CurrentPosition >= 0) && this.MoveCurrentToPosition(this.CurrentPosition - 1));
        }

        /// <summary>
        /// Sets the item at the specified index to be the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view.
        /// </summary>
        /// <param name="position">The index to set the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> to.</param>
        /// <returns>
        /// true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.
        /// </returns>
        public bool MoveCurrentToPosition(int position)
        {
            if ((position < -1) || (position > this.Count))
            {
                throw new ArgumentOutOfRangeException("position");
            }

            if (((position != this.CurrentPosition) || !this.IsCurrentInSync) && this.IsOKToChangeCurrent())
            {
                bool isCurrentAfterLast = this.IsCurrentAfterLast;
                bool isCurrentBeforeFirst = this.IsCurrentBeforeFirst;

                ChangeCurrentToPosition(position);

                OnCurrentChanged();

                if (this.IsCurrentAfterLast != isCurrentAfterLast)
                {
                    this.OnPropertyChanged("IsCurrentAfterLast");
                }

                if (this.IsCurrentBeforeFirst != isCurrentBeforeFirst)
                {
                    this.OnPropertyChanged("IsCurrentBeforeFirst");
                }

                this.OnPropertyChanged("CurrentPosition");
                this.OnPropertyChanged("CurrentItem");
            }

            return this.IsCurrentInView;
        }

        /// <summary>
        /// Changes the current to position.
        /// </summary>
        /// <param name="position">The position.</param>
        private void ChangeCurrentToPosition(int position)
        {
            if (position < 0)
            {
                this._currentItem = null;
                this._currentPosition = -1;
            }
            else if (position >= this.Count)
            {
                this._currentItem = null;
                this._currentPosition = this.Count;
            }
            else
            {
                this._currentItem = this[position];
                this._currentPosition = position;
            }
        }

        /// <summary>
        /// Determines whether it is OK to change current item.
        /// </summary>
        /// <returns>true if is OK to change current item; otherwise, false.</returns>
        protected bool IsOKToChangeCurrent()
        {
            CurrentChangingEventArgs args = new CurrentChangingEventArgs();
            this.OnCurrentChanging(args);
            return !args.Cancel;
        }

        /// <summary>
        /// Called when current item has changed.
        /// </summary>
        protected virtual void OnCurrentChanged()
        {
            if (this.CurrentChanged != null)
            {
                this.CurrentChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CurrentChanging"/> event.
        /// </summary>
        /// <param name="args">The <see cref="System.ComponentModel.CurrentChangingEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCurrentChanging(CurrentChangingEventArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            if (this.CurrentChanging != null)
            {
                this.CurrentChanging(this, args);
            }
        }

        /// <summary>
        /// Called when the current item is changing.
        /// </summary>
        protected void OnCurrentChanging()
        {
            this._currentPosition = -1;
            this.OnCurrentChanging(new CurrentChangingEventArgs(false));
        }

        /// <summary>
        /// Called when a property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets a collection of <see cref="T:System.ComponentModel.SortDescription"/> instances that describe how the items in the collection are sorted in the view.
        /// </summary>
        /// <returns>
        /// A collection of values that describe how the items in the collection are sorted in the view.
        /// </returns>
        public SortDescriptionCollection SortDescriptions
        {
            get
            {
                if (this._sortDescriptions == null)
                {
                    this.SetSortDescriptions(new SortDescriptionGeoFeatureCollection());
                }
                return this._sortDescriptions;
            }
        }

        /// <summary>
        /// Sets a collection of <see cref="T:System.ComponentModel.SortDescription"/> instances that describe how the items in the collection are sorted in the view.
        /// </summary>
        private void SetSortDescriptions(SortDescriptionGeoFeatureCollection descriptions)
        {
            if (this._sortDescriptions != null)
            {
                this._sortDescriptions.SortDescriptionCollectionChanged -= new NotifyCollectionChangedEventHandler(this.OnSortDescriptionsChanged);
            }

            this._sortDescriptions = descriptions;

            if (this._sortDescriptions != null)
            {
                this._sortDescriptions.SortDescriptionCollectionChanged += new NotifyCollectionChangedEventHandler(this.OnSortDescriptionsChanged);
            }
        }

        /// <summary>
        /// Change the collection view when Sort Description changed
        /// </summary>
        private void OnSortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.NewStartingIndex == -1 && SortDescriptions.Count > 0)
            {
                return;
            }

            if (((e.Action != NotifyCollectionChangedAction.Reset) || (e.NewItems != null)) || (((e.NewStartingIndex != -1) || (e.OldItems != null)) || (e.OldStartingIndex != -1)))
            {
                return;
                //Do not need to sort if the consumer call Refresh();
                //this.ApplySort();
            }
        }

        /// <summary>
        /// An inner callback function that will be used to filter the collection by a geometry
        /// </summary>
        /// <param name="value">An item in the source collection - an object of Feature</param>
        /// <returns>Ture, if the item is determined in the view, otherwise, False</returns>
        private bool FilterByGeometry(Object value)
        {
            bool retVal = false;
            Graphic f = value as Graphic;

            if (f.Geometry is MapPoint)
            {
                MapPoint p = f.Geometry as MapPoint;
                retVal = (p.X >= _filterEnvelope.XMin) && (p.X <= _filterEnvelope.XMax) && (p.Y >= _filterEnvelope.YMin) && (p.Y <= _filterEnvelope.YMax);
            }
            else if (f.Geometry.Extent != null)
            {
                //MapPoint c = f.Geometry.Extent.GetCenter();
                //retVal = (c.X >= _filterEnvelope.XMin) && (c.X <= _filterEnvelope.XMax) && (c.Y >= _filterEnvelope.YMin) && (c.Y <= _filterEnvelope.YMax);

                retVal = f.Geometry.Extent.Intersects(_filterEnvelope);
            }

            return retVal;
        }

        /// <summary>
        /// Load the source collection into the view
        /// </summary>
        private void LoadSource()
        {
            this.Clear();

            foreach (Graphic f in _sourceCollection)
            {
                this.Add(f);
            }
        }

        /// <summary>
        /// Filter the view by applying Filterdescriptions
        /// </summary>
        private void ApplyFilter()
        {
            if (Filter == null && GeometryFilter == null) return;

            if (GeometryFilter != null)
            {
                ObservableCollection<Graphic> envelopedRows = new ObservableCollection<Graphic>();

                foreach (Graphic row in this)
                {
                    if (GeometryFilter.Invoke(row))
                    {
                        envelopedRows.Add(row);
                    }
                }

                _suppressCollectionChanged = true;

                this.Clear();
                foreach (Graphic f in envelopedRows)
                {
                    this.Add(f);
                }

                _suppressCollectionChanged = false;
            }

            if (Filter != null)
            {
                ObservableCollection<Graphic> filteredRows = new ObservableCollection<Graphic>();

                foreach (Graphic row in this)
                {
                    if (Filter.Invoke(row))
                    {
                        filteredRows.Add(row);
                    }
                }

                _suppressCollectionChanged = true;

                this.Clear();
                foreach (Graphic f in filteredRows)
                {
                    this.Add(f);
                }

                _suppressCollectionChanged = false;
            }

            // Raise the required notification if ApplySort will not be executed.
            if (SortDescriptions.Count == 0)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Sort the view by applying SortDescriptions 
        /// </summary>
        private void ApplySort()
        {
            if (SortDescriptions.Count == 0) return;

            IOrderedEnumerable<Graphic> orderedRows = null;

            bool firstSort = true;

            for (int sortIndex = 0; sortIndex < _sortDescriptions.Count; sortIndex++)
            {
                SortDescription sort = _sortDescriptions[sortIndex];
                Func<Graphic, object> function = feature => feature.Attributes[sort.PropertyName];

                if (firstSort)
                {
                    orderedRows = sort.Direction == ListSortDirection.Ascending ? this.OrderBy(function) : this.OrderByDescending(function);
                    firstSort = false;
                }
                else
                {
                    orderedRows = sort.Direction == ListSortDirection.Ascending ? orderedRows.ThenBy(function) : orderedRows.ThenByDescending(function);
                }
            }

            // Re-order and page this collection based on the result if the above
            if (orderedRows != null)
            {
                _suppressCollectionChanged = true;

                int index = 0;
                foreach (Graphic f in orderedRows)
                {
                    this[index++] = f;
                }

                _suppressCollectionChanged = false;

                // raise the required notification
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Recreates the view, by firing CollectionChanged event.
        /// </summary>
        public virtual void Refresh()
        {
            LoadSource();
            ApplyFilter();
            ApplySort();
        }

        /// <summary>
        /// Gets the IEnumerable collection underlying this view.
        /// </summary>
        public System.Collections.IEnumerable CollectionView
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets the IEnumerable collection underlying this Source collection.
        /// </summary>
        public System.Collections.IEnumerable SourceCollection
        {
            get
            {
                return this._sourceCollection;
            }

        }
        #endregion
    }
}
