using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace PTPDevelopmentLibrary.Validation.DefaultValidation
{
    /// <summary>
    /// This Validation Manager uses .Net's Data Annotations to handle validation.
    /// http://msdn.microsoft.com/en-us/library/dd901590(v=vs.95).aspx (Silverlight)
    /// http://msdn.microsoft.com/en-us/library/ee256141.aspx (.Net)
    /// </summary>
    public class DataAnnotationsValidatorManager : IValidationManager
    {
        object _syncObject;

        /// <summary>
        /// Constructor
        /// </summary>
        public DataAnnotationsValidatorManager()
        {
            _syncObject = new object();
        }

        #region IValidationManager Members

        /// <summary>
        /// Inherited from <see cref="IValidationManager"/>.
        /// <para>
        /// This manager assumes all objects within <paramref name="validationResult"/> are of type <see cref="ValidationResult"/>.
        /// </para>
        /// <para>
        /// This manager assumes all <see cref="ValidationResult"/> objects within <paramref name="validationResult"/> that 
        /// have an empty <see cref="P:ValidationResult.MemberNames"/> collection are top level or class level issues.
        /// </para>
        /// </summary>
        /// <param name="errorCollection">A <see cref="ValidationErrorCollection"/> that houses the validation results.</param>
        /// <param name="validationResult">A <see cref="IEnumerable{T}"/> that represents the collection of validation issues that need to be applied to <paramref name="errorCollection"/>.</param>
        public virtual void SetErrors(ValidationErrorCollection errorCollection, IEnumerable<object> validationResult)
        {
            lock (_syncObject)
            {
                IEnumerable<ValidationResult> vResults = validationResult.Cast<ValidationResult>();

                var results =
                    (from g in
                         (from tuple in
                              (from r in vResults
                               from name in r.MemberNames
                               select new Tuple<string, ValidationResult>(name, r))
                          group tuple by tuple.Item1)
                     select new Tuple<string, IEnumerable<ValidationResult>>(g.Key, g.Select(a => a.Item2))).ToArray();

                if (vResults.Any(a => a.MemberNames.Count() == 0))
                {
                    Tuple<string, IEnumerable<ValidationResult>> newTuple = new Tuple<string, IEnumerable<ValidationResult>>(string.Empty, from vr in vResults
                                                                                                                                           where vr.MemberNames.Count() == 0
                                                                                                                                           select vr);
                    results = results.Concat(new Tuple<string, IEnumerable<ValidationResult>>[] { newTuple }).ToArray();
                }

                errorCollection.Clear();

                foreach (Tuple<string, IEnumerable<ValidationResult>> item in results)
                    errorCollection.SetErrors(item.Item1, item.Item2.Cast<object>());
            }
        }

        /// <summary>
        /// Inherited from <see cref="IValidationManager"/>.
        /// <para>
        /// This manager assumes all objects within <paramref name="validationResult"/> are of type <see cref="ValidationResult"/>.
        /// </para>
        /// <para>
        /// This manager removes all <see cref="ValidationResult"/> objects from the property represented by <paramref name="member"/>.
        /// If any of the removed <see cref="ValidationResult"/> objects are present for any other properties on the object, those are removed as well.
        /// </para>
        /// <remarks>This effectively means that if a single ValidationResult spans multiple properties, and one of those properties changes (i.e. is revalidated)
        /// then the ValdiationResult will be removed from all of the spaned properties.</remarks>
        /// </summary>
        /// <param name="errorCollection">A <see cref="ValidationErrorCollection"/> that houses the validation results.</param>
        /// <param name="validationResult">A <see cref="IEnumerable{T}"/> that represents the collection of validation issues that need to be applied to <paramref name="errorCollection"/>.</param>
        /// <param name="member">A string representing the name of the property that is being validated.</param>
        public virtual void SetMemberErrors(ValidationErrorCollection errorCollection, IEnumerable<object> validationResult, string member)
        {
            lock (_syncObject)
            {
                IEnumerable<ValidationResult> vResults = validationResult.Cast<ValidationResult>();

                var results =
                    (from g in
                         (from tuple in
                              (from r in vResults
                               from name in r.MemberNames
                               select new Tuple<string, ValidationResult>(name, r))
                          group tuple by tuple.Item1)
                     select new Tuple<string, IEnumerable<ValidationResult>>(g.Key, g.Select(a => a.Item2))).ToArray();

                if (vResults.Any(a => a.MemberNames.Count() == 0))
                {
                    Tuple<string, IEnumerable<ValidationResult>> newTuple = new Tuple<string, IEnumerable<ValidationResult>>(string.Empty, from vr in vResults
                                                                                                                                           where vr.MemberNames.Count() == 0
                                                                                                                                           select vr);
                    results = results.Concat(new Tuple<string, IEnumerable<ValidationResult>>[] { newTuple }).ToArray();
                }

                if (errorCollection.ContainsKey(member))
                {
                    var itemsToRemove =
                        (from g in
                             (from tuple in
                                  (from r in errorCollection[member].Cast<ValidationResult>()
                                   from name in r.MemberNames
                                   select new Tuple<string, ValidationResult>(name, r))
                              group tuple by tuple.Item1)
                         select new Tuple<string, IEnumerable<ValidationResult>>(g.Key, g.Select(a => a.Item2))).ToArray();

                    foreach (Tuple<string, IEnumerable<ValidationResult>> item in itemsToRemove)
                        errorCollection.RemoveErrors(item.Item1, item.Item2.Cast<object>());
                }

                foreach (Tuple<string, IEnumerable<ValidationResult>> item in results)
                    errorCollection.SetErrors(item.Item1, item.Item2.Cast<object>());
            }
        }

        #endregion
    }
}