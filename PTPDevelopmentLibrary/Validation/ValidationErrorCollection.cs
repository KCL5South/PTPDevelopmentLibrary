using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System;
using PTPDevelopmentLibrary.Framework;
namespace PTPDevelopmentLibrary.Validation
{
#if SILVERLIGHT
    /// <summary>
    /// This object represents a collection of validation errors.
    /// </summary>
    /// <remarks>
    /// This object implements <see cref="IDataErrorInfo"/> and <see cref="INotifyDataErrorInfo"/> so
    /// subscribing to it's events and using it's methods will help with validation error reporting.
    /// </remarks>
#else
    /// <summary>
    /// This object represents a collection of validation errors.
    /// </summary>
    /// <remarks>
    /// This object implements <see cref="IDataErrorInfo"/> so using it's methods will help with validation error reporting.
    /// </remarks>
#endif
    public class ValidationErrorCollection : DictionaryModel<string, ICollection<object>>, IDataErrorInfo
#if SILVERLIGHT
        , INotifyDataErrorInfo
#endif
    {
        IValidationInteraction _parentModel;
        ValidationStates _validationState;
        List<Guid> _asyncOperations;
        object _syncObject;

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidationErrorCollection()
        {
            _asyncOperations = new List<Guid>();
            _syncObject = new object();
        }

        /// <summary>
        /// Use this method to replace the validation issues with the given <paramref name="results"/>.
        /// </summary>
        /// <param name="member">The name of the member who's validation issues need to be reset.</param>
        /// <param name="results">An <see cref="IEnumerable{T}"/> representing the new validation issues for the member.</param>
        public void SetErrors(string member, IEnumerable<object> results)
        {
            if (string.IsNullOrWhiteSpace(member))
                member = string.Empty;
            if (results == null)
                results = Enumerable.Empty<object>();

            if (this.ContainsKey(member))
            {
                ICollection<object> col = this[member];
                col.Clear();
                foreach (object result in results)
                    col.Add(result);
            }
            else
                this.Add(member, new List<object>(results));

            NotifyErrorsChanged(member);
        }

        /// <summary>
        /// Use this method to remove issues from a given member.
        /// </summary>
        /// <param name="member">The name of the member who contains issues that are being removed.</param>
        /// <param name="results">An <see cref="IEnumerable{T}"/> representing the validation issues to remove.</param>
        public void RemoveErrors(string member, IEnumerable<object> results)
        {
            if (string.IsNullOrWhiteSpace(member))
                member = string.Empty;
            if (results == null)
                results = Enumerable.Empty<object>();

            bool removed = false;

            if (this.ContainsKey(member))
            {
                foreach (object result in results)
                    removed |= this[member].Remove(result);
            }

            if(removed)
                NotifyErrorsChanged(member);
        }

        /// <summary>
        /// Gets the <see cref="ValidationStates"/> of the collection.
        /// </summary>
        public ValidationStates ValidationState
        {
            get
            {
                return _validationState;
            }
            private set
            {
                if (_validationState != value)
                {
                    _validationState = value;
                    if (_parentModel != null)
                        _parentModel.ValidationState = value;
                }
            }
        }

        internal ValidationErrorCollection(IValidationInteraction model)
        {
            _parentModel = model;
            _asyncOperations = new List<Guid>();
            _syncObject = new object();
        }

        internal Guid BeginAsyncOperation()
        {
            lock (_syncObject)
            {
                Guid result = Guid.NewGuid();
                _asyncOperations.Add(result);
                SetValidationState();
                return result;
            }
        }

        internal void EndAsyncOperation(Guid asyncId)
        {
            lock (_syncObject)
            {
                if (_asyncOperations.Contains(asyncId))
                {
                    _asyncOperations.Remove(asyncId);
                    SetValidationState();
                }
                else
                    throw new InvalidOperationException(string.Format("asyncId was not received form this instance of {0}", this.GetType()));
            }
        }

        /// <summary>
        /// Overriden.
        /// Effectivly clears the errors from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            bool hasItems = false;
            IEnumerable<KeyValuePair<string, ICollection<object>>> kvPairs = null;
            lock (_syncObject)
            {
                kvPairs = this.ToArray();
            }

            foreach (KeyValuePair<string, ICollection<object>> kvp in kvPairs)
            {
                //We clear the collection within the dictionary instead of the
                //dictionary itself, because we continue to use the collection
                //for future errors.
                hasItems = kvp.Value.Count > 0;
                kvp.Value.Clear();

                if (hasItems)
                    NotifyErrorsChanged(kvp.Key);
            }
        }

