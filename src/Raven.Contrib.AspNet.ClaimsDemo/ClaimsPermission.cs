using System.Collections.ObjectModel;
using System.IdentityModel.Services;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Claims;
using System.Security.Permissions;
using System.Security.Policy;

namespace Raven.Contrib.AspNet.ClaimsDemo
{
    public static class ClaimPermission
    {
        /// <summary>
        /// Default action claim type.
        /// </summary>
        public const string ActionType = "http://application/claims/authorization/action";

        /// <summary>
        /// Default resource claim type
        /// </summary>
        public const string ResourceType = "http://application/claims/authorization/resource";

        /// <summary>
        /// Calls ClaimsAuthorizationManager.
        /// </summary>
        /// <param name="operation">The action.</param>
        /// <param name="resources">The resources for to be checked </param>
        /// <exception cref="SecurityException">If access is denied</exception>
        public static void CheckAccess(
            string operation,
            params Claim[] resources)
        {
            var actionClaim = new Claim(ActionType, operation);
            var context = new AuthorizationContext(
                ClaimsPrincipal.Current,
                new Collection<Claim>(resources),
                new Collection<Claim> { actionClaim });

            var federationConfiguration = FederatedAuthentication.FederationConfiguration;
            var claimsAuthorizationManager =
                federationConfiguration.IdentityConfiguration.ClaimsAuthorizationManager;
            if (!claimsAuthorizationManager.CheckAccess(context))
            {
                string resource = resources.Any() ? resources.First().Value : "None";
                var permission = new ClaimsPrincipalPermission(resource, operation);
                ThrowSecurityException(permission);
            }
        }

        private static void ThrowSecurityException(IPermission permThatFailed)
        {
            AssemblyName assemblyName = null;
            Evidence evidence = null;
            new PermissionSet(PermissionState.Unrestricted).Assert();

            try
            {
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                assemblyName = callingAssembly.GetName();
                if (callingAssembly != Assembly.GetExecutingAssembly())
                {
                    evidence = callingAssembly.Evidence;
                }
            }
            catch
            {
            }

            PermissionSet.RevertAssert();
            throw new SecurityException("Access Denied", assemblyName, null, null, null,
                SecurityAction.Demand, permThatFailed, permThatFailed, evidence);
        }
    }
}