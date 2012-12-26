using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth.Interfaces
{
    public interface IRoleProvider
    {
        void CreateRole(string roleName);
        bool RoleExists(string roleName);
        bool DeleteRole(string roleName, bool throwOnPopulatedRole);

        bool IsUserInRole(string username, string roleName);
        void AddUsersToRoles(string[] usernames, string[] roleNames);
        void RemoveUsersFromRoles(string[] usernames, string[] roleNames);

        string[] GetAllRoles();
        string[] GetUsersInRole(string roleName);
        string[] GetRolesForUser(string username);
    }
}