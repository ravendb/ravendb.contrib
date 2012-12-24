using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DotNetOpenAuth.OAuth2;
using Raven.Contrib.MVC.Auth.Interfaces;

namespace Raven.Contrib.MVC.Auth.Providers
{
    public abstract class OAuth2Authenticator : IExternalAuthProvider
    {
        private readonly WebServerClient _client;
        private readonly Func<string, AuthResult.Data> _selector;
        private readonly IEnumerable<string> _scope;

        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        public abstract string ProviderName
        {
            get;
        }

        protected OAuth2Authenticator(
            AuthorizationServerDescription description,
            Func<string, AuthResult.Data> selector,
            string consumerKey,
            string consumerSecret,
            IEnumerable<string> scope)
        {
            _scope    = scope;
            _selector = selector;
            _client   = new WebServerClient(description)
            {
                ClientIdentifier           = consumerKey,
                ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(consumerSecret),
            };
        }

        /// <summary>
        /// Attempts to authenticate users by forwarding them to an external website,
        /// and upon succcess or failure, redirect users back to the specified url.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="callbackUri"> The URI to return to in order to complete authentication.</param>
        public void RequestAuthentication(HttpContextBase context, Uri callbackUri)
        {
            _client.RequestUserAuthorization(_scope, callbackUri);
        }

        /// <summary>
        /// Check if authentication succeeded after user is redirected back
        /// from the service provider.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <returns>
        /// An instance of DotNetOpenAuth.AspNet.AuthenticationResult
        /// containing authentication result.
        /// </returns>
        public AuthResult VerifyAuthentication(HttpContextBase context)
        {
            try
            {
                var authorization = _client.ProcessUserAuthorization();

                if (authorization == null || authorization.AccessToken == null)
                    return new AuthResult { Result = AuthResult.Status.Canceled };

                var request = WebRequest.Create("https://graph.facebook.com/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));

                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream == null)
                            throw new ApplicationException("Bad provider response.");

                        using (var reader = new StreamReader(stream))
                        {
                            return new AuthResult
                            {
                                Result      = AuthResult.Status.Authenticated,
                                Information = _selector.Invoke(reader.ReadToEnd()),
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new AuthResult
                {
                    Result    = AuthResult.Status.Failed,
                    Exception = e,
                };
            }
        }
    }
}
