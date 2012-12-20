using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Raven.Client;
using Raven.Contrib.MVC.Auth.Default;
using Raven.Contrib.MVC.Auth.Interfaces;

namespace Raven.Contrib.MVC.Auth
{
    public class AuthConfiguration
    {
        public AuthConfiguration()
        {
            Authenticator   = new FormsAuthenticator();
            SecurityEncoder = new BCryptSecurityEncoder();
        }

        /// <summary>
        /// The authenticator to use.
        /// </summary>
        public IAuthenticator Authenticator
        {
            get;
            set;
        }

        /// <summary>
        /// The RavenDB document store to use.
        /// </summary>
        public IDocumentStore DocumentStore
        {
            get;
            set;
        }

        /// <summary>
        /// The security encoder to use for securing user identifiers.
        /// </summary>
        public ISecurityEncoder SecurityEncoder
        {
            get;
            set;
        }
    }
}