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
using System.Net.Sockets;
using System.IO;

using MineSharp.Handlers;
using MineSharp.Logic;
using MineSharp.Logic.Authentication;

namespace MineSharp.Networking
{
    class Client
    {
        private string Username { get; set; }
        public bool IsConnected { get { return connected; } }
        public bool IsAuthenticated { get { return authenticated; } }
        public Player Player { get { return player; } } 

        private Socket sock;
        private PacketReader reader;
        private Player player;
        private bool connected;
        private bool handling;
        private bool authenticated;

        public event EventHandler<EventArgs> OnDisconnect;

        public Client(Socket client)
        {
            this.sock = client;
            this.reader = new PacketReader(client);
            connected = true;
        }

        public string GetHostName()
        {
            return sock.RemoteEndPoint.ToString();
        }

        public void StartHandlers()
        {
            if (!handling)
            {
                Task.Factory.StartNew(async () =>
                {
                    while (connected)
                    {
                        try
                        {
                            RecvOpcode opcode = await reader.ReadOpcode();
                            await PacketManager.Instance.ExecuteHandler(opcode, this, reader);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Disconnect();
                        }
                    }
                });
                handling = true;
            }
        }

        public void Disconnect()
        {
            if (connected)
            {
                if (OnDisconnect != null)
                    OnDisconnect(this, EventArgs.Empty);

                connected = false;
            }
        }

        public void Send(PacketWriter packet)
        {
            byte[] data = packet.GetBytes();
            sock.Send(data); //TODO: async send (if possible)
           // Console.WriteLine("Sent: {0}", BytesToString(data));
        }

        private static string BytesToString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append(b.ToString("X2") + " ");
            return sb.ToString();
        }

        public LoginResult Authenticate(string username, string host, uint port)
        {
            LoginResult res = Authenticator.Authenticate(username, host, port);
            if (res == LoginResult.LoggedIn)
            {
                //TODO: key exchange
                this.player = new Player(username);
            }
            return res;
        }
    }
}
