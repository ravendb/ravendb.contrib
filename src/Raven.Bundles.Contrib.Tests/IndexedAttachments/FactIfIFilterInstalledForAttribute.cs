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
            if (!FilterLoader.FilterIsInstalledFor(extension))
                Skip = string.Format("The test cannot run because there is no IFilter installed for {0} files.", extension);
        }
    }
}
