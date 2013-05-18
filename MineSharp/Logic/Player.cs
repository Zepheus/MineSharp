using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSharp.Logic
{
    class Player
    {
        public string Username { get; private set; }

        public Player(string name)
        {
            this.Username = name;
        }
    }
}
