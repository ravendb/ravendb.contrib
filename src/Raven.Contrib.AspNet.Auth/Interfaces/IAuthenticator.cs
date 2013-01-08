using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth.Interfaces
{
    public interface IAuthenticator
    {
        /// <summary>
        /// Whether the user is logged in.
        /// </summary>
        bool IsAuthenticated
        {
            get;
        }

        /// <summary>
        /// The identifier of the currently logged-in account.
        /// </summary>
        string Current
        {
            get;
        }

        /// <summary>
        /// Issues an authentication ticket.
        /// </summary>
        /// <param name="identifier">The authentication identifier.</param>
        /// <param name="persistent">true for persistent authentication, false otherwise</param>
        void IssueAuthTicket(string identifier, bool persistent);

        /// <summary>
        /// Revokes the authentication ticket.
        /// </summary>
        void RevokeAuthTicket();
    }
}
