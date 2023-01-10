using System;
using System.IO;
using Org.BouncyCastle.Bcpg;
using ETL.Crypto.Pgp;
using System.Data;
using System.Management.Automation;
using ETL.File;
using System.Text;
using ETL.File.InputTypes;

namespace ETL.Crypto.Pgp
{

    [Cmdlet(VerbsSecurity.Protect, "PgpMessage")]
    // [CmdletBinding(DefaultParameterSetName = "message")]
    // [OutputType(typeof(void), ParameterSetName = new String[] { "pipe" })]
    // [OutputType(typeof(void), ParameterSetName = new String[] { "file" })]
    public class ProtectPgpMessage : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true, Position = 0)] // , ParameterSetName = "pipe"
        public String Message;
        /// <summary>
        /// Specify PGP Public key as FilePath (String) , byte array or memorystream. 
        /// </summary>
        /// 

        [Parameter()] // ParameterSetName = "file"
        public StreamData Input;

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
        public SwitchParameter ToStdout;

        [Parameter()]
        public CompressionAlgorithmTag Compression = CompressionAlgorithmTag.Zip;
        [Parameter()]
        public SymmetricKeyAlgorithmTag Encryption = SymmetricKeyAlgorithmTag.Aes256;


        [Parameter()]
        public Stream OutStream;

        [Parameter()]
        public String OutFile;

        private PgpWriter pw;

        private Boolean _print = false;
        private MemoryStream ms;

        protected override void BeginProcessing()
        {
            if (To != null) // specify public key as a string or ref to env variable
            {
                PublicKey = (StreamData)Encoding.UTF8.GetBytes(ETL.Util.ResolveString(To));
            }

            if (OutFile != null)
            {
                OutStream = System.IO.File.OpenWrite(OutFile);
            }

            if (OutStream is null) // output goes to:
            {
                if (ToStdout.IsPresent) // to stdout
                {
                    OutStream = Console.OpenStandardOutput();
                }
                else // to success stream (need to buffer). This is default behavior if no outfile set
                {
                    this._print = true;
                    this.ms = new MemoryStream();
                    OutStream = this.ms;
                }

            }



            /// case 1 (preferred) - data comes from a file/stream. Perform stream-to-stream encryption
            if (Input != null)
            {
                PgpEtlUtil.EncryptStream(Input.Stream, OutStream, PublicKey, Armor.IsPresent, Buffer, Compression, Encryption);
                //InFile.Stream.CopyTo(pw.GetInStream());
            }
            else  // case 2 - data comes from pipeline, set up PgPWriter
            {
                
                this.pw = new PgpWriter(OutStream, PublicKey, Buffer, Encryption, Compression, Armor.IsPresent);
            }

        }

        protected override void ProcessRecord()
        {   // ignore pipeline data if Input is specified            
            if (this.pw != null) this.pw.WriteLine(Message);
        }

        protected override void EndProcessing()
        {
            //  base.EndProcessing();
            if (Input is null) this.pw.Dispose();
            // if writing to "success stream":
            if (this._print) WriteObject(Encoding.Default.GetString(this.ms.ToArray()));
        }

    }


}