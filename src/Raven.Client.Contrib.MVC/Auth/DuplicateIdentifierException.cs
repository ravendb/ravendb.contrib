using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Client.Contrib.MVC.Auth
{
    public class DuplicateIdentifierException : AuthException
    {
        public DuplicateIdentifierException(string identifier)
            : base("Duplicate identifier: " + identifier)
        {

        }
    }
}
