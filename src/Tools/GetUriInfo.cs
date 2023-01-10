
using System;
using System.Management.Automation;

namespace ETL.Tools {

[Cmdlet(VerbsCommon.Get, "UriInfo")]
    [OutputType(typeof(Uri))]

    public class GetUriInfo : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public String UriString { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            WriteObject(ETL.Config.GetUriInfo(UriString));

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

