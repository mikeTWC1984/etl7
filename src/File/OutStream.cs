using System;
using System.IO;

namespace ETL.File
{
    public class OutStream : IDisposable
    {
        public Stream Stream { get; }
        OutStream(Stream stream) { 
            if(!stream.CanWrite) throw new Exception("Stream is not writable");
            this.Stream = stream;
        }

        public static explicit operator OutStream(String filePath)
        {
             return new OutStream(System.IO.File.Create(filePath));
        }
        public static explicit operator OutStream(Byte[] bytes)
        {
            return new OutStream(new MemoryStream(bytes));
        }

        public static explicit operator OutStream(Stream ms)
        {
            return new OutStream(ms);
        }

        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }
}