using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class DuplicateIdentifierException : AuthenticationException
    {
        public DuplicateIdentifierException(string identifier)
            : base("Duplicate identifier: " + identifier)
        {

        }
    }
}
