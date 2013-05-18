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
using System.Reflection;

using MineSharp.Util;

namespace MineSharp.Networking
{
    class PacketManager
    {
        private static PacketManager _instance;
        public static PacketManager Instance { get { return _instance ?? (_instance = new PacketManager()); } }

        public delegate Task PacketHandler(Client client, PacketReader reader);

        private Dictionary<RecvOpcode, PacketHandler> handlers;

        public PacketManager()
        {
            handlers = new Dictionary<RecvOpcode, PacketHandler>();
        }

        public async Task ExecuteHandler(RecvOpcode opcode, Client client, PacketReader reader)
        {
            PacketHandler handler;
            if (handlers.TryGetValue(opcode, out handler))
            {
                await handler(client, reader);
            }
            else throw new Exception("No handler found for: " + ((byte)opcode).ToString("X2"));
        }

        public void LoadHandlers()
        {
            //handlers.Add(RecvOpcode.ServerStats, Handlers.HandleServerStats);
            handlers.Clear();
            foreach (var info in Reflector.FindMethodsByAttribute<PacketHandlerAttribute>())
            {
                PacketHandlerAttribute attribute = info.Item1;
                MethodInfo method = info.Item2;
                handlers.Add(attribute.Opcode, (PacketHandler)Delegate.CreateDelegate(typeof(PacketHandler), method));
            }
            Console.WriteLine("Loaded {0} handlers.", handlers.Count);
        }
    }
}
