using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Web;
using Raven.Client.Contrib.MVC.Auth.Interfaces;
using Raven.Json.Linq;

namespace Raven.Client.Contrib.MVC.Auth
{
    public partial class AuthProvider : IAuthProvider
    {
        private readonly IDocumentStore _store;
        private readonly ISecurityEncoder _encoder;
        private readonly IAuthenticator _authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthProvider" /> class.
        /// </summary>
        public AuthProvider()
        {
            _store         = Configuration.DocumentStore;
            _encoder       = Configuration.SecurityEncoder;
            _authenticator = Configuration.Authenticator;
        }

        /// <summary>
        /// Logs the user in if the <paramref name="userName"/> and <paramref name="password"/> combination is valid.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="persistent">true for persistent authentication, false otherwise</param>
        /// <returns>true if the authentication was successful, false otherwise.</returns>
        public void Login(string userName, string password, bool persistent = false)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (password == null)
                throw new ArgumentNullException("password");

            using (var db = _store.OpenSession())
            {
                var account = db.Query<Account>().SingleOrDefault(a => a.UserName == userName);
                if (account == null)
                    throw new InvalidCredentialException();

                if (!_encoder.Verify(password, account.Password))
                    throw new InvalidCredentialException();

                _authenticator.IssueAuthTicket(account.Id, persistent);
            }
        }

        /// <summary>
        /// Logs the user in if the <paramref name="identifier"/> is valid.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="persistent">true for persistent authentication, false otherwise</param>
        /// <returns>true if the authentication was successful, false otherwise.</returns>
        public void Login(string identifier, bool persistent = false)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            using (var db = _store.OpenSession())
            {
                var account = db.Query<Account>().SingleOrDefault(a => a.Identifier == identifier);
                if (account == null)
                    throw new InvalidIdentifierException(identifier);

                _authenticator.IssueAuthTicket(account.Id, persistent);
            }
        }

        /// <summary>
        /// Logs the current user out.
        /// </summary>
        public void Logout()
        {
            _authenticator.RevokeAuthTicket();
        }

        /// <summary>
        /// Creates a local user account.
        /// </summary>
        /// <param name="userName">The username of the new account.</param>
        /// <param name="password">The password of the new account.</param>
        /// <returns>The ID of the user account.</returns>
        public void CreateAccount(string userName, string password)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            using (var db = _store.OpenSession())
            {
                if (db.Query<Account>().Any(a => a.UserName == userName))
                    throw new DuplicateUserNameException(userName);

                db.Store(new Account
                {
                    UserName = userName,
                    Password = _encoder.Hash(password, _encoder.GenerateHash()),
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Creates an external user account.
        /// </summary>
        /// <param name="identifier">The identifier of the new account.</param>
        /// <returns>The ID of the user account.</returns>
        public void CreateAccount(string identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            using (var db = _store.OpenSession())
            {
                if (db.Query<Account>().Any(a => a.Identifier == identifier))
                    throw new DuplicateIdentifierException(identifier);

                db.Store(new Account { Identifier = identifier });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Changes the password for a user.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns> </returns>
        public void ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (oldPassword == null)
                throw new ArgumentNullException("oldPassword");

            if (newPassword == null)
                throw new ArgumentNullException("newPassword");

            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>().SingleOrDefault(a => a.UserName == userName);
                if (user == null)
                    throw new InvalidUserNameException(userName);

                if (!_encoder.Verify(oldPassword, user.Password))
                    throw new InvalidPasswordException();

                user.Password = _encoder.Hash(newPassword, _encoder.GenerateHash());

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Sets the password for a user.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="newPassword">The new password.</param>
        public void SetPassword(string userName, string newPassword)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (newPassword == null)
                throw new ArgumentNullException("newPassword");

            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>().SingleOrDefault(a => a.UserName == userName);
                if (user == null)
                    throw new InvalidUserNameException(userName);

                user.Password = _encoder.Hash(newPassword, _encoder.GenerateHash());

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Generates a password reset token for a user.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="expiry">The token expiration.</param>
        /// <returns>The password reset token.</returns>
        public string GeneratePasswordResetToken(string userName, TimeSpan expiry)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (expiry == null)
                throw new ArgumentNullException("expiry");

            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>().SingleOrDefault(a => a.UserName == userName);
                if (user == null)
                    throw new InvalidUserNameException(userName);

                var expiration = DateTime.UtcNow.Add(expiry);

                user.PasswordResetToken           = GenerateToken();
                user.PasswordResetTokenExpiration = expiration;

                db.Advanced.GetMetadataFor(user)["Raven-Expiration-Date"] = new RavenJValue(expiration);
                db.SaveChanges();

                return user.PasswordResetToken;
            }
        }

        /// <summary>
        /// Resets the password for the supplied <paramref name="passwordResetToken" />
        /// </summary>
        /// <param name="passwordResetToken">The password reset token to perform the lookup on.</param>
        /// <param name="newPassword">The new password for the user.</param>
        public void ResetPassword(string passwordResetToken, string newPassword)
        {
            if (passwordResetToken == null)
                throw new ArgumentNullException("passwordResetToken");

            if (newPassword == null)
                throw new ArgumentNullException("newPassword");

            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>().SingleOrDefault(a => a.PasswordResetToken == passwordResetToken);
                if (user == null)
                    throw new InvalidUserNameException(passwordResetToken);

                user.Password                     = _encoder.Hash(newPassword, _encoder.GenerateHash());
                user.PasswordResetToken           = null;
                user.PasswordResetTokenExpiration = null;

                db.SaveChanges();
            }
        }

        private static string GenerateToken()
        {
            var guid  = new Guid();
            var token = guid.ToString()
                            .ToLowerInvariant()
                            .Replace("-", "");

            return HttpUtility.UrlEncode(token);
        }
    }
}
