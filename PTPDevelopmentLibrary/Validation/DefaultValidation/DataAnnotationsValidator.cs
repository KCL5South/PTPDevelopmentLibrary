using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PTPDevelopmentLibrary.Framework;
namespace PTPDevelopmentLibrary.Validation.DefaultValidation
{
    /// <summary>
    /// This Validator uses .Net's Data Annotations to handle validation.
    /// http://msdn.microsoft.com/en-us/library/dd901590(v=vs.95).aspx (Silverlight)
    /// http://msdn.microsoft.com/en-us/library/ee256141.aspx (.Net)
    /// </summary>
    public class DataAnnotationsValidator : IValidator, IServiceProvider
    {
        IDictionary<Type, object> _services;

        /// <summary>
        /// Constructor
        /// </summary>
        public DataAnnotationsValidator()
        {
            _services = new Dictionary<Type, object>();
        }

        #region IValidationManager Members

        /// <summary>
        /// Inherited from <see cref="IValidator"/>
        /// </summary>
        /// <param name="model">A <see cref="BaseModel"/> that will be validated.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that represents the results of the valdiation.</returns>
        public virtual IEnumerable<object> ValidateModel(BaseModel model)
        {
            ValidationContext context = new ValidationContext(model, this, null);
            ICollection<ValidationResult> result = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, context, result);

            return result.Cast<object>();
        }

        /// <summary>
        /// Inherited from <see cref="IValidator"/>
        /// </summary>
        /// <param name="model">A <see cref="BaseModel"/> that will be validated.</param>
        /// <param name="value">The value that needs to be validated.</param>
        /// <param name="propertyName">The name of the property that is changing and needs to be validated.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that represents the results of the valdiation.</returns>
        public virtual IEnumerable<object> ValidateProperty(BaseModel model, object value, string propertyName)
        {
            ValidationContext context = new ValidationContext(model, this, null);
            context.MemberName = propertyName;
            ICollection<ValidationResult> result = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(value, context, result);

            return result.Cast<object>();
        }

        /// <summary>
        /// Inherited from <see cref="IValidator"/>
        /// <remarks>This implementation returns all results at once.  This means that all 
        /// asyncronous operations complete before a result is returned.</remarks>
        /// </summary>
        /// <param name="model">A <see cref="BaseModel"/> that will be validated.</param>
        /// <param name="callback">An <see cref="AsyncCallback"/> reference that will be called when the operation completes.</param>
        /// <param name="state">A user defined object.</param>
        /// <returns>An <see cref="IAsyncResult"/> representing the background operation.</returns>
        public virtual IAsyncResult BeginValidateModel(BaseModel model, AsyncCallback callback, object state)
        {
            return new DefaultValidateModelAsyncResult(model, this, callback, state);
        }

        /// <summary>
        /// Inherited from <see cref="IValidator"/>
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> representing the background operation that has completed.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that represents the results of the valdiation.</returns>
        public virtual IEnumerable<object> EndValidateModel(IAsyncResult result)
        {
            if (result is DefaultValidateModelAsyncResult)
            {
                DefaultValidateModelAsyncResult newResult = result as DefaultValidateModelAsyncResult;
                if (newResult.Exception != null)
                    throw newResult.Exception;
                else
                    return newResult.Result;
            }
            else
                throw new InvalidOperationException(string.Format("result must be of type {0}", typeof(DefaultValidateModelAsyncResult)));
        }

        /// <summary>
        /// Inherited from <see cref="IValidator"/>
        /// <remarks>This implementation returns all results at once.  This means that all 
        /// asyncronous operations complete before a result is returned.</remarks>
        /// </summary>
        /// <param name="model">A <see cref="BaseModel"/> that will be validated.</param>
        /// <param name="value">The value that needs to be validated.</param>
        /// <param name="propertyName">The name of the property who's value has changed and needs to be validated.</param>
        /// <param name="callback">An <see cref="AsyncCallback"/> reference that will be called when the operation completes.</param>
        /// <param name="state">A user defined object.</param>
        /// <returns>An <see cref="IAsyncResult"/> representing the background operation.</returns>
        public virtual IAsyncResult BeginValidateProperty(BaseModel model, object value, string propertyName, AsyncCallback callback, object state)
        {
            return new DefaultValidatePropertyAsyncResult(model, this, value, propertyName, callback, state);
        }

        /// <summary>
        /// Inherited from <see cref="IValidator"/>
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> representing the background operation that has completed.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that represents the results of the valdiation.</returns>
        public virtual IEnumerable<object> EndValidateProperty(IAsyncResult result)
        {
            if (result is DefaultValidatePropertyAsyncResult)
            {
                DefaultValidatePropertyAsyncResult newResult = result as DefaultValidatePropertyAsyncResult;
                if (newResult.Exception != null)
                    throw newResult.Exception;
                else
                    return newResult.Result;
            }
            else
                throw new InvalidOperationException(string.Format("result must be of type {0}", typeof(DefaultValidatePropertyAsyncResult)));
        }

        /// <summary>
        /// Inherited from <see cref="IValidator"/>
        /// </summary>
        /// <param name="serviceType">A <see cref="Type"/> object representing the type of the service to register.</param>
        /// <param name="service">The actual service object itself.</param>
        /// <returns>A boolean value indicating wether the registration was successful.</returns>
        public virtual bool RegisterService(System.Type serviceType, object service)
        {
            if (_services.ContainsKey(serviceType))
                return false;
            else
                _services.Add(serviceType, service);

            return true;
        }

        /// <summary>
        /// Inherited from <see cref="IValidator"/>
        /// </summary>
        /// <param name="serviceType">A <see cref="Type"/> object representing the type of the service to unregister.</param>
        /// <returns>A boolean value indicating wether the unregistration was successful.</returns>
        public virtual bool UnregisterService(System.Type serviceType)
        {
            return _services.Remove(serviceType);
        }

        #endregion

        #region IServiceProvider Members

        object IServiceProvider.GetService(Type serviceType)
        {
            if (_services.ContainsKey(serviceType))
                return _services[serviceType];
            else
                return null;
        }

        #endregion
    }
}