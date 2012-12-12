using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Linq;

namespace Raven.Client.Contrib.Extensions
{
    public static class PagingExtensions
    {
        /// <summary>
        /// Executes the query and iterates over every page, returning all results.
        /// </summary>
        public static IEnumerable<T> GetAllResultsWithPaging<T>(this IRavenQueryable<T> queryable, int pageSize = 1024)
        {
            var skipped = 0;
            var total = 0;
            var page = 0;

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

                page++;
            }
        }

        /// <summary>
        /// Executes the query and iterates over every page, returning all document keys that matched the query.
        /// </summary>
        public static IEnumerable<string> GetAllDocumentKeysWithPaging<T>(this IRavenQueryable<T> queryable, int pageSize = 1024)
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

                page++;
            }
        }

        /// <summary>
        /// Executes the query and iterates over every page.  As each page is retrieved, the action is executed on the items for that page.
        /// </summary>
        public static void ForEachWithPaging<T>(this IRavenQueryable<T> queryable, Action<T> action, int pageSize = 1024)
        {
            var skipped = 0;
            var total = 0;
            var page = 0;

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

                page++;
            }
        }

        
    }
}
