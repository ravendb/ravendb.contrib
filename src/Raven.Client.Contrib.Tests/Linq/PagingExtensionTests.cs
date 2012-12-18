using System.Linq;
using Raven.Client.Contrib.Tests.TestEntities;
using Raven.Client.Linq;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.Contrib.Tests.Linq
{
    public class PagingExtensionTests : RavenTestBase
    {
        [Fact]
        public void GetAllResultsWithPaging_Can_Expand_To_Exceed_Request_Limit()
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    for (int i = 0; i < 1000; i++)
                        session.Store(Ball.Random);

                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var originalLimit = session.Advanced.MaxNumberOfRequestsPerSession;

                    var allBalls = session.Query<Ball>()
                                          .Customize(x => x.WaitForNonStaleResults())
                                          .GetAllResultsWithPaging(pageSize: 25, expandSessionRequestLimitAsNeededWhichMightBeDangerous: true)
                                          .ToList();

                    Assert.Equal(1000, allBalls.Count);

                    // 1000 items, 25 per page = 40 pages, which exceeds the default 30 request max.

                    // The above query should have only taken one away from the limit, regardless of how many pages there were.
                    // The request limit should have been expanded to account for this.
                    var newLimit = session.Advanced.MaxNumberOfRequestsPerSession;
                    var numRequests = session.Advanced.NumberOfRequests;
                    Assert.Equal(originalLimit + numRequests - 1, newLimit);
                }
            }
        }
    }
}
