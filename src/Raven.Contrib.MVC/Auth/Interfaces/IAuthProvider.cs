using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.MVC.Auth.Interfaces
{
    public interface IAuthProvider
    {
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
        /// <param name="identifier">The identifier of the new account.</param>
        /// <returns>The ID of the user account.</returns>
        void CreateAccount(string identifier);

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
