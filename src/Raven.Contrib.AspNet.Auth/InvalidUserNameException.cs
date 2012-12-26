using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class InvalidUserNameException : AuthException
    {
        public InvalidUserNameException(string userName)
            : base("Invalid username: " + userName)
        {
            
        }
    }
}
