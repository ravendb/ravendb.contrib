using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

namespace Raven.Contrib.AspNet.Auth.Providers
{
    public class YahooAuthenticator : OpenIdAuthenticator
    {
        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        public override string ProviderName
        {
            get
            {
                return "Yahoo";
            }
        }

        public YahooAuthenticator(ClaimsRequest requestedData)
            : base("http://me.yahoo.com", requestedData)
        {

        }
    }
}
