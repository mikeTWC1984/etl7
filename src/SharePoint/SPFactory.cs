
// this is only for PS Core (on Linux min version is 7). On WIndows powershell use new-webproxy instead

using System;
using System.Management.Automation;
using System.Security.Principal;
using System.ServiceModel;
using ETL.SharePoint.List;

namespace ETL.SharePoint
{
    public static class SPFactory
    {

        // This option is windows only to login with current user creds
        public static ListsSoapClient GetListClient(String siteUrl)
        {

            String listUrl = siteUrl.TrimEnd('/').Replace("/SitePages/Home.aspx", "") + "/_vti_bin/Lists.asmx";
            var endPoint = new EndpointAddress(listUrl);

            var result = new BasicHttpBinding();
            result.MaxBufferSize = int.MaxValue;
            result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
            result.MaxReceivedMessageSize = int.MaxValue;
            result.AllowCookies = true;
            result.Security.Mode = BasicHttpSecurityMode.Transport;
            result.TextEncoding = System.Text.Encoding.UTF8;
            result.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
            result.TransferMode = TransferMode.Buffered;

            return new ListsSoapClient(result, endPoint);
        }

        // this option can be used for impersonation (using different AD creds). Can be used on Linux (outside AD domain)
        public static ListsSoapClient GetListClient(String siteUrl, PSCredential credential)
        {
            var client = GetListClient(siteUrl);
            client.ClientCredentials.Windows.ClientCredential = credential.GetNetworkCredential();
            client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            return client;

        }
    }
}