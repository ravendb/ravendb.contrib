using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Client.Contrib.MVC.Auth
{
    public class InvalidCredentialsException : AuthException
    {
        public InvalidCredentialsException()
            : base("Invalid username or password")
        {
            
        }
    }
}
