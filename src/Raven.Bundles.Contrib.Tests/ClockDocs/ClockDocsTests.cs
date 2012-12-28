using System;
using System.Threading;
using Raven.Abstractions;
using Raven.Client.Bundles.ClockDocs;
using Raven.Database.Config;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Bundles.Contrib.Tests.ClockDocs
{
    public class ClockDocsTests : RavenTestBase
    {
        protected override void ModifyConfiguration(RavenConfiguration configuration)
        {
            Bundles.ClockDocs.ClockDocsHelper.RegisterBundle(configuration);
        }

        [Fact]
        public void ClockDocsBundle_Creates_The_Clock_Doc()
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Advanced.ConfigureClockDoc("EverySecond", TimeSpan.FromSeconds(1));
                    session.Advanced.ConfigureClockDoc("EveryFiveSeconds", TimeSpan.FromSeconds(5));
                    session.SaveChanges();
                }

                // a brief delay to spin up the clocks
                Thread.Sleep(100);

                using (var session = documentStore.OpenSession())
                {
                    var clock1 = session.Load<Clock>(Clock.IdBase + "EverySecond");
                    var clock2 = session.Load<Clock>(Clock.IdBase + "EveryFiveSeconds");

                    Assert.NotNull(clock1);
                    Assert.NotNull(clock2);
                    if (clock1 == null || clock2 == null) return;

                    Assert.InRange(clock1.UtcTime, SystemTime.UtcNow.AddSeconds(-2), SystemTime.UtcNow);
                    Assert.Equal(DateTimeKind.Utc, clock1.UtcTime.Kind);
                    Assert.Equal(TimeZoneInfo.Local.Id, clock1.ServerTimeZone);
                    Assert.Equal(new DateTimeOffset(clock1.UtcTime).ToLocalTime(), clock1.ServerLocalTime);
                    Assert.Equal(TimeZoneInfo.Local.GetUtcOffset(clock1.UtcTime), clock1.ServerLocalTime.Offset);

                    Assert.InRange(clock2.UtcTime, SystemTime.UtcNow.AddSeconds(-6), SystemTime.UtcNow);
                    Assert.Equal(DateTimeKind.Utc, clock2.UtcTime.Kind);
                    Assert.Equal(TimeZoneInfo.Local.Id, clock2.ServerTimeZone);
                    Assert.Equal(new DateTimeOffset(clock2.UtcTime).ToLocalTime(), clock2.ServerLocalTime);
                    Assert.Equal(TimeZoneInfo.Local.GetUtcOffset(clock2.UtcTime), clock2.ServerLocalTime.Offset);
                }

                // When debugging the test, you can watch the clock doc ticking away in raven studio.
                WaitForUserToContinueTheTest(documentStore);
            }
        }
    }
}
