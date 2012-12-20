using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.MVC.Auth
{
    public class InvalidPasswordException : AuthException
    {
        public InvalidPasswordException()
            : base("Invalid password")
        {
            
        }
    }
}
