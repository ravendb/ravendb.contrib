using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class InvalidIdentifierException : AuthException
    {
        public InvalidIdentifierException(string identifier)
            : base("Invalid identifier: " + identifier)
        {
            
        }
    }
}
