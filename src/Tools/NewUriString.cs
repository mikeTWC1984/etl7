
using System;
using System.Management.Automation;
using System.IO;


namespace ETL.Tools {

[Cmdlet(VerbsCommon.New, "UriString")]
    [CmdletBinding(DefaultParameterSetName="file")]
    [OutputType(typeof(String))]

    public class NewUriString : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true)]
        [Alias("Host")]
        public String HostName { get; set; }

        [Parameter()]
        public int Port { get; set; }

        [Parameter()]
        public String Protocol { get; set; } = "http";

        [Parameter()]
        public String Username { get; set; }

        [Parameter()]
        public String Password { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            var sw = new StringWriter();
            sw.Write($"{Protocol}://");

            if (!String.IsNullOrEmpty(Username))
            {
                Username = Uri.EscapeDataString(Username);
                sw.Write(Username + ":");
            }
            if(!String.IsNullOrEmpty(Password))
            {
                if(String.IsNullOrEmpty(Username)) throw new MissingFieldException("Username is requeired");
                Password = Uri.EscapeDataString(Password);
                sw.Write(Password + '@');
            }

            sw.Write(HostName);
            if(Port > 0) sw.Write(":" + Port);

            WriteObject(sw.ToString());

        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }

        protected override void EndProcessing()

        {
            base.EndProcessing();

        }

    }

}

