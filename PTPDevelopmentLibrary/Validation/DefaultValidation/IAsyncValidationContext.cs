using System;
using System.Collections.Generic;
using PTPDevelopmentLibrary.Framework;
namespace PTPDevelopmentLibrary.Validation.DefaultValidation
{
    /// <summary>
    /// This service is used by the <see cref="DataAnnotationsValidator"/> to support asyncronous validation.
    /// </summary>
    /// <remarks>
    /// If you are using the <see cref="DataAnnotationsValidator"/> for validation and wish to use asyncronous
    /// validation, simply query for this type within the <see cref="M:ValidationContext.GetService"/> method.
    /// </remarks>
    public interface IAsyncValidationContext
    {
        /// <summary>
        /// Gets the <see cref="BaseModel"/> that is being validated.
        /// </summary>
        BaseModel Target { get; }
        /// <summary>
        /// Call this method to register an asyncronous operation.  The engine will register that your asyncronous operation
        /// is executing and will wait until <see cref="M:IAsyncValidationContext.EndAsyncValidation"/> is called with the
        /// returned id.
        /// </summary>
        /// <returns>A <see cref="Guid"/> representing your asyncronous operation's unique execution.</returns>
        Guid BeginAsyncValidation();
        /// <summary>
        /// Call this method to unregister/complete your asyncronous operation.
        /// </summary>
        /// <param name="asyncId">A <see cref="Guid"/> representing the unique id the engine gave your operation.</param>
        /// <param name="result">A <see cref="IEnumerable{T}"/> representing the validation results from your asyncronous operation.</param>
        void EndAsyncValidation(Guid asyncId, IEnumerable<object> result);
    }
}