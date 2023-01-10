using System;
using System.IO;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Data;
using System.Management.Automation;
using ETL.File;
using System.Text;
using ETL.File.InputTypes;

namespace ETL.Crypto.Pgp
{

    [Cmdlet(VerbsCommon.Get, "PgpReader")]
    [OutputType(typeof(PgpReader))]
    public class GetPgpReader : PSCmdlet
    {
        /// <summary>
        /// Specify PGP Private key as FilePath (String) , byte array or memorystream. 
        /// </summary>
        /// 

        [Parameter()]
        public StreamData Key;

        /// <summary>
        ///  Specify key as text or ref to env variable
        /// </summary>
        /// 
        [Parameter()]
        public String To;

        [Parameter()]
        public String PassPhrase = "";

        [Parameter(Mandatory = true)]
        public StreamData File;

        protected override void BeginProcessing()
        {
            if (To != null)
            {
                Key = (StreamData)Encoding.UTF8.GetBytes(ETL.Util.ResolveString(To));
            }

            WriteObject(new PgpReader(File.Stream, Key, PassPhrase));
            return;

        }

    }


}