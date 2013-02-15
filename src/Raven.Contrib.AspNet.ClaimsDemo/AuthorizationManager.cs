using System.Linq;
using System.Security.Claims;

namespace Raven.Contrib.AspNet.ClaimsDemo
{
    /// <summary>
    /// This class is responsible for deciding whether a user is allowed
    /// to do something based on the claims he has. In the context you can define
    /// claims for <see cref="ClaimPermission.ActionType"/> and <see cref="ClaimPermission.ResourceType"/>
    /// and decide.
    /// </summary>
    public class AuthorizationManager : ClaimsAuthorizationManager
    {
        public override bool CheckAccess(AuthorizationContext context)
        {
            string action = context.Action.Single().Value;
            string resource = context.Resource.Single().Value;

            // could be anything - check e.g. context.Principal.HasClaim()
            if(action == "Do" && resource == "R6" && context.Principal.IsInRole("Admin"))
            {
                return true;
            }

            // deny otherwise
            return false;
        }
    }
}