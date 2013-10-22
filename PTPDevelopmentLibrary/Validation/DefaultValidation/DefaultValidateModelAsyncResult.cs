using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;
using PTPDevelopmentLibrary.Framework;
namespace PTPDevelopmentLibrary.Validation.DefaultValidation
{
    /// <summary>
    /// This object handles asyncronous model validation for the default validation objects.
    /// </summary>
    internal class DefaultValidateModelAsyncResult : BaseAsyncResult<IEnumerable<object>>, IAsyncValidationContext
    {
        object _syncObject;

        public DefaultValidateModelAsyncResult(BaseModel target, DataAnnotationsValidator validator, AsyncCallback callback, object state)
            : base(callback, state)
        {
            if (target == null)
                this.HandleException(new ArgumentNullException("target"), true);
            if (validator == null)
                this.HandleException(new ArgumentNullException("validator"), true);

            if (this.Exception != null)
                return;

            RunningThread = new Thread(Background_ValidateModel);
            RunningThread.Name = string.Format("DefaultValidateModelAsyncResult: {0}", target.GetType());

            Target = target;
            Validator = validator;
            ValidationResults = new List<object>();
            AsyncOperationIDs = new List<Guid>();
            _syncObject = new object();

            RunningThread.Start(this);
        }

        private Thread RunningThread { get; set; }
        public BaseModel Target { get; private set; }
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
            DefaultValidateModelAsyncResult result = asyncResult as DefaultValidateModelAsyncResult;
            if (result == null)
                return;

            try
            {
                lock (typeof(IAsyncValidationContext))
                {
                    result.Validator.UnregisterService(typeof(IAsyncValidationContext));
                    result.Validator.RegisterService(typeof(IAsyncValidationContext), result);

                    result.ValidationResults.AddRange(result.Validator.ValidateModel(result.Target));
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