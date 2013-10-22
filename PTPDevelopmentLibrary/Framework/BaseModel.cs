using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Serialization;
using PTPDevelopmentLibrary.Constants;
using System.Reflection;
namespace PTPDevelopmentLibrary.Framework
{
    /// <summary>
    /// This object represents an all encompassing base model for all technologies (Win Forms, WPF, Silverlight).
    /// </summary>
    [DataContract(Name = "BaseModel", Namespace = SerializationConstants.FrameworkNamespace, IsReference = true)]
    public abstract class BaseModel : IBaseModel, IDataErrorInfo, Validation.IValidationInteraction
#if SILVERLIGHT
        , INotifyDataErrorInfo
#else
        , INotifyPropertyChanging, ICloneable
#endif
    {
        [DataContract(Name = "DefaultModel", Namespace = SerializationConstants.FrameworkNamespace)]
        private class InternalDefaultModel : BaseModel
        { }

        /// <summary>
        /// This object is passed to the <see cref="M:BaseModel.ValueChanged"/> event.
        /// </summary>
        public class ValueChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="oldValue">The previous value of the property.</param>
            /// <param name="newValue">The current value of the property.</param>
            /// <param name="propertyName">The name of the property.</param>
            public ValueChangedEventArgs(object oldValue, object newValue, string propertyName)
            {
                OldValue = oldValue;
                NewValue = newValue;
                PropertyName = propertyName;
            }

            /// <summary>
            /// Gets the previous value of the property.
            /// </summary>
            public object OldValue { get; private set; }
            /// <summary>
            /// Gets the current value of the property.
            /// </summary>
            public object NewValue { get; private set; }
            /// <summary>
            /// Gets the name of the property.
            /// </summary>
            public string PropertyName { get; private set; }
        }

        /// <summary>
        /// This object is passed to the <see cref="M:BaseModel.ValueChanging"/> event.
        /// </summary>
        public class ValueChangingEventArgs : ValueChangedEventArgs
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="oldValue">The current value of the property.</param>
            /// <param name="newValue">The value the property will be changed too.</param>
            /// <param name="propertyName">The name of the property.</param>
            public ValueChangingEventArgs(object oldValue, object newValue, string propertyName)
                : base(oldValue, newValue, propertyName)
            {
                Cancel = false;
            }

            /// <summary>
            /// Gets or sets a value that determins wether the change will happen or not.
            /// </summary>
            public bool Cancel { get; set; }
        }

        //Fields
        Validation.ValidationErrorCollection _validationCollection;
        Validation.ValidationStates _validationState;
#if SILVERLIGHT
        bool _hasErrors;
#endif

        //Events
        PropertyChangedEventHandler _propertyChangedEvent;
#if SILVERLIGHT
        EventHandler<DataErrorsChangedEventArgs> _errorsChangedEvent;
#else
        PropertyChangingEventHandler _propertyChangingEvent;
