using System;
using System.Runtime.InteropServices;

namespace Raven.Bundles.IndexedAttachments.Extraction
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Describes the position and extent of a specified portion of text within an object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct FILTERREGION
    {
        /// <summary>
        /// The Chunk ID
        /// </summary>
        public int idChunk;

        /// <summary>
        /// The beginning of the region, specified as an offset from the beginning of the chunk.
        /// </summary>
        public int cwcStart;

        /// <summary>
        /// The extent of the region, specified as a number of Unicode characters.
        /// </summary>
        public int cwcExtent;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct STAT_CHUNK
    {
        /// <summary>
        /// The chunk identifier. Chunk identifiers must be unique for the
        /// current instance of the IFilter interface. 
        /// Chunk identifiers must be in ascending order. The order in which
        /// chunks are numbered should correspond to the order in which they appear
        /// in the source document. Some search engines can take advantage of the
        /// proximity of chunks of various properties. If so, the order in which
        /// chunks with different properties are emitted will be important to the
        /// search engine. 
        /// </summary>
        public int idChunk;

        /// <summary>
        /// The type of break that separates the previous chunk from the current
        /// chunk. Values are from the CHUNK_BREAKTYPE enumeration. 
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public CHUNK_BREAKTYPE breakType;

        /// <summary>
        /// Flags indicate whether this chunk contains a text-type or a
        /// value-type property. 
        /// Flag values are taken from the CHUNKSTATE enumeration. If the CHUNK_TEXT flag is set, 
        /// IFilter::GetText should be used to retrieve the contents of the chunk
        /// as a series of words. 
        /// If the CHUNK_VALUE flag is set, IFilter::GetValue should be used to retrieve 
        /// the value and treat it as a single property value. If the filter dictates that the same 
        /// content be treated as both text and as a value, the chunk should be emitted twice in two 
        /// different chunks, each with one flag set. 
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public CHUNKSTATE flags;

        /// <summary>
        /// The language and sublanguage associated with a chunk of text. Chunk locale is used 
        /// by document indexers to perform proper word breaking of text. If the chunk is 
        /// neither text-type nor a value-type with data type VT_LPWSTR, VT_LPSTR or VT_BSTR, 
        /// this field is ignored. 
        /// </summary>
        public int locale;

        /// <summary>
        /// The property to be applied to the chunk. If a filter requires that the same text 
        /// have more than one property, it needs to emit the text once for each property 
        /// in separate chunks. 
        /// </summary>
        public FULLPROPSPEC attribute;

        /// <summary>
        /// The ID of the source of a chunk. The value of the idChunkSource member depends on the nature of the chunk: 
        /// If the chunk is a text-type property, the value of the idChunkSource member must be the same as the value of the idChunk member. 
        /// If the chunk is an public value-type property derived from textual content, the value of the idChunkSource member is the chunk ID for the
        /// text-type chunk from which it is derived. 
        /// If the filter attributes specify to return only public value-type
        /// properties, there is no content chunk from which to derive the current
        /// public value-type property. In this case, the value of the
        /// idChunkSource member must be set to zero, which is an invalid chunk. 
        /// </summary>
        public int idChunkSource;

        /// <summary>
        /// The offset from which the source text for a derived chunk starts in
        /// the source chunk. 
        /// </summary>
        public int cwcStartSource;

        /// <summary>
        /// The length in characters of the source text from which the current
        /// chunk was derived. 
        /// A zero value signifies character-by-character correspondence between
        /// the source text and 
        /// the derived text. A nonzero value means that no such direct
        /// correspondence exists
        /// </summary>
        public int cwcLenSource;
    }

    /// <summary>
    /// Specifies a property either by its property identifier (ID) or the associated string name.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct PROPSPEC
    {
        /// <summary>
        /// Indicates whether the propid or lpwstr member used.
        /// 0 - string used; 1 - PROPID
        /// </summary>
        [FieldOffset(0)]
        public int ulKind;

        /// <summary>
        /// Specifies the value of the property ID.
        /// Use either this value or the lpwstr, not both.
        /// </summary>
        [FieldOffset(4)]
        public int propid;

        /// <summary>
        /// Specifies the string name of the property as a null-terminated Unicode string.
        /// Use either this value or the propid, not both.
        /// </summary>
        [FieldOffset(4)]
        public IntPtr lpwstr;
    }

    /// <summary>
    /// Specifies a property set, and a property within the property set.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct FULLPROPSPEC
    {
        /// <summary>
        /// The guid that identifies the property set.
        /// </summary>
        public Guid guidPropSet;

        /// <summary>
        /// Pointer to the PROPSPEC structure that specifies a property either by its
        /// property identifier (propid) or by the associated string name (lpwstr).
        /// </summary>
        public PROPSPEC psProperty;
    }

    // ReSharper restore InconsistentNaming
}
