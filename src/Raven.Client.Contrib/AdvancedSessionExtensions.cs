using System;
using System.Linq;
using Raven.Client.Connection;
using Raven.Client.Document;
using Raven.Json.Linq;

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

        /// <summary>
        /// Adds one or more document keys to an entity's cascade delete metadata.
        /// Requires the cascade delete bundle on the server.
        /// </summary>
        /// <param name="session">The Raven advanced session.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="documentKeys">One or more keys to foreign documents.</param>
        public static void AddCascadeDeleteReference(this IAdvancedDocumentSessionOperations session, object entity, params string[] documentKeys)
        {
            var metadata = session.GetMetadataFor(entity);
            if (metadata == null)
                throw new InvalidOperationException("The entity must be tracked in the session before calling this method.");

            if (documentKeys.Length == 0)
                throw new ArgumentException("At least one document key must be specified.");

            const string metadataKey = "Raven-Cascade-Delete-Documents";

            RavenJToken token;
            if (!metadata.TryGetValue(metadataKey, out token))
                token = new RavenJArray();

            var list = (RavenJArray)token;
            foreach (var documentKey in documentKeys.Where(key => !list.Contains(key)))
                list.Add(documentKey);

            metadata[metadataKey] = list;
        }
    }
}
