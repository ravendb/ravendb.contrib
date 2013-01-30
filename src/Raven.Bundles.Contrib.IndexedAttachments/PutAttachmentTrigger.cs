using System;
using System.ComponentModel.Composition;
using System.IO;
using Raven.Bundles.IndexedAttachments.Extraction;
using Raven.Database.Plugins;
using Raven.Json.Linq;

namespace Raven.Bundles.IndexedAttachments
{
    [InheritedExport(typeof(AbstractAttachmentPutTrigger))]
    [ExportMetadata("Bundle", "IndexedAttachments")]
    public class PutAttachmentTrigger : AbstractAttachmentPutTrigger
    {
        public override void OnPut(string key, Stream data, RavenJObject metadata)
        {
            // see if we have a filename in the metadata
            var filename = metadata.Value<string>("Raven-Attachment-Filename");
            if (!string.IsNullOrEmpty(filename))
                return;

            // try to get it from the key by convention
            filename = Utils.GetFilename(key);
            if (filename != null)
                metadata["Raven-Attachment-Filename"] = filename;
        }

        public override void AfterCommit(string key, Stream data, RavenJObject metadata, Guid etag)
        {
            // Make sure we have a filename
            var filename = metadata.Value<string>("Raven-Attachment-Filename");
            if (string.IsNullOrEmpty(filename))
                return;

            RavenJObject doc;
            try
            {
                // Extract the text in the attachment as a json document
                var extension = Path.GetExtension(filename);
                doc = Extractor.GetJson(data, extension);
            }
            catch (InvalidOperationException ex)
            {
                // If there is no ifilter installed, don't do the extraction, but still do everything else.
                if (!ex.Message.Contains("HRESULT: 0x80004005")) throw;

                // Still write a dummmy doc so we get metadata in the index.
                doc = new RavenJObject();
            }

            // Write the results to a document.  Include all of the attachment's metadata, and a reference back to the attachment key.
            var md = new RavenJObject(metadata) { { "Raven-Attachment-Key", key } };
            Database.Put(key + "/text", null, doc, md, null);
        }
    }
}
