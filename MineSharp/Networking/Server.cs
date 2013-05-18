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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MineSharp.Networking
{
    class Server
    {
        public const ushort MCPort = 25565;
        public const byte MaxPlayers = 20;
        public const string Protocol = "61";
        public const string Version = "1.5.2";

        private static Server _instance;
        public static Server Instance { get { return _instance ?? (_instance = new Server(MCPort, MaxPlayers)); } }

        public ushort Port { get; private set; }
        public byte Max { get; private set; }
        public bool IsListening { get; private set; }
        public int PlayerCount { get { return clients.Count; } }

        private Socket socket;
        private List<Client> clients;

        public Server(ushort port, byte max)
        {
            this.Port = port;
            this.Max = max;
            this.clients = new List<Client>();

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        public void Start()
        {
            if (!IsListening)
            {
                Console.WriteLine("Server listening on {0}.", Port);

                Task.Factory.StartNew(async () =>
                {
                    socket.Listen(32);
                    while (IsListening)
                    {
                        Socket accepted = await Task.Factory.FromAsync(
                            (c, s) => socket.BeginAccept(c, s),
                            iar => socket.EndAccept(iar), null);

                        Client client = new Client(accepted);
                        Console.WriteLine("{0} connected.", client.GetHostName());
                        client.OnDisconnect += client_OnDisconnect;
                        client.StartHandlers();
                        clients.Add(client);
                    }
                    Console.WriteLine("Server stopped listening.");
                });
                IsListening = true;
            }
        }

        public string GetMOTD()
        {
            return "Hello world"; //TODO: config or database
        }

        void client_OnDisconnect(object sender, EventArgs e)
        {
            Client client = (Client)sender;
            clients.Remove(client);

            Console.WriteLine("{0} disconnected.", client.GetHostName());
        }

        public void Stop()
        {
            IsListening = false; //warning: volatile bool!!
        }
    }
}
