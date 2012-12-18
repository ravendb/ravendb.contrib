using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DotNetOpenAuth.AspNet;
using Raven.Contrib.MVC.Auth.Interfaces;
using Raven.Contrib.MVC.Extensions;

namespace Raven.Contrib.MVC.Auth
{
    public partial class AuthProvider
    {
        private static readonly IDictionary<string, IExternalAuthProvider> Providers;

        /// <summary>
        /// The authentication provider configuration.
        /// </summary>
        public static AuthConfiguration Configuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the static properties of the <see cref="AuthProvider" /> class.
        /// </summary>
        static AuthProvider()
        {
            Configuration = new AuthConfiguration();
            Providers     = new Dictionary<string, IExternalAuthProvider>();
        }

        /// <summary>
        /// Registers an OAuth/OpenID authentication provider.
        /// </summary>
        /// <param name="client">The authentication provider.</param>
        public static void RegisterProvider(IExternalAuthProvider client)
        {
            Providers.Add(client.ProviderName, client);
        }

        /// <summary>
        /// Requests the OAuth/OpenID authentication.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="verifyUrl">The URL to return for verification.</param>
        public static void RequestAuthentication(string providerName, string verifyUrl)
        {
            if (providerName == null)
                throw new ArgumentNullException("providerName");

            if (verifyUrl == null)
                throw new ArgumentNullException("verifyUrl");

            var provider = Providers[providerName];
            var context  = new HttpContextWrapper(HttpContext.Current);

            provider.RequestAuthentication(context, ConvertToAbsoluteUri(verifyUrl, context));
        }

        /// <summary>
        /// Verifies the OAuth/OpenID authentication.
        /// </summary>
        /// <returns>The authentication result.</returns>
        public static AuthResult VerifyAuthentication()
        {
            string providerName = OpenAuthSecurityManager.GetProviderName(Configuration.AuthContext);

            if (String.IsNullOrEmpty(providerName))
            {
                return new AuthResult
                {
                    Result    = AuthResult.Status.Failed,
                    Exception = new InvalidOperationException("Current provider name is null"),
                };
            }

            var provider = Providers[providerName];
            var context  = new HttpContextWrapper(HttpContext.Current);

            return provider.VerifyAuthentication(context);
        }

        private static Uri ConvertToAbsoluteUri(string returnUrl, HttpContextBase context)
        {
            if (Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute))
                return new Uri(returnUrl, UriKind.Absolute);

            if (!VirtualPathUtility.IsAbsolute(returnUrl))
                returnUrl = VirtualPathUtility.ToAbsolute(returnUrl);

            return new Uri(context.PublicUri(), returnUrl);
        }
    }
}
