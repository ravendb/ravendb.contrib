using System.ComponentModel.Composition.Hosting;
using Raven.Database.Config;

namespace Raven.Bundles.ClockDocs
{
    public static class ClockDocsHelper
    {
        /// <summary>
        /// Registers the ClockDocsBundle in the configuration of a database instance.
        /// </summary>
        /// <param name="configuration"></param>
        public static void RegisterBundle(InMemoryRavenConfiguration configuration)
        {
            configuration.Catalog.Catalogs.Add(new AssemblyCatalog(typeof(ClockDocsHelper).Assembly));
        }
    }
}
