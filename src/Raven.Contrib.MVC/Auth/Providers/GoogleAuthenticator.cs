using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

namespace Raven.Contrib.MVC.Auth.Providers
{
    public class GoogleAuthenticator : OpenIdAuthenticator
    {
        /// <summary>
        /// The name of the provider which provides authentication service.
        /// </summary>
        public override string ProviderName
        {
            get
            {
                return "Google";
            }
        }

        public GoogleAuthenticator(ClaimsRequest requestedData)
            : base("https://www.google.com/accounts/o8/id", requestedData)
        {

        }
    }
}
