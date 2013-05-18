using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSharp.Logic.Authentication
{
    class Authenticator
    {
        public static LoginResult Authenticate(string username, string host, uint port)
        {
            Console.WriteLine("{0} logging in...", username);
            return LoginResult.IllegalAccount; //TODO: check through post
        }
    }
}
