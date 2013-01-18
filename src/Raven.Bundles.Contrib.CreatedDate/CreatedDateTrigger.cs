using Raven.Abstractions;
using Raven.Abstractions.Data;
using Raven.Database.Plugins;
using Raven.Json.Linq;

namespace Raven.Bundles.CreatedDate
{
    /// <summary>
    /// A server-side trigger that adds a Created metadata value to every document being stored.
    /// Does not need to be "activated" like other bundles.  Just drop it into the Plugins directory and it will be active on all new documents.
    /// </summary>
    public class CreatedDateTrigger : AbstractPutTrigger
    {
        public override void OnPut(string key, RavenJObject document, RavenJObject metadata, TransactionInformation transactionInformation)
        {
            // leave raven system docs alone
            if (key.StartsWith("Raven/"))
                return;

            // when there's already a Created date written, this is not the original insert
            if (metadata.ContainsKey("Created"))
                return;

            // add the timestamp to the metadata
            metadata.Add("Created", new RavenJValue(SystemTime.UtcNow));
        }
    }
}
