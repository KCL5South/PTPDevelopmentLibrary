using System.Collections.Generic;
using System;
using System.Threading;
using PTPDevelopmentLibrary.Framework;
using System.Linq;
namespace PTPDevelopmentLibrary.Validation.DefaultValidation
{
    /// <summary>
    /// This object handles asyncronous property validation for the default validation objects.
    /// </summary>
    internal class DefaultValidatePropertyAsyncResult : BaseAsyncResult<IEnumerable<object>>, IAsyncValidationContext
    {
        object _syncObject;

        public DefaultValidatePropertyAsyncResult(BaseModel target, DataAnnotationsValidator validator, object value, string propertyName, AsyncCallback callback, object state)
            : base(callback, state)
        {
            if (target == null)
                this.HandleException(new ArgumentNullException("target"), true);
            if (validator == null)
                this.HandleException(new ArgumentNullException("validator"), true);
            if (string.IsNullOrWhiteSpace(propertyName))
                this.HandleException(new ArgumentException("propertyName must not be null or empty."), true);

            if (this.Exception != null)
                return;

            RunningThread = new Thread(Background_ValidateModel);
            RunningThread.Name = string.Format("DefaultValidateModelAsyncResult: {0}", target.GetType());

            Target = target;
            Value = value;
            PropertyName = propertyName;
            Validator = validator;
            ValidationResults = new List<object>();
            AsyncOperationIDs = new List<Guid>();
            _syncObject = new object();

            RunningThread.Start(this);
        }

        private Thread RunningThread { get; set; }
        public BaseModel Target { get; private set; }
        public object Value { get; private set; }
        public string PropertyName { get; private set; }
        internal DataAnnotationsValidator Validator { get; private set; }
        private List<object> ValidationResults { get; set; }
        private List<Guid> AsyncOperationIDs { get; set; }

        #region IAsyncValidationContext Members

        public Guid BeginAsyncValidation()
        {
            lock (_syncObject)
            {
                Guid result = Guid.NewGuid();
                this.AsyncOperationIDs.Add(result);
                return result;
            }
        }

        public void EndAsyncValidation(Guid asyncId, IEnumerable<object> result)
        {
            lock (_syncObject)
            {
                if (this.AsyncOperationIDs.Contains(asyncId))
                {
                    AsyncOperationIDs.Remove(asyncId);
                    ValidationResults.AddRange(result.Where(a => a != null));
                }
                else
                    throw new InvalidOperationException("asyncId was not received from this instance of IAsyncValidationContext");
            }
        }

        #endregion

        public static void Background_ValidateModel(object asyncResult)
        {
            DefaultValidatePropertyAsyncResult result = asyncResult as DefaultValidatePropertyAsyncResult;
            if (result == null)
                return;

            try
            {
                lock (typeof(IAsyncValidationContext))
                {
                    result.Validator.UnregisterService(typeof(IAsyncValidationContext));
                    result.Validator.RegisterService(typeof(IAsyncValidationContext), result);

                    result.ValidationResults.AddRange(result.Validator.ValidateProperty(result.Target, result.Value, result.PropertyName));
                }

                while (result.AsyncOperationIDs.Count != 0)
                {
                    Thread.Sleep(10);
                }

                result.Complete(result.ValidationResults, false);
            }
            catch (Exception ex)
            {
                result.HandleException(ex, false);
            }
        }
    }
}