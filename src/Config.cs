

using System;
using System.Data;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using ETL.File.InputTypes;
using System.Net;

namespace ETL
{
    public static class Config
    {
        public static X509Certificate2 ETL_CERT { get; set; }
        public static String ETL_SMTP { get; set; }
        public static String ETL_FROM { get; set; }
        public static String ETL_TO { get; set; }
        public static String ETL_HOME { get; set; }
        public static String DOMAIN { get; set; }
        public static String LOGONSERVER { get; set; }
        public static DataSet ds { get; set; } = GetDefaultDataSet();

        public static X509Certificate2Collection GetCertificateById(X509FindType idType, Object id)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates.Find(idType, id, validOnly: false);

            }
        }

        public static X509Certificate2 GetCertificateByThumbPrint(String thumbPrint)
        {
            foreach (var cert in GetCertificateById(X509FindType.FindByThumbprint, thumbPrint))
            {
                return cert;
            }

            return null;

        }

        public static X509Certificate2Collection GetCertificateBySubject(String subject)
        {
            return GetCertificateById(X509FindType.FindBySubjectName, subject);
        }

        // ---------------------------------------
        private static DataSet GetDefaultDataSet()
        {
            var ds = new DataSet("Secret");

            foreach (String t in "String,Blob,Credential,Key".Split(','))
            {
                var dt = new DataTable(t);
                dt.PrimaryKey = new DataColumn[] { dt.Columns.Add("Id") };
                switch (t)
                {
                    case "String": dt.Columns.Add("String").AllowDBNull = false; break;
                    case "Blob": dt.Columns.Add("Blob", typeof(Byte[])).AllowDBNull = false; break;
                    case "Credential":
                        dt.Columns.Add("Username").AllowDBNull = false;
                        dt.Columns.Add("Password").AllowDBNull = false;
                        break;
                    case "Key":
                        dt.Columns.Add("Key", typeof(Byte[])).AllowDBNull = false;
                        dt.Columns.Add("Passphrase");
                        dt.Columns.Add("IsPrivate", typeof(Boolean)).DefaultValue = false;
                        break;
                }

                dt.Columns.Add("MEMO");
            }

            return ds;

        }

        ///  ------------- getters/setters 

        public static void AddString(String id, String str, String memo = "")
        {
            ds.Tables["String"].Rows.Add(id, str, memo);
        }

        public static String GetString(String id)
        {
            String result = null;
            try
            {
                result = (String)ds.Tables["String"].Rows.Find(id)["String"];
            }
            catch
            {    // just return null
                //throw new Exception("Table (String), primary key (id) or column (String) does not exist");
            }
            return result;
        }

        public static UriBuilder GetUriInfo(String uriString)
        {
            UriBuilder result = null;
            try
            {
                result = new UriBuilder(ETL.Util.ResolveString(uriString));
                result.Password = Uri.UnescapeDataString(result.Password);
                result.UserName = Uri.UnescapeDataString(result.UserName);
            }
            catch { }
            return result;
        }

        public static Object GetBlob(String id)
        {
            Byte[] result = null;
            try
            {
                result = (Byte[])ds.Tables["Blob"].Rows.Find(id)["Blob"];
            }
            catch
            {    // just return null
                //throw new Exception("Table (String), primary key (id) or column (String) does not exist");
            }
            return result;
        }

        public static void AddBlob(String id, Byte[] blob, String memo = "")
        {
            ds.Tables["Blob"].Rows.Add(id, blob, memo);
        }

        public static PSCredential GetCredential(String id)
        {
            if (String.IsNullOrWhiteSpace(id)) { return null; }

            PSCredential result = null;

            try
            {
                NetworkCredential nc = null;
                var dr = ds.Tables["Credential"].Rows.Find(id);

                if (dr != null)
                { // check credential store
                    nc = new System.Net.NetworkCredential((String)dr["UserName"], (String)dr["Password"]);
                    result = new PSCredential(nc.UserName, nc.SecurePassword);
                }
                else
                { // check string store / env variables or string itself
                    var credStr = ETL.Util.ResolveString(id);
                    var userCheck = credStr.IndexOf(':');
                    if (userCheck > -1)
                    {
                        nc = new System.Net.NetworkCredential(credStr.Substring(0, userCheck), credStr.Substring(userCheck + 1));
                        result = new PSCredential(nc.UserName, nc.SecurePassword);
                    }
                }

            }
            catch
            {
                // return null on error
            }

            return result;

        }

        public static NetworkCredential GetNetworkCredential(string credAlias)
        {
            NetworkCredential netCred = null;
            var psCred = GetCredential(credAlias);
            if (psCred != null) { netCred = psCred.GetNetworkCredential(); }
            return netCred;
        }

        public static NetworkCredential GetNetworkCredential(PSCredential cred)
        {
            return cred == null ? null : cred.GetNetworkCredential();
        }

        public static void AddCredential(String id, PSCredential cred, String memo = "")
        {
            var dr = ds.Tables["Credential"].NewRow();
            dr["id"] = id;
            dr["UserName"] = cred.UserName;
            dr["Password"] = cred.GetNetworkCredential().Password;
            dr["MEMO"] = memo;
            ds.Tables["Credential"].Rows.Add(dr);
        }

        public static EtlKey GetKey(String id)
        {
            EtlKey result = null;

            var dr = ds.Tables["KEY"].Rows.Find(id);

            if (dr == null) { return null; }

            try
            {
                var keyType = Util.ParseEnum<EtlKeyType>(dr["TYPE"] as String);
                result = new EtlKey(dr["PUBLIC"] as Byte[], dr["PRIVATE"] as Byte[], dr["PASSPHRASE"] as String, keyType);
            }
            catch
            {
                //Console.WriteLine(e.InnerException);
            }

            return result;
        }
        public static void AddKey(String id, EtlKey key, String memo = "")
        {
            ds.Tables["Key"].Rows.Add(id, key.PubKey, key.PrivateKey, key.Passphrase, key.KeyType, memo);
        }

        public static Object GetObject(String tableName, String id, String propertyName)
        {
            Object result = null;
            try
            {
                result = ds.Tables[tableName].Rows.Find(id)[propertyName];
            }
            catch
            {
                // return null
            }

            return Convert.IsDBNull(result) ? null : result;
        }

        public static PSObject GetObject(String tableName, String id)
        {
            PSObject result = new PSObject();
            try
            {
                var dr = ds.Tables[tableName].Rows.Find(id);
                if (dr != null)
                {
                    foreach (DataColumn col in dr.Table.Columns)
                    {
                        Object val = Convert.IsDBNull(dr[col.ColumnName]) ? null : dr[col.ColumnName];
                        result.Properties.Add(new PSNoteProperty(col.ColumnName, val));
                    }
                }

            }
            catch
            {
                // return null
            }

            return result;
        }

        public static void SetObject(String tableName, PSObject obj)
        {
            var dt = ds.Tables[tableName];
            if (dt != null)
            {
                var dr = dt.NewRow();
                foreach (var p in obj.Properties)
                {
                    dr[p.Name] = p.Value;
                }

                dt.Rows.Add(dr);
            }
        }

        public static void ListConfig(String configType = null)
        {
            foreach (DataTable dt in ds.Tables)
            {
                if (configType != null & dt.TableName.ToUpperInvariant() != configType.ToUpperInvariant()) { continue; }
                Console.WriteLine("----" + dt.TableName);
                foreach (DataRow dr in dt.Rows)
                {
                    Console.WriteLine(String.Format("     > {0} ({1})", dr["id"], dr["MEMO"]));
                }
            }
        }

    }

    public enum EtlKeyType { PGP, PEM, PFX, SSH, OTHER }
    public class EtlKey
    {
        public Byte[] PubKey;
        public Byte[] PrivateKey;
        public String Passphrase;
        public EtlKeyType KeyType;
        public EtlKey(Byte[] pubKey, Byte[] privateKey, String pass, EtlKeyType type)
        {
            this.PubKey = pubKey; this.PrivateKey = privateKey; this.Passphrase = pass; this.KeyType = type;
        }
        public EtlKey(Byte[] pubKey, EtlKeyType type)
        {
            this.PubKey = pubKey; this.KeyType = type;
        }

    }

}