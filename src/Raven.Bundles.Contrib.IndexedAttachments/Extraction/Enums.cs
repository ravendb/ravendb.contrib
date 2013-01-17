using System;

namespace Raven.Bundles.IndexedAttachments.Extraction
{
    // ReSharper disable InconsistentNaming

    internal enum IFilterReturnCodes : uint
    {
        /// <summary>
        /// The operation was completed successfully.
        /// </summary>
        S_OK = 0,

        /// <summary>
        /// The function was denied access to the filter file. 
        /// </summary>
        E_ACCESSDENIED = 0x80070005,

        /// <summary>
        /// The function encountered an invalid handle,
        /// probably due to a low-memory situation. 
        /// </summary>
        E_HANDLE = 0x80070006,

        /// <summary>
        /// Count and contents of attributes do not agree.
        /// </summary>
        E_INVALIDARG = 0x80070057,

        /// <summary>
        /// Out of memory
        /// </summary>
        E_OUTOFMEMORY = 0x8007000E,

        /// <summary>
        /// Not implemented
        /// </summary>
        E_NOTIMPL = 0x80004001,

        /// <summary>
        /// File to filter was not previously loaded.
        /// </summary>
        E_FAIL = 0x80000008,

        /// <summary>
        /// Access has been denied because of password protection or similar security measures.
        /// </summary>
        FILTER_E_PASSWORD = 0x8004170B,

        /// <summary>
        /// The document format is not recognized by the filter
        /// </summary>
        FILTER_E_UNKNOWNFORMAT = 0x8004170C,

        /// <summary>
        /// No text in current chunk
        /// </summary>
        FILTER_E_NO_TEXT = 0x80041705,

        /// <summary>
        /// No values in current chunk
        /// </summary>
        FILTER_E_NO_VALUES = 0x80041706,

        /// <summary>
        /// No more chunks of text available in object
        /// </summary>
        FILTER_E_END_OF_CHUNKS = 0x80041700,

        /// <summary>
        /// No more text available in chunk
        /// </summary>
        FILTER_E_NO_MORE_TEXT = 0x80041701,

        /// <summary>
        /// No more property values available in chunk
        /// </summary>
        FILTER_E_NO_MORE_VALUES = 0x80041702,

        /// <summary>
        /// Unable to access object
        /// </summary>
        FILTER_E_ACCESS = 0x80041703,

        /// <summary>
        /// Moniker doesn't cover entire region
        /// </summary>
        FILTER_W_MONIKER_CLIPPED = 0x00041704,

        /// <summary>
        /// Unable to bind IFilter for embedded object
        /// </summary>
        FILTER_E_EMBEDDING_UNAVAILABLE = 0x80041707,

        /// <summary>
        /// Unable to bind IFilter for linked object
        /// </summary>
        FILTER_E_LINK_UNAVAILABLE = 0x80041708,

        /// <summary>
        ///  This is the last text in the current chunk
        /// </summary>
        FILTER_S_LAST_TEXT = 0x00041709,

        /// <summary>
        /// This is the last value in the current chunk
        /// </summary>
        FILTER_S_LAST_VALUES = 0x0004170A
    }

    /// <summary>
    /// Flags controlling the operation of the IFilter
    /// instance.
    /// </summary>
    [Flags]
    internal enum IFILTER_INIT
    {
        /// <summary>
        /// Paragraph breaks should be marked with the Unicode PARAGRAPH
        /// SEPARATOR (0x2029)
        /// </summary>
        IFILTER_INIT_CANON_PARAGRAPHS = 1,

        /// <summary>
        /// Soft returns, such as the newline character in Microsoft Word, should
        /// be replaced by hard returnsLINE SEPARATOR (0x2028). Existing hard
        /// returns can be doubled. A carriage return (0x000D), line feed (0x000A),
        /// or the carriage return and line feed in combination should be considered
        /// a hard return. The intent is to enable pattern-expression matches that
        /// match against observed line breaks. 
        /// </summary>
        IFILTER_INIT_HARD_LINE_BREAKS = 2,

        /// <summary>
        /// Various word-processing programs have forms of hyphens that are not
        /// represented in the host character set, such as optional hyphens
        /// (appearing only at the end of a line) and nonbreaking hyphens. This flag
        /// indicates that optional hyphens are to be converted to nulls, and
        /// non-breaking hyphens are to be converted to normal hyphens (0x2010), or
        /// HYPHEN-MINUSES (0x002D). 
        /// </summary>
        IFILTER_INIT_CANON_HYPHENS = 4,

