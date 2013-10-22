using System.Collections.Generic;
using System;
using System.Threading;
using PTPDevelopmentLibrary.Framework;
namespace PTPDevelopmentLibrary.Validation
{
    /// <summary>
    /// This internal object handle asyncronous property validation for the validation engine.
    /// </summary>
    internal class ValidatePropertyAsyncResult : BaseAsyncResult<IEnumerable<object>>
    {
        public ValidatePropertyAsyncResult(IValidator currentValidator, BaseModel target, object value, string propertyName, AsyncCallback callback, object state)
            : base(callback, state)
        {
            if (currentValidator == null)
                this.HandleException(new ArgumentNullException("currentValidator"), true);
            if (target == null)
                this.HandleException(new ArgumentNullException("target"), true);
            if (string.IsNullOrWhiteSpace(propertyName))
                this.HandleException(new ArgumentException("propertyName must not be null or empty"), true);

            if (this.Exception != null)
                return;

            RunningThread = new Thread(Background_ValidateProperty);
            RunningThread.Name = string.Format("Validator's Asyncronous Validate Model: {0}", target.GetType().ToString());

            CurrentValidator = currentValidator;
            Target = target;
            Value = value;
            PropertyName = propertyName;
            AsyncId = (Target as IValidationInteraction).ValidationErrors.BeginAsyncOperation();

            RunningThread.Start(this);
        }

        public Thread RunningThread { get; private set; }
        public IValidator CurrentValidator { get; private set; }
        public BaseModel Target { get; private set; }
        public object Value { get; private set; }
        public string PropertyName { get; private set; }
        public Guid AsyncId { get; private set; }

        private static void Background_ValidateProperty(object asyncResult)
        {
            ValidatePropertyAsyncResult result = asyncResult as ValidatePropertyAsyncResult;
            if (result == null)
                return;

            try
            {
                IAsyncResult curResult = result.CurrentValidator.BeginValidateProperty(result.Target, result.Value, result.PropertyName, null, null);
                curResult.AsyncWaitHandle.WaitOne();
                IEnumerable<object> finalResult = result.CurrentValidator.EndValidateProperty(curResult);

                (result.Target as IValidationInteraction).ValidationErrors.EndAsyncOperation(result.AsyncId);

                result.Complete(finalResult, false);
            }
            catch (Exception ex)
            {
                result.HandleException(ex, false);
            }
        }
    }
}