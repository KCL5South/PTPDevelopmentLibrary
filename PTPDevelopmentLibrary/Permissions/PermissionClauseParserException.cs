using System;

namespace PTPDevelopmentLibrary.Permissions
{
    /// <summary>
    /// Represents a generic exception encountered within the <see cref="PermissionClauseParser"/> object.
    /// </summary>
    public class PermissionClauseParserException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The message to tie to the exception</param>
        public PermissionClauseParserException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The message to tie to the exception</param>
        /// <param name="innerException">An inner exception</param>
        public PermissionClauseParserException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}