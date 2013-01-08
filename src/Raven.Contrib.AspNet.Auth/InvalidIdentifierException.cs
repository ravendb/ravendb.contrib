using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class InvalidIdentifierException : AuthenticationException
    {
        public InvalidIdentifierException(string identifier)
            : base("Invalid identifier: " + identifier)
        {
            
        }
    }
}
