using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    internal class Account
    {
        /*\ *** *** *** *** *** Classes *** *** *** *** *** \*/
        public class Identifier
        {
            public string Provider
            {
                get;
                set;
            }

            public string Value
            {
                get;
                set;
            }
        }

        /*\ *** *** *** *** *** Key *** *** *** *** *** \*/
        public string Id
        {
            get;
            set;
        }

        /*\ *** *** *** *** *** Basic Properties *** *** *** *** *** \*/
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

        /*\ *** *** *** *** *** Embedded Properties *** *** *** *** *** \*/
        public List<Identifier> Identifiers
        {
            get;
            set;
        }

        /*\ *** *** *** *** *** Constructor *** *** *** *** *** \*/
        public Account()
        {
            Identifiers = new List<Identifier>();
        }
    }
}
