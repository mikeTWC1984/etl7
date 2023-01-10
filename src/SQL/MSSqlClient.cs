
using System;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Reflection;
using System.Collections;

namespace ETL.SQL
{
    public class MSSqlClient : BaseClient
    {
        public Int32 BcpBatchSize { get; set; } = 10000;
        public Int32 BcpTimeout { get; set; } = 0;
        public Boolean BcpVerbose { get; set; } = false;
        public MSSqlClient(String connectionString) : base(new SqlConnection(ETL.Util.ResolveString(connectionString)))
        {
            SetClientParameters(
                 dbDriver: "MSSQL (System.Data.SqlClient)",
                 paramChar: '@',
                 testSql: @"
                     SELECT 'MSSQL (System.Data.SqlClient)' AS DbDriver, CURRENT_USER as CURR_USER
                        , DB_NAME() AS CURRENT_DATABASE
                        , @@SERVERNAME AS HOST
                        , @@VERSION AS DbVersion
                        , GETDATE() AS  [CURRENT_TIME]
                     "
            );

        }


        public SqlBulkCopy GetBulkCopy(String destTable)
        {
            SqlBulkCopy bcp;

            if (this.HasOpenTransaction)
            {
                var tran = this.GetTransaction() as SqlTransaction;
                bcp = new SqlBulkCopy(this.GetConnection() as SqlConnection, SqlBulkCopyOptions.Default, tran);
            }
            else
            {
                bcp = new SqlBulkCopy(this.GetConnection() as SqlConnection);
            }

            bcp.BatchSize = this.BcpBatchSize;
            bcp.BulkCopyTimeout = this.BcpTimeout;
            bcp.DestinationTableName = destTable;
            if (this.BcpVerbose)
            {
                bcp.NotifyAfter = this.BcpBatchSize;
                bcp.SqlRowsCopied += c_Notify;
            }
            return bcp;
        }

        static void c_Notify(object sender, SqlRowsCopiedEventArgs e)
        {
            Console.WriteLine(String.Format("[{0}] rows copied so far: {1}", DateTime.Now, e.RowsCopied));
        }

        public long BulkCopy(IDataReader reader, String destTable)
        {
            using (var bcp = GetBulkCopy(destTable))
            {
                bcp.WriteToServer(reader);
                return Convert.ToInt64(bcp.GetType().GetField("_rowsCopied", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bcp));
            }
        }

        public long BulkCopy(DataTable dataTable, String destTable, Hashtable mapping = null)
        {
            using (var bcp = GetBulkCopy(destTable))
            {
                if (mapping != null)
                {
                    foreach (var key in mapping.Keys)
                    {
                        bcp.ColumnMappings.Add(key as String, mapping[key] as String);
                    }
                }
                bcp.WriteToServer(dataTable);
                return dataTable.Rows.Count;
            }
        }

        public long BulkCopy(DataRow[] rows, String destTable)
        {
            using (var bcp = GetBulkCopy(destTable))
            {
                bcp.WriteToServer(rows);
                return rows.Length;
            }
        }
    }


}