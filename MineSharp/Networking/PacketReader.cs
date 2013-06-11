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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using MineSharp.Handlers;

namespace MineSharp.Networking
{
    public class PacketReader
    {
        private static Encoding Enc = System.Text.Encoding.Unicode;
        private const ushort BufferSize = 8 * 1024;

        private byte[] buffer;
        private int read;
        private int offset;
        private Socket sock;

        public PacketReader(Socket socket)
        {
            this.buffer = new byte[BufferSize];
            this.sock = socket;
            this.read = 0;
            this.offset = 0;
        }

        public async Task<int> RequestBytes(int len)
        {
            if (len > BufferSize)
                throw new OverflowException("Buffer is too small"); //possibly double buffer at runtime

            while(len > read - offset)
            {
                if (buffer.Length - read <= len)
                {
                    if (offset == read)
                    {
                        offset = 0;
                        read = 0;
                    }
                    else
                    {
                        // Copy data to the beginning of the buffer since there's not enough space
                        Buffer.BlockCopy(buffer, offset, buffer, 0, (read - offset));
                        read -= offset;
                        offset = 0;
                    }
                }

                int recv = await Task.Factory.FromAsync(
                         (cb, s) => sock.BeginReceive(buffer, read, buffer.Length - read, SocketFlags.None, cb, s),
                         ias => sock.EndReceive(ias), null);

                if (recv <= 0)
                    throw new SocketException(); //disconnect

                read += recv;
            }
            return len;
        }

        public async Task<uint> ReadUInt32()
        {
            await RequestBytes(4);
            return (uint)((buffer[offset++] << 24) | (buffer[offset++] << 16) | (buffer[offset++] << 8) | buffer[offset++]);
        }

        public async Task<float> ReadFloat()
        {
            await RequestBytes(4);
            float val = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            return val;
        }

        public async Task<double> ReadDouble()
        {
            await RequestBytes(8);
            double val = BitConverter.ToDouble(buffer, offset);
            offset += 8;
            return val;
        }

        public async Task<ushort> ReadUInt16()
        {
            await RequestBytes(2);
            return (ushort)((buffer[offset++] << 8) | buffer[offset++]);
        }

        public async Task<byte> ReadByte()
        {
            await RequestBytes(1);
            return buffer[offset++];
        }

        public async Task SkipBytes(ushort n)
        {
            await RequestBytes(n);
            offset += n;
        }

        public async Task<string> ReadString()
        {
            ushort len = await ReadUInt16();
            int recv = await RequestBytes(len * 2); // unicode 2 chars

            for (ushort i = 0; i < len; ++i)
            {
                byte tmp = buffer[offset + (i * 2)];
                buffer[offset + (i * 2)] = buffer[offset + ((i * 2) + 1)];
                buffer[offset + ((i * 2) + 1)] = tmp;
            }
            string value = Enc.GetString(buffer, offset, len * 2);
            offset += len * 2;
            return value;
        }

        public async Task<bool> ReadBoolean()
        {
            return await ReadByte() > 0;
        }

        public async Task<RecvOpcode> ReadOpcode()
        {
            return (RecvOpcode)(await ReadByte());
        }
    }
}
