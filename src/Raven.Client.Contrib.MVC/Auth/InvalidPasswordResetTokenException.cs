using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Client.Contrib.MVC.Auth
{
    public class InvalidPasswordResetTokenException : AuthException
    {
        public InvalidPasswordResetTokenException(string token)
            : base("Invalid password reset token: " + token)
        {
            
        }
    }
}
