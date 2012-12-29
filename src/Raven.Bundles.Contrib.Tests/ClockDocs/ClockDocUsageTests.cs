using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Bundles.ClockDocs;
using Raven.Client.Indexes;
using Raven.Database.Config;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Bundles.Contrib.Tests.ClockDocs
{
    public class ClockDocsUsageTests : RavenTestBase
    {
        protected override void ModifyConfiguration(RavenConfiguration configuration)
        {
            Bundles.ClockDocs.ClockDocsHelper.RegisterBundle(configuration);
        }

        [Fact]
        public void ClockDocsBundle_Usage_Tests()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new Orders_TotalByCustomerFor30Days());

                using (var session = documentStore.OpenSession())
                {
                    session.Advanced.ConfigureClockDoc("EveryDay", TimeSpan.FromDays(1));
                    session.SaveChanges();
                }

                // generate random orders over the past few months
                const int numOrdersToGenerate = 100;
                GenerateRandomOrders(documentStore, numOrdersToGenerate, DateTime.Today.AddMonths(-3), DateTime.Today);

                WaitForUserToContinueTheTest(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    // example of how to query the index to get the top customers over the last 30 days.
                    var topCustomersDuringLast30Days = session.Query<OrderTotal, Orders_TotalByCustomerFor30Days>()
                                                              .Customize(x => x.WaitForNonStaleResults())
                                                              .OrderByDescending(x => x.Total)
                                                              .Take(10)
                                                              .ToList();
                }
            }
        }

        public static void GenerateRandomOrders(IDocumentStore documentStore, int numOrders, DateTime starting, DateTime ending)
        {
            starting = starting.ToUniversalTime();
            ending = ending.ToUniversalTime();

            using (var session = documentStore.OpenSession())
            {
                var names = new[]
                            {
                                "Alice", "Bob", "Charlie", "David", "Ethel", "Frank", "George",
                                "Henry", "Iris", "Josh", "Kelly", "Larry", "Mike", "Natalie",
                                "Oscar", "Paul", "Quincy", "Rose", "Sam", "Tina", "Uma", "Victor",
                                "William", "Xavier", "Yvonne", "Zach"
                            };

                var random = new Random();

                var customers = names.Select(name => new Customer { Name = name }).ToList();

                foreach (var customer in customers)
                    session.Store(customer);

                for (int i = 0; i < numOrders; i++)
                {
                    var customer = customers[random.Next(customers.Count)];
                    var amount = random.Next(100000) / 100m;

                    var range = ending - starting;
                    var placed = starting.AddSeconds(random.Next((int) range.TotalSeconds));

                    session.Store(new Order { CustomerId = customer.Id, Amount = amount, Placed = placed });
                }

                session.SaveChanges();
            }
        }

        public class Customer
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Order
        {
            public string Id { get; set; }
            public string CustomerId { get; set; }
            public decimal Amount { get; set; }
            public DateTime Placed { get; set; }
        }

        public class OrderTotal
        {
            public string CustomerId { get; set; }
            public decimal Total { get; set; }
        }

        public class Orders_TotalByCustomerFor30Days : AbstractIndexCreationTask<Order, OrderTotal>
        {
            public Orders_TotalByCustomerFor30Days()
            {
                Map = orders => from order in orders
                                let now = LoadDocument<Clock>("Raven/Clocks/EveryDay").UtcTime
                                let age = now - order.Placed
                                where age.TotalDays <= 30
                                select new
                                       {
                                           order.CustomerId,
                                           Total = order.Amount,
                                       };

                Reduce = results => from result in results
                                    group result by result.CustomerId
                                    into g
                                    select new
                                           {
                                               CustomerId = g.Key,
                                               Total = g.Sum(x => x.Total),
                                           };

                Sort(x => x.Total, SortOptions.Double);
            }
        }
    }
}