        /// <summary>
        /// Overriden.
        /// Adds a new member validation error collection to the collection
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="item">The item to insert.</param>
        protected override void InsertItem(int index, KeyValuePair<string, ICollection<object>> item)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
                base.InsertItem(index, new KeyValuePair<string, ICollection<object>>(string.Empty, item.Value));
            else
                base.InsertItem(index, item);

            if (item.Value != null && item.Value.Count > 0)
                NotifyErrorsChanged(item.Key);
        }

        /// <summary>
        /// Overriden.
        /// Removes an item at a given index.
        /// </summary>
        /// <param name="index">The index to remove the item at.</param>
        protected override void RemoveItem(int index)
        {
            string memberName = null;
            ICollection<object> col = null;
            bool isValid = false;

            if (index > -1 && index < this.Count)
            {
                memberName = this[index].Key;
                col = this[index].Value;
                isValid = col != null && col.Count > 0;
            }
#if DEBUG
            else
                System.Diagnostics.Debug.WriteLine(string.Format("Core.Validation.ValidationErrorCollection.RemoveItem: The index ({0}) is invalid and nothing will be removed.", index));
#endif

            base.RemoveItem(index);

            if (isValid)
                NotifyErrorsChanged(memberName);
        }

        /// <summary>
        /// Overriden.
        /// Resets an item at a given index.
        /// </summary>
        /// <param name="index">The index to reset.</param>
        /// <param name="item">The item to set to.</param>
        protected override void SetItem(int index, KeyValuePair<string, ICollection<object>> item)
        {
            string memberName = null;
            bool isValid = false;

            if (index > -1 && index < this.Count)
            {
                memberName = this[index].Key;
                isValid = true;
            }
#if DEBUG
            else
                System.Diagnostics.Debug.WriteLine(string.Format("PTPDevelopmentLibrary.Validation.ValidationErrorCollection.SetItem: The index ({0}) is invalid and nothing will be set.", index));
#endif

            base.SetItem(index, item);

            if (isValid)
                NotifyErrorsChanged(memberName);
        }

        private void SetValidationState()
        {
#if SILVERLIGHT
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
#endif
            if (_asyncOperations.Count() > 0)
                ValidationState = ValidationStates.Validating;
            else if (this.Any(a => a.Value.Count > 0))
                ValidationState = ValidationStates.Invalid;
            else
                ValidationState = ValidationStates.Valid;
#if SILVERLIGHT
            });
#endif
        }

        private void NotifyErrorsChanged(string property)
        {
#if SILVERLIGHT
                if (ErrorsChanged != null)
                    ErrorsChanged(this, new DataErrorsChangedEventArgs(property));
#endif
                if (_parentModel != null)
                {
                    _parentModel.RaiseErrorsChanged(property);
#if SILVERLIGHT
                    _parentModel.HasErrors = this.Any(a => a.Value.Count > 0);
#endif
                }

            SetValidationState();
        }

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get
            {
                return string.Join(Environment.NewLine, from kvp in this
                                                        from error in kvp.Value
                                                        select error);

            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get 
            {
                if (this.ContainsKey(columnName))
                    return string.Join(Environment.NewLine, from error in this[columnName]
                                                            select error);
                else
                    return string.Empty;
            }
        }

        #endregion

        #region INotifyDataErrorInfo Members

#if SILVERLIGHT

        /// <summary>
        /// This event is fired whenever the validation errors for a given member changes.
        /// </summary>
        public event System.EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Call this method to retrieve the collection of validation errors for a given property.
        /// </summary>
        /// <param name="propertyName">The property to retreive the validation errors for.</param>
        /// <returns>A <see cref="System.Collections.IEnumerable"/> containing to validation errors.</returns>
        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                propertyName = string.Empty;

            if (this.ContainsKey(propertyName))
                return this[propertyName];
            else
            {
                this.Add(propertyName, new List<object>());
                return this[propertyName];
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not there are any validation errors.
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return this.Values.Any(a => a.Count() > 0);
            }
        }

#endif

        #endregion
    }
}