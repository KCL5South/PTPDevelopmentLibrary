namespace PTPDevelopmentLibrary.Validation
{
    /// <summary>
    /// Represents the different states an <see cref="IValidationManager"/> can be in.
    /// </summary>
    public enum ValidationStates
    {
        /// <summary>
        /// The object is valid.
        /// </summary>
        Valid,
        /// <summary>
        /// The object has validation errors.
        /// </summary>
        Invalid,
        /// <summary>
        /// The object is currently validating.  Validation State is unknown.
        /// </summary>
        Validating
    }
}