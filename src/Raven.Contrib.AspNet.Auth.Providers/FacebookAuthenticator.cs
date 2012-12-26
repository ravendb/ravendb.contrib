using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth2;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.Contrib.AspNet.Auth.Providers
{
    public class FacebookAuthenticator : OAuth2Authenticator
    {
        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        public override string ProviderName
        {
            get
            {
                return "Facebook";
            }
        }

        public FacebookAuthenticator(string appId, string appSecret, params string[] scope)
            : base(Description, Selector, appId, appSecret, scope)
        {

        }

        public FacebookAuthenticator(string appId, string appSecret, IEnumerable<string> scope)
            : base(Description, Selector, appId, appSecret, scope)
        {

        }

        private static readonly AuthorizationServerDescription Description = new AuthorizationServerDescription
        {
            TokenEndpoint         = new Uri("https://graph.facebook.com/oauth/access_token"),
            AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize"),
            ProtocolVersion       = ProtocolVersion.V20,
        };

        private static readonly Func<string, AuthResult.Data> Selector = json =>
        {
            dynamic data = JsonConvert.DeserializeObject(json);

            return new AuthResult.Data
            {
                Identifier = data.id,
                Profile    = data,
            };
        };
    }
}
