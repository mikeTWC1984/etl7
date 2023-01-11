
using System.Management.Automation;
using System;
using System.Xml;

namespace ETL.SharePoint
{

    [Cmdlet(VerbsCommon.Get, "SPListItems")]
    [OutputType(typeof(XmlElement))]
    [Alias("spitems")]
    public class GetSpListItems : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public String SiteUrl { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public String List { get; set; }

        [Parameter(Position = 1)]
        public PSCredential Credentials { get; set; }

        [Parameter(Position = 2)]
        public String As { get; set; }

        protected override void BeginProcessing()
        {
            if(this.As != null) {
                this.Credentials = ETL.Config.GetCredential(As);
            }

            if (this.Credentials == null)
            {
                var client = SPFactory.GetListClient(SiteUrl); // will work on windows with default credentials
                WriteObject(client.GetListItems(List, null, null, null, "0", null, null));
            }
            else {
               var client = SPFactory.GetListClient(SiteUrl, Credentials); // should work on linux
               WriteObject(client.GetListItems(List, null, null, null, "0", null, null));             

            }
        }
    }
}