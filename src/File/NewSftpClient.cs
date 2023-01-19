
using System.Management.Automation;
using System;
using Renci.SshNet;

namespace ETL.File
{

    [Cmdlet(VerbsCommon.New, "SftpClient")]
    [OutputType(typeof(SftpClient))]
    [Alias("newsftp")]
    public class NewSftpClient : PSCmdlet
    {

        [Parameter(Position = 0)]
        public String Uri;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [Alias("Host")]
        public String Server;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int Port = 22;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public String UserName;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public String Password;
        [Parameter()]
        public String As;

        [Parameter()]
        public SwitchParameter Scp; // file copy
        [Parameter()]
        public SwitchParameter Ssh; // shell emulator


        protected override void BeginProcessing()
        {
            if (!String.IsNullOrEmpty(Uri))
            {
                var uriInfo = ETL.Util.GetUriInfo(Uri);
                Server = uriInfo.Host;
                UserName = uriInfo.UserName;
                Password = uriInfo.Password;
                if (uriInfo.Port > 0) Port = uriInfo.Port;
            }

            if (!String.IsNullOrEmpty(As))
            {
                var cred = ETL.Util.GetNetworkCredential(As);
                if (cred != null)
                {
                    UserName = cred.UserName;
                    Password = cred.Password;
                }
            }

            BaseClient client;
            if (Scp.IsPresent) client = ETL.SSH.SshFactory.GetScpClient(Server, Port, UserName, Password);
            else if (Ssh.IsPresent) client = ETL.SSH.SshFactory.GetSshClient(Server, Port, UserName, Password);
            else client = ETL.SSH.SshFactory.GetSftpClient(Server, Port, UserName, Password);

            client.Connect();
            WriteObject(client);

        }
    }
}