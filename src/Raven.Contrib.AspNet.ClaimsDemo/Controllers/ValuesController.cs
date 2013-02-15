using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Mvc;
using Raven.Client.Document;

namespace Raven.Contrib.AspNet.ClaimsDemo.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly DocumentStore store;

        public ValuesController()
        {
            this.store = DependencyResolver.Current.GetService<DocumentStore>();
        }

        public string GetAnonymous()
        {
            return "Everyone can see this";
        }

        [System.Web.Http.Authorize]
        public string GetAuthenticated()
        {
            return "Any authenticated user can see this";
        }

        [System.Web.Http.Authorize(Roles = "Admin")]
        public string GetAdminsOnly()
        {
            return "Admins only for this one";
        }

        public string GetEnforced()
        {
            ClaimPermission.CheckAccess("Do", new Claim(ClaimPermission.ResourceType, "R6"));
            return "We have enforced admins only access without an attribute";
        }

        public string PostLogin(FormDataCollection body)
        {
            string username = body.Get("username");
            string password = body.Get("password");

            using(var session = store.OpenSession())
            {
                var profile = session.Load<Profile>("profiles/" + username);
                if(profile.Password == password)
                {
                    var defaultPrincipal = new ClaimsPrincipal(
                        new ClaimsIdentity(new[] {new Claim(MyClaimTypes.ProfileKey, profile.Id)}, 
                            "Application" // this is important. if it's null or empty, IsAuthenticated will be false
                            ));
                    var principal = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.
                            ClaimsAuthenticationManager.Authenticate(
                                Request.RequestUri.AbsoluteUri, // this, or any other string can be available
                                                                // to your ClaimsAuthenticationManager
                                defaultPrincipal);
                    AuthenticationManager.EstablishSession(principal);
                    return "login ok";
                }
                return "login failed";
            }
        }
    }
}