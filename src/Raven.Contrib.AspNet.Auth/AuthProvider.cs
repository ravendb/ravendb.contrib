using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Web;
using Raven.Client;
using Raven.Contrib.AspNet.Auth.Interfaces;
using Raven.Json.Linq;

namespace Raven.Contrib.AspNet.Auth
{
    public partial class AuthProvider : IAuthProvider
    {
        private readonly IDocumentStore _store;
        private readonly ISecurityEncoder _encoder;
        private readonly IAuthenticator _authenticator;

        private class UserId
        {
            public string AccountId
            {
                get;
                set;
            }

            public string UserName
            {
                get;
                set;
            }
        }

        /// <summary>
        /// The currently logged-in account.
        /// </summary>
        public string Current
        {
            get
            {
                return _authenticator.IsAuthenticated ? GetUserId().UserName : null;
            }
        }

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
                var account = db.Query<Account>()
                                .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                                .SingleOrDefault(a => a.UserName == userName);

                if (account == null)
                    throw new InvalidCredentialException();

                if (!_encoder.Verify(password, account.Password))
                    throw new InvalidCredentialException();

                SetUserId(new UserId { AccountId = account.Id, UserName = account.UserName }, persistent);
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
                var account = db.Query<Account>()
                                .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                                .SingleOrDefault(a => a.Identifiers.Any(i => i.Value == identifier));

                if (account == null)
                    throw new InvalidIdentifierException(identifier);

                SetUserId(new UserId { AccountId = account.Id, UserName = account.UserName }, persistent);
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
        /// Determines whether the user is logged in.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return _authenticator.IsAuthenticated;
            }
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

            if (password == null)
                throw new ArgumentNullException("password");

            using (var db = _store.OpenSession())
            {
                var query = db.Query<Account>()
                              .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                              .Where(a => a.UserName == userName);

                if (query.Any())
                    throw new DuplicateUserNameException(userName);

                db.Store(new Account
                {
                    UserName = userName,
                    Password = _encoder.Hash(password),
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Creates an external user account.
        /// </summary>
        /// <param name="userName">The username of the new account.</param>
        /// <param name="identifier">The identifier of the new account.</param>
        /// <param name="providerName">The name of the external auth provider.</param>
        /// <returns>The ID of the user account.</returns>
        public void CreateExternalAccount(string userName, string identifier, string providerName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (identifier == null)
                throw new ArgumentNullException("identifier");

            if (providerName == null)
                throw new ArgumentNullException("providerName");

            providerName = providerName.ToLowerInvariant();

            using (var db = _store.OpenSession())
            {
                var query = from a in db.Query<Account>().Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                            let i = a.Identifiers.SingleOrDefault(i => i.Provider == providerName && i.Value == identifier)
                            where i != null
                            select i;

                if (query.Any())
                    throw new DuplicateIdentifierException(identifier);

                db.Store(new Account
                {
                    UserName    = userName,
                    Identifiers =
                    {
                        new Account.Identifier
                        {
                            Provider = providerName,
                            Value    = identifier,
                        }
                    }
                });
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
                var user = db.Query<Account>()
                             .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                             .SingleOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new InvalidUserNameException(userName);

                if (!_encoder.Verify(oldPassword, user.Password))
                    throw new InvalidPasswordException();

                user.Password = _encoder.Hash(newPassword);

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
                var user = db.Query<Account>()
                             .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                             .SingleOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new InvalidUserNameException(userName);

                user.Password = _encoder.Hash(newPassword);

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Checks whether an account is a local account.
        /// </summary>
        /// <param name="userName">The username of the account to check.</param>
        /// <returns>true if the current account is a local account, false otherwise</returns>
        public bool IsLocalAccount(string userName)
        {
            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>()
                             .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                             .SingleOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new ArgumentOutOfRangeException("userName");

                return !String.IsNullOrEmpty(user.Password);
            }
        }

        /// <summary>
        /// Adds an identifier for an external auth provider to an account. 
        /// </summary>
        /// <param name="userName">The username of the account.</param>
        /// <param name="identifier">The identifier to add.</param>
        /// <param name="providerName">The name of the external auth provider.</param>
        public void AddIdentifier(string userName, string identifier, string providerName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (identifier == null)
                throw new ArgumentNullException("identifier");

            if (providerName == null)
                throw new ArgumentNullException("providerName");

            providerName = providerName.ToLowerInvariant();

            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>()
                             .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                             .SingleOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new InvalidUserNameException(userName);

                user.Identifiers.Add(new Account.Identifier { Provider = providerName, Value = identifier });

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Removes an identifier for an external authenticator from an account. 
        /// </summary>
        /// <param name="userName">The username or identifier of the account</param>
        /// <param name="providerName">The name of the external auth provider.</param>
        public void RemoveIdentifier(string userName, string providerName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (providerName == null)
                throw new ArgumentNullException("providerName");

            providerName = providerName.ToLowerInvariant();

            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>()
                             .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                             .SingleOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new InvalidUserNameException(userName);

                user.Identifiers.RemoveAll(i => i.Provider == providerName);

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Retrieves all the identifiers for an account.
        /// </summary>
        /// <param name="userName">The username of the account.</param>
        /// <returns>A dictionary with the provider name as the key and the identifier as the value.</returns>
        public IDictionary<string, string> GetIdentifiers(string userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>()
                             .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                             .SingleOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new InvalidUserNameException(userName);

                return user.Identifiers.ToDictionary(i => i.Provider, i => i.Value);
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
                var user = db.Query<Account>()
                             .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                             .SingleOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new InvalidUserNameException(userName);

                var token      = GenerateToken();
                var expiration = DateTime.UtcNow.Add(expiry);

                user.PasswordResetToken           = _encoder.Hash(token);
                user.PasswordResetTokenExpiration = expiration;

                db.Advanced.GetMetadataFor(user)["Raven-Expiration-Date"] = new RavenJValue(expiration);
                db.SaveChanges();

                return token;
            }
        }

        /// <summary>
        /// Resets the password for the supplied <paramref name="passwordResetToken" />
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="passwordResetToken">The password reset token.</param>
        /// <param name="newPassword">The new password for the user.</param>
        public void ResetPassword(string userName, string passwordResetToken, string newPassword)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (passwordResetToken == null)
                throw new ArgumentNullException("passwordResetToken");

            if (newPassword == null)
                throw new ArgumentNullException("newPassword");

            using (var db = _store.OpenSession())
            {
                var user = db.Query<Account>()
                             .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                             .SingleOrDefault(a => a.UserName == userName);

                if (user == null)
                    throw new InvalidUserNameException(userName);

                if (user.PasswordResetTokenExpiration >= DateTime.UtcNow)
                    throw new InvalidPasswordResetTokenException();

                if (!_encoder.Verify(passwordResetToken, user.PasswordResetToken))
                    throw new InvalidPasswordResetTokenException();

                user.Password                     = _encoder.Hash(newPassword);
                user.PasswordResetToken           = null;
                user.PasswordResetTokenExpiration = null;

                db.SaveChanges();
            }
        }

        private UserId GetUserId()
        {
            string[] data = _authenticator.Current.Split('|');

            return new UserId
            {
                AccountId = data[0],
                UserName  = data[1],
            };
        }

        private void SetUserId(UserId id, bool persistent)
        {
            var data = String.Join("|", id.AccountId, id.UserName);

            _authenticator.IssueAuthTicket(data, persistent);
        }

        private string GenerateToken()
        {
            var guid  = new Guid();
            var token = guid.ToString()
                            .ToLowerInvariant()
                            .Replace("-", "");

            return HttpUtility.UrlEncode(token);
        }
    }
}
