using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Raven.Bundles.IndexedAttachments;
using Raven.Client;
using Raven.Client.Linq;
using Raven.Database.Config;
using Raven.Json.Linq;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Bundles.Contrib.Tests.IndexedAttachments
{
    public class HighlightsWithIndexedAttachments : RavenTestBase
    {
        protected override void ModifyConfiguration(RavenConfiguration configuration)
        {
            // Wire up the bundle to the embedded database
            configuration.Catalog.Catalogs.Add(new AssemblyCatalog(typeof(PutAttachmentTrigger).Assembly));
        }

        [Fact]
        public void IndexedAttachmentsBundle_Can_Query_By_Text_With_Highlighting()
        {
            using (var documentStore = NewDocumentStore())
            {
                const string path = @"IndexedAttachments\docs\medium.docx";
                var filename = Path.GetFileName(path);
                var key = "articles/1/" + filename;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var metadata = new RavenJObject();
                    documentStore.DatabaseCommands.PutAttachment(key, null, stream, metadata);
                }

                using (var session = documentStore.OpenSession())
                {
                    const int fragmentLength = 31;       // see note below
                    const int maxFragmentsToReturn = 50;

                    FieldHighlightings highlights;
                    var results = session.Advanced.LuceneQuery<object>("Raven/Attachments")
                                         .SkipTransformResults()
                                         .Highlight("Text", fragmentLength, maxFragmentsToReturn, out highlights)
                                         .SetHighlighterTags("*", "*")
                                         .Search("Text", "congress")
                                         .WaitForNonStaleResults()
                                         .ToList();

                    Assert.Equal(1, results.Count);

                    var id = session.Advanced.GetDocumentId(results[0]);
                    var fragments = highlights.GetFragments(id);

                    Assert.Equal(29, fragments.Length);
                    foreach (var fragment in fragments)
                        Debug.WriteLine(fragment);

/*
 *  This test passes.  There are 29 instances of "congress" in the document (the us constitution).
 *  But what is strange is that when the fragment length is changed to 32 or greater, only 28 fragments come back.
 *  The line of text with the missing fragment is as follows (without quotes)
 *    
 *  "Clause 2: The Congress shall have Power to dispose of and make all needful Rules and Regulations respecting the Territory or other Property belonging to the United States; and nothing in this Constitution shall be so construed as to Prejudice any Claims of the United States, or of any particular State. "
 *  
 *  http://issues.hibernatingrhinos.com/issue/RavenDB-866
*/

                }
            }
        }
    }
}
