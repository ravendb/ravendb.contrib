using Raven.Client.Connection;
using Raven.Client.Document;

namespace Raven.Client
{
    public static class AdvancedSessionExtensions
    {
        /// <summary>
        /// Provides access to DatabaseCommands for the same database that the session was opened for.
        /// </summary>
        /// <param name="session">The Raven advanced session.</param>
        /// <returns>A DatabaseCommands instance.</returns>
        public static IDatabaseCommands GetDatabaseCommands(this IAdvancedDocumentSessionOperations session)
        {
            // Note - We must cast to a DocumentSession to get the correct DatabaseCommands instance.
            //        session.DocumentStore.DatabaseCommands will not necessarily point at the same database
            //        that the session was opened for.  Hence the usefulness of this extension method.

            return ((DocumentSession) session).DatabaseCommands;
        }

        /// <summary>
        /// Gets the database name that the session was opened for.
        /// </summary>
        /// <param name="session">The Raven advanced session.</param>
        /// <returns>The database name.</returns>
        public static string GetDatabaseName(this IAdvancedDocumentSessionOperations session)
        {
            return ((DocumentSession)session).DatabaseName;
        }
    }
}
