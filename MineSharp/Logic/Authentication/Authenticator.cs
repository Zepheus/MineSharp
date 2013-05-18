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

            return LoginResult.IllegalAccount; //TODO: check through post
        }
    }
}
