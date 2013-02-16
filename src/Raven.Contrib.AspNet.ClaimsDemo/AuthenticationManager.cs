using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Raven.Client.Document;

namespace Raven.Contrib.AspNet.ClaimsDemo
{
    /// <summary>
    /// This class is tasked with populating the principal with the appropriate claims
    /// </summary>
    public class AuthenticationManager : ClaimsAuthenticationManager
    {
        // you'll need to manually inject this because this is instantiated by the framework and there seems to be a bug 
        // that doesn't allow you to create it manually
        public static DocumentStore Store { get; set; }

        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
                return base.Authenticate(resourceName, incomingPrincipal);

            return CreatePrincipal(incomingPrincipal);
        }

        private static ClaimsPrincipal CreatePrincipal(ClaimsPrincipal principal)
        {
            string profileKey = principal.FindFirst(MyClaimTypes.ProfileKey).Value;

            List<Claim> claims = new List<Claim>();
            using (var session = Store.OpenSession())
            {
                var profile = session.Load<Profile>(profileKey);

                claims.AddRange(new[]
                {
                    // copy over claim with profile key
                    principal.FindFirst(MyClaimTypes.ProfileKey),

                    new Claim(ClaimTypes.NameIdentifier, profile.Username),
                    
                    // add custom claims here                   
                    //new Claim(ClaimTypes.Email, profile.Email),
                    //new Claim(ClaimTypes.Name, profile.FirstName),
                    new Claim(ClaimTypes.GivenName, profile.Name)
                });
                claims.AddRange(profile.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims.ToArray(), "Application", 
                ClaimTypes.NameIdentifier, ClaimTypes.Role));
        }

        public static void EstablishSession(ClaimsPrincipal principal)
        {
            var sessionToken = new SessionSecurityToken(principal, TimeSpan.FromHours(12))
            {
                // IsReferenceMode = true // cache on server - caused me some problems
            };

            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionToken);
        }
    }
}