#endif
        EventHandler<ValueChangingEventArgs> _valueChangingEvent;
        EventHandler<ValueChangedEventArgs> _valueChangedEvent;


        /// <summary>
        /// Constructor.
        /// </summary>
        public BaseModel()
        {
            Deserializing(default(StreamingContext));
            Deserialized(default(StreamingContext));
        }

        /// <summary>
        /// Gets the current <see cref="Validation.ValidationStates"/> state of this object's underlying <see cref="Validation.IValidationManager"/>
        /// </summary>
        [Display(Name = "Validation State", AutoGenerateField = false)]
        [Bindable(true, BindingDirection.OneWay)]
        [Editable(false)]
        public PTPDevelopmentLibrary.Validation.ValidationStates ValidationState
        {
            get
            {
                return _validationState;
            }
            private set
            {
                ChangeValue(ref _validationState, value, "ValidationState");
            }
        }

        /// <summary>
        /// Implementors can override this method to signal to the Validation
        /// Engine whether or not asyncronos or syncronous validation is needed.
        /// </summary>
        [Display(Name = "User Asyncronous Validation", AutoGenerateField = false)]
        [Bindable(true, BindingDirection.OneWay)]
        [Editable(false)]
        protected virtual bool UseAsyncValidation { get { return false; } }

        /// <summary>
        /// This event is called when <see cref="M:BaseModel.ChangeValue{T}"/> is called.
        /// It is provided to extend the functionality of the object since <see cref="M:BaseModel.ChangeValue{T}"/>
        /// is a closed method.  An implementer can cancel the change by setting the
        /// <see cref="P:BaseModel.ValueCHangingEventArgs.Cancel"/> property to true.
        /// </summary>
        protected event EventHandler<ValueChangingEventArgs> ValueChanging
        {
            add
            {
                _valueChangingEvent += value;
            }
            remove
            {
                _valueChangingEvent -= value;
            }
        }
        /// <summary>
        /// This event is called when <see cref="M:BaseModel.ChangeValue{T}"/> changes the target value.
        /// It is provided to extend the functionality of the object since <see cref="M:BaseModel.ChangeValue{T}"/>
        /// is a closed method.
        /// </summary>
        protected event EventHandler<ValueChangedEventArgs> ValueChanged
        {
            add
            {
                _valueChangedEvent += value;
            }
            remove
            {
                _valueChangedEvent -= value;
            }
        }

        /// <summary>
        /// Call this method to handle the assignment of a value within a Property's set asscessor.
        /// </summary>
        /// <example>
        /// <![CDATA[
        ///     public int TestInteger
        ///     {
        ///         get
        ///         {
        ///             return _testInteger;
        ///         }
        ///         set
        ///         {
        ///             ChangeValue(ref _testInteger, value, "TestInteger");
        ///         }
        ///     }
        /// ]]>
        /// </example>
        /// <typeparam name="T">The Type of the value being changed.</typeparam>
        /// <param name="target">A reference to the field holding the current value.</param>
        /// <param name="value">The new value.</param>
        /// <param name="property">The name of the property being changed.</param>
        /// <returns>True if the value was changed, and false otherwise.</returns>
        protected bool ChangeValue<T>(ref T target, T value, string property) 
        {
            //Run the initial changing event.
            if(_valueChangingEvent != null)
            {
                ValueChangingEventArgs changingEventArgs = new ValueChangingEventArgs(target, value, property);
                _valueChangingEvent(this, changingEventArgs);

                if(changingEventArgs.Cancel)
                    return false;
            }

            //error checking.
            if (target == null && value == null)
                return false;
            else if (target != null && target.Equals(value))
                return false;

            //Execute the validation from the validation engine...
            if (Validation.Validator.IsInitialized)
            {
                if (UseAsyncValidation)
                    Validation.Validator.BeginValidateProperty(this, value, property, (a) => Validation.Validator.EndValidateProperty(a), null);
                else
                    Validation.Validator.ValidateProperty(this, value, property);
            }

#if !SILVERLIGHT
            RaisePropertyChanging(property);
#endif
            T oldValue = target;
            target = value;

            if (this._valueChangedEvent != null)
                this._valueChangedEvent(this, new ValueChangedEventArgs(oldValue, value, property));

            RaisePropertyChanged(property);

            return true;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Call this method to raise the <see cref="E:PropertyChanging"/> event.
        /// See <see cref="M:ChangeValue{}"/>.
        /// </summary>
        /// <param name="property">The name of the property that is changing.</param>
        protected void RaisePropertyChanging(string property)
        {
            if (_propertyChangingEvent != null)
                _propertyChangingEvent(this, new PropertyChangingEventArgs(property));
        }
#endif

        /// <summary>
        /// Call this method to raise the <see cref="E:PropertyChanged"/> event.
        /// See <see cref="M:ChangeValue{}"/>.
        /// </summary>
        /// <param name="property">The name of the property that has changed.</param>
        protected void RaisePropertyChanged(string property)
        {
            if (_propertyChangedEvent != null)
                _propertyChangedEvent(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Called before a serialization engine deserializes this object.
        /// </summary>
        /// <param name="sc">A <see cref="StreamingContext"/> object describing the type of serialization taking place.</param>
        [OnDeserializing]
        internal void Deserializing(StreamingContext sc)
        {
            _validationCollection = new Validation.ValidationErrorCollection(this);
            _validationState = Validation.ValidationStates.Valid;
            OnDeserializing(sc);
        }
        /// <summary>
        /// Called after a serialization engine deserializes this object.
        /// </summary>
        /// <param name="sc">A <see cref="StreamingContext"/> object describing the type of serialization taking place.</param>
        [OnDeserialized]
        internal void Deserialized(StreamingContext sc)
        {
            OnDeserialized(sc);
        }
        /// <summary>
        /// Called before a serialization engine serializes this object.
        /// </summary>
        /// <param name="sc">A <see cref="StreamingContext"/> object describing the type of serialization taking place.</param>
        [OnSerializing]
        internal void Serializing(StreamingContext sc)
        {
            OnSerializing(sc);
        }
        /// <summary>
        /// Called after a serialization engine serializes this object.
        /// </summary>
        /// <param name="sc">A <see cref="StreamingContext"/> object describing the type of serialization taking place.</param>
        [OnSerialized]
        internal void Serialized(StreamingContext sc)
        {
            OnSerialized(sc);
        }

        /// <summary>
        /// Inheriting objects can override this method to add logic to the deserializing process.
        /// </summary>
        /// <param name="sc">A <see cref="StreamingContext"/> object describing the type of serialization taking place.</param>
        protected virtual void OnDeserializing(StreamingContext sc)
        { }

        /// <summary>
        /// Inheriting objects can override this method to add logic to the deserialized process.
        /// </summary>
        /// <param name="sc">A <see cref="StreamingContext"/> object describing the type of serialization taking place.</param>
        protected virtual void OnDeserialized(StreamingContext sc)
        { }

        /// <summary>
        /// Inheriting objects can override this method to add logic to the serializing process.
        /// </summary>
        /// <param name="sc">A <see cref="StreamingContext"/> object describing the type of serialization taking place.</param>
        protected virtual void OnSerializing(StreamingContext sc)
        { }

        /// <summary>
        /// Inheriting objects can override this method to add logic to the serialized process.
        /// </summary>
        /// <param name="sc">A <see cref="StreamingContext"/> object describing the type of serialization taking place.</param>
        protected virtual void OnSerialized(StreamingContext sc)
        { }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// This event is called whenever the value of a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                _propertyChangedEvent += value;
            }
            remove
            {
                _propertyChangedEvent -= value;
            }
        }

        #endregion

        #region IDataErrorInfo Members

        /// <summary>
        /// Gets a description of the current validation errors for the object.
        /// </summary>
        [Display(Name = "Error", AutoGenerateField = false)]
        [Bindable(true, BindingDirection.OneWay)]
        [Editable(false)]
        public string Error
        {
            get 
            {
                return (_validationCollection as IDataErrorInfo).Error;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                return (_validationCollection as IDataErrorInfo)[columnName];
            }
        }

        #endregion

#if SILVERLIGHT
        #region INotifyDataErrorInfo Members

        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add { _errorsChangedEvent += value; }
            remove { _errorsChangedEvent -= value; }
        }

        System.Collections.IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return _validationCollection.GetErrors(propertyName);
        }

        bool INotifyDataErrorInfo.HasErrors
        {
            get { return _hasErrors; }
        }

        #endregion
