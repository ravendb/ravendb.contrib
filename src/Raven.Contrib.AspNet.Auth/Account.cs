using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    internal class Account
    {
        public string Id
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string Identifier
        {
            get;
            set;
        }

        public string PasswordResetToken
        {
            get;
            set;
        }

        public DateTime? PasswordResetTokenExpiration
        {
            get;
            set;
        }
    }
}
