using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Media;
namespace PTPDevelopmentLibrary.Controls
{
    /// <summary>
    /// This object can be used to automate state transitions within a Silverlight UI Control.
    /// </summary>
    /// <example>
    /// Here is a quick example on how to use this object to automate your 
    /// state transitions within a user control.
    /// 
    /// NOTE: This object was build around the MVP and MVVP methodologies.  Incorperating it
    /// into another design may not be strait forward.
    /// 
    /// Within your presenter, define an <c>enum</c> that mimics the states within your view.
    /// The names of the <c>enum</c> values must match the state names exactly.
    /// <code>
    ///     <![CDATA[
    ///         [Flags]
    ///         public enum ViewStates
    ///         {
    ///             Open = 1,
    ///             Closed = 2
    ///         }
    ///     ]]>
    /// </code>
    /// 
    /// NOTE: You must use enumerations if you wish the view to be in multiple states at once.
    /// 
    /// Create a property that houses the above <c>enum</c> within your presenter (MVVP) or model (MVP)
    /// so that the View can use the <see cref="P:StateBinder.CurrentState"/> attached property to know
    /// which state it's supposed to be in.
    /// <code>
    ///     <![CDATA[
    ///         public ViewStates CurrentState
    ///         {
    ///             get
    ///             {
    ///                 return _currentState;
    ///             }
    ///             set
    ///             {
    ///                 if(_currentState != null)
    ///                 {
    ///                     _currentState = value;
    ///                     RaisePropertyChanged("CurrentState");
    ///                 }
    ///             }
    ///         }
    ///     ]]>
    /// </code>
    /// <code>
    ///     <![CDATA[
    ///         <UserControl xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///                      x:Class="GPS.ProposalSystem.Views.DefaultView"
    ///                      StateBinder.CurrentState = {Binding CurrentState}/>
    ///     ]]>
    /// </code>
    /// Since the state is bound, whenever it changes, <see cref="StateBinder"/> knows about it and automatically
    /// handles the state transitions for you.
    /// </example>
    public class StateBinder : DependencyObject
    {
        /// <summary>
        /// An attached property that houses the current state of the view.
        /// </summary>
        public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.RegisterAttached("CurrentState", typeof(string), typeof(StateBinder), new PropertyMetadata(CurrentState_Changed));
        /// <summary>
        /// An attached property that indicates wether transitions will be used when the state changes.
        /// </summary>
        public static readonly DependencyProperty UseTransitionsProperty = DependencyProperty.RegisterAttached("UseTransitions", typeof(bool), typeof(StateBinder), new PropertyMetadata(false));
        
        private static void CurrentState_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control && e.NewValue != null)
            {
                Control control = (Control)d;
                string newValue = e.NewValue.ToString();
                bool useTransitions = (bool)control.GetValue(UseTransitionsProperty);

                foreach(string s in newValue.Split(','))
                    VisualStateManager.GoToState(control, s.Trim(), useTransitions);
            }
        }

        /// <summary>
        /// Sets the current state of the given <see cref="DependencyObject"/> (View).
        /// </summary>
        /// <param name="d">A <see cref="DependencyObject"/> that represents the view.</param>
        /// <param name="state">A string that represents the current state.</param>
        public static void SetCurrentState(DependencyObject d, string state)
        {
            d.SetValue(CurrentStateProperty, state);
        }
        /// <summary>
        /// Gets the current state of the given <see cref="DependencyObject"/> (View).
        /// </summary>
        /// <param name="d">A <see cref="DependencyObject"/> that represents the view.</param>
        /// <returns>A string that represents the current state.</returns>
        public static string GetCurrentState(DependencyObject d)
        {
            return (string)d.GetValue(CurrentStateProperty);
        }
        /// <summary>
        /// Sets a value indicating wether the state changes will use transitions.
        /// </summary>
        /// <param name="d">A <see cref="DependencyObject"/> that represents the View.</param>
        /// <param name="value">The value that indicates wether transitions will be used.</param>
        public static void SetUseTransitions(DependencyObject d, bool value)
        {
            d.SetValue(UseTransitionsProperty, value);
        }
        /// <summary>
        /// Gets a value indicating wether the state changes will use transitions.
        /// </summary>
        /// <param name="d">A <see cref="DependencyObject"/> that represents the View.</param>
        /// <returns>The value that indicates wether transitions will be used.</returns>
        public static bool GetUseTransitions(DependencyObject d)
        {
            return (bool)d.GetValue(UseTransitionsProperty);
        }
    }
}