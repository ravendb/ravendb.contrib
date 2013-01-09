using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PagedList;

namespace Raven.Client.Collections
{
    public class DocumentQueryPagedList<T> : PagedListMetaData, IPagedList<T>
    {
        private List<T> Subset { get; set; }

        /// <summary>
        /// Turns a DocumentQuery into a property paged list
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageNumber">page number starts at 1</param>
        /// <param name="pageSize">page size is still limited by RavenDB limit</param>
        public DocumentQueryPagedList(IDocumentQuery<T> query, int pageNumber, int pageSize)
        {
            RavenQueryStatistics stats;
            Subset = query.Statistics(out stats)
                          .Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .ToList();

            // set source to blank list if superset is null to prevent exceptions
            TotalItemCount = stats.TotalResults;
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageCount = TotalItemCount > 0
                            ? (int)Math.Ceiling(TotalItemCount / (double)PageSize)
                            : 0;
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            var numberOfLastItemOnPage = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = numberOfLastItemOnPage > TotalItemCount
                                 ? TotalItemCount
                                 : numberOfLastItemOnPage;
            Count = Subset.Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Subset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IPagedList GetMetaData()
        {
            return new PagedListMetaData(this);
        }

        public T this[int index]
        {
            get { return Subset[index]; }
        }

        public int Count { get; private set; }
    }
}
