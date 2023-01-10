
using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using ETL.File.InputTypes;
using System.Collections.Generic;


namespace ETL.Crypto.Pgp
{

    public class PgpReader: IDisposable
    {
        private Stream OutStream;
        public StreamReader sr;
        private Stream InStream;

        public Boolean HasMoreData {get {return !this.sr.EndOfStream;}}
        public long linesRed {get; private set;} = 0;


        public PgpReader(Stream inStream, StreamData keyData, String passPhrase)
        {
            this.InStream = inStream;

            PgpObjectFactory pgpObjF = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inStream));
            PgpEncryptedDataList enc;
            PgpObject obj = pgpObjF.NextPgpObject();
            if (obj is PgpEncryptedDataList)
            {
                enc = (PgpEncryptedDataList)obj;
            }
            else
            {
                enc = (PgpEncryptedDataList)pgpObjF.NextPgpObject();
            }

            PgpPublicKeyEncryptedData pbe = enc.GetEncryptedDataObjects().Cast<PgpPublicKeyEncryptedData>().First();
            PgpPrivateKey privKey = PgpEtlUtil.GetPgpPrivateKeyById(keyData, passPhrase, pbe.KeyId);
            // PgpPrivateKey privKey = PgpEtlUtil.GetPgpPrivateKey(keyData, passPhrase);


            PgpObjectFactory plainFact = new PgpObjectFactory(pbe.GetDataStream(privKey));
            PgpObject message = plainFact.NextPgpObject();

            if (message is PgpCompressedData)
            {
                PgpCompressedData cData = (PgpCompressedData)message;
                Stream compDataIn = cData.GetDataStream();
                PgpObjectFactory o = new PgpObjectFactory(compDataIn);
                message = o.NextPgpObject();
                if (message is PgpOnePassSignatureList)
                {
                    message = o.NextPgpObject();
                }
                this.OutStream = ((PgpLiteralData)message).GetInputStream();
                this.sr = new StreamReader(OutStream);
            }
        }

        public String ReadLine() {
            var line = this.sr.ReadLine(); 
            this.linesRed +=1;
            return line;           
        }

        public Stream GetOutStream() {
            return this.OutStream;
        }

       public List<String> ReadLines(int lineCount) {
            var result = new List<String>();
            for(int i = 0; i < lineCount; i++) {
                if(this.sr.EndOfStream) {break;}
                result.Add(this.sr.ReadLine());
                this.linesRed +=1;
            }
            return result;           
        }

        public List<String> ReadAllLines() {
            var result = new List<String>();
            while (!this.sr.EndOfStream){
                result.Add(this.sr.ReadLine());
                this.linesRed +=1;
            }
            return result;           
        }

        public void Dispose(){
            this.sr.Dispose();
            this.OutStream.Dispose();
            this.InStream.Dispose();
        }
    }
}