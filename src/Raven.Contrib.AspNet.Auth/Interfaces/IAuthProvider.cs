using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth.Interfaces
{
    public interface IAuthProvider
    {
        /// <summary>
        /// The username of the currently logged-in account.
        /// </summary>
        string Current
        {
            get;
        }

        /// <summary>
        /// Determines whether the user is logged in.
        /// </summary>
        bool IsAuthenticated
        {
            get;
        }

        /// <summary>
        /// Logs the user in if the <paramref name="userName"/> and <paramref name="password"/> combination is valid.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="persistent">true for persistent authentication, false otherwise</param>
        /// <returns>true if the authentication was successful, false otherwise.</returns>
        void Login(string userName, string password, bool persistent = false);

        /// <summary>
        /// Logs the user in if the <paramref name="identifier"/> is valid.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="persistent">true for persistent authentication, false otherwise</param>
        /// <returns>true if the authentication was successful, false otherwise.</returns>
        void Login(string identifier, bool persistent = false);

        /// <summary>
        /// Logs the current user out.
        /// </summary>
        void Logout();
        /// <summary>
        /// Creates a local user account.
        /// </summary>
        /// <param name="userName">The username of the new account.</param>
        /// <param name="password">The password of the new account.</param>
        /// <returns>The ID of the user account.</returns>
        void CreateAccount(string userName, string password);

        /// <summary>
        /// Creates an external user account.
        /// </summary>
        /// <param name="userName">The username of the new account.</param>
        /// <param name="identifier">The identifier of the new account.</param>
        /// <param name="providerName">The name of the external auth provider.</param>
        /// <returns>The ID of the user account.</returns>
        void CreateExternalAccount(string userName, string identifier, string providerName);

        /// <summary>
        /// Changes the password for a user.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns> </returns>
        void ChangePassword(string userName, string oldPassword, string newPassword);

        /// <summary>
        /// Sets the password for a user.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="newPassword">The new password.</param>
        void SetPassword(string userName, string newPassword);

        /// <summary>
        /// Checks whether an account is a local account.
        /// </summary>
        /// <param name="userName">The username of the account to check.</param>
        /// <returns>true if the current account is a local account, false otherwise</returns>
        bool IsLocalAccount(string userName);

        /// <summary>
        /// Adds an identifier for an external auth provider to an account. 
        /// </summary>
        /// <param name="userName">The username of the account.</param>
        /// <param name="identifier">The identifier to add.</param>
        /// <param name="providerName">The name of the external auth provider.</param>
        void AddIdentifier(string userName, string identifier, string providerName);

        /// <summary>
        /// Removes an identifier for an external authenticator from an account. 
        /// </summary>
        /// <param name="userName">The username of the account.</param>
        /// <param name="providerName">The name of the external auth provider.</param>
        void RemoveIdentifier(string userName, string providerName);

        /// <summary>
        /// Retrieves all the identifiers for an account.
        /// </summary>
        /// <param name="userName">The username of the account.</param>
        /// <returns>A dictionary with the provider name as the key and the identifier as the value.</returns>
        IDictionary<string, string> GetIdentifiers(string userName);

        /// <summary>
        /// Generates a password reset token for a user.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="expiry">The token expiration.</param>
        /// <returns>The password reset token.</returns>
        string GeneratePasswordResetToken(string userName, TimeSpan expiry);

        /// <summary>
        /// Resets the password for the supplied <paramref name="passwordResetToken" />
        /// </summary>
        /// <param name="passwordResetToken">The password reset token to perform the lookup on.</param>
        /// <param name="newPassword">The new password for the user.</param>
        void ResetPassword(string passwordResetToken, string newPassword);
    }
}
