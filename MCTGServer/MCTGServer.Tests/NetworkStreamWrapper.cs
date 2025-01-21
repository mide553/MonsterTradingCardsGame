using System;
using System.IO;

namespace MCTG.Tests
{
    public class NetworkStreamWrapper : Stream
    {
        private readonly MemoryStream _memoryStream;

        public NetworkStreamWrapper()
        {
            _memoryStream = new MemoryStream();
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            _memoryStream.Write(buffer, offset, size);
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            return _memoryStream.Read(buffer, offset, size);
        }

        public byte[] ToArray()
        {
            return _memoryStream.ToArray();
        }

        public override bool CanRead => _memoryStream.CanRead;
        public override bool CanSeek => _memoryStream.CanSeek;
        public override bool CanWrite => _memoryStream.CanWrite;
        public override long Length => _memoryStream.Length;
        public override long Position
        {
            get => _memoryStream.Position;
            set => _memoryStream.Position = value;
        }

        public override void Flush()
        {
            _memoryStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _memoryStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _memoryStream.SetLength(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _memoryStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
