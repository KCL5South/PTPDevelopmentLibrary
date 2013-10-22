using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using PTPDevelopmentLibrary.Constants;
namespace PTPDevelopmentLibrary.Framework
{
    /// <summary>
    /// A basic collection implementation that implements the <see cref="INotifyCollectionChanged"/> needed for
    /// WPF and Silverlight's binding engines.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CollectionDataContract(Name = "CollectionModel_{0}", Namespace = SerializationConstants.FrameworkNamespace)]
    public class CollectionModel<T> : Collection<T>, IBaseCollectionModel<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CollectionModel()
            : base()
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">An <see cref="IList{T}"/> that will be recreated as a new list.</param>
        public CollectionModel(IList<T> source)
            : base(source)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> that will be recreated as new list.</param>
        public CollectionModel(IEnumerable<T> source)
            : base(new List<T>(source))
        { }

        /// <summary>
        /// Call this method to clear the collection of all items.
        /// </summary>
        protected override void ClearItems()
        {
            bool countChanged = this.Count > 0;
#if !SILVERLIGHT
            if(PropertyChanging != null && countChanged)
                PropertyChanging(this, new PropertyChangingEventArgs("Count"));
#endif

            base.ClearItems();

            if (PropertyChanged != null && countChanged)
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));

            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Call this method to insert an item into the collection.
        /// </summary>
        /// <param name="index">The position the item should be placed in.</param>
        /// <param name="item">The item to place into the collection.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        protected override void InsertItem(int index, T item)
        {
#if !SILVERLIGHT
            if(PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs("Count"));
#endif

            base.InsertItem(index, item);

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));

            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Call this method to remove an item from the collection.
        /// </summary>
        /// <param name="index">The position the item will be removed from.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        protected override void RemoveItem(int index)
        {
#if !SILVERLIGHT
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs("Count"));
#endif
            T removedItem = this[index];
            base.RemoveItem(index);

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));

            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        /// <summary>
        /// Call this method to replace an item within the collection.
        /// </summary>
        /// <param name="index">The position to replace the item at.</param>
        /// <param name="item">The item to replace with.</param>
        protected override void SetItem(int index, T item)
        {
            T oldItem = this[index];
            base.SetItem(index, item);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }

        /// <summary>
        /// Call this method whenever the collection changes.
        /// </summary>
        /// <param name="e">A <see cref="NotifyCollectionChangedEventArgs"/> object needed for the <see cref="E:CollectionChanged"/> event.</param>
        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        /// <summary>
        /// Call this method whenever a property has changed.
        /// </summary>
        /// <param name="property">A string representing the name of the property that changed.</param>
        protected void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

#if !SILVERLIGHT
        /// <summary>
        /// Call this method whenever a property is about to change.
        /// </summary>
        /// <param name="property">A string representing the name of the property that is changing.</param>
        protected void RaisePropertyChanging(string property)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(property));
        }
#endif

        #region INotifyCollectionChanged Members

        /// <summary>
        /// This event is fired whenever the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// This event is fired whenever a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

#if !SILVERLIGHT
        #region INotifyPropertyChanging Members

        /// <summary>
        /// This even is fired whenever a property is about to change
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion
#endif
    }
}
