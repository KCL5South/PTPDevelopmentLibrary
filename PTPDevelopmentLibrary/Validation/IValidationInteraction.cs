namespace PTPDevelopmentLibrary.Validation
{
    /// <summary>
    /// This contract is inherited explicitly by <see cref="PTPDevelopmentLibrary.Framework.BaseModel"/> so that the validation engine
    /// can more easily handle validation.
    /// </summary>
    /// <remarks>
    /// This object basically opens up some private objects within <see cref="PTPDevelopmentLibrary.Framework.BaseModel"/> to the Validation Engine.
    /// </remarks>
    internal interface IValidationInteraction
    {
        ValidationStates ValidationState { set; }
#if !SILVERLIGHT
        void RaiseErrorsChanging(string propertyName);
#endif
        void RaiseErrorsChanged(string propertyName);
#if SILVERLIGHT
        bool HasErrors { set; }
#endif
        ValidationErrorCollection ValidationErrors { get; }
    }
}