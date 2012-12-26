using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class DuplicateUserNameException : AuthException
    {
        public DuplicateUserNameException(string userName)
            : base("Duplicate username: " + userName)
        {
            
        }
    }
}
