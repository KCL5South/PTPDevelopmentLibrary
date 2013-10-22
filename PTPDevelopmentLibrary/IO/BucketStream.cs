using System;
using System.IO;
namespace PTPDevelopmentLibrary.IO
{
    /// <summary>
    /// This <see cref="Stream"/> implementation is meant to read from a <see cref="Bucket"/>.
    /// </summary>
    internal class BucketStream : Stream
    {
        private Bucket _targetBuffer;
        private int _readTimeout = -1;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="buffer">A <see cref="Bucket"/> object.</param>
        public BucketStream(Bucket buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            _targetBuffer = buffer;
        }

        /// <summary>
        /// Gets a value signifiying that this stream supports reading.
        /// </summary>
        public override bool CanRead { get { return true; } }

        /// <summary>
        /// Gets a value signifiying that this stream supports seeking.
        /// </summary>
        public override bool CanSeek { get { return true; } }

        /// <summary>
        /// Gets a value signifiying that this stream supports writing.
        /// </summary>
        public override bool CanWrite { get { return false; } }

        /// <summary>
        /// The backing <see cref="Bucket"/> can't be flushed, so this method does nothing.
        /// </summary>
        public override void Flush() { }

        /// <summary>
        /// Gets a value representing the length of the backing <see cref="Bucket"/>.
        /// </summary>
        public override long Length { get { return _targetBuffer.Length; } }

        /// <summary>
        /// Gets a value representing where in the Bucket reads will start from.
        /// </summary>
        public override long Position { get; set; }

        /// <summary>
        /// Reads <paramref name="count"/> bytes from the bucket and writes them to <paramref name="buffer"/> starting
        /// at the position represented by <paramref name="offset"/>.
        /// </summary>
        /// <param name="buffer">The destination of the requested bytes.</param>
        /// <param name="offset">The offset within <paramref name="buffer"/> to start writing.</param>
        /// <param name="count">The number of bytes to write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes actually read.  This value may be less than <paramref name="count"/> given
        /// how many bytes are available to read.  A return value of zero represents the end of the bucket.
        /// (Bottom of the bucket.)</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = _targetBuffer.Read(buffer, offset, Position, count, this.ReadTimeout);
            if (result > 0)
                Position += result;

            return result;
        }

        /// <summary>
        /// Advances <see cref="P:Position"/> to the desired offset.
        /// </summary>
        /// <param name="offset">The new value for <see cref="P:Position"/>.</param>
        /// <param name="origin">A <see cref="SeekOrigin"/> enumeration representing where on the buffer to start the seek.</param>
        /// <returns>The new value of <see cref="P:Position"/>.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return Position = Math.Max(0, Math.Min(offset, _targetBuffer.Length));
                case SeekOrigin.Current:
                    return Position = Math.Max(0, Math.Min(_targetBuffer.Length, offset + Position));
                case SeekOrigin.End:
                    return Position = Math.Max(0, Math.Min(offset + _targetBuffer.Length, _targetBuffer.Length));
                default:
                    return Position;
            }
        }

        /// <summary>
        /// This is not implemented.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Unable to set length");
        }

        /// <summary>
        /// This is not implemented.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("This stream is not able to write.");
        }

        /// <summary>
        /// Gets or sets a value, in milliseconds, that is the max amount of time to wait for a read.
        /// </summary>
        public override int ReadTimeout
        {
            get
            {
                return _readTimeout;
            }
            set
            {
                _readTimeout = value;
            }
        }
    }
}