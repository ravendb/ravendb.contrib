using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class InvalidUserNameException : InvalidCredentialException
    {
        public InvalidUserNameException(string userName)
            : base("Invalid username: " + userName)
        {
            
        }
    }
}
