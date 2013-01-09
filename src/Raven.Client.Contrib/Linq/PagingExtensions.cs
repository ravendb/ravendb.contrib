using System;
using System.Collections.Generic;
using System.Linq;
using PagedList;

namespace Raven.Client.Linq
{
    public static class PagingExtensions
    {
        private const string ExceedsLimitMessage = @"This operation can't be completed because it would require exceeding the session request limit.
You should probably narrow the scope of the query, but you can also increase the request limit with session.Advanced.MaxNumberOfRequestsPerSession.
Alternatively, you can pass expandSessionRequestLimitAsNeededWhichMightBeDangerous = true and all results will be returned, but use this cautiously,
as it could needlessly consume a lot of resources for a very large result set.";

        /// <summary>
        /// Executes the query and iterates over every page, returning all results.
        /// </summary>
        public static IEnumerable<T> GetAllResultsWithPaging<T>(this IRavenQueryable<T> queryable, int pageSize = 128,
                                                                bool expandSessionRequestLimitAsNeededWhichMightBeDangerous = false)
        {
            var skipped = 0;
            var total = 0;
            var page = 0;

            var session = ((RavenQueryInspector<T>) queryable).Session;

            queryable = queryable.SyncCutoff();

            while (true)
            {
                RavenQueryStatistics stats;
                var results = queryable.Statistics(out stats)
                                       .Skip(page * pageSize + skipped)
                                       .Take(pageSize);

                foreach (var item in results)
                {
                    yield return item;
                    total++;
                }

                skipped += stats.SkippedResults;

                if (total + skipped >= stats.TotalResults)
                    break;

                if (expandSessionRequestLimitAsNeededWhichMightBeDangerous)
                    session.MaxNumberOfRequestsPerSession++;
                else if ((stats.TotalResults / pageSize) + session.NumberOfRequests > session.MaxNumberOfRequestsPerSession)
                    throw new InvalidOperationException(ExceedsLimitMessage);

                page++;
            }
        }

        /// <summary>
        /// Executes the query and iterates over every page, returning all document keys that matched the query.
        /// </summary>
        public static IEnumerable<string> GetAllDocumentKeysWithPaging<T>(this IRavenQueryable<T> queryable, int pageSize = 128,
                                                                          bool expandSessionRequestLimitAsNeededWhichMightBeDangerous = false)
        {
            var skipped = 0;
            var total = 0;
            var page = 0;

            var session = ((RavenQueryInspector<T>) queryable).Session;

            queryable = queryable.SyncCutoff().Customize(x => x.MetadataOnly());

            while (true)
            {
                RavenQueryStatistics stats;
                var results = queryable.Statistics(out stats)
                                       .Skip(page * pageSize + skipped)
                                       .Take(pageSize);

                foreach (var item in results)
                {
                    var metadata = session.GetMetadataFor(item);
                    yield return metadata.Value<string>("@id");
                    total++;
                }

                skipped += stats.SkippedResults;

                if (total + skipped >= stats.TotalResults)
                    break;

                if (expandSessionRequestLimitAsNeededWhichMightBeDangerous)
                    session.MaxNumberOfRequestsPerSession++;
                else if ((stats.TotalResults / pageSize) + session.NumberOfRequests > session.MaxNumberOfRequestsPerSession)
                    throw new InvalidOperationException(ExceedsLimitMessage);

                page++;
            }
        }

        /// <summary>
        /// Executes the query and iterates over every page.  As each page is retrieved, the action is executed on the items for that page.
        /// </summary>
        public static void ForEachWithPaging<T>(this IRavenQueryable<T> queryable, Action<T> action, int pageSize = 128,
                                                bool expandSessionRequestLimitAsNeededWhichMightBeDangerous = false)
        {
            var skipped = 0;
            var total = 0;
            var page = 0;

            var session = ((RavenQueryInspector<T>) queryable).Session;

            queryable = queryable.SyncCutoff();

            while (true)
            {
                RavenQueryStatistics stats;
                var results = queryable.Statistics(out stats)
                                       .Skip(page * pageSize + skipped)
                                       .Take(pageSize)
                                       .ToList();

                foreach (var item in results)
                {
                    action(item);
                    total++;
                }

                skipped += stats.SkippedResults;

                if (total + skipped >= stats.TotalResults)
                    break;

                if (expandSessionRequestLimitAsNeededWhichMightBeDangerous)
                    session.MaxNumberOfRequestsPerSession++;
                else if ((stats.TotalResults / pageSize) + session.NumberOfRequests > session.MaxNumberOfRequestsPerSession)
                    throw new InvalidOperationException(ExceedsLimitMessage);

                page++;
            }
        }

        /// <summary>
        /// Turn a document query (Lucene) into a properly paged list.
        /// </summary>
        /// <typeparam name="T">The type of the collection</typeparam>
        /// <param name="query">the document query</param>
        /// <param name="page">the page to start at. Starts at 1.</param>
        /// <param name="pageSize">the number of items in a page.</param>
        /// <returns></returns>
        public static IPagedList<T> ToPagedList<T>(this IDocumentQuery<T> query, int page = 1, int pageSize = 128)
        {
            return new Collections.DocumentQueryPagedList<T>(query, page, pageSize);
        }
    }
}
