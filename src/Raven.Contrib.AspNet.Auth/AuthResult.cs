using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth
{
    public class AuthResult
    {
        /// <summary>
        /// An enumeration of the possible results of an authentication attempt.
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// The authentication was canceled by the user agent while at the provider.
            /// </summary>
            Canceled = 0,

            /// <summary>
            /// The authentication failed because an error was detected in the communication.
            /// </summary>
            Failed = 1,

            /// <summary>
            /// Authentication was completed successfully.
            /// </summary>
            Authenticated = 2,
        }

        /// <summary>
        /// A container for the data returned by the provider.
        /// </summary>
        public class Data
        {
            /// <summary>
            /// The user's identifier.
            /// </summary>
            public string Identifier
            {
                get;
                set;
            }

            /// <summary>
            /// The user's profile.
            /// </summary>
            public dynamic Profile
            {
                get;
                set;
            }
        };

        /// <summary>
        /// The result of the authentication.
        /// </summary>
        public Status Result
        {
            get;
            set;
        }

        /// <summary>
        /// The exception that occured during authentication.
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }

        /// <summary>
        /// The information given by the authentication provider.
        /// </summary>
        public Data Information
        {
            get;
            set;
        }
    }
}
