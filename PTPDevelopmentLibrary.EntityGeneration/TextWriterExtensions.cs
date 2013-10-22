using System.IO;
namespace PTPDevelopmentLibrary.EntityGeneration
{
    /// <summary>
    /// An extension class used to extend the <see cref="TextWriter"/> object.
    /// </summary>
    public static class TextWriterExtensions
    {
        /// <summary>
        /// Writes <paramref name="format"/> to the underlying stream with the tabs given by <paramref name="queue"/>.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> used to write to a stream.</param>
        /// <param name="queue">A <see cref="TabQueue"/> describing the current state of the tabs.</param>
        /// <param name="format">A string representing the format of the output.</param>
        /// <param name="args">An array of objects used to populate the arguments within <paramref name="format"/>.</param>
        public static void WriteLine(this TextWriter writer, TabQueue queue, string format, params object[] args)
        {
            writer.WriteLine(queue.ToString() + format, args);
        }
        /// <summary>
        /// Writes <paramref name="text"/> to the underlying stream with the tabs given by <paramref name="queue"/>.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> used to write to a stream.</param>
        /// <param name="queue">A <see cref="TabQueue"/> describing the current state of the tabs.</param>
        /// <param name="text">A string representing the output text.</param>
        public static void WriteLine(this TextWriter writer, TabQueue queue, string text)
        {
            writer.WriteLine(queue.ToString() + text);
        }
    }
}