        /// <summary>
        /// Just as the IFILTER_INIT_CANON_HYPHENS flag standardizes hyphens,
        /// this one standardizes spaces. All special space characters, such as
        /// nonbreaking spaces, are converted to the standard space character
        /// (0x0020). 
        /// </summary>
        IFILTER_INIT_CANON_SPACES = 8,

        /// <summary>
        /// Indicates that the client wants text split into chunks representing
        /// public value-type properties. 
        /// </summary>
        IFILTER_INIT_APPLY_INDEX_ATTRIBUTES = 16,

        /// <summary>
        /// Indicates that the client wants text split into chunks representing
        /// properties determined during the indexing process. 
        /// </summary>
        IFILTER_INIT_APPLY_CRAWL_ATTRIBUTES = 256,

        /// <summary>
        /// Any properties not covered by the IFILTER_INIT_APPLY_INDEX_ATTRIBUTES
        /// and IFILTER_INIT_APPLY_CRAWL_ATTRIBUTES flags should be emitted. 
        /// </summary>
        IFILTER_INIT_APPLY_OTHER_ATTRIBUTES = 32,

        /// <summary>
        /// Optimizes IFilter for indexing because the client calls the
        /// IFilter::Init method only once and does not call IFilter::BindRegion.
        /// This eliminates the possibility of accessing a chunk both before and
        /// after accessing another chunk. 
        /// </summary>
        IFILTER_INIT_INDEXING_ONLY = 64,

        /// <summary>
        /// The text extraction process must recursively search all linked
        /// objects within the document. If a link is unavailable, the
        /// IFilter::GetChunk call that would have obtained the first chunk of the
        /// link should return FILTER_E_LINK_UNAVAILABLE. 
        /// </summary>
        IFILTER_INIT_SEARCH_LINKS = 128,

        /// <summary>
        /// The content indexing process can return property values set by the  filter. 
        /// </summary>
        IFILTER_INIT_FILTER_OWNED_VALUE_OK = 512
    }

    [Flags]
    internal enum IFILTER_FLAGS
    {
        /// <summary>
        /// The caller should use the IPropertySetStorage and IPropertyStorage
        /// interfaces to locate additional properties. 
        /// When this flag is set, properties available through COM
        /// enumerators should not be returned from IFilter. 
        /// </summary>
        IFILTER_FLAGS_OLE_PROPERTIES = 1
    }

    internal enum CHUNKSTATE
    {
        /// <summary>
        /// The current chunk is a text-type property.
        /// </summary>
        CHUNK_TEXT = 0x1,

        /// <summary>
        /// The current chunk is a value-type property. 
        /// </summary>
        CHUNK_VALUE = 0x2,

        /// <summary>
        /// Reserved
        /// </summary>
        CHUNK_FILTER_OWNED_VALUE = 0x4
    }

    /// <summary>
    /// Enumerates the different breaking types that occur between 
    /// chunks of text read out by the FileFilter.
    /// </summary>
    internal enum CHUNK_BREAKTYPE
    {
        /// <summary>
        /// No break is placed between the current chunk and the previous chunk.
        /// The chunks are glued together. 
        /// </summary>
        CHUNK_NO_BREAK = 0,

        /// <summary>
        /// A word break is placed between this chunk and the previous chunk that
        /// had the same attribute. 
        /// Use of CHUNK_EOW should be minimized because the choice of word
        /// breaks is language-dependent, 
        /// so determining word breaks is best left to the search engine. 
        /// </summary>
        CHUNK_EOW = 1,

        /// <summary>
        /// A sentence break is placed between this chunk and the previous chunk
        /// that had the same attribute. 
        /// </summary>
        CHUNK_EOS = 2,

        /// <summary>
        /// A paragraph break is placed between this chunk and the previous chunk
        /// that had the same attribute.
        /// </summary>     
        CHUNK_EOP = 3,

        /// <summary>
        /// A chapter break is placed between this chunk and the previous chunk
        /// that had the same attribute. 
        /// </summary>
        CHUNK_EOC = 4
    }

    // ReSharper restore InconsistentNaming
}
