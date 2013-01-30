using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;

namespace Raven.Bundles.IndexedAttachments.Extraction
{
    public static class Extractor
    {
        public static RavenJObject GetJson(Stream stream, string extension)
        {
            IFilter filter = null;
            try
            {
                filter = FilterLoader.LoadForStream(stream, extension);
                return GetJson(filter);
            }
            finally
            {
                if (filter != null)
                    Marshal.ReleaseComObject(filter);
            }
        }

        private static RavenJObject GetJson(IFilter filter)
        {
            // initialize a buffer for text results
            const int defaultBufferSize = 4096;
            var buffer = new StringBuilder(defaultBufferSize);

            // Initialize the json writers
            using (var textWriter = new RavenJTokenWriter())
            using (var propWriter = new RavenJTokenWriter())
            {
                // Write the beginning of the json arrays
                textWriter.WriteStartArray();
                propWriter.WriteStartArray();

                string last = null;

                // Outer loop will read chunks from the document.
                // For those chunks that have text, the contents will be written to json.
                while (true)
                {
                    // Try to get a chunk of data
                    STAT_CHUNK statChunk;
                    var chunkStatus = filter.GetChunk(out statChunk);
                    switch (chunkStatus)
                    {
                        case IFilterReturnCodes.S_OK:
                            // We have a good chunk of data
                            break;

                        case IFilterReturnCodes.FILTER_E_END_OF_CHUNKS:
                            // No more data.
                            if (buffer.Length > 0)
                            {
                                // Make sure we have no unwritten data first.
                                textWriter.WriteLines(buffer.ToString());
                                buffer.Clear();
                            }

                            // close the json array and flush the writers
                            textWriter.WriteEndArray();
                            propWriter.WriteEndArray();
                            textWriter.Flush();
                            propWriter.Flush();

                            // assemble and return the document
                            return new RavenJObject
                                   {
                                       //{ "Properties", propWriter.Token }, // TODO: restore this when properties can be retrieved
                                       { "Text", textWriter.Token }
                                   };

                        case IFilterReturnCodes.FILTER_E_EMBEDDING_UNAVAILABLE:
                        case IFilterReturnCodes.FILTER_E_LINK_UNAVAILABLE:
                            // Ignore these warnings
                            continue;

                        default:
                            // Something else - throw an exception
                            throw new COMException("IFilter COM error while getting a chunk of data: " + chunkStatus);
                    }

                    //// Handle property value chunks  TODO: make this work so we can index properties in addition to text
                    //if (statChunk.flags.HasFlag(CHUNKSTATE.CHUNK_VALUE))
                    //{
                    //    // get the property name  TODO: This doesn't seem to work
                    //    var propInfo = statChunk.attribute.psProperty;
                    //    var propName = propInfo.ulKind == 0 ? Marshal.PtrToStringAuto(propInfo.lpwstr) : propInfo.propid.ToString();
                    //    

                    //    // will this help?
                    //    var propGuid = statChunk.attribute.guidPropSet;

                    //    // get the value  TODO: This doesn't seem to work
                    //    PROPVARIANT ppPropValue;
                    //    var valueStatus = filter.GetValue(out ppPropValue);
                    //    if (valueStatus == IFilterReturnCodes.S_OK || valueStatus == IFilterReturnCodes.FILTER_S_LAST_VALUES)
                    //    {
                    //        // write the value to json
                    //        propWriter.WriteStartObject();
                    //        propWriter.WritePropertyName(propName);
                    //        propWriter.WriteValue(ppPropValue.Value);
                    //        propWriter.WriteEndObject();

                    //        // free unmanaged memory from the PropVariant
                    //        ppPropValue.Clear();
                    //    }
                    //}

                    // the rest of this code is for text chunks only
                    if (!statChunk.flags.HasFlag(CHUNKSTATE.CHUNK_TEXT))
                        continue;

                    // Check for white space items and add the appropriate breaks.
                    switch (statChunk.breakType)
                    {
                        case CHUNK_BREAKTYPE.CHUNK_EOW:
                            if (buffer.Length > 0 && !char.IsWhiteSpace(buffer[buffer.Length - 1]))
                                buffer.Append(' ');
                            break;

                        case CHUNK_BREAKTYPE.CHUNK_EOC:
                        case CHUNK_BREAKTYPE.CHUNK_EOP:
                        case CHUNK_BREAKTYPE.CHUNK_EOS:
                            // Each chapter, paragraph or sentence break can be in a new json value in our array.
                            // This will keep any one string from getting too big.
                            if (buffer.Length > 0)
                            {
                                textWriter.WriteLines(buffer.ToString());
                                
                                buffer.Clear();
                            }
                            break;
                    }

                    while (true)
                    {
                        // Create a temporary string buffer we can use for the parsing algorithm.
                        int cBuffer = defaultBufferSize;
                        var sbBuffer = new StringBuilder(defaultBufferSize);

                        // Read the next piece of data up to the size of our local buffer.
                        var textStatus = filter.GetText(ref cBuffer, sbBuffer);
                        if (textStatus == IFilterReturnCodes.S_OK || textStatus == IFilterReturnCodes.FILTER_S_LAST_TEXT)
                        {
                            // If any data was returned, add it to the buffer.
                            buffer.Append(sbBuffer.ToString(), 0, cBuffer);
                        }

                        // Once all data is exhausted, we are done so terminate the loop.
                        if (textStatus == IFilterReturnCodes.FILTER_S_LAST_TEXT || textStatus == IFilterReturnCodes.FILTER_E_NO_MORE_TEXT)
                            break;
                    }


                }
            }
        }

        private static void WriteLines(this JsonWriter writer, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in lines.Select(x => x.Trim()).Where(x => x != string.Empty))
                writer.WriteValue(s);
        }
    }
}
