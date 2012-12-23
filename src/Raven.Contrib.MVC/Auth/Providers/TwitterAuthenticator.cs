using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.Contrib.MVC.Auth.Providers
{
    public class TwitterAuthenticator : OAuthAuthenticator
    {
        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        public override string ProviderName
        {
            get
            {
                return "Twitter";
            }
        }

        public TwitterAuthenticator(string consumerKey, string consumerSecret)
            : base(Description, Selector, "http://api.twitter.com/1/account/verify_credentials.json", consumerKey, consumerSecret)
        {

        }
        private static readonly ServiceProviderDescription Description = new ServiceProviderDescription
        {
            AccessTokenEndpoint       = new MessageReceivingEndpoint("http://twitter.com/oauth/access_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            RequestTokenEndpoint      = new MessageReceivingEndpoint("http://twitter.com/oauth/request_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            UserAuthorizationEndpoint = new MessageReceivingEndpoint("http://twitter.com/oauth/authenticate", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            TamperProtectionElements  = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
            ProtocolVersion           = ProtocolVersion.V10a,
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