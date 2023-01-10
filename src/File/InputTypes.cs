
using System;
using System.IO;

namespace ETL.File.InputTypes
{
    public class StreamData : IDisposable
    {
        public Stream Stream { get; }
        StreamData(Stream stream) { this.Stream = stream; }

        public static explicit operator StreamData(String filePath)
        {
            if (!(new System.IO.FileInfo(filePath)).Exists) { throw new Exception(filePath + " doesn't exist"); }
            return new StreamData(System.IO.File.OpenRead(filePath));
        }
        public static explicit operator StreamData(Byte[] bytes)
        {
            return new StreamData(new MemoryStream(bytes));
        }

        public static explicit operator StreamData(MemoryStream ms)
        {
            return new StreamData(ms);
        }

        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }


    public class ReaderData : IDisposable
    {
        public TextReader Stream { get; }
        ReaderData(TextReader stream) { this.Stream = stream; }

        public static explicit operator ReaderData(String filePath)
        {
            if (!(new System.IO.FileInfo(filePath)).Exists) { throw new Exception(filePath + " doesn't exist"); }
            return new ReaderData(new StreamReader(filePath));
        }
        public static explicit operator ReaderData(Byte[] bytes)
        {
            return new ReaderData(new StreamReader(new MemoryStream(bytes)));
        }

        public static explicit operator ReaderData(Stream fileStream)
        {
            return new ReaderData(new StreamReader(fileStream));
        }

        public static explicit operator ReaderData(MemoryStream ms)
        {
            return new ReaderData(new StreamReader(ms));
        }

        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }

}