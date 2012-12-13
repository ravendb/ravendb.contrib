using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Contrib.Tests.Helpers;
using Raven.Client.Indexes;
using Xunit;

namespace Raven.Client.Contrib.Tests.ExtensionTests
{
    // ReSharper disable InconsistentNaming

    public class DocumentStoreExtensionFacts
    {
        public static IEnumerable<IEnumerable> CreateFakeData()
        {
            var fakeUsers = new List<FakeUser>
                            {
                                new FakeUser
                                {
                                    Name = "Leah Culver",
                                    Tags = new List<string> {"pounce", "grove.io", "babe"}
                                },
                                new FakeUser
                                {
                                    Name = "Kristen Bell",
                                    Tags =
                                        new List<string> {"veronica mars", "slave leia", "star wars", "babe"}
                                },
                                new FakeUser
                                {
                                    Name = "Ada Lovelace",
                                    Tags = new List<string> {"first computer programmer EVA", "countess"}
                                },
                                new FakeUser
                                {
                                    Name = "Han Solo",
                                    Tags = new List<string> {"star wars", "scoundrel", "stud"}
                                },
                            };

            return new List<IEnumerable>
                   {
                       fakeUsers
                   };
        }

        public class FakeUser
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public IList<string> Tags { get; set; }
        }

        public class FakeUsers_Search : AbstractIndexCreationTask<FakeUser>
        {
            public FakeUsers_Search()
            {
                Map = users => from user in users
                               select new
                                      {
                                          user.Name
                                      };
            }
        }

        public class FakeUsers_TagsSummary : AbstractIndexCreationTask<FakeUser, FakeUsers_TagsSummary.ReduceResult>
        {
            public FakeUsers_TagsSummary()
            {
                Map = users => from user in users
                               from tag in user.Tags
                               select new
                                      {
                                          TagName = tag,
                                          Count = 0
                                      };

                Reduce = results => from result in results
                                    group result by result.Tag
                                    into g
                                    select new
                                           {
                                               TagName = g.Key,
                                               Count = g.Sum(x => x.Count)
                                           };
            }

            public class ReduceResult
            {
                public string Tag { get; set; }
                public int Count { get; set; }
            }
        }

        public class InitializeWithDefaultsFacts : RavenDbTestBase
        {
            [Fact]
            public void GivenNoSeedDataAndNoIndexes_InitializeWithDefaults_WorksWithNoDataAndNoIndexes()
            {
                // Arrange.

                // Act.
                var documentSession = DocumentSession;

                // Assert.
                Assert.NotNull(documentSession);
                Assert.NotNull(documentSession.Advanced.DocumentStore);
                Assert.Equal(0, documentSession.Advanced.DocumentStore.DatabaseCommands.GetStatistics().CountOfDocuments);
                Assert.Equal(0, documentSession.Advanced.DocumentStore.DatabaseCommands.GetStatistics().CountOfIndexes);
            }

            [Fact]
            public void GivenSomeSeedDataAndNoIndexes_InitializeWithDefaults_WorksWithSomeDataAndNoIndexes()
            {
                // Arrange.
                DataToBeSeeded = CreateFakeData();
                // NOTE: Each collection has an identity counter (it is assumed).
                int numberOfDocuments = DataToBeSeeded.Cast<IList>().Sum(collection => collection.Count) +
                                        DataToBeSeeded.Count();

                // Act.
                var documentSession = DocumentSession;

                // Assert.
                Assert.NotNull(documentSession);
                Assert.NotNull(documentSession.Advanced.DocumentStore);
                Assert.Equal(numberOfDocuments,
                             documentSession.Advanced.DocumentStore.DatabaseCommands.GetStatistics().CountOfDocuments);
                Assert.Equal(0, documentSession.Advanced.DocumentStore.DatabaseCommands.GetStatistics().CountOfIndexes);
            }

            [Fact]
            public void GivenSomeSeedDataAndOneIndex_InitializeWithDefaults_WorksWithSomeDataAndOneIndex()
            {
                // Arrange.
                DataToBeSeeded = CreateFakeData();
                // NOTE: Each collection has an identity counter (it is assumed).
                int numberOfDocuments = DataToBeSeeded.Cast<IList>().Sum(collection => collection.Count) +
                                        DataToBeSeeded.Count();
                IndexesToExecute = new List<Type> {typeof (FakeUsers_Search), typeof (FakeUsers_TagsSummary)};

                // Act.
                var documentSession = DocumentSession;

                // Assert.
                Assert.NotNull(documentSession);
                Assert.NotNull(documentSession.Advanced.DocumentStore);
                Assert.Equal(numberOfDocuments,
                             documentSession.Advanced.DocumentStore.DatabaseCommands.GetStatistics().CountOfDocuments);
                Assert.Equal(IndexesToExecute.Count,
                             documentSession.Advanced.DocumentStore.DatabaseCommands.GetStatistics().CountOfIndexes);
            }

            [Fact(Skip = "I need to create a new index that compiles but fails during runtime. I can't remember a simple one, off the top of my head :(")]
            public void GivenSomeSeedDataAndABadIndex_InitializeWithDefaults_StoresTheDataButFailsToCreateTheIndex()
            {
                // Arrange.

                // Act.

                // Assert.
            }
        }
    }

    // ReSharper restore InconsistentNaming
}