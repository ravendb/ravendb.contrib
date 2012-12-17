using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Client.Contrib.MVC.Auth.Interfaces
{
    public interface ISecurityEncoder
    {
        /// <summary>
        /// Generates a salt suitable for hashing.
        /// </summary>
        /// <returns>The salt.</returns>
        string GenerateHash();

        /// <summary>
        /// Generates a unique token.
        /// </summary>
        /// <returns>The unique token.</returns>
        string GenerateToken();

        /// <summary>
        /// Hashes the identifier with the provided salt.
        /// </summary>
        /// <param name="identifier">The identifier to hash.</param>
        /// <param name="salt">The salt to use in the hashing algorithm.</param>
        /// <returns>The hashed identifier.</returns>
        string Hash(string identifier, string salt);

        /// <summary>
        /// Verifies if the <paramref name="identifier"/> matches the hash.
        /// </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <param name="hash">The hash to check against.</param>
        /// <returns>true if the identifiers match, false otherwise.</returns>
        bool Verify(string identifier, string hash);
    }
}
