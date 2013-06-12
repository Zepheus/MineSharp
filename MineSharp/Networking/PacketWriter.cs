/*
 * This file is part of MineSharp. Copyright 2013 Cedric Van Goethem 
 * 
 * MineSharp. is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MineSharp.Handlers;

namespace MineSharp.Networking
{
    public class PacketWriter : BinaryWriter
    {
        private static Encoding Enc = System.Text.Encoding.Unicode;
        private const int DefaultBuffer = 256;

        public PacketWriter(SendOpcode opcode, int buffersize)
            : base(new MemoryStream(buffersize), Enc)
        {
            Write((byte)opcode);
        }

        public PacketWriter(SendOpcode opcode)
            : this(opcode, DefaultBuffer) {}

        public override void Write(uint x)
        {
            base.Write(((x & 0x000000ff) << 24) |
             ((x & 0xff00) << 8) |
             ((x & 0xff0000) >> 8) |
             ((x & 0xff000000) >> 24));
        }

        public override void Write(ulong x)
        {
            // Hack since we're lazy
            byte[] data = BitConverter.GetBytes(x);
            Array.Reverse(data);
            Write(data);
        }

        public override void Write(long x)
        {
            // Same hack, same reason
            byte[] data = BitConverter.GetBytes(x);
            Array.Reverse(data);
            Write(data);
        }

        public override void Write(int value)
        {
            this.Write((uint)value);
        }

        public override void Write(ushort x)
        {
            base.Write((ushort)(((x & 0xff) << 8) | (x & 0xff00)));
        }

        public override void Write(double x)
        {
            byte[] buffer = BitConverter.GetBytes(x);
            Array.Reverse(buffer);
            Write(buffer);
        }

        public override void Write(float x)
        {
            byte[] buffer = BitConverter.GetBytes(x);
            Array.Reverse(buffer);
            Write(buffer);
        }

        public override void Write(short value)
        {
            this.Write((ushort)value);
        }

        private void WriteSwappedRawString(string value)
        {
            Write(ASCIIEncoding.BigEndianUnicode.GetBytes(value));
        }

        public void WriteString(string format, params object[] list) {
            string value = String.Format(format, list);
            Write((ushort)value.Length);
            WriteSwappedRawString(value);
        }

        public void WritePaddedString(string value)
        {
            WriteSwappedRawString(value);
            Write('\0');
        }

        public byte[] GetBytes()
        {
            Stream stream = this.OutStream;
            long len = stream.Length;
            byte[] buff = new byte[(int)len];
            long start = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buff, 0, (int)len);
            stream.Seek(start, SeekOrigin.Begin);
            return buff;
        }
    }
}
