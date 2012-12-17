using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Client.Contrib.MVC.Auth
{
    public class InvalidPasswordException : AuthException
    {
        public InvalidPasswordException()
            : base("Invalid password")
        {
            
        }
    }
}
