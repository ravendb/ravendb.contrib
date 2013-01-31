using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Bundles.IndexedAttachments;
using Raven.Database.Config;
using Raven.Json.Linq;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Bundles.Contrib.Tests.IndexedAttachments
{
    public class IndexedAttachmentsTests : RavenTestBase
    {
        private const string TestDocExt = @".doc";
        private const string TestDocPath = @"IndexedAttachments\docs\small.doc";
        private const string TestDocContentType = "application/msword";

        //private const string TestDocExt = @".pdf";
        //private const string TestDocPath = @"IndexedAttachments\docs\small.pdf";
        //private const string TestDocContentType = "application/pdf";

        protected override void ModifyConfiguration(RavenConfiguration configuration)
        {
            // Wire up the bundle to the embedded database
            configuration.Catalog.Catalogs.Add(new AssemblyCatalog(typeof(PutAttachmentTrigger).Assembly));
            configuration.Settings[Constants.ActiveBundles] = "IndexedAttachments";
        }

        [Fact]
        public void IndexedAttachmentsBundle_Can_Use_Filename_From_Metadata()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                const string key = "articles/1";
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject { { "Raven-Attachment-Filename", filename } };
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                var attachment = documentStore.DatabaseCommands.GetAttachment(key);
                Assert.Equal(filename, attachment.Metadata.Value<string>("Raven-Attachment-Filename"));
            }
        }

        [Fact]
        public void IndexedAttachmentsBundle_Can_Use_Filename_From_Metadata_OnReadInfo()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                const string key = "articles/1";
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject { { "Raven-Attachment-Filename", filename } };
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                var attachment = documentStore.DatabaseCommands.GetAttachmentHeadersStartingWith(key, 0, 1).First();
                Assert.Equal(filename, attachment.Metadata.Value<string>("Raven-Attachment-Filename"));
            }
        }

        [Fact]
        public void IndexedAttachmentsBundle_Can_Use_Filename_From_Key_By_Convention()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                var attachment = documentStore.DatabaseCommands.GetAttachment(key);
                Assert.Equal(filename, attachment.Metadata.Value<string>("Raven-Attachment-Filename"));
            }
        }

        [Fact]
        public void IndexedAttachmentsBundle_Sets_Content_Disposition_Header()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                var attachment = documentStore.DatabaseCommands.GetAttachment(key);
                Assert.Equal(string.Format("attachment; filename=\"{0}\"", filename), attachment.Metadata.Value<string>("Content-Disposition"));
            }
        }

        [Fact]
        public void IndexedAttachmentsBundle_Sets_Content_Type_Header()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                var attachment = documentStore.DatabaseCommands.GetAttachment(key);
                Assert.Equal(TestDocContentType, attachment.Metadata.Value<string>("Content-Type"));
            }
        }

        [Fact]
        public void IndexedAttachmentsBundle_Sets_Raven_Attachment_Content_Type_Header_OnReadInfo()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                var attachment = documentStore.DatabaseCommands.GetAttachmentHeadersStartingWith(key,0,1).First();
                Assert.Equal(TestDocContentType, attachment.Metadata.Value<string>("Raven-Attachment-Content-Type"));
            }
        }

        [FactIfIFilterInstalledForAttribute(TestDocExt)]
        public void IndexedAttachmentsBundle_Creates_Text_Document()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                var doc = documentStore.DatabaseCommands.Get(key + "/text");
                Assert.NotNull(doc);
                var text = doc.DataAsJson["Text"].ToString();
                Assert.Contains("thundercats", text, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void IndexedAttachmentsBundle_Can_Query_By_Filename()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                using (var session = documentStore.OpenSession())
                {
                    var results = session.Advanced.LuceneQuery<object>("Raven/Attachments")
                                         .WhereEquals("Filename", filename)
                                         .WaitForNonStaleResults()
                                         .ToList();

                    Assert.Equal(1, results.Count);

                    dynamic result = results.First();
                    Assert.Equal(filename, result.Filename);
                    Assert.Equal(key, result.AttachmentKey);
                }
            }
        }

        [Fact]
        public void IndexedAttachmentsBundle_Can_Query_By_AttachmentKey()
        {
            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                using (var session = documentStore.OpenSession())
                {
                    var results = session.Advanced.LuceneQuery<object>("Raven/Attachments")
                                         .WhereEquals("AttachmentKey", key)
                                         .WaitForNonStaleResults()
                                         .ToList();

                    Assert.Equal(1, results.Count);

                    dynamic result = results.First();
                    Assert.Equal(filename, result.Filename);
                    Assert.Equal(key, result.AttachmentKey);
                }
            }
        }

        [FactIfIFilterInstalledForAttribute(TestDocExt)]
        public void IndexedAttachmentsBundle_Can_Query_By_Text()
        {
            // This is the best part - what the whole thing is about.

            using (var documentStore = NewDocumentStore())
            {
                var filename = Path.GetFileName(TestDocPath);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(TestDocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                using (var session = documentStore.OpenSession())
                {
                    var results = session.Advanced.LuceneQuery<object>("Raven/Attachments")
                                         .Search("Text", "thundercats") // <<--- Full text analyzed search of the attachment text!
                                         .WaitForNonStaleResults()
                                         .ToList();

                    Assert.Equal(1, results.Count);

                    dynamic result = results.First();
                    Assert.Equal(filename, result.Filename);
                    Assert.Equal(key, result.AttachmentKey);

                    // Now you can take the attachment key and go load the attachment if you want.
                }
            }
        }
    }
}
