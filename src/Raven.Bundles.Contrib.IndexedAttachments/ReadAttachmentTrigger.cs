using System;
using System.ComponentModel.Composition;
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
        // This is fired when actually retrieving the attachment
        public override void OnRead(string key, Attachment attachment)
        {
            // Get the filename, and make sure its in the returned metadata
            var filename = AugmentMetadataWithFilename(key, attachment.Metadata);

            // We can't do anything else without a filename
            if (filename == null)
                return;

            // Add a Content-Type header
            var contentType = Utils.GetMimeType(filename);
            if (!string.IsNullOrEmpty(contentType))
                attachment.Metadata["Content-Type"] = contentType;

            // Add a Content-Disposition header

            // The filename is supposed to be quoted, but it doesn't escape the quotes properly.
            // http://issues.hibernatingrhinos.com/issue/RavenDB-824
            // This was fixed in 2235.  For prior versions, don't send the quotes.

            if (Utils.RavenVersion < Version.Parse("2.0.2235.0"))
                attachment.Metadata["Content-Disposition"] = string.Format("attachment; filename={0}", filename);
            else
                attachment.Metadata["Content-Disposition"] = string.Format("attachment; filename=\"{0}\"", filename);
        }

        // This is fired when retrieving INFORMATION about the attachment
        public override void OnRead(AttachmentInformation information)
        {
            // Get the filename, and make sure its in the returned metadata
            var filename = AugmentMetadataWithFilename(information.Key, information.Metadata);

            // We can't do anything else without a filename
            if (filename == null)
                return;

            // Add a Raven-Attachment-Content-Type header
            var contentType = Utils.GetMimeType(filename);
            if (!string.IsNullOrEmpty(contentType))
                information.Metadata["Raven-Attachment-Content-Type"] = contentType;
        }

        private static string AugmentMetadataWithFilename(string key, RavenJObject metadata)
        {
            // if we have a filename in metadata, use it.
            var filename = metadata.Value<string>("Raven-Attachment-Filename");
            if (!string.IsNullOrEmpty(filename))
                return filename;

            // if not, try to get it from the key by convention
            filename = Utils.GetFilename(key);
            if (filename != null)
                metadata["Raven-Attachment-Filename"] = filename;

            return filename;
        }
    }
}