#else
        #region INotifyPropertyChanging Members

        /// <summary>
        /// This event is called whenever the value of a property is about to change.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging
        {
            add
            {
                _propertyChangingEvent += value;
            }
            remove
            {
                _propertyChangingEvent -= value;
            }
        }

        #endregion
#endif

        #region IValidationInteraction Members

        Validation.ValidationStates Validation.IValidationInteraction.ValidationState
        {
            set { ValidationState = value; }
        }
#if !SILVERLIGHT
        void Validation.IValidationInteraction.RaiseErrorsChanging(string propertyName)
        {
            RaisePropertyChanging("Error");
        }
#endif

        void Validation.IValidationInteraction.RaiseErrorsChanged(string propertyName)
        {
            RaisePropertyChanged("Error");
#if SILVERLIGHT
            if (_errorsChangedEvent != null)
                _errorsChangedEvent(this, new DataErrorsChangedEventArgs(propertyName));
#endif
        }

#if SILVERLIGHT
        bool Validation.IValidationInteraction.HasErrors
        {
            set
            {
                if (_hasErrors != value)
                {
                    _hasErrors = value;
                    RaisePropertyChanged("HasErrors");
                }
            }
        }
#endif

        Validation.ValidationErrorCollection Validation.IValidationInteraction.ValidationErrors
        {
            get { return _validationCollection; }
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// This method returns a shallow copy of the existing object.  
        /// NOTE: The events are nulled however.  All other references should still exists between the returned
        /// object and the existing object.
        /// NOTE TO IMPLEMENTERS:  The copy is created with a <see cref="M:Object.MemberwiseClone"/> call which only 
        /// creates a shallow copy.  This means that all reference types will be the same between the existing an resulting object.
        /// The base implementation nulls the events of <see cref="BaseModel"/>, however any events that exist in your implementation
        /// need to be nulled as well.
        /// </summary>
        /// <returns>A shallow copy of the existing object.</returns>
        public virtual object Clone()
        {
            BaseModel result = (BaseModel)this.MemberwiseClone();
            result.OnDeserializing(default(StreamingContext));
            result.OnDeserialized(default(StreamingContext));
            result._propertyChangedEvent = null;
            result._valueChangedEvent = null;
            result._valueChangingEvent = null;
#if SILVERLIGHT
            result._errorsChangedEvent = null;
#else
            result._propertyChangingEvent = null;
#endif
            return result;
        }

        #endregion

        /// <summary>
        /// Gets an empty basic model.
        /// </summary>
        public static BaseModel DefaultModel
        {
            get
            {
                return _internalDefaultModel;
            }
        }
        private static InternalDefaultModel _internalDefaultModel = new InternalDefaultModel();
    }
}