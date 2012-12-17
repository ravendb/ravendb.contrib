using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Raven.Client.Contrib.MVC.Auth.Default;
using Raven.Client.Contrib.MVC.Auth.Interfaces;

namespace Raven.Client.Contrib.MVC.Auth
{
    public class AuthConfiguration
    {
        private HttpContextBase _authContext;

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

        /// <summary>
        /// The current authentication context.
        /// </summary>
        public HttpContextBase AuthContext
        {
            get
            {
                if (_authContext == null)
                {
                    return new HttpContextWrapper(HttpContext.Current);
                }
                else
                {
                    return _authContext;
                }
            }
            set
            {
                _authContext = value;
            }
        }
    }
}