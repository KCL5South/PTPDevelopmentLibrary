using System.Collections.Generic;
namespace PTPDevelopmentLibrary.Validation
{
    /// <summary>
    /// This contract is used by the Validation Engine to handle how individual validation issues are handled.
    /// Implementors should inherit this contract in order to create custom validation managers.
    /// </summary>
    public interface IValidationManager
    {
        /// <summary>
        /// This method overrides all validation issues within a <see cref="ValidationErrorCollection"/> with
        /// the given <paramref name="validationResult"/> issues.
        /// </summary>
        /// <param name="errorCollection">A <see cref="ValidationErrorCollection"/> object that should be populated with the new validation issues.</param>
        /// <param name="validationResult">A <see cref="IEnumerable{T}"/> collection of validation issues.</param>
        /// <exception cref="System.ArgumentNullException">If errorCollection or validationResult is null.  Make sure you pass an empty collection to validationResult if a successful validation is the result.</exception>
        void SetErrors(ValidationErrorCollection errorCollection, IEnumerable<object> validationResult);
        /// <summary>
        /// This method distribues validation issues for a single member.
        /// </summary>
        /// <param name="errorCollection">A <see cref="ValidationErrorCollection"/> object that should be populated with the new validation issues.</param>
        /// <param name="validationResult">A <see cref="IEnumerable{T}"/> collection of validation issues.</param>
        /// <param name="member">A string representing the name of the member the validation occured for.</param>
        /// <exception cref="System.ArgumentNullException">If errorCollection or validationResult is null.  Make sure you pass an empty collection to validationResult if a successful validation is the result.</exception>
        /// <exception cref="System.ArgumentException">member is null or empty.</exception>
        void SetMemberErrors(ValidationErrorCollection errorCollection, IEnumerable<object> validationResult, string member);
    }
}