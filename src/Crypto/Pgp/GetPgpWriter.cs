using System;
using System.IO;
using Org.BouncyCastle.Bcpg;
using System.Data;
using System.Management.Automation;
using ETL.File;
using System.Text;
using ETL.File.InputTypes;

namespace ETL.Crypto.Pgp
{

    [Cmdlet(VerbsCommon.Get, "PgpWriter")]
    [OutputType(typeof(PgpReader))]
    public class GetPgpWriter : PSCmdlet
    {
        /// <summary>
        /// Specify PGP Public key as FilePath (String) , byte array or memorystream. 
        /// </summary>
        /// 

        [Parameter()]
        public StreamData PublicKey;

        /// <summary>
        ///  Specify public key as text or ref to env variable
        /// </summary>
        /// 
        [Parameter()]
        public String To;

        [Parameter()]
        public Int32 Buffer = 32768;

        [Parameter()]
        public SwitchParameter Armor;

        [Parameter()]
        public CompressionAlgorithmTag Compression = CompressionAlgorithmTag.Zip;
        [Parameter()]
        public SymmetricKeyAlgorithmTag Encryption = SymmetricKeyAlgorithmTag.Aes256;


        [Parameter()]
        public Stream OutStream;

        [Parameter()]
        public String OutFile;
        protected override void BeginProcessing()
        {
            if (To != null)
            {
                PublicKey = (StreamData)Encoding.UTF8.GetBytes(ETL.Util.ResolveString(To));
            }

            if (OutFile != null) {
                OutStream = System.IO.File.OpenWrite(OutFile);
            }

            WriteObject(new PgpWriter(OutStream, PublicKey, Buffer, Encryption, Compression, Armor.IsPresent));
            return;

        }

    }


}