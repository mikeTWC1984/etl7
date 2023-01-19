
using System;
using System.IO;
using Renci.SshNet;
using FluentFTP;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using ETL.File;


namespace ETL.SSH

{

    public static class SshFactory
    {
        public static SftpClient GetSftpClient(String host, int? port, String userName, String password)
        {
            return new SftpClient(host, port ?? 22, userName, password);
        }

        public static SftpClient GetSftpClient(String uri)
        {
            var uriInfo = ETL.Util.GetUriInfo(uri);
            if(uriInfo == null) throw new Exception("Invalid URI string or alias");
            if(uriInfo.Port < 0) uriInfo.Port = 22;
            return new SftpClient(uriInfo.Host, uriInfo.Port, uriInfo.UserName, uriInfo.Password);
        }

        public static SftpClient GetSftpClient(String host, String userName, String password)
        {
            return new SftpClient(host, userName, password);
        }
        // as of March 2020 SSHNET only accept RSA key files in PEM format ( ssh-keygen -a 100 -t rsa -f ~/.ssh/id_rsa2 -m pem )
        public static SftpClient GetSftpClient(String host, int? port, String userName, Byte[] privateKey, String passphrase)
        {
            var ms = new MemoryStream(privateKey);
            return new SftpClient(host, port ?? 22, userName, new PrivateKeyFile(ms, passphrase));
        }

        public static ScpClient GetScpClient(String host, int? port, String userName, String password)
        {
            return new ScpClient(host, port ?? 22, userName, password);
        }

        public static ScpClient GetScpClient(String uri)
        {
            var uriInfo = ETL.Util.GetUriInfo(uri);
            if(uriInfo == null) throw new Exception("Invalid URI string or alias");
            if(uriInfo.Port < 0) uriInfo.Port = 22;
            return new ScpClient(uriInfo.Host, uriInfo.Port, uriInfo.UserName, uriInfo.Password);
        }

        public static ScpClient GetScpClient(String host, String userName, String password)
        {
            return new ScpClient(host, userName, password);
        }

        public static ScpClient GetScpClient(String host, int? port, String userName, Byte[] privateKey, String passphrase)
        {
            var ms = new MemoryStream(privateKey);
            return new ScpClient(host, port ?? 22, userName, new PrivateKeyFile(ms, passphrase));
        }

        public static SshClient GetSshClient(String host, Int32 port, String userName, String password)
        {
            return new SshClient(host, port, userName, password);
        }

        public static SshClient GetSshClient(String uri)
        {
            var uriInfo = ETL.Util.GetUriInfo(uri);
            if(uriInfo == null) throw new Exception("Invalid URI string or alias");
            if(uriInfo.Port < 0) uriInfo.Port = 22;
            return new SshClient(uriInfo.Host, uriInfo.Port, uriInfo.UserName, uriInfo.Password);
        }

        public static SshClient GetSshClient(String host, String userName, String password)
        {
            return new SshClient(host, userName, password);
        }

        public static SshClient GetSshClient(String host, Int32 port, String userName, Byte[] privateKey, String passphrase)
        {
            var ms = new MemoryStream(privateKey);
            return new SshClient(host, port, userName, new PrivateKeyFile(ms, passphrase));
        }

        public static FtpClient GetFtpClient(String host, int? port, String userName, String password)
        {
            var conf = new FtpConfig();
            return new FtpClient(host, userName, password, port ?? 21);
        }

        public static FtpClient GetFtpClient(String uri)
        {
            var uriInfo = ETL.Util.GetUriInfo(uri);
            if(uriInfo == null) throw new Exception("Invalid URI string or alias");
            if(uriInfo.Port < 0) uriInfo.Port = 21;
            return new FtpClient(uriInfo.Host, uriInfo.UserName, uriInfo.Password, uriInfo.Port);
        }
        public static FtpClient GetFtpClient(String host, String userName, String password)
        {
            return new FtpClient(host, userName, password);
        }

        public static S3Client GetS3Client(String url, String accessKey, String secretKey, String region = "us-east-1")
        {
            return new S3Client(url, accessKey, secretKey, region);
        }

        public static S3Client GetS3Client(String uri, String region = "us-east-1")
        {  // assumes uri is http[s]://accessKey:secretKey@hostname:port"
            var uriInfo = ETL.Util.GetUriInfo(uri);
            if(uriInfo == null) throw new Exception("Invalid URI string or alias");
            return new S3Client(uriInfo.Uri.AbsoluteUri, uriInfo.UserName, uriInfo.Password, region);
        }

        public class S3Client : AmazonS3Client
        {
            public S3Client(
                String url,
                String accessKey,
                String secretKey,
                String region =  "us-east-1"
                ) : base(accessKey, secretKey, new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(region),
                    ServiceURL = url,
                    ForcePathStyle = true
                })
            { }
            public ListBucketsResponse ListBuckets()
            {
                return this.ListBucketsAsync().GetAwaiter().GetResult();
            }
            public ListObjectsResponse ListObjects(string bucketName)
            {
                return this.ListObjectsAsync(bucketName).GetAwaiter().GetResult();
            }

            public ListObjectsResponse ListObjects(string bucketName, string prefix)
            {
                return this.ListObjectsAsync(bucketName, prefix).GetAwaiter().GetResult();
            }

            public Stream GetObjectStream(string bucketName, string objectKey)
            {
                return this.GetObjectAsync(bucketName, objectKey).GetAwaiter().GetResult().ResponseStream;
            }

            public Stream GetObjectSeekableStream(string bucketName, string objectKey, long page = 26214400, int maxpages = 20) {
                return new SeekableS3Stream(this, bucketName, objectKey, page, maxpages);
            }

            public PutObjectResponse PutObject(string bucketName, string key, Stream stream, bool autoclose = true)
            {
                return this.PutObjectAsync(new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = stream,
                    AutoCloseStream = autoclose

                }).GetAwaiter().GetResult();

            }

             public PutObjectResponse PutText(string bucketName, string key, String text)
            {
                return this.PutObjectAsync(new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = key,
                    ContentBody = text
                }).GetAwaiter().GetResult();

            }

            public PutObjectResponse PutObject(string bucketName, string key, string filePath)
            {
                return this.PutObjectAsync(new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = key,
                    FilePath = filePath
                }).GetAwaiter().GetResult();
            }

            public StreamReader GetObject(string bucketName, string key){
                return new StreamReader(this.GetObjectAsync(bucketName, key).GetAwaiter().GetResult().ResponseStream);
            }
            public System.IO.FileInfo GetObject(string bucketName, string key, string outFile){
                using(var str = System.IO.File.Create(outFile))
                using(var s3 = this.GetObjectAsync(bucketName, key).GetAwaiter().GetResult().ResponseStream)
                {
                    s3.CopyTo(str);
                }

                return new System.IO.FileInfo(outFile);
            }

        }

    }
}






