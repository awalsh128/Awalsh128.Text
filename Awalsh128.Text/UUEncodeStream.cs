/*
 * Copyright (c) 2015, Andrew Walsh
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License (LGPL)
 * version 2 as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU Library General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
namespace Awalsh128.Text
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a stream for encoding text to Unix-to-Unix format.
    /// </summary>
    public class UUEncodeStream
        : Stream
    {
        private readonly string fileName;
        private bool headerWritten;
        private readonly byte[] lineBuffer;
        private int lineBufferIndex;
        private readonly byte[] lineEnding;
        private readonly Stream stream;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get { return this.stream.Length; }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get { return this.stream.Length; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Construct an encoding stream.
        /// </summary>
        /// <param name="stream">The plain text stream to encode.</param>
        /// <param name="unixLineEnding">Encode with Unix line endings if true, otherwise use Windows.</param>
        /// <param name="lineLength">The fixed length for every encoded line.</param>
        /// <param name="fileName">The filename to include in the encoded payload's header.</param>
        public UUEncodeStream(Stream stream, bool unixLineEnding = false, int lineLength = 45, string fileName = "file.dat")
        {
            stream.ThrowIfInvalid("stream", s => s.CanWrite, "Stream must be writable so encoded data can be written to it.");

            this.headerWritten = false;
            this.stream = stream;
            this.lineBuffer = new byte[lineLength];
            this.lineBufferIndex = 0;
            this.lineEnding = Encoding.ASCII.GetBytes(unixLineEnding ? "\n" : "\r\n");
            this.fileName = fileName;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed
        /// resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged
        /// resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            this.FlushBuffer(this.lineBuffer.Take(this.lineBufferIndex).ToArray());
            this.WriteFooter();
            base.Dispose(disposing);
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to
        /// the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            this.stream.Flush();
        }

        private void FlushBuffer(byte[] buffer)
        {
            byte[] encodedBuffer = UUEncoder.EncodeLine(buffer);
            this.stream.Write(encodedBuffer, 0, encodedBuffer.Length);
            this.stream.Write(this.lineEnding, 0, this.lineEnding.Length);
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within
        /// the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes
        /// are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains the specified byte array with the
        /// values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by
        /// the bytes read from the current source.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read
        /// from the current stream.
        /// </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
        /// <exception cref="T:System.ArgumentException">
        /// The sum of <paramref name="offset" /> and <paramref name="count" /> is
        /// larger than the buffer length.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is
        /// negative.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
        /// <param name="origin">
        /// A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to
        /// obtain the new position.
        /// </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The stream does not support seeking, such as if the stream is
        /// constructed from a pipe or console output.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The stream does not support both writing and seeking, such as if the
        /// stream is constructed from a pipe or console output.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position
        /// within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. This method copies <paramref name="count" /> bytes from
        /// <paramref name="buffer" /> to the current stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the
        /// current stream.
        /// </param>
        /// <param name="count">The number of bytes to be written to the current stream. </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            buffer.ThrowIfInvalid("buffer", b => b != null, "Buffer cannot be null.");
            offset.ThrowIfInvalid("offset", o => o >= 0, "Offset must be a value greater than or equal to zero.");
            count.ThrowIfInvalid("count", c => c >= 0, "Count must be a value greater than or equal to zero.");
            if (buffer.Length - offset < count)
            {
                throw new ArgumentException(
                    String.Format(
                        "Buffer length minus the offset must be greater than or equal to count (buffer.Length = {0}, offset = {1}, count = {2}).",
                        buffer.Length, offset, count));
            }

            if (!this.headerWritten)
            {
                this.WriteHeader();
            }
            int remainingCount = count;
            int remainingOffset = offset;
            while (remainingCount > 0)
            {
                if (this.lineBufferIndex > 0)
                {
                    int lineBufferTailCount = this.lineBuffer.Length - this.lineBufferIndex;
                    Array.Copy(buffer, remainingOffset, this.lineBuffer, this.lineBufferIndex, lineBufferTailCount);
                    this.FlushBuffer(this.lineBuffer);
                    remainingCount -= lineBufferTailCount;
                    remainingOffset += lineBufferTailCount;
                    this.lineBufferIndex = 0;
                }
                else
                {
                    if (remainingCount >= this.lineBuffer.Length)
                    {
                        Array.Copy(buffer, remainingOffset, this.lineBuffer, 0, this.lineBuffer.Length);
                        this.FlushBuffer(this.lineBuffer);
                        remainingCount -= this.lineBuffer.Length;
                        remainingOffset += this.lineBuffer.Length;
                    }
                    else
                    {
                        Array.Copy(buffer, remainingOffset, this.lineBuffer, this.lineBufferIndex, remainingCount);
                        this.lineBufferIndex = remainingCount;
                        remainingCount = 0;
                    }
                }
            }
        }

        private void WriteFooter()
        {
            this.FlushBuffer(new byte[0]);
            byte[] footer = Encoding.ASCII.GetBytes("end");
            this.stream.Write(footer, 0, footer.Length);
            this.stream.Write(this.lineEnding, 0, this.lineEnding.Length);
        }

        private void WriteHeader()
        {
            byte[] header = Encoding.ASCII.GetBytes("begin 0744 " + this.fileName).Concat(this.lineEnding).ToArray();
            this.stream.Write(header, 0, header.Length);
            this.headerWritten = true;
        }
    }
}