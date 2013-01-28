using System.ComponentModel.Composition;
using Raven.Abstractions.Indexing;
using Raven.Database;
using Raven.Database.Plugins;

namespace Raven.Bundles.IndexedAttachments
{
    [ExportMetadata("Bundle", "IndexedAttachments")]
    public class Startup : IStartupTask
    {
        public void Execute(DocumentDatabase database)
        {
            if (!database.IsBundleActive("IndexedAttachments"))
                return;

            var index = new IndexDefinition
                        {
                            Map = @"from doc in docs
where doc[""@metadata""][""Raven-Attachment-Key""] != null
select new
{
    AttachmentKey = doc[""@metadata""][""Raven-Attachment-Key""],
    Filename = doc[""@metadata""][""Raven-Attachment-Filename""],
    Text = doc.Text
}",
                            TransformResults = @"from result in results
select new
{
    AttachmentKey = result[""@metadata""][""Raven-Attachment-Key""],
    Filename = result[""@metadata""][""Raven-Attachment-Filename""]
}"
                        };

            // NOTE: The transform above is specifically there to keep the Text property
            //       from being returned.  The results could get very large otherwise.
            

            index.Indexes.Add("Text", FieldIndexing.Analyzed);
            index.Stores.Add("Text", FieldStorage.Yes);

            if (database.GetIndexDefinition("Raven/Attachments") == null)
                database.PutIndex("Raven/Attachments", index);
        }
    }
}
