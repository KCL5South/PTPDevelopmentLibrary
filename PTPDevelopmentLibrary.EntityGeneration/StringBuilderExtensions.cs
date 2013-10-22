using System.Text;
namespace PTPDevelopmentLibrary.EntityGeneration
{
    /// <summary>
    /// A Place holder for the <see cref="StringBuilder"/> extension methods used within this library.
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Appends a copy of the specified string to the end of this instance.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> object to extend.</param>
        /// <param name="value">The value to append</param>
        /// <param name="queue">A <see cref="TabQueue"/> object that keeps track of tabs.</param>
        /// <returns>The result with the appropriate tabs appended to the begining of value.</returns>
        public static StringBuilder Append(this StringBuilder builder, string value, TabQueue queue)
        {
            return builder.Append(queue.ToString() + value);
        }
        /// <summary>
        /// Appends a copy of the specified string followed by the default line terminator to the end of the current System.Text.StringBuilder object.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> object to extend.</param>
        /// <param name="value">The value to append</param>
        /// <param name="queue">A <see cref="TabQueue"/> object that keeps track of tabs.</param>
        /// <returns>The result with the appropriate tabs appended to the begining of value.</returns>
        public static StringBuilder AppendLine(this StringBuilder builder, string value, TabQueue queue)
        {
            return builder.AppendLine(queue.ToString() + value);
        }
    }
}