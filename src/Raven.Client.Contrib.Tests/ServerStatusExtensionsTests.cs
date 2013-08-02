using System.Diagnostics;
using Raven.Abstractions.Data;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.Contrib.Tests
{
    public class ServerStatusExtensionsTests : RavenTestBase
    {
        [Fact]
        public void Can_Get_Server_Build_Number()
        {
            using (var store = NewDocumentStore())
            {
                BuildNumber buildNumber;
                var ok = store.TryGetServerVersion(out buildNumber);
                Assert.True(ok);
                Debug.WriteLine(buildNumber.ProductVersion);
                Debug.WriteLine(buildNumber.BuildVersion);
            }
        }

        [Fact]
        public void Can_Get_Server_Online_Status()
        {
            using (var store = NewDocumentStore())
            {
                Assert.True(store.IsServerOnline());
            }
        }
    }
}
