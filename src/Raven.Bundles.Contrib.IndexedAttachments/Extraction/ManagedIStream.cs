using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace Raven.Bundles.IndexedAttachments.Extraction
{
    /// <summary>
    /// Provides an implementation of the COM IStream interface that wraps a .NET Stream.
    /// </summary>
    internal sealed class ManagedIStream : IStream
    {
        private readonly Stream _stream;

        public ManagedIStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            _stream = stream;
        }

        void IStream.Read(byte[] buffer, int cb, IntPtr pcbRead)
        {
            int val = _stream.Read(buffer, 0, cb);
            if (pcbRead != IntPtr.Zero)
                Marshal.WriteInt32(pcbRead, val);
        }

        void IStream.Write(byte[] buffer, int cb, IntPtr pcbWritten)
        {
            _stream.Write(buffer, 0, cb);
            if (pcbWritten != IntPtr.Zero)
                Marshal.WriteInt32(pcbWritten, cb);
        }

        void IStream.Seek(long offset, int dwOrigin, IntPtr plibNewPosition)
        {
            SeekOrigin origin;
            switch (dwOrigin)
            {
                case Win32.STREAM_SEEK_SET:
                    origin = SeekOrigin.Begin;
                    break;
                case Win32.STREAM_SEEK_CUR:
                    origin = SeekOrigin.Current;
                    break;
                case Win32.STREAM_SEEK_END:
                    origin = SeekOrigin.End;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dwOrigin");
            }

            long val = _stream.Seek(offset, origin);
            if (plibNewPosition != IntPtr.Zero)
                Marshal.WriteInt64(plibNewPosition, val);
        }

        void IStream.SetSize(long libNewSize)
        {
            _stream.SetLength(libNewSize);
        }

        void IStream.CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            throw new NotSupportedException();
        }

        void IStream.Commit(int grfCommitFlags)
        {
            throw new NotSupportedException();
        }

        void IStream.Revert()
        {
            throw new NotSupportedException();
        }

        void IStream.LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotSupportedException();
        }

        void IStream.UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotSupportedException();
        }

        void IStream.Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new STATSTG
                       {
                           type = 2,
                           cbSize = _stream.Length,
                       };

            if (_stream.CanRead && _stream.CanWrite)
                pstatstg.grfMode = Win32.STGM_READWRITE;
            else if (_stream.CanWrite)
                pstatstg.grfMode = Win32.STGM_WRITE;
            else if (_stream.CanRead)
                pstatstg.grfMode = Win32.STGM_READ;
            else
                throw new IOException();
        }

        void IStream.Clone(out IStream ppstm)
        {
            ppstm = null;
            throw new NotSupportedException();
        }

        private static class Win32
        {
            // ReSharper disable InconsistentNaming
            public const int STGM_READ = 0;
            public const int STGM_WRITE = 1;
            public const int STGM_READWRITE = 2;

            public const int STREAM_SEEK_SET = 0;
            public const int STREAM_SEEK_CUR = 1;
            public const int STREAM_SEEK_END = 2;
            // ReSharper restore InconsistentNaming
        }
    }
}
