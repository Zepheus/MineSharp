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
using System.Threading.Tasks;

using MineSharp.Networking;
using MineSharp.Logic.Authentication;

namespace MineSharp.Handlers
{
    class Handlers
    {
        
        [PacketHandler(RecvOpcode.ServerStats)]
        public static async Task HandleServerStats(Client client, PacketReader reader)
        {
            await reader.ReadByte();
            using (var packet = new PacketWriter(SendOpcode.Kick))
            {
                packet.WriteString("§1\0{0}\0{1}\0{2}\0{3}\0{4}", 
                    Server.Protocol, Server.Version, Server.Instance.GetMOTD(),
                    Server.Instance.PlayerCount, Server.Instance.Max);

                byte[] data = packet.GetBytes();
                client.Send(packet);
            }
        }

        [PacketHandler(RecvOpcode.Handshake)]
        public static async Task HandleHandshake(Client client, PacketReader reader)
        {
            byte protocol = await reader.ReadByte();
            string username = await reader.ReadString();
            string host = await reader.ReadString();
            uint port = await reader.ReadUInt32();

            LoginResult res = client.Authenticate(username, host, port);
            if (res != LoginResult.LoggedIn)
            {
                using (var packet = new PacketWriter(SendOpcode.Kick))
                {
                    packet.WriteString("DERP!!! Server disconnected, reason: {0}", res.ToString());
                    client.Send(packet);
                }
            }
            else
            {
                //TODO: chunks?
            }
        }
       
    }
}
