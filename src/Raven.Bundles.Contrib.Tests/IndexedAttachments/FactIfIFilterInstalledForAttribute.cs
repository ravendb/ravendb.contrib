using System;
using Raven.Bundles.IndexedAttachments.Extraction;
using Xunit;

namespace Raven.Bundles.Contrib.Tests.IndexedAttachments
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class FactIfIFilterInstalledForAttribute : FactAttribute
    {
        public FactIfIFilterInstalledForAttribute(string extension)
        {
            if (FilterLoader.FilterIsInstalledFor(extension))
                return;

            Skip = string.Format("The test cannot run because there is no IFilter installed for {0} files.", extension);

            switch (extension.ToLower())
            {
                case ".pdf":
                    if (Environment.Is64BitProcess)
                        Skip += @"

You can install one of the following 64 bit PDF IFilters:

Adobe:  http://www.adobe.com/support/downloads/detail.jsp?ftpID=5542
Foxit:  http://www.foxitsoftware.com/products/ifilter

The Foxit filter is a commerical product, but is much faster - especially on large documents.
The desktop edition should work in most cases.
";
                    else
                        Skip += @"

You can install one of the following 32 bit PDF IFilters:

Adobe:  http://get.adobe.com/reader
Foxit:  http://www.foxitsoftware.com/products/ifilter

The Adobe IFilter for 32 bit systems is included when you install Acrobat Reader.

The Foxit filter is a commerical product, but is much faster - especially on large documents.
The desktop edition should work in most cases.
";
                    break;

                case ".docx":
                case ".xlsx":
                case ".pptx":
                case ".vsdx":
                case ".zip":
                case ".doc":
                case ".xls":
                case ".ppt":
                case ".vsd":
                case ".one":
                case ".odf":
                    if (Environment.Is64BitProcess)
                        Skip += @"

Try installing the Microsoft Office 2010 Filter Pack.

Base Installation: http://www.microsoft.com/en-us/download/details.aspx?id=17062
Service Pack 1: http://www.microsoft.com/en-us/download/details.aspx?id=26604
";
                    else
                        Skip += @"

Try installing the Microsoft Office 2010 Filter Pack.

Base Installation: http://www.microsoft.com/en-us/download/details.aspx?id=17062
Service Pack 1: http://www.microsoft.com/en-us/download/details.aspx?id=26606
";
                    return;
            }
        }
    }
}
