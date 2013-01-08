using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class InvalidPasswordException : InvalidCredentialException
    {
        public InvalidPasswordException()
            : base("Invalid password")
        {
            
        }
    }
}
