using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Raven.Contrib.AspNet.Auth.Interfaces;

namespace Raven.Contrib.AspNet.Auth.Default
{
    internal class FormsAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Whether the user is logged in.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }

        /// <summary>
        /// The identifier of the currently logged-in account.
        /// </summary>
        public string Current
        {
            get
            {
                return IsAuthenticated ? HttpContext.Current.User.Identity.Name : null;
            }
        }

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
