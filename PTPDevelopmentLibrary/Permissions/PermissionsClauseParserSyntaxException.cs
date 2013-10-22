using System;
namespace PTPDevelopmentLibrary.Permissions
{
    /// <summary>
    /// Represents a syntax error within the <see cref="PermissionClauseParser"/> object.
    /// </summary>
    public class PermissionsClauseParserSyntaxException : PermissionClauseParserException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The message to tie to the exception</param>
        public PermissionsClauseParserSyntaxException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The message to tie to the exception</param>
        /// <param name="innerException">An inner exception</param>
        public PermissionsClauseParserSyntaxException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}