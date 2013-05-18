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

namespace MineSharp.Networking
{
    public class PacketReader
    {
        private static Encoding Enc = System.Text.Encoding.Unicode;
        private const ushort BufferSize = 8 * 1024;

        private byte[] buffer;
        private Socket sock;

        public PacketReader(Socket socket)
        {
            this.buffer = new byte[BufferSize];
            this.sock = socket;
        }

        public async Task<int> RequestBytes(int len)
        {
            if (len > BufferSize)
                throw new OverflowException("Buffer is too small");

            int recv = await Task.Factory.FromAsync(
                         (cb, s) => sock.BeginReceive(buffer, 0, len, SocketFlags.None, cb, s),
                         ias => sock.EndReceive(ias), null);
            if (recv < 0 || len != recv)
                throw new Exception("Failed to read bytes");

            return recv;
        }

        public async Task<uint> ReadUInt32()
        {
            await RequestBytes(4);
            return (uint)((buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | buffer[0]);
        }

        public async Task<ushort> ReadUInt16()
        {
            await RequestBytes(2);
            return (ushort)((buffer[1] << 8) | buffer[0]);
        }

        public async Task<byte> ReadByte()
        {
            await RequestBytes(1);
            return buffer[0];
        }

        public async Task<string> ReadString()
        {
            ushort len = await ReadUInt16();
            int recv = await RequestBytes(len * 2); // unicode 2 chars
            for (ushort i = 0; i < len; ++i)
            {
                byte tmp = buffer[i * 2];
                buffer[(i * 2)] = buffer[(i * 2) + 1];
                buffer[(i * 2) + 1] = tmp;
            }
            return Enc.GetString(buffer, 0, len * 2);
        }

        public async Task<RecvOpcode> ReadOpcode()
        {
            return (RecvOpcode)(await ReadByte());
        }
    }
}
