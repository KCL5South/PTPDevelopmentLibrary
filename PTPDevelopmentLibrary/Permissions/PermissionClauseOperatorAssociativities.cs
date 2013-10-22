namespace PTPDevelopmentLibrary.Permissions
{
    /// <summary>
    /// Represents an operators associativity.
    /// http://en.wikipedia.org/wiki/Operator_associativity
    /// </summary>
    public enum PermissionClauseOperatorAssociativities
    {
        /// <summary>
        /// The operator does not have associativity.
        /// </summary>
        None,
        /// <summary>
        /// The operator is evaluated from left to right.
        /// </summary>
        LeftToRight,
        /// <summary>
        /// The operator is evaluated from right to left.
        /// </summary>
        RightToLeft
    }
}