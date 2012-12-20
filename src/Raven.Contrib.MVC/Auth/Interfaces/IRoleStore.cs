using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.MVC.Auth.Interfaces
{
    public interface IRoleStore
    {
        void CreateRole(string roleName);
        bool RoleExists(string roleName);
        bool DeleteRole(string roleName);

        string[] GetAllRoles();
        string[] GetUsersInRole(string roleName);
        string[] GetRolesForUser(string username);

        void AddUsersToRoles(string[] usernames, string[] roleNames);
        void RemoveUsersFromRoles(string[] usernames, string[] roleNames);
    }
}