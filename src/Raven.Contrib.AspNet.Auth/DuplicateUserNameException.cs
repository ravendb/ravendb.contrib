using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class DuplicateUserNameException : AuthenticationException
    {
        public DuplicateUserNameException(string userName)
            : base("Duplicate username: " + userName)
        {
            
        }
    }
}
