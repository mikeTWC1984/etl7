
using System.Management.Automation;
using System;
using ETL.SSH;
using System.Net;
using SharpCifs.Smb;

namespace ETL.File
{

    [Cmdlet(VerbsCommon.New, "S3Client")]
    [OutputType(typeof(SshFactory.S3Client))]
    [Alias("news3")]
    public class NewS3Client : PSCmdlet
    {

        [Parameter(Position = 0)]
        public String Uri;

        [Parameter(ValueFromPipelineByPropertyName = true)]
        [Alias("Host")]
        public String BaseUrl;

        [Parameter(ValueFromPipelineByPropertyName = true)]
        [Alias("UserName")]
        public String AccessKey;

        [Parameter(ValueFromPipelineByPropertyName = true)]
        [Alias("Password")]
        public String SecretKey;

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public String Region = "us-east-1";


        protected override void BeginProcessing()
        {
            if (!String.IsNullOrEmpty(Uri))
            {
                WriteObject(ETL.SSH.SshFactory.GetS3Client(Uri, Region));
            }
            else
            {
                WriteObject(ETL.SSH.SshFactory.GetS3Client(BaseUrl, AccessKey, SecretKey, Region));
            }

        }
    }

    /// FTP CLIENT
    [Cmdlet(VerbsCommon.New, "FtpClient")]
    [OutputType(typeof(FluentFTP.FtpClient))]
    [Alias("newftp")]
    public class NewFtpClient : PSCmdlet
    {

        [Parameter(Position = 0)]
        public String Uri;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [Alias("Host")]
        public String Server;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int Port = 21;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public String UserName;
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public String Password;
        [Parameter()]
        public String As;

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

            var client = ETL.SSH.SshFactory.GetFtpClient(Server, Port, UserName, Password);
            client.Connect();
            WriteObject(client);

        }
    }

    /// SMB FILE

    [Cmdlet(VerbsCommon.Get, "SmbFile")]
    [OutputType(typeof(SmbFile))]
    public class GetSmbFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public String UncPath;
        [Parameter()]
        public PSCredential Credential;

        [Parameter()]
        public String As;

        protected override void BeginProcessing()
        {
            if (!UncPath.StartsWith("smb://")) UncPath = "smb://" + UncPath.TrimStart('\\').TrimStart('\\').Replace("\\", "/");
            NetworkCredential creds = null;
            if (Credential != null) creds = Credential.GetNetworkCredential();
            if (!String.IsNullOrEmpty(As)) creds = ETL.Util.GetNetworkCredential(As);

            if (creds is null)
            {
                // if no cred - should follow this format: smb://domain;user:password@path
                WriteObject(new SmbFile(UncPath));
            }
            else
            {
                var auth = new NtlmPasswordAuthentication(creds.Domain, creds.UserName, creds.Password);
                WriteObject(new SmbFile(UncPath, auth));
            }
        }

    }

}