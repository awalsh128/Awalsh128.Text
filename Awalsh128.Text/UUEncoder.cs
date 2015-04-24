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
 * Maps and bit twiddling taken from Szymon Kobalczyk's implementation 
 * http://geekswithblogs.net/kobush/archive/2005/12/18/63486.aspx
 * which is a port of KDE's codec implementation
 * http://websvn.kde.org/trunk/KDE/kdelibs/kdecore/kcodecs.cpp?view=markup&pathrev=486059
 * Copyright (c) 2000-2001 Dawit Alemayehu <adawit@kde.org>
 * Copyright (c) 2001 Rik Hemsley (rikkus) <rik@kde.org>
 * 
 */
namespace Awalsh128.Text
{
    using System;
    using System.IO;

    /// <summary>
    /// Fixed line length encoder for Unix-to-Unix format.
    /// </summary>
    /// <remarks>
    /// Maps and bit twiddling taken from <see cref="http://geekswithblogs.net/kobush/archive/2005/12/18/63486.aspx">
    /// Szymon Kobalczyk's implementation</see>, which is a port of <see cref="http://websvn.kde.org/trunk/KDE/kdelibs/kdecore/kcodecs.cpp?view=markup&pathrev=486059"> 
    /// KDE's codec implementation</see>.
    /// </remarks>
    public static class UUEncoder
    {
        private static readonly byte[] decoderMap =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
            0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
            0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private static readonly byte[] encoderMap =
        {
            0x60, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
            0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
            0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
            0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47,
            0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
            0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57,
            0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F
        };

        /// <summary>
        /// Decode a single line from the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to decode from.</param>
        /// <returns>A decoded single line from the buffer.</returns>
        public static byte[] DecodeLine(Stream buffer)
        {
            int decodedLineLength = decoderMap[buffer.ReadByte()];
            var decodedPaddedLineLength = (int)Math.Ceiling(decodedLineLength / 3.0) * 3;
            var decodedLine = new byte[decodedLineLength];

            // 4 UU bytes to every 3 characters
            var encodedLineLength = (decodedPaddedLineLength * 4) / 3;
            var encodedLine = new byte[encodedLineLength];

            int actualLineLength = buffer.Read(encodedLine, 0, encodedLine.Length);
            if (actualLineLength != encodedLine.Length)
            {
                throw new IOException(
                    String.Format(
                        "Expected read length to be the encoded line length of {0} but got {1}.",
                        encodedLine.Length,
                        actualLineLength));
            }

            int decodedIndex, encodedIndex;
            for (decodedIndex = 0, encodedIndex = 0; decodedIndex <= decodedLineLength - 3; decodedIndex += 3, encodedIndex += 4)
            {
                byte a = decoderMap[encodedLine[encodedIndex]];
                byte b = decoderMap[encodedLine[encodedIndex + 1]];
                byte c = decoderMap[encodedLine[encodedIndex + 2]];
                byte d = decoderMap[encodedLine[encodedIndex + 3]];
                decodedLine[decodedIndex] = (byte)(((a << 2) & 255) | ((b >> 4) & 3));
                decodedLine[decodedIndex + 1] = (byte)(((b << 4) & 255) | ((c >> 2) & 15));
                decodedLine[decodedIndex + 2] = (byte)(((c << 6) & 255) | (d & 63));
            }
            if (decodedIndex < decodedLineLength)
            {
                byte a = decoderMap[encodedLine[encodedIndex]];
                byte b = decoderMap[encodedLine[encodedIndex + 1]];
                decodedLine[decodedIndex] = (byte)(((a << 2) & 255) | ((b >> 4) & 3));
                decodedIndex++;

                if (decodedIndex < decodedLineLength)
                {
                    byte c = decoderMap[encodedLine[encodedIndex + 2]];
                    decodedLine[decodedIndex] = (byte)(((b << 4) & 255) | ((c >> 2) & 15));
                }
            }
            return decodedLine;
        }

        /// <summary>
        /// Encode a buffer into a single line.
        /// </summary>
        /// <param name="buffer">The buffer to encode.</param>
        /// <returns>An encoded single line.</returns>
        public static byte[] EncodeLine(byte[] buffer)
        {
            int count = buffer.Length;
            var decodedPaddedLineLength = (int)Math.Ceiling(count / 3.0) * 3;

            // 4 UU bytes to every 3 characters, plus the line length byte
            var encodedLineLength = ((decodedPaddedLineLength * 4) / 3) + 1;
            var encodedLine = new byte[encodedLineLength];
            encodedLine[0] = encoderMap[count];

            int decodedIndex, encodedIndex;
            for (decodedIndex = 0, encodedIndex = 1; decodedIndex <= count - 3; decodedIndex += 3, encodedIndex += 4)
            {
                byte a = buffer[decodedIndex];
                byte b = buffer[decodedIndex + 1];
                byte c = buffer[decodedIndex + 2];
                encodedLine[encodedIndex] = encoderMap[(a >> 2) & 63];
                encodedLine[encodedIndex + 1] = encoderMap[(b >> 4) & 15 | (a << 4) & 63];
                encodedLine[encodedIndex + 2] = encoderMap[(c >> 6) & 3 | (b << 2) & 63];
                encodedLine[encodedIndex + 3] = encoderMap[c & 63];
            }
            if (decodedIndex < count)
            {
                switch (count - decodedIndex)
                {
                    case 1:
                        encodedLine[encodedIndex] = encoderMap[(buffer[decodedIndex] >> 2) & 63];
                        encodedLine[encodedIndex + 1] = encoderMap[(buffer[decodedIndex] << 4) & 63];
                        encodedLine[encodedIndex + 2] = encoderMap[0];
                        encodedLine[encodedIndex + 3] = encoderMap[0];
                        break;
                    case 2:
                        encodedLine[encodedIndex] = encoderMap[(buffer[decodedIndex] >> 2) & 63];
                        encodedLine[encodedIndex + 1] = encoderMap[(buffer[decodedIndex + 1] >> 4) & 15 | (buffer[decodedIndex] << 4) & 63];
                        encodedLine[encodedIndex + 2] = encoderMap[(buffer[decodedIndex + 1] << 2) & 63];
                        encodedLine[encodedIndex + 3] = encoderMap[0];
                        break;
                    case 3:
                        encodedLine[encodedIndex] = encoderMap[(buffer[decodedIndex] >> 2) & 63];
                        encodedLine[encodedIndex + 1] = encoderMap[(buffer[decodedIndex + 1] >> 4) & 15 | (buffer[decodedIndex] << 4) & 63];
                        encodedLine[encodedIndex + 2] = encoderMap[(buffer[decodedIndex + 2] >> 2) & 63];
                        encodedLine[encodedIndex + 3] = encoderMap[0];
                        break;
                    default:
                        throw new InvalidOperationException("Method invariant violated. Padding should never be outside 1-3.");
                }
            }
            return encodedLine;
        }
    }
}