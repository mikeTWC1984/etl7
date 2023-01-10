

using System;
using System.IO;
using System.Collections.Generic;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Bcpg;
using ETL.File.InputTypes;



namespace ETL.Crypto.Pgp
{

    public class PgpWriter : IDisposable
    {

        public StreamWriter sw;
        //public Boolean armor { get; }
        private Stream outStream;
        private Stream originalStream;
        private Stream literalData;
        private Stream compressedData;
        private Stream encryptedData;
        public long linesWritten {get;private set;} = 0;
        
        public PgpWriter(Stream outStream, StreamData keyData, Int32 bufferSize, SymmetricKeyAlgorithmTag symKeyAlg, CompressionAlgorithmTag compressionAlg, Boolean armor)
        {
            if (!outStream.CanWrite) { throw new Exception("Input stream must be writeable"); }
            
            this.originalStream = outStream;
            this.outStream = armor ? new ArmoredOutputStream(outStream) : outStream;
            //this.armor = armor;

            var key = PgpEtlUtil.GetPgpPublicKey(keyData);
            var encryptor = new PgpEncryptedDataGenerator(symKeyAlg, true);
            encryptor.AddMethod(key);

            this.encryptedData = encryptor.Open(this.outStream, new Byte[bufferSize]);
            this.compressedData = (new PgpCompressedDataGenerator(compressionAlg)).Open(this.encryptedData, new Byte[bufferSize]);
            this.literalData = (new PgpLiteralDataGenerator()).Open(this.compressedData, PgpLiteralData.Binary, "", DateTime.Now, new Byte[bufferSize]);

            this.sw = new StreamWriter(this.literalData);

        }

        public void WriteLine(String data)
        {
            this.sw.WriteLine(data);
            this.linesWritten +=1; 
        }

        public Stream GetInStream() {
            return this.literalData;
        }

        public long CopyFromReader(System.Data.Common.DbDataReader reader, string delimiter, bool includeHeaders, bool quote) {

            return Util.ReaderToCsv(reader, this.sw, delimiter, includeHeaders, quote );
        }

        public void Dispose()
        {
            this.sw.Flush();
            this.literalData.Dispose();
            this.compressedData.Dispose();
            this.encryptedData.Dispose();
            this.sw.Dispose();
            this.outStream.Dispose();
            this.originalStream.Dispose();
        }

    }

}