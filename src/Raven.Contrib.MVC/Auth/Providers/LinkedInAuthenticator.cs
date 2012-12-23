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
    public class LinkedInAuthenticator : OAuthAuthenticator
    {
        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        public override string ProviderName
        {
            get
            {
                return "LinkedIn";
            }
        }

        public LinkedInAuthenticator(string apiKey, string secret, params string[] fields)
            : base(Description, Selector, String.Format("http://api.linkedin.com/v1/people/~:({0})?format=json", String.Join(",", fields)), apiKey, secret)
        {

        }

        public LinkedInAuthenticator(string apiKey, string secret, IEnumerable<string> fields)
            : base(Description, Selector, String.Format("http://api.linkedin.com/v1/people/~:({0})?format=json", String.Join(",", fields)), apiKey, secret)
        {

        }

        private static readonly ServiceProviderDescription Description = new ServiceProviderDescription
        {
            AccessTokenEndpoint       = new MessageReceivingEndpoint("https://api.linkedin.com/uas/oauth/accessToken", HttpDeliveryMethods.GetRequest),
            RequestTokenEndpoint      = new MessageReceivingEndpoint("https://api.linkedin.com/uas/oauth/requestToken", HttpDeliveryMethods.GetRequest),
            UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://api.linkedin.com/uas/oauth/authenticate", HttpDeliveryMethods.GetRequest),
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