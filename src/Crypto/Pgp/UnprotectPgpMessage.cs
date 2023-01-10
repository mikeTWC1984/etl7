using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using ETL.File.InputTypes;

namespace ETL.Crypto.Pgp
{

    [Cmdlet(VerbsSecurity.Unprotect, "PgpMessage")]
    [OutputType(typeof(void), ParameterSetName = new String[] { "pipe" })]
    [OutputType(typeof(void), ParameterSetName = new String[] { "file" })]

    public class UnprotectPgpMessage : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "pipe")]
        public String Message;

        [Parameter(ParameterSetName = "file")]
        public StreamData Input;

        /// <summary>
        /// Specify PGP Private key as FilePath (String) , byte array or memorystream. 
        /// </summary>
        /// 

        [Parameter()]
        public StreamData PrivateKey;

        /// <summary>
        ///  Specify key as text or ref to env variable
        /// </summary>
        /// 
        [Parameter()]
        public String To;

        [Parameter()]
        public String PassPhrase = "";

        [Parameter()]
        public Stream OutStream;

        [Parameter()]
        public String OutFile;

        [Parameter()]
        public SwitchParameter ToStdout;

        private StringBuilder sb;
        private Boolean _print = false;
        private MemoryStream ms;

        protected override void BeginProcessing()
        {
            if (To != null)
            {
                PrivateKey = (StreamData)Encoding.Default.GetBytes(ETL.Util.ResolveString(To));
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

            // case 1 (preferred) - encrypted data comes from the file/stream

            if (Input != null)
            {


            }
            else // case 2 - encrypted data comes from pipeline, prepare string builder
            {
                this.sb = new StringBuilder();
            }

            return;

        }

        protected override void ProcessRecord()
        {
            if (this.sb != null) sb.Append(Message);
        }

        protected override void EndProcessing()
        {
            Stream inputStream = null;

            if (Input is null)
            {
                inputStream = new MemoryStream(Encoding.Default.GetBytes(sb.ToString()));
            }
            else
            {
                inputStream = Input.Stream;
            }

            PgpEtlUtil.DecryptStream(inputStream, OutStream, PrivateKey, PassPhrase);

            if (this._print) WriteObject(Encoding.Default.GetString(this.ms.ToArray()));

        }

    }

}