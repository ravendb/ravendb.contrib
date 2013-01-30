using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Raven.Bundles.IndexedAttachments.Extraction;
using Raven.Imports.Newtonsoft.Json;
using Xunit;

// Tested using the Adobe Acrobat PDF IFilter 64 11.0.01
// http://www.adobe.com/support/downloads/detail.jsp?ftpID=5542  (x64)
// NOTE: If you are on an x86 system, Adobe bundles the 32 bit IFilter with Acrobat Reader.  There is no separate install.

// Also tested using the Foxit PDF IFilter  (x64 desktop edition)
// http://www.foxitsoftware.com/products/ifilter

// Both Acrobat and Foxit work, but with the "large" document, Acrobat took about 90 seconds, while Foxit was done in under 3 seconds.
// If you have large pdf files, you might want to purchase the Foxit IFilter.

namespace Raven.Bundles.Contrib.Tests.IndexedAttachments.IFilterTests
{
    public class PdfTests
    {
        [FactIfIFilterInstalledFor(".pdf")]
        public void Can_Extract_Json_From_Small_Pdf()
        {
            using (var stream = File.OpenRead(@"IndexedAttachments\docs\small.pdf"))
            {
                var json = Extractor.GetJson(stream, ".pdf");
                var str = json.ToString(Formatting.Indented);
                Debug.WriteLine("{0} characters", str.Length);
                Debug.WriteLine(str);

                const string expected = "Thundercats are on the move";
                var found = json["Text"].Values<string>().Any(x => x.IndexOf(expected, StringComparison.InvariantCultureIgnoreCase) >= 0);
                Assert.True(found);
            }
        }

        [FactIfIFilterInstalledFor(".pdf")]
        public void Can_Extract_Json_From_Medium_Pdf()
        {
            using (var stream = File.OpenRead(@"IndexedAttachments\docs\medium.pdf"))
            {
                var json = Extractor.GetJson(stream, ".pdf");
                var str = json.ToString(Formatting.Indented);
                Debug.WriteLine("{0} characters", str.Length);
                Debug.WriteLine(str);

                const string expected = "We the People";
                var found = json["Text"].Values<string>().Any(x => x.IndexOf(expected, StringComparison.InvariantCultureIgnoreCase) >= 0);
                Assert.True(found);
            }
        }

        [FactIfIFilterInstalledFor(".pdf")]
        public void Can_Extract_Json_From_Large_Pdf()
        {
            using (var stream = File.OpenRead(@"IndexedAttachments\docs\large.pdf"))
            {
                var json = Extractor.GetJson(stream, ".pdf");

                Debug.WriteLine("(Too big to dump without hanging visual studio.)");
                //var str = json.ToString(Formatting.Indented);
                //Debug.WriteLine("{0} characters", str.Length);
                //Debug.WriteLine(str);

                const string expected = "Holy Bible";
                var found = json["Text"].Values<string>().Any(x => x.IndexOf(expected, StringComparison.InvariantCultureIgnoreCase) >= 0);
                Assert.True(found);
            }
        }
    }
}
