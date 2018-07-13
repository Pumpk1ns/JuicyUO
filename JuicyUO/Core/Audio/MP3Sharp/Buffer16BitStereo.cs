﻿#region license
//  Copyright (C) 2018 JuicyUO Development Community on Github
//
//	This project is an alternative client for the game Ultima Online.
//	The goal of this is to develop a lightweight client considering 
//	new technologies such as DirectX (MonoGame included). The foundation
//	is originally licensed (GNU) on JuicyUO and the JuicyUO Development
//	Team. (Copyright (c) 2015 JuicyUO Development Team)
//    
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using JuicyUO.Core.Audio.MP3Sharp.Decoding;

namespace JuicyUO.Core.Audio.MP3Sharp
{
    /// <summary>
    ///     Internal class used to queue samples that are being obtained from an Mp3 stream. 
    ///     This class handles stereo 16-bit data! Switch it out if you want mono or something.
    /// </summary>
    class Buffer16BitStereo : ABuffer
    {
        // This is stereo!
        private static readonly int CHANNELS = 2;
        // Write offset used in append_bytes
        private readonly byte[] m_Buffer = new byte[OBUFFERSIZE * 2]; // all channels interleaved
        private readonly int[] m_Bufferp = new int[MAXCHANNELS]; // offset in each channel not same!
        // end marker, one past end of array. Same as bufferp[0], but
        // without the array bounds check.
        private int m_End;
        // Read offset used to read from the stream, in bytes.
        private int m_Offset;

        public Buffer16BitStereo()
        {
            // Initialize the buffer pointers
            ClearBuffer();
        }

        /// <summary>
        ///     Gets the number of bytes remaining from the current position on the buffer.
        /// </summary>
        public int BytesLeft
        {
            get
            {
                return m_End - m_Offset;
            }
        }

        /// <summary>
        ///     Reads a sequence of bytes from the buffer and advances the position of the 
        ///     buffer by the number of bytes read.
        /// </summary>
        /// <returns>
        ///     The total number of bytes read in to the buffer. This can be less than the
        ///     number of bytes requested if that many bytes are not currently available, or
        ///     zero if th eend of the buffer has been reached.
        /// </returns>
        public int Read(byte[] bufferOut, int offset, int count)
        {
            if (bufferOut == null)
            {
                throw new ArgumentNullException(nameof(bufferOut));
            }
            if ((count + offset) > bufferOut.Length)
            {
                throw new ArgumentException("The sum of offset and count is larger than the buffer length");
            }
            int remaining = BytesLeft;
            int copySize;
            if (count > remaining)
            {
                copySize = remaining;
            }
            else
            {
                // Copy an even number of sample frames
                int remainder = count % (2 * CHANNELS);
                copySize = count - remainder;
            }

            Array.Copy(m_Buffer, m_Offset, bufferOut, offset, copySize);

            m_Offset += copySize;
            return copySize;
        }

        /// <summary>
        ///     Writes a single sample value to the buffer.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="sampleValue">The sample value.</param>
        public override void Append(int channel, short sampleValue)
        {
            m_Buffer[m_Bufferp[channel]] = (byte)(sampleValue & 0xff);
            m_Buffer[m_Bufferp[channel] + 1] = (byte)(sampleValue >> 8);

            m_Bufferp[channel] += CHANNELS * 2;
        }

        /// <summary>
        ///     Writes 32 samples to the buffer.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="samples">An array of sample values.</param>
        /// <remarks>
        ///     The <paramref name="samples"/> parameter must have a length equal to
        ///     or greater than 32.
        /// </remarks>
        public override void AppendSamples(int channel, float[] samples)
        {
            if (samples == null)
            {
                // samples is required.
                throw new ArgumentNullException(nameof(samples));
            }
            if (samples.Length < 32)
            {
                throw new ArgumentException("samples must have 32 values");
            }
            // Always, 32 samples are appended
            int pos = m_Bufferp[channel];

            for (int i = 0; i < 32; i++)
            {
                float fs = samples[i];
                if (fs > 32767.0f) // can this happen?
                    fs = 32767.0f;
                else if (fs < -32767.0f)
                    fs = -32767.0f;

                int sample = (int)fs;
                m_Buffer[pos] = (byte)(sample & 0xff);
                m_Buffer[pos + 1] = (byte)(sample >> 8);

                pos += CHANNELS * 2;
            }

            m_Bufferp[channel] = pos;
        }

        /// <summary>
        ///     This implementation does not clear the buffer.
        /// </summary>
        public override sealed void ClearBuffer()
        {
            m_Offset = 0;
            m_End = 0;

            for (int i = 0; i < CHANNELS; i++)
                m_Bufferp[i] = i * 2; // two bytes per channel
        }

        public override void SetStopFlag()
        {
        }

        public override void WriteBuffer(int val)
        {
            m_Offset = 0;

            // speed optimization - save end marker, and avoid
            // array access at read time. Can you believe this saves
            // like 1-2% of the cpu on a PIII? I guess allocating
            // that temporary "new int(0)" is expensive, too.
            m_End = m_Bufferp[0];
        }

        public override void Close()
        {
        }
    }
}
