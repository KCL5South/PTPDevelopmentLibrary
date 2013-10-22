using System.ComponentModel;
using System;
using System.Windows;

namespace PTPDevelopmentLibrary.Framework
{
    /// <summary>
    /// The base presenter object for the MVP methodology as interpreted by Mitch.
    /// </summary>
    /// <typeparam name="TView">The type of the target view.</typeparam>
    /// <typeparam name="TModel">The type of the target model.  Notice that the model must inherit from <see cref="INotifyPropertyChanged"/>.</typeparam>
    public abstract class Presenter<TView, TModel> : IPresenter<TView, TModel>, IBaseModel
        where TView : class
        where TModel : INotifyPropertyChanged
#if !SILVERLIGHT
        , INotifyPropertyChanging
#endif
    {
        private bool _isViewModel;
        private TView _view;
        private TModel _model;

        /// <summary>
        /// Constructor
        /// </summary>
        public Presenter()
        {
            IsViewModel = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The target view.</param>
        /// <param name="model">The target model.</param>
        public Presenter(TView view, TModel model)
        {
            IsViewModel = false;
            Model = model;
            View = view;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The target view.</param>
        /// <param name="model">The target model.</param>
        /// <param name="isViewModel">A boolean value indicating weather the <see cref="P:FrameworkElement.DataContext"/>
        /// property of the view is set to the model or the presenter.  If it is set to the presenter then that makes the 
        /// presenter a View-Model within the MVVM framework.</param>
        public Presenter(TView view, TModel model, bool isViewModel)
        {
            IsViewModel = isViewModel;
            Model = model;
            View = view;
        }
        /// <summary>
        /// Gets or sets a boolean value indicating weather or not the Presenter is considered a "View Model"
        /// <remarks>
        /// If this is true, then the presenter itself is the databound model to the view, and if it is false
        /// then the model is the model for the view.
        /// </remarks>
        /// </summary>
        public bool IsViewModel
        {
            get
            {
                return _isViewModel;
            }
            set
            {
                if (_isViewModel != value)
                {
#if !SILVERLIGHT
                    RaisePropertyChanging("IsViewModel");
#endif
                    _isViewModel = value;
                    RaisePropertyChanged("IsViewModel");
                }
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Inherited objects will override this method so that the object will be notified when the model is changing.
        /// </summary>
        /// <param name="sender">A reference to the model.</param>
        /// <param name="e">A <see cref="System.ComponentModel.PropertyChangingEventArgs"/> object representing the event arguments.</param>
        protected virtual void Model_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {

        }
#endif
        /// <summary>
        /// Inherited objects will override this method so that the object will be notified when the model changes.
        /// </summary>
        /// <param name="sender">A reference to the model.</param>
        /// <param name="e">A <see cref="System.ComponentModel.PropertyChangedEventArgs"/> object representing the event arguments.</param>
        protected virtual void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        { }

        /// <summary>
        /// Call this method to signify when a property is changed.
        /// </summary>
        /// <param name="property">The name of the property that has changed.</param>
        protected void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

#if !SILVERLIGHT
        /// <summary>
        /// Call this method to signify when a property is changing.
        /// </summary>
        /// <param name="property">The name of the property that is changing.</param>
        protected void RaisePropertyChanging(string property)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(property));
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        private void SetupViewAndModel()
        {
            if (_view is FrameworkElement)
            {
                if (_isViewModel)
                    (_view as FrameworkElement).DataContext = this;
                else
                    (_view as FrameworkElement).DataContext = _model;
            }
        }

        /// <summary>
        /// This event is fired when <see cref="P:View"/> is changing.
        /// </summary>
        protected event EventHandler OnViewChanging;
        /// <summary>
        /// This event is fired after <see cref="P:View"/> is changed.
        /// </summary>
        protected event EventHandler OnViewChanged;
        /// <summary>
        /// This event is fired when <see cref="P:Model"/> is changing.
        /// </summary>
        protected event EventHandler OnModelChanging;
        /// <summary>
        /// This event is fired after <see cref="P:Model"/> is changed.
        /// </summary>
        protected event EventHandler OnModelChanged;

        #region IPresenter<TView,TModel> Members

        /// <summary>
        /// Gets a reference to the target view.
        /// </summary>
        public TView View
        {
            get
            {
                return _view;
            }
            protected set
            {
                if (_view != value)
                {
#if !SILVERLIGHT
                    RaisePropertyChanging("View");
#endif
                    if (OnViewChanging != null)
                        OnViewChanging(this, EventArgs.Empty);
                    _view = value;
                    RaisePropertyChanged("View");
                    SetupViewAndModel();
                    if (OnViewChanged != null)
                        OnViewChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets a reference to the target model.
        /// </summary>
        public TModel Model
        {
            get
            {
                return _model;
            }
            protected set
            {
                if (!object.ReferenceEquals(_model, value))
                {
#if !SILVERLIGHT
                    RaisePropertyChanging("Model");
#endif
                    if (OnModelChanging != null)
                        OnModelChanging(this, EventArgs.Empty);
                    if (_model != null)
                    {
#if !SILVERLIGHT
                        _model.PropertyChanging -= Model_PropertyChanging;
#endif
                        _model.PropertyChanged -= Model_PropertyChanged;
                    }
                    _model = value;
                    RaisePropertyChanged("Model");
                    SetupViewAndModel();
                    if (_model != null)
                    {
#if !SILVERLIGHT
                        _model.PropertyChanging += Model_PropertyChanging;
#endif
                        _model.PropertyChanged += Model_PropertyChanged;
                    }
                    if (OnModelChanged != null)
                        OnModelChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region IPresenter Members

        object IPresenter.View
        {
            get
            {
                return View;
            }
            set
            {
                View = (TView)value;
            }
        }

        object IPresenter.Model
        {
            get
            {
                return Model;
            }
            set
            {
                Model = (TModel)value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// This event is fired whenever an property is changed within this object.
        /// Inherited from <see cref="INotifyPropertyChanged"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

#if !SILVERLIGHT
        #region INotifyPropertyChanging Members
        /// <summary>
        /// This event is fired when a property is about the change.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion
#endif
    }
}