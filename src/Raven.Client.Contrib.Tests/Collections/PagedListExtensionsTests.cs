using System.Diagnostics;
using PagedList;
using Raven.Client.Embedded;
using Raven.Client.Linq;
using Xunit;

namespace Raven.Client.Contrib.Tests.Collections
{
    public class PagedListExtensionsTests
    {
        EmbeddableDocumentStore Store { get; set; }

        public PagedListExtensionsTests()
        {
            Store = new EmbeddableDocumentStore
            {
                Configuration =
                {
                    RunInUnreliableYetFastModeThatIsNotSuitableForProduction = true,
                    RunInMemory = true,
                }
            };
            Store.Initialize();
        }

        [Fact]
        public void Can_Get_Paged_List_From_DocumentQuery()
        {
            using (var session = Store.OpenSession()) {
                for (int i = 1; i <= 1000; i++)
                    session.Store(new Test { Id = i.ToString() });


                session.SaveChanges();
            }

            using (var session = Store.OpenSession())
            {
                var result = session
                        .Advanced
                        .LuceneQuery<Test>()
                        .OrderBy(x => x.Id)
                        .WaitForNonStaleResultsAsOfLastWrite()
                        .ToPagedList(1, 10);

                Assert.NotNull(result);
                Assert.IsAssignableFrom<IPagedList<Test>>(result);
                Assert.True(result.Count == 10);
                Assert.True(result.TotalItemCount == 1000);
                
                foreach (var test in result)
                    Debug.WriteLine(test.Id);

                Assert.True(result[0].Id == "1");
                // the result is 106, because the ordering is by not numeric
                Assert.True(result[9].Id == "106");
            }
        }

        public class Test
        {
            public string Id { get; set; }
        }

    }
}
