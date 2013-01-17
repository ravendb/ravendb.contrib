using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Raven.Bundles.IndexedAttachments.Extraction
{
    /// <summary>
    /// Scans documents for text and properties (also called attributes).
    /// It extracts chunks of text from these documents, filtering out embedded formatting
    /// and retaining information about the position of the text. It also extracts chunks of
    /// values, which are properties of an entire document or of well-defined parts of a document.
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/ms691105.aspx
    /// </remarks>
    [ComImport, Guid("89BCB740-6119-101A-BCB7-00DD010655AF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFilter
    {
        /// <summary>
        /// Initializes a filtering session.
        /// </summary>
        /// <param name="grfFlags">
        /// Values from the IFILTER_INIT enumeration for controlling text standardization, property output,
        /// embedding scope, and IFilter access patterns.
        /// </param>
        /// <param name="cAttributes">
        /// The size of the attributes array. When nonzero, cAttributes takes precedence over attributes specified in grfFlags.
        /// If no attribute flags are specified and cAttributes is zero, the default is given by the PSGUID_STORAGE storage
        /// property set, which contains the date and time of the last write to the file, size, and so on; and by
        /// the PID_STG_CONTENTS 'contents' property, which maps to the main contents of the file.</param>
        /// <param name="aAttributes">
        /// Pointer to an array of FULLPROPSPEC structures for the requested properties.
        /// When cAttributes is nonzero, only the properties in aAttributes are returned.</param>
        /// <param name="pdwFlags">
        /// Information about additional properties available to the caller, from the IFILTER_FLAGS enumeration.
        /// </param>
        /// <returns>A status code from the IFilterReturnCodes enumeration.</returns>
        [PreserveSig]
        IFilterReturnCodes Init(IFILTER_INIT grfFlags, int cAttributes, IntPtr aAttributes, out IFILTER_FLAGS pdwFlags);

        /// <summary>
        /// Positions filter at beginning of first or next chunk and returns a descriptor.
        /// </summary>
        /// <param name="pStat">A pointer to a STAT_CHUNK structure containing a description of the current chunk.</param>
        /// <returns>A status code from the IFilterReturnCodes enumeration.</returns>
        [PreserveSig]
        IFilterReturnCodes GetChunk(out STAT_CHUNK pStat);

        /// <summary>
        /// Retrieves text from the current chunk.
        /// </summary>
        /// <param name="pcwcBuffer">On entry, the size of awcBuffer array in wide/Unicode characters. On exit, the number of Unicode characters written to awcBuffer.</param>
        /// <param name="awcBuffer">Text retrieved from the current chunk.</param>
        /// <returns>A status code from the IFilterReturnCodes enumeration.</returns>
        [PreserveSig]
        IFilterReturnCodes GetText(ref int pcwcBuffer, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder awcBuffer);

        ///// <summary>
        ///// Retrieves a value (internal value-type property) from a chunk, which must have a CHUNKSTATE enumeration value of CHUNK_VALUE.
        ///// </summary>
        ///// <param name="ppPropValue">A pointer to an output variable that receives a pointer to the PROPVARIANT structure that contains the value-type property.</param>
        ///// <returns>A status code from the IFilterReturnCodes enumeration.</returns>
        //[PreserveSig]
        //IFilterReturnCodes GetValue([MarshalAs(UnmanagedType.Struct)] out IntPtr ppPropValue);

        /// <summary>
        /// Retrieves a value (internal value-type property) from a chunk, which must have a CHUNKSTATE enumeration value of CHUNK_VALUE.
        /// </summary>
        /// <param name="ppPropValue">A pointer to an output variable that receives a pointer to the PROPVARIANT structure that contains the value-type property.</param>
        /// <returns>A status code from the IFilterReturnCodes enumeration.</returns>
        [PreserveSig]
        IFilterReturnCodes GetValue(out PROPVARIANT ppPropValue);

        /// <summary>
        /// Retrieves an interface representing the specified portion of object.
        /// Currently reserved for future use.
        /// </summary>
        [PreserveSig]
        IFilterReturnCodes BindRegion(FILTERREGION origPos, Guid riid, out object ppunk);
    }
}
