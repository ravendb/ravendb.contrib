using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Raven.Client.Contrib.MVC.Auth.Interfaces;

namespace Raven.Client.Contrib.MVC.Auth.Default
{
    internal class FormsAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Issues an authentication ticket.
        /// </summary>
        /// <param name="identifier">The authentication identifier.</param>
        /// <param name="persistent">true for persistent authentication, false otherwise</param>
        public void IssueAuthTicket(string identifier, bool persistent)
        {
            FormsAuthentication.SetAuthCookie(identifier, persistent);
        }

        /// <summary>
        /// Revokes the authentication ticket.
        /// </summary>
        public void RevokeAuthTicket()
        {
            FormsAuthentication.SignOut();
        }
    }
}
