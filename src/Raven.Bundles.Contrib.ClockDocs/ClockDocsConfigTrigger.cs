using System;
using System.Linq;
using Raven.Abstractions.Extensions;
using Raven.Database.Plugins;
using Raven.Json.Linq;

namespace Raven.Bundles.ClockDocs
{
    public class ClockDocsConfigTrigger : AbstractPutTrigger
    {
        public override void AfterCommit(string key, RavenJObject document, RavenJObject metadata, Guid etag)
        {
            // When a new clocks config doc is written, reconfigure the active timers

            if (key != ClocksConfig.Id)
                return;

            var config = document.JsonDeserialization<ClocksConfig>();
            if (config == null)
                return;

            var maintainer = Database.StartupTasks.OfType<ClockDocsMaintainer>().SingleOrDefault();
            if (maintainer == null)
                return;

            maintainer.Configure(config);
        }
    }
}
