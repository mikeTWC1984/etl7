using System;
using System.IO;

namespace ETL.File
{
    public class InStream : IDisposable
    {
        public Stream Stream { get; }
        InStream(Stream stream) { this.Stream = stream; }

        public static explicit operator InStream(String filePath)
        {
            return new InStream(System.IO.File.OpenRead(filePath));
        }
        public static explicit operator InStream(Byte[] bytes)
        {
            return new InStream(new MemoryStream(bytes));
        }

        public static explicit operator InStream(Stream ms)
        {
            return new InStream(ms);
        }

        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }
}