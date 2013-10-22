using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using Microsoft.Practices.Unity;
using PTPDevelopmentLibrary.Framework;
namespace PTPDevelopmentLibrary.Controls
{
    /// <summary>
    /// A control that can be used within xaml to present a MVP or MVVP.
    /// </summary>
    public class PresenterContainer : ContentControl
    {
        /// <summary>
        /// This dependency property allows the <see cref="P:PresenterContainer.View"/> 
        /// property to be bindable.
        /// </summary>
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register("View", typeof(object), typeof(PresenterContainer), new PropertyMetadata(ViewProperty_Changed));
        /// <summary>
        /// This dependency property allows the <see cref="P:PresenterContainer.Model"/> 
        /// property to be bindable.
        /// </summary>
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(object), typeof(PresenterContainer), new PropertyMetadata(ModelProperty_Changed));
        /// <summary>
        /// This dependency property allows the <see cref="P:PresenterContainer.Presenter"/> 
        /// property to be bindable.
        /// </summary>
        public static readonly DependencyProperty PresenterProperty = DependencyProperty.Register("Presenter", typeof(object), typeof(PresenterContainer), new PropertyMetadata(PresenterProperty_Changed));

        private static IUnityContainer Container;
        /// <summary>
        /// This static method should be called before any <see cref="PresenterContainer"/>
        /// is instantiated.  
        /// </summary>
        /// <remarks>
        /// The <see cref="IUnityContainer"/> is needed in order to build
        /// a presenter up. So, if none of your presenters need to be built up, then
        /// 'technically' you wouldn't need to call this method.
        /// </remarks>
        /// <param name="container">A <see cref="IUnityContainer"/> object used to build up the presenter.</param>
        public static void InitializePresenterContainer(IUnityContainer container)
        {
            Container = container;
        }

        private static void ViewProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PresenterContainer container = (PresenterContainer)d;

            if (e.NewValue != null)
            {
                if (!(e.NewValue is FrameworkElement))
                    throw new ArgumentException("Only objects of type System.Windows.FrameworkElement can be passed to the View property.");

                if (e.NewValue != null && container.Presenter != null)
                    (container.Presenter as IPresenter).View = e.NewValue;
            }

            container.Content = e.NewValue;
        }
        private static void ModelProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PresenterContainer container = (PresenterContainer)d;
            if (e.NewValue != null && container.Presenter != null)
                (container.Presenter as IPresenter).Model = e.NewValue;
        }
        private static void PresenterProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                if (!(e.NewValue is IPresenter))
                    throw new ArgumentException("Only objects of type IPresenter can be passed to the Presenter property.");

                IPresenter presenter = (IPresenter)e.NewValue;
                PresenterContainer container = (PresenterContainer)d;
                if (presenter != null)
                {
                    presenter.Model = container.Model;
                    presenter.View = container.View;

                    if (container.BuildUp && Container != null)
                        Container.BuildUp(presenter.GetType(), presenter);
                }
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public PresenterContainer()
        {
            SetValue(ViewProperty, null);
            SetValue(ModelProperty, null);
            SetValue(PresenterProperty, null);
        }

        /// <summary>
        /// Gets or sets the presenter for this container.
        /// </summary>
        public object Presenter
        {
            get
            {
                return GetValue(PresenterProperty);
            }
            set
            {
                SetValue(PresenterProperty, value);
            }
        }
        /// <summary>
        /// Gets or sets the view for this container.
        /// </summary>
        public object View
        {
            get
            {
                return GetValue(ViewProperty);
            }
            set
            {
                SetValue(ViewProperty, value);
            }
        }
        /// <summary>
        /// Gets or sets the model for this container.
        /// </summary>
        public object Model
        {
            get
            {
                return GetValue(ModelProperty);
            }
            set
            {
                SetValue(ModelProperty, value);
            }
        }
        /// <summary>
        /// Gets or sets a value indicating that when <see cref="P:PresenterContainer.Presenter"/> is set, 
        /// the presenter is built up using dependency injection via the <see cref="IUnityContainer"/> that 
        /// was supplied when <see cref="M:PresenterContainer.InitializePresenterContainer"/> was called.
        /// </summary>
        public bool BuildUp { get; set; }
    }
}