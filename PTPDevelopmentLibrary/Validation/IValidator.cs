using System.Collections.Generic;
using System;
using PTPDevelopmentLibrary.Framework;
namespace PTPDevelopmentLibrary.Validation
{
    /// <summary>
    /// This contract is used by the Validation Engine to handle how validation is executed.
    /// Implementors should inherit this contract in order to create a custom validator.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// This method handles the validation of a <see cref="BaseModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="BaseModel"/> that will be validated.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> representing the results of the validation.</returns>
        /// <exception cref="ArgumentNullException">If model is null.</exception>
        IEnumerable<object> ValidateModel(BaseModel model);
        /// <summary>
        /// This method handles the validation of a property within a <see cref="BaseModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="BaseModel"/> that will be validated.</param>
        /// <param name="value">The value of the property to be validated.</param>
        /// <param name="propertyName">The name of the property to be validated.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> representing the results of the validation.</returns>
        /// <exception cref="ArgumentNullException">If model is null.</exception>
        /// <exception cref="ArgumentException">If proeprtyName is null or empty.</exception>
        IEnumerable<object> ValidateProperty(BaseModel model, object value, string propertyName);
        /// <summary>
        /// This method begins an asycronous validation of a <see cref="BaseModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="BaseModel"/> that will be validated.</param>
        /// <param name="callback">A <see cref="AsyncCallback"/> that will be called when the operatino completes.</param>
        /// <param name="state">A user defined object representing the state of the the operation.</param>
        /// <returns>An <see cref="IAsyncResult"/> representing the asyncronous operation.</returns>
        /// <exception cref="ArgumentNullException">If model is null.</exception>
        IAsyncResult BeginValidateModel(BaseModel model, AsyncCallback callback, object state);
        /// <summary>
        /// This method ends an asycronous validation of a <see cref="BaseModel"/>.
        /// </summary>
        /// <param name="result">An <see cref="IAsyncResult"/> representing the asyncronous operation.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> representing the results of the validation.</returns>
        /// <exception cref="InvalidOperationException">If result was not recieved from the corresponding begin method.</exception>
        IEnumerable<object> EndValidateModel(IAsyncResult result);
        /// <summary>
        /// This method begins an asyncronous validation of a property within a <see cref="BaseModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="BaseModel"/> that will be validated.</param>
        /// <param name="value">The value of the property to validate.</param>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <param name="callback">A <see cref="AsyncCallback"/> that will be called when the operatino completes.</param>
        /// <param name="state">A user defined object representing the state of the the operation.</param>
        /// <returns>An <see cref="IAsyncResult"/> representing the asyncronous operation.</returns>
        /// <exception cref="ArgumentNullException">If model is null.</exception>
        /// <exception cref="ArgumentException">If proeprtyName is null or empty.</exception>
        IAsyncResult BeginValidateProperty(BaseModel model, object value, string propertyName, AsyncCallback callback, object state);
        /// <summary>
        /// This method ends an asyncronous validation of a property within a <see cref="BaseModel"/>.
        /// </summary>
        /// <param name="result">An <see cref="IAsyncResult"/> representing the asyncronous operation.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> representing the results of the validation.</returns>
        /// <exception cref="InvalidOperationException">If result was not recieved from the corresponding begin method.</exception>
        IEnumerable<object> EndValidateProperty(IAsyncResult result);
        /// <summary>
        /// This method registers a service for use in validation.
        /// </summary>
        /// <param name="serviceType">A <see cref="Type"/> representing the type of the service object.</param>
        /// <param name="service">The service object itself.</param>
        /// <returns>A boolean value representing whether the registration was successful or not.</returns>
        /// <exception cref="ArgumentNullException">If serviceType or service are null.</exception>
        bool RegisterService(Type serviceType, object service);
        /// <summary>
        /// This method unregisters a service used in validation.
        /// </summary>
        /// <param name="serviceType">A <see cref="Type"/> representing the type of the service object.</param>
        /// <returns>A boolean value representing whether the registration was successful or not.</returns>
        /// <exception cref="ArgumentNullException">If serviceType is null.</exception>
        bool UnregisterService(Type serviceType);
    }
}