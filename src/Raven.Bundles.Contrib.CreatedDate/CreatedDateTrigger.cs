using System;
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
        public override void AfterPut(string key, RavenJObject document, RavenJObject metadata, Guid etag, TransactionInformation transactionInformation)
        {
            // leave raven system docs alone
            if (key.StartsWith("Raven/"))
                return;

            // when there's already a Created date written, this is not the original insert
            if (metadata.ContainsKey("Created"))
                return;

            // get the timestamp set for the last-modified date
            var timestamp = metadata.Value<DateTime>(Constants.LastModified);

            // copy the metadata and add the timestamp
            var newMetadata = (RavenJObject)metadata.CreateSnapshot();
            newMetadata.Add("Created", timestamp);

            // update the metadata in the document
            using (Database.DisableAllTriggersForCurrentThread())
                Database.PutDocumentMetadata(key, newMetadata);
        }
    }
}
