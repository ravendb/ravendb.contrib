namespace Raven.Client.Linq
{
    public static class QueryExtensions
    {
        /// <summary>
        /// Modifies the query such that repeat calls use the same cutoff time as the first call.
        /// Useful when paginating over results obtained with WaitForNonStaleResultsAsOfNow so that all pages use the same now.
        /// </summary>
        public static IRavenQueryable<T> SyncCutoff<T>(this IRavenQueryable<T> queryable)
        {
            return queryable.Customize(x => x.TransformResults((query, results) =>
            {
                if (query.Cutoff.HasValue)
                    queryable.Customize(z => z.WaitForNonStaleResultsAsOf(query.Cutoff.Value));

                return results;
            }));
        }

        /// <summary>
        /// When running in server mode, customizes the query to return only metadata - conserving http bandwidth.
        /// Does nothing when running in embedded mode.
        /// </summary>
        public static void MetadataOnly(this IDocumentQueryCustomization customization)
        {
            // This is a hack, but there's no other clear way to modify the querystring of the url.
            // It works because it ends of appending "&include=&metadata-only=true",
            // and the first term gets ignored by the server because it has no value.

            customization.Include("&metadata-only=true");
        }

        /// <summary>
        /// Order the results by the specified fields
        /// The fields are the names of the fields to sort, defaulting to sorting by ascending.
        /// You can prefix a field name with '-' to indicate sorting by descending or '+' to sort by ascending
        /// </summary>
        public static IRavenQueryable<T> OrderBy<T>(this IRavenQueryable<T> source, params string[] fields)
        {
            return source.Customize(x => ((IDocumentQuery<T>) x).OrderBy(fields));
        }

        /// <summary>
        /// Order the results by the specified fields
        /// The fields are the names of the fields to sort, defaulting to sorting by descending.
        /// You can prefix a field name with '-' to indicate sorting by descending or '+' to sort by ascending
        /// </summary>
        public static IRavenQueryable<T> OrderByDescending<T>(this IRavenQueryable<T> source, params string[] fields)
        {
            return source.Customize(x => ((IDocumentQuery<T>) x).OrderByDescending(fields));
        }
    }
}
