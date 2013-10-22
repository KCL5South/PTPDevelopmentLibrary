using System;
using System.Linq;
using System.Collections.Generic;
using PTPDevelopmentLibrary.Validation.DefaultValidation;
using PTPDevelopmentLibrary.Framework;
using System.Reflection;
namespace PTPDevelopmentLibrary.Validation
{
    /// <summary>
    /// This is the heart of the Validation Engine.  All validation should come through this singleton.
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// Gets the current validator that is used by the engine.
        /// </summary>
        public static IValidator CurrentValidator { get; private set; }

        /// <summary>
        /// Gets the current Manager that is used by the engine.
        /// </summary>
        public static IValidationManager CurrentManager { get; private set; }

        private static bool _isInitialized = false;
        /// <summary>
        /// Gets a boolean representing whether or not the engine is initialized yet.
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        /// <summary>
        /// This method should be called before any of the other methods.  It effectivly initializes the Validation Engine.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">If the Engine has already been initialized, this will be thrown.</exception>
        public static void Initialize()
        {
            if (_isInitialized)
                throw new InvalidOperationException("Initialization of the Validation Engine can only happen once.");

            CurrentValidator = new DataAnnotationsValidator();
            CurrentManager = new DataAnnotationsValidatorManager();

            CurrentValidator.RegisterService(typeof(IValidationManager), CurrentManager);
            CurrentValidator.RegisterService(typeof(IValidator), CurrentValidator);

            _isInitialized = true;
        }

        /// <summary>
        /// This method should be called before any of the other methods.  It effectivly initializes the Validation Engine.
        /// Use this overloaded method to provide your own custom validator and manager.
        /// </summary>
        /// <param name="validator">A <see cref="IValidator"/> representing a custom validator.</param>
        /// <param name="manager">A <see cref="IValidationManager"/> representing a custom manager.</param>
        /// <exception cref="System.InvalidOperationException">If the Engine has already been initialized, this will be thrown.</exception>
        /// <exception cref="System.ArgumentNullException">If validator or manager are null.</exception>
        public static void Initialize(IValidator validator, IValidationManager manager)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Initialization of the Validation Engine can only happen once.");

            if (validator == null)
                throw new ArgumentNullException("validator");
            if (manager == null)
                throw new ArgumentNullException("manager");

            CurrentValidator = validator;
            CurrentManager = manager;

            CurrentValidator.RegisterService(typeof(IValidationManager), CurrentManager);
            CurrentValidator.RegisterService(typeof(IValidator), CurrentValidator);

