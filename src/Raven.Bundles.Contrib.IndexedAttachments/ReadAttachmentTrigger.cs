using System;
using System.ComponentModel.Composition;
using System.Web;
using Raven.Abstractions.Data;
using Raven.Database.Data;
using Raven.Database.Plugins;
using Raven.Json.Linq;

namespace Raven.Bundles.IndexedAttachments
{
    [InheritedExport(typeof(AbstractAttachmentReadTrigger))]
    [ExportMetadata("Bundle", "IndexedAttachments")]
    public class ReadAttachmentTrigger : AbstractAttachmentReadTrigger
    {
        // Two implementations to override?  Yuk! http://issues.hibernatingrhinos.com/issue/RavenDB-818

        public override void OnRead(string key, Attachment attachment)
        {
            AugmentMetadata(key, attachment.Metadata);
        }

        public override void OnRead(AttachmentInformation information)
        {
            AugmentMetadata(information.Key, information.Metadata);
        }

        private static void AugmentMetadata(string key, RavenJObject metadata)
        {
            // if we have a filename in metadata, use it.
            var filename = metadata.Value<string>("Raven-Attachment-Filename");
            if (string.IsNullOrEmpty(filename))
            {
                // if not, try to get it from the key by convention
                filename = Utils.GetFilename(key);
                if (filename != null)
                {
                    metadata["Raven-Attachment-Filename"] = filename;
                }
                else
                {
                    // forget it, we can't do anything else without a filename
                    return;
                }
            }

            // Add a Content-Type header
            var contentType = Utils.GetMimeType(filename);
            if (!string.IsNullOrEmpty(contentType))
                metadata["Content-Type"] = contentType;

            // Add a Content-Disposition header

            // The filename is supposed to be quoted, but it doesn't escape the quotes properly.
            // http://issues.hibernatingrhinos.com/issue/RavenDB-824

            if (Utils.RavenVersion <= Version.Parse("2.0.2230.0")) // leave them off for 2230 and lower
                metadata["Content-Disposition"] = string.Format("attachment; filename={0}", filename);
            else
                metadata["Content-Disposition"] = string.Format("attachment; filename=\"{0}\"", filename);
        }
    }
}
