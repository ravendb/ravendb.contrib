using Raven.Client.Connection;

namespace Raven.Client
{
    public static class TenantDatabaseExtensions
    {
        /// <summary>
        /// Checks if the database with the given name exists.
        /// </summary>
        /// <param name="documentStore">The Raven document store.</param>
        /// <param name="databaseName">The name of the database to check for.</param>
        /// <returns>True if the database exists, false otherwise.</returns>
        public static bool DatabaseExists(this IDocumentStore documentStore, string databaseName)
        {
            var key = "Raven/Databases/" + databaseName;
            return documentStore.DatabaseCommands.ForSystemDatabase().DocumentExists(key);
        }
    }
}