            _isInitialized = true;
        }

        /// <summary>
        /// This method will validate a <see cref="BaseModel"/>.  Also, if the children of the given
        /// model need to be validated at the same time, it can do that as well.
        /// </summary>
        /// <param name="model">A <see cref="BaseModel"/> to validate.</param>
        /// <param name="validateChildren">If the chilren of the given model should be validated as well, supply true.</param>
        /// <returns>A boolean value indicating whether the validation was successful or not.</returns>
        /// <exception cref="InvalidOperationException">If the engine is not initialized</exception>
        public static bool ValidateModel(BaseModel model, bool validateChildren = false)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must initialize the engine before it is used.");

            IEnumerable<object> validationErrors = CurrentValidator.ValidateModel(model);
            ValidationErrorCollection errorCollection = (model as IValidationInteraction).ValidationErrors;

            CurrentManager.SetErrors(errorCollection, validationErrors);

            //If we're validating the children as well.
            if (validateChildren)
            {
                IEnumerable<PropertyInfo> properties = model.GetType().GetProperties().Where(a => a.CanRead && typeof(BaseModel).IsAssignableFrom(a.PropertyType));
                IEnumerable<PropertyInfo> colProperties = model.GetType().GetProperties().Where(a => a.CanRead && typeof(IEnumerable<BaseModel>).IsAssignableFrom(a.PropertyType));

                foreach (PropertyInfo pi in properties)
                    ValidateModel((BaseModel)pi.GetValue(model, null), validateChildren);
                foreach (PropertyInfo pi in colProperties)
                {
                    foreach (BaseModel bm in (IEnumerable<BaseModel>)pi.GetValue(model, null))
                        ValidateModel(bm, validateChildren);
                }
            }

            return validationErrors.Count() == 0;
        }

        /// <summary>
        /// This method will validate a property value in a <see cref="BaseModel"/> object.
        /// </summary>
        /// <param name="model">A <see cref="BaseModel"/> to validate.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <returns>A boolean value indicating whether the validation was successful or not.</returns>
        /// <exception cref="InvalidOperationException">If the engine is not initialized</exception>
        public static bool ValidateProperty(BaseModel model, object value, string propertyName)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must initialize the engine before it is used.");

            IEnumerable<object> validationErrors = CurrentValidator.ValidateProperty(model, value, propertyName);
            ValidationErrorCollection errorCollection = (model as IValidationInteraction).ValidationErrors;

            CurrentManager.SetMemberErrors(errorCollection, validationErrors, propertyName);

            return validationErrors.Count() == 0;
        }

        /// <summary>
        /// This method will validate a <see cref="BaseModel"/> asyncronously.  Also, if the children of the given
        /// model need to be validated at the same time, it can do that as well.
        /// </summary>
        /// <param name="target">A <see cref="BaseModel"/> to validate.</param>
        /// <param name="validateChildren">If the chilren of the given model should be validated as well, supply true.</param>
        /// <param name="callback">A <see cref="AsyncCallback"/> that will be called when the operation completes.</param>
        /// <param name="state">A user defined object providing state to the asycronous operation.</param>
        /// <returns>A <see cref="IAsyncResult"/> object representing the asyncronous operation.</returns>
        /// <exception cref="InvalidOperationException">If the engine is not initialized</exception>
        public static IAsyncResult BeginValidateModel(BaseModel target, bool validateChildren, AsyncCallback callback, object state)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must initialize the engine before it is used.");

            return new ValidateModelAsyncResult(CurrentValidator, target, validateChildren, callback, state);
        }

        /// <summary>
        /// This method should only be called after <see cref="M:Validator.BeginValidateModel"/> and with the return result
        /// of that method.
        /// </summary>
        /// <param name="result">A <see cref="IAsyncResult"/> representing the completed operation.</param>
        /// <returns>A boolean value indicating whether the validation was successful or not.</returns>
        /// <exception cref="InvalidOperationException">If the engine is not initialized</exception>
        public static bool EndValidateModel(IAsyncResult result)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must initialize the engine before it is used.");

            if (result is ValidateModelAsyncResult)
            {
                ValidateModelAsyncResult newResult = result as ValidateModelAsyncResult;
                if (newResult.Exception != null)
                    throw newResult.Exception;

                IEnumerable<object> validationResult = newResult.Result;
                
                
#if SILVERLIGHT
System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate
{
#endif
                CurrentManager.SetErrors((newResult.Target as IValidationInteraction).ValidationErrors,
                                          validationResult);
#if SILVERLIGHT
});
#endif

                return validationResult.Count() == 0;
            }
            else
                throw new InvalidOperationException(string.Format("result must be of type {0}", typeof(ValidateModelAsyncResult)));
        }

        /// <summary>
        /// This method will validate a property value in a <see cref="BaseModel"/> object asyncronously.
        /// </summary>
        /// <param name="model">A <see cref="BaseModel"/> to validate.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <param name="callback">A <see cref="AsyncCallback"/> that will be called when the operation completes.</param>
        /// <param name="state">A user defined object providing state to the asycronous operation.</param>
        /// <returns>A <see cref="IAsyncResult"/> object representing the asyncronous operation.</returns>
        /// <exception cref="InvalidOperationException">If the engine is not initialized</exception>
        public static IAsyncResult BeginValidateProperty(BaseModel model, object value, string propertyName, AsyncCallback callback, object state)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must initialize the engine before it is used.");

            return new ValidatePropertyAsyncResult(CurrentValidator, model, value, propertyName, callback, state);
        }

        /// <summary>
        /// This method should only be called after <see cref="M:Validator.BeginValidateProperty"/> and with the return result
        /// of that method.
        /// </summary>
        /// <param name="result"></param>
        /// <returns>A boolean value indicating whether the validation was successful or not.</returns>
        /// <exception cref="InvalidOperationException">If the engine is not initialized</exception>
        public static bool EndValidateProperty(IAsyncResult result)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must initialize the engine before it is used.");

            if (result is ValidatePropertyAsyncResult)
            {
                ValidatePropertyAsyncResult newResult = result as ValidatePropertyAsyncResult;
                if (newResult.Exception != null)
                    throw newResult.Exception;

                IEnumerable<object> validationResult = newResult.Result;
                
#if SILVERLIGHT
System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate
{
#endif
                CurrentManager.SetMemberErrors((newResult.Target as IValidationInteraction).ValidationErrors,
                                               validationResult, newResult.PropertyName);
#if SILVERLIGHT
});
#endif

                return validationResult.Count() == 0;
            }
            else
                throw new InvalidOperationException(string.Format("result must be of type {0}", typeof(ValidateModelAsyncResult)));
        }

        /// <summary>
        /// This method will register a service with the current validator.
        /// </summary>
        /// <param name="serviceType">A <see cref="Type"/> representing the type of the service</param>
        /// <param name="service">The service object itself.</param>
        /// <returns>A boolean value indicating whether the registration was successful or not.</returns>
        /// <exception cref="InvalidOperationException">If the engine is not initialized</exception>
        public static bool RegisterService(Type serviceType, object service)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must initialize the engine before it is used.");

            return CurrentValidator.RegisterService(serviceType, service);
        }

        /// <summary>
        /// This method will unregister a service from the current validator.
        /// </summary>
        /// <param name="serviceType">A <see cref="Type"/> representing the type of the service</param>
        /// <returns>A boolean value indicating whether the unregistration was successful or not.</returns>
        /// <exception cref="InvalidOperationException">If the engine is not initialized</exception>
        public static bool UnregisterService(Type serviceType)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must initialize the engine before it is used.");

            return CurrentValidator.UnregisterService(serviceType);
        }

        /// <summary>
        /// Call this method to clear all validation issues with a given model.
        /// </summary>
        /// <param name="model">A <see cref="BaseModel"/> to clear validation issues from.</param>
        /// <param name="clearChildIssues">A boolean value indicating whether or not to clear child objects issues as well.</param>
        public static void ClearValidationIssues(BaseModel model, bool clearChildIssues = false)
        {
            if (model == null)
                return;

            (model as IValidationInteraction).ValidationErrors.Clear();

            if (clearChildIssues)
            {
                IEnumerable<PropertyInfo> properties = model.GetType().GetProperties().Where(a => a.CanRead && typeof(BaseModel).IsAssignableFrom(a.PropertyType));
                IEnumerable<PropertyInfo> colProperties = model.GetType().GetProperties().Where(a => a.CanRead && typeof(IEnumerable<BaseModel>).IsAssignableFrom(a.PropertyType));

                foreach (PropertyInfo pi in properties)
                    ClearValidationIssues((BaseModel)pi.GetValue(model, null), clearChildIssues);
                foreach (PropertyInfo pi in colProperties)
                {
                    foreach (BaseModel bm in (IEnumerable<BaseModel>)pi.GetValue(model, null))
                        ClearValidationIssues(bm, clearChildIssues);
                }
            }
        }
    }
}