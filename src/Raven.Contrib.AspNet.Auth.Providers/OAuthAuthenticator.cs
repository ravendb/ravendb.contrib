using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using Raven.Contrib.AspNet.Auth.Interfaces;

namespace Raven.Contrib.AspNet.Auth.Providers
{
    public abstract class OAuthAuthenticator : IExternalAuthProvider
    {
        private readonly WebConsumer _client;
        private readonly Func<string, AuthResult.Data> _selector;
        private readonly MessageReceivingEndpoint _endpoint;

        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        public abstract string ProviderName
        {
            get;
        }

        protected OAuthAuthenticator(
            ServiceProviderDescription description,
            Func<string, AuthResult.Data> selector,
            string endpoint,
            string consumerKey,
            string consumerSecret)
        {
            _endpoint = new MessageReceivingEndpoint(endpoint, HttpDeliveryMethods.GetRequest);
            _selector = selector;
            _client = new WebConsumer(description, new InMemoryTokenManager(consumerKey, consumerSecret));
        }
        
        /// <summary>
        /// Attempts to authenticate users by forwarding them to an external website,
        /// and upon succcess or failure, redirect users back to the specified url.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="callbackUri"> The URI to return to in order to complete authentication.</param>
        public void RequestAuthentication(HttpContextBase context, Uri callbackUri)
        {
            _client.Channel.Send(_client.PrepareRequestUserAuthorization(callbackUri, null, null));
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

                var request = _client.PrepareAuthorizedRequest(_endpoint, authorization.AccessToken);

                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream == null)
                            throw new InvalidOperationException("Bad provider response.");

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
