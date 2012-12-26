using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Raven.Contrib.AspNet.Auth.Interfaces
{
    /// <summary>
    /// Represents a client which can authenticate users via an external website/provider.
    /// </summary>
    public interface IExternalAuthProvider
    {
        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        string ProviderName
        {
            get;
        }

        /// <summary>
        /// Attempts to authenticate users by forwarding them to an external website,
        /// and upon succcess or failure, redirect users back to the specified url.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="callbackUri"> The URI to return to in order to complete authentication.</param>
        void RequestAuthentication(HttpContextBase context, Uri callbackUri);

        /// <summary>
        /// Check if authentication succeeded after user is redirected back
        /// from the service provider.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <returns>
        /// An instance of DotNetOpenAuth.AspNet.AuthenticationResult
        /// containing authentication result.
        /// </returns>
        AuthResult VerifyAuthentication(HttpContextBase context);
    }
}
