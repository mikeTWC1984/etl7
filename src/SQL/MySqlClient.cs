
using System;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;

namespace ETL.SQL
{
    public class MySqlClient : BaseClient
    {
        public Int32 BcpBatchSize { get; set; } = 10000; // only used for NotifyAfter
        public Boolean BcpVerbose { get; set; } = false;

        public MySqlClient(String connectionString) : base(new MySqlConnection(ETL.Util.ResolveString(connectionString)))
        {
            SetClientParameters(
                 dbDriver: "MySql (MySqlConnector)",
                 paramChar: '?',
                 testSql: @"
                     SELECT 'MySql (MySqlConnector)' AS DbDriver, CURRENT_USER() as CURR_USER
                      , DATABASE() AS CURR_DB
                      , @@hostname AS DB_HOST
                      , VERSION() AS DB_VERSION
                      , NOW() AS  CURR_TIME
                    "
            );

        }


        static void c_Notify(object sender, MySqlRowsCopiedEventArgs e)
        {
            Console.WriteLine(String.Format("[{0}] rows copied so far: {1}", DateTime.Now, e.RowsCopied));
        }
        public long BulkCopy(DataTable dataTable, String destTable)
        {

            var bcp = new MySqlBulkCopy(this.GetConnection() as MySqlConnection);
            bcp.NotifyAfter = this.BcpBatchSize;
            bcp.DestinationTableName = destTable;                       
            return bcp.WriteToServer(dataTable).RowsInserted;

        }

        public long BulkCopy(IDataReader reader, String destTable)
        {
            var bcp = new MySqlBulkCopy(this.GetConnection() as MySqlConnection);
            bcp.DestinationTableName = destTable;
            return bcp.WriteToServer(reader).RowsInserted;

        }

        public Task<Int64> BulkCopyAsync(DataTable dataTable, String destTable)
        {
            return Task<Int64>.Run(() => { return this.BulkCopy(dataTable, destTable); });

        }

        public Task<Int64> BulkCopyAsync(IDataReader reader, String destTable)
        {
            return Task<Int64>.Run(() => { return this.BulkCopy(reader, destTable); });

        }
    }
}


