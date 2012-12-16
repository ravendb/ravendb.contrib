using System;

namespace Raven.Client.Contrib.Extensions
{
    public static class DocumentIdExtensions
    {
        /// <summary>
        /// Gets the document key prefix for the given entity type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="documentStore">The Raven document store.</param>
        /// <returns>The document key prefix string.</returns>
        public static string GetDocumentKeyPrefix<T>(this IDocumentStore documentStore)
        {
            var conventions = documentStore.Conventions;
            var name = conventions.GetTypeTagName(typeof(T));
            return conventions.TransformTypeTagNameToDocumentKeyPrefix(name);
        }

        /// <summary>
        /// Checks if the document id specified is valid for the given entity type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="documentStore">The Raven document store.</param>
        /// <param name="id">The string document id.</param>
        /// <returns>True if the id is valid, false otherwise.</returns>
        public static bool DocumentIdMatches<T>(this IDocumentStore documentStore, string id)
        {
            var conventions = documentStore.Conventions;
            var prefix = documentStore.GetDocumentKeyPrefix<T>();
            return id.StartsWith(prefix + conventions.IdentityPartsSeparator);
        }

        /// <summary>
        /// Gets a string id for an entity, given an integer id.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="documentStore">The Raven document store.</param>
        /// <param name="id">The integer id.</param>
        /// <returns>The string id.</returns>
        public static string GetStringIdFor<T>(this IDocumentStore documentStore, int id)
        {
            return documentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(T), false);
        }

        /// <summary>
        /// Gets a string id for an entity, given a guid id.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="documentStore">The Raven document store.</param>
        /// <param name="id">The guid id.</param>
        /// <returns>The string id.</returns>
        public static string GetStringIdFor<T>(this IDocumentStore documentStore, Guid id)
        {
            return documentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(T), false);
        }

        /// <summary>
        /// Safely gets the integer portion of a string identifier for a particular entity type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="documentStore">The Raven document store.</param>
        /// <param name="id">The string id, such as foos/1.</param>
        /// <returns>The integer id.</returns>
        /// <exception cref="ArgumentException">Thrown if the inputs are not valid.</exception>
        public static int GetIntegerIdFor<T>(this IDocumentStore documentStore, string id)
        {
            var parts = documentStore.GetDocumentIdParts(id);
            if (parts.Length < 2)
                throw new ArgumentException(
                    String.Format(@"The string id is not of the correct form: entity{0}integer.",
                                  documentStore.Conventions.IdentityPartsSeparator),
                    "id");

            var prefix = documentStore.GetDocumentKeyPrefix<T>();
            if (!prefix.Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    String.Format("The string id has a prefix of {0} which is not valid for the {1} entity.", prefix, typeof(T).Name),
                    "id");

            int intId;
            if (!Int32.TryParse(parts[1], out intId))
                throw new ArgumentException(@"The second part of the id is not a valid integer.", "id");

            return intId;
        }

        /// <summary>
        /// Safely gets the guid portion of a string identifier for a particular entity type, when guid ids are used.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="documentStore">The Raven document store.</param>
        /// <param name="id">The string id, such as foos/d2dbfef3-a42e-4ab8-aa17-19a87da0358b.</param>
        /// <returns>The guid id.</returns>
        /// <exception cref="ArgumentException">Thrown if the inputs are not valid.</exception>
        public static Guid GetGuidIdFor<T>(this IDocumentStore documentStore, string id)
        {
            var parts = documentStore.GetDocumentIdParts(id);
            if (parts.Length < 2)
                throw new ArgumentException(
                    String.Format(@"The string id is not of the correct form: entity{0}guid.",
                                  documentStore.Conventions.IdentityPartsSeparator),
                    "id");

            var prefix = documentStore.GetDocumentKeyPrefix<T>();
            if (!prefix.Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    String.Format("The string id has a prefix of {0} which is not valid for the {1} entity.", prefix, typeof(T).Name),
                    "id");

            Guid guidId;
            if (!Guid.TryParse(parts[1], out guidId))
                throw new ArgumentException(@"The second part of the id is not a valid guid.", "id");

            return guidId;
        }

        /// <summary>
        /// Splits a composite document id into its parts for easy consumption.
        /// </summary>
        /// <param name="documentStore">The Raven document store.</param>
        /// <param name="id">The string id, such as foos/1/bar/2/whatever.</param>
        /// <returns>An array of strings with each part as a separate element.</returns>
        public static string[] GetDocumentIdParts(this IDocumentStore documentStore, string id)
        {
            var separator = documentStore.Conventions.IdentityPartsSeparator;
            return id.Split(new[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Generates a document id using the registered conventions, such as the Raven HiLo generator.
        /// Returns the id without applying it to any particular entity, thus allowing you to pre-generate the id.
        /// Useful for detached operations, such as in CQRS. For example, the id could be generated ahead of time,
        /// returned to the caller, then used in a command message.  The entity would then be created in a command
        /// handler, using the id from the message.  There are other scenarios that this can also be useful.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="session">The current session.</param>
        /// <returns>The generated string id.</returns>
        public static string GenerateIdFor<T>(this IAdvancedDocumentSessionOperations session)
        {
            // An entity instance is required to generate a key, but we only have a type.
            // We might not have a public constructor, so we must use reflection.
            var entity = Activator.CreateInstance(typeof(T), true);

            // Generate an ID using the commands and conventions from the current session
            var conventions = session.DocumentStore.Conventions;
            var databaseCommands = session.GetDatabaseCommands();
            return conventions.GenerateDocumentKey(databaseCommands, entity);
        }
    }
}
