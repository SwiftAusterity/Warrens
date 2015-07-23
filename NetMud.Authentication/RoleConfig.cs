using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Security;

namespace NetMud.Authentication
{
    public class RoleProvider : SqlRoleProvider
    {
        private const string authenticatedUsersRoleName = "Authenticated Users";

        /// <summary>
        /// Adds the "Authenticated Users" role to the list of roles retrieved
        /// from the database.
        /// </summary>
        private static string[] AddAuthenticatedUsersRole(string[] sqlRoles)
        {
            // The "Authenticated Users" role hasn't been removed from the database
            if (sqlRoles.Any(roleName => string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) == 0))
                return sqlRoles;

            string[] roles = new string[sqlRoles.Length + 1];
            roles[0] = authenticatedUsersRoleName;

            sqlRoles.CopyTo(roles, 1);

            return roles;
        }

        /// <summary>
        /// Adds the specified user names to each of the specified roles.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to
        /// the specified roles.</param>
        /// <param name="roleNames">A string array of role names to add the
        /// specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (roleNames == null)
                throw new ArgumentNullException("roleNames");
            else if (roleNames.Length == 0)
            {
                // Let the .NET Framework handle the error
                // (i.e. "The array parameter 'roleNames' should not be empty.")
                base.AddUsersToRoles(usernames, roleNames);
                return;
            }

            List<string> filteredRoleList = new List<string>();

            foreach (string roleName in roleNames)
            {
                if (string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) != 0)
                    filteredRoleList.Add(roleName);
                else
                {
                    //logging
                }
            }

            if (!filteredRoleList.Any())
                return;

            string[] filteredRoles = filteredRoleList.ToArray();

            base.AddUsersToRoles(usernames, filteredRoles);
        }

        /// <summary>
        /// Adds a new role to the role database.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            if (string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "The '{0}' role already exists.", roleName);

                throw new ArgumentException(message, "roleName");
            }

            base.CreateRole(roleName);
        }

        /// <summary>
        /// Removes a role from the role database.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">If <c>true</c>, throws an exception
        /// if roleName has one or more members.</param>
        /// <returns><c>true</c> if the role was successfully deleted;
        /// otherwise, <c>false</c>.</returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            // Allow the "Authenticated Users" role to be deleted
            // if and only if it exists in the database (i.e. the
            // role created previously with the ASP.NET SqlRoleProvider
            // was not properly cleaned up prior to switching over to
            // the custom role provider.
            if (string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) == 0
                && !base.RoleExists(roleName))
            {
                string message = string.Format(CultureInfo.CurrentCulture, "The '{0}' role cannot be deleted.", authenticatedUsersRoleName);

                throw new ArgumentException(message, "roleName");
            }

            return base.DeleteRole(roleName, throwOnPopulatedRole);
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains
        /// the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>A string array containing the names of all the users where
        /// the user name matches usernameToMatch and the user is a member of
        /// the specified role.</returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "This method is not supported for the '{0}' role.", authenticatedUsersRoleName);

                throw new ArgumentException(message);
            }

            return base.FindUsersInRole(roleName, usernameToMatch);
        }

        /// <summary>
        /// Gets a list of all the roles for the application.
        /// </summary>
        /// <returns>A string array containing the names of all the roles stored
        /// in the database for a particular application.</returns>
        public override string[] GetAllRoles()
        {
            string[] sqlRoles = base.GetAllRoles();

            string[] roles = AddAuthenticatedUsersRole(sqlRoles);

            return roles;
        }

        /// <summary>
        /// Gets a list of the roles that a user is in.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.
        /// </param>
        /// <returns>A string array containing the names of all the roles that
        /// the specified user is in.</returns>
        public override string[] GetRolesForUser(string username)
        {
            string[] sqlRoles = base.GetRolesForUser(username);

            string[] roles = AddAuthenticatedUsersRole(sqlRoles);

            return roles;
        }

        /// <summary>
        /// Gets a list of users in the specified role.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users
        /// for.</param>
        /// <returns>A string array containing the names of all the users who
        /// are members of the specified role.</returns>
        public override string[] GetUsersInRole(string roleName)
        {
            if (string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) == 0)
                return new string[0];

            return base.GetUsersInRole(roleName);
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the
        /// specified role.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns><c>true</c> if the specified user name is in the
        /// specified role; otherwise, <c>false</c>.</returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            if (string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) == 0)
                return true; // assume the specified user is valid

            return base.IsUserInRole(username, roleName);
        }

        /// <summary>
        /// Removes the specified user names from the specified roles.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed
        /// from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the
        /// specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (roleNames == null)
                throw new ArgumentNullException("roleNames");
            else if (roleNames.Length == 0)
            {
                // Let the .NET Framework handle the error
                // (i.e. "The array parameter 'roleNames' should not be empty.")
                base.AddUsersToRoles(usernames, roleNames);
                return;
            }

            List<string> filteredRoleList = new List<string>();

            foreach (string roleName in roleNames)
            {
                if (string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) != 0)
                    filteredRoleList.Add(roleName);
                else
                {
                    //logging
                }
            }

            if (filteredRoleList.Count == 0)
                return;

            string[] filteredRoles = filteredRoleList.ToArray();

            base.RemoveUsersFromRoles(usernames, filteredRoles);
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already
        /// exists in the role database.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the
        /// database.</param>
        /// <returns><c>true</c> if the role name already exists in the
        /// database; otherwise, <c>false</c>.</returns>
        public override bool RoleExists(string roleName)
        {
            if (string.Compare(roleName, authenticatedUsersRoleName, StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            return base.RoleExists(roleName);
        }
    }
}
