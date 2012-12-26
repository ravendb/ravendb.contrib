using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class InvalidCredentialsException : AuthException
    {
        public InvalidCredentialsException()
            : base("Invalid username or password")
        {
            
        }
    }
}
