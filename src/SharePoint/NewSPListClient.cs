
using System.Management.Automation;
using ETL.SharePoint.List;
using System;

namespace ETL.SharePoint {

    [Cmdlet(VerbsCommon.New, "SPListClient")]
    [OutputType(typeof(ListsSoapClient))]
    [Alias("newsplist")]
    public class NewSPListClient : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public String SiteUrl { get; set; }

        [Parameter(Position = 1)]
        public PSCredential Credentials { get; set; }

        [Parameter(Position = 2)]
        public String As { get; set; }

        protected override void BeginProcessing()
        {
            if(this.As != null) {
                this.Credentials = ETL.Util.GetCredential(As);
            }

            if (this.Credentials == null)
            {
                WriteObject(SPFactory.GetListClient(SiteUrl)); // will work on windows with default credentials
            }
            else {
                WriteObject(SPFactory.GetListClient(SiteUrl, Credentials)); // should work on linux
            }
        }
    }
}