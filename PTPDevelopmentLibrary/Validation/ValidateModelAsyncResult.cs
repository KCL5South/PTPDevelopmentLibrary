using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;
using PTPDevelopmentLibrary.Framework;
using System.Reflection;
namespace PTPDevelopmentLibrary.Validation
{
    /// <summary>
    /// This internal object handles asyncronous validation for the validation engine.
    /// </summary>
    internal class ValidateModelAsyncResult : BaseAsyncResult<IEnumerable<object>>
    {
        public ValidateModelAsyncResult(IValidator currentValidator, BaseModel target, bool validateChildren, AsyncCallback callback, object state)
            : base(callback, state)
        {
            if (currentValidator == null)
                this.HandleException(new ArgumentNullException("currentValidator"), true);
            if (target == null)
                this.HandleException(new ArgumentNullException("target"), true);

            if (this.Exception != null)
                return;

            RunningThread = new Thread(Background_ValidateModel);
            RunningThread.Name = string.Format("Validator's Asyncronous Validate Model: {0}", target.GetType().ToString());

            CurrentValidator = currentValidator;
            Target = target;
            ValidateChildren = validateChildren;
            AsyncId = (Target as IValidationInteraction).ValidationErrors.BeginAsyncOperation();

            RunningThread.Start(this);
        }

        public Thread RunningThread { get; private set; }
        public IValidator CurrentValidator { get; private set; }
        public BaseModel Target { get; private set; }
        public bool ValidateChildren { get; private set; }
        public Guid AsyncId { get; private set; }

        private static void Background_ValidateModel(object asyncResult)
        {
            ValidateModelAsyncResult result = asyncResult as ValidateModelAsyncResult;
            if (result == null)
                return;

            try
            {
                IAsyncResult curResult = result.CurrentValidator.BeginValidateModel(result.Target, null, null);
                curResult.AsyncWaitHandle.WaitOne();
                IEnumerable<object> finalResult = result.CurrentValidator.EndValidateModel(curResult);

                (result.Target as IValidationInteraction).ValidationErrors.EndAsyncOperation(result.AsyncId);

                //If we're validating the children as well...
                if (result.ValidateChildren)
                {
                    IEnumerable<PropertyInfo> properties = result.Target.GetType().GetProperties().Where(a => a.CanRead && typeof(BaseModel).IsAssignableFrom(a.PropertyType));
                    IEnumerable<PropertyInfo> colProperties = result.Target.GetType().GetProperties().Where(a => a.CanRead && typeof(IEnumerable<BaseModel>).IsAssignableFrom(a.PropertyType));

                    foreach (PropertyInfo pi in properties)
                    {
                        curResult = Validator.BeginValidateModel((BaseModel)pi.GetValue(result.Target, null), result.ValidateChildren, null, null);
                        curResult.AsyncWaitHandle.WaitOne();
                        Validator.EndValidateModel(curResult);
                    }
                    foreach (PropertyInfo pi in colProperties)
                    {
                        foreach (BaseModel bm in (IEnumerable<BaseModel>)pi.GetValue(result.Target, null))
                        {
                            curResult = Validator.BeginValidateModel(bm, result.ValidateChildren, null, null);
                            curResult.AsyncWaitHandle.WaitOne();
                            Validator.EndValidateModel(curResult);
                        }
                    }
                }

                //signal completion.
                result.Complete(finalResult, false);
            }
            catch (Exception ex)
            {
                result.HandleException(ex, false);
            }
        }
    }
}