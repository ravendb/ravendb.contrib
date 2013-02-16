using System.Web.Http;

namespace Raven.Contrib.AspNet.ClaimsDemo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute("Anonymous", "api/values/anonymous", 
                new { controller = "Values", action = "GetAnonymous" });
            config.Routes.MapHttpRoute("Authenticated", "api/values/authenticated",
                new { controller = "Values", action = "GetAuthenticated" });
            config.Routes.MapHttpRoute("AdminsOnly", "api/values/adminsOnly",
                new { controller = "Values", action = "GetAdminsOnly" });
            config.Routes.MapHttpRoute("Enforced", "api/values/enforced",
                new { controller = "Values", action = "GetEnforced" });
            config.Routes.MapHttpRoute("Login", "api/login",
                new { controller = "Values", action = "PostLogin" });
        }
    }
}
