using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Raven.Bundles.IndexedAttachments.Extraction
{
    internal static class FilterLoader
    {
        public static IFilter LoadForStream(Stream stream, string extension)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
                throw new InvalidOperationException("The stream is not readable.");

            if (extension == null)
                throw new ArgumentNullException("extension");

            if (!extension.StartsWith("."))
                throw new ArgumentException("The extension is invalid. Pass the extension including the leading period.");

            // Make sure we start at the beginning of the stream
            if (stream.Position != 0)
            {
                if (!stream.CanSeek)
                    throw new InvalidOperationException("The stream is not at the beginning, and it is not seekable.");

                stream.Seek(0, SeekOrigin.Begin);
            }

            // Load an IFilter
            IFilter filter;
            int hResult = NativeMethods.LoadIFilter(extension, new IntPtr(0), out filter);
            if (hResult != 0)
                throw new InvalidOperationException(string.Format("Failed to find an IFilter for {0} files.  HRESULT: 0x{1:x}", extension, hResult));

            try
            {
                // Make sure it can work with streaming data
                var iPersistStream = filter as IPersistStream;
                if (iPersistStream == null)
                    throw new InvalidOperationException(string.Format("The installed IFilter for {0} files does not implement IPersistStream.", extension));

                // Load the stream into the filter
                iPersistStream.Load(new ManagedIStream(stream));

                // Configure options for the filter
                const IFILTER_INIT options = IFILTER_INIT.IFILTER_INIT_CANON_PARAGRAPHS |
                                             IFILTER_INIT.IFILTER_INIT_CANON_HYPHENS |
                                             IFILTER_INIT.IFILTER_INIT_CANON_SPACES |
                                             IFILTER_INIT.IFILTER_INIT_HARD_LINE_BREAKS |
                                             IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES |
                                             IFILTER_INIT.IFILTER_INIT_INDEXING_ONLY;

                // Initialize the filter provider
                IFILTER_FLAGS uflags;
                var initStatus = filter.Init(options, 0, new IntPtr(0), out uflags);
                if (initStatus != IFilterReturnCodes.S_OK)
                    throw new InvalidOperationException(string.Format("Could not initialize the IFilter for {0} files.", extension));

                return filter;
            }
            catch
            {
                if (filter != null)
                    Marshal.ReleaseComObject(filter);
                throw;
            }
        }

        public static IFilter LoadForFile(string path)
        {
            // Try to load the filter for the path given.
            IFilter filter;
            int hResult = NativeMethods.LoadIFilter(path, new IntPtr(0), out filter);
            if (hResult != 0)
                throw new InvalidOperationException(string.Format("Failed to find IFilter for file {0}.  HRESULT: 0x{1:x}", path, hResult));

            // Initialize the filter provider.
            const IFILTER_INIT options = IFILTER_INIT.IFILTER_INIT_CANON_PARAGRAPHS |
                                         IFILTER_INIT.IFILTER_INIT_CANON_HYPHENS |
                                         IFILTER_INIT.IFILTER_INIT_CANON_SPACES |
                                         IFILTER_INIT.IFILTER_INIT_HARD_LINE_BREAKS |
                                         IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES |
                                         IFILTER_INIT.IFILTER_INIT_INDEXING_ONLY;

            IFILTER_FLAGS uflags;
            var initStatus = filter.Init(options, 0, new IntPtr(0), out uflags);
            if (initStatus != IFilterReturnCodes.S_OK)
                return null;

            return filter;
        }

        private static class NativeMethods
        {
            [DllImport("query.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern int LoadIFilter(string pwcsPath, [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, out IFilter ppIUnk);
        }
    }
}
