using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using Raven.Contrib.AspNet.Auth.Interfaces;

namespace Raven.Contrib.AspNet.Auth.Providers
{
    public abstract class OpenIdAuthenticator : IExternalAuthProvider
    {
        private readonly Identifier _provider;
        private readonly ClaimsRequest _requestedData;
        private readonly OpenIdRelyingParty _openid = new OpenIdRelyingParty();

        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        public abstract string ProviderName
        {
            get;
        }

        protected OpenIdAuthenticator(string providerUrl, ClaimsRequest requestedData)
        {
            _requestedData = requestedData;
            _provider      = Identifier.Parse(providerUrl);
        }
        
        /// <summary>
        /// Attempts to authenticate users by forwarding them to an external website,
        /// and upon succcess or failure, redirect users back to the specified url.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="callbackUri"> The URI to return to in order to complete authentication.</param>
        public void RequestAuthentication(HttpContextBase context, Uri callbackUri)
        {
            var request = _openid.CreateRequest(_provider, Realm.AutoDetect, callbackUri);

            request.AddExtension(_requestedData);
            request.RedirectToProvider();
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
                var response = _openid.GetResponse();

                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        return new AuthResult
                        {
                            Result      = AuthResult.Status.Authenticated,
                            Information = new AuthResult.Data
                            {
                                Identifier = response.ClaimedIdentifier,
                                Profile    = response.GetExtension<ClaimsResponse>(),
                            },
                        };

                    case AuthenticationStatus.Canceled:
                        return new AuthResult
                        {
                            Result = AuthResult.Status.Canceled
                        };

                    default:
                        return new AuthResult
                        {
                            Result    = AuthResult.Status.Failed,
                            Exception = response.Exception,
                        };
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