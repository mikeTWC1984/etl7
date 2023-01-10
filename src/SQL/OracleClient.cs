using System;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Collections;

namespace ETL.SQL
{
    public class OracleClient : BaseClient
    {
        public Int32 BcpBatchSize { get; set; } = 10000; // for reader BCP
        public Boolean BcpVerbose {get;set;} = false;
        public OracleClient(String connectionString) : base(new OracleConnection(ETL.Util.ResolveString(connectionString)))
        {
            SetClientParameters(
                 dbDriver: "Oracle (Oracle.ManagedDataAccess.Core)",
                 paramChar: ':',
                 testSql: @"
                     SELECT 'Oracle (Oracle.ManagedDataAccess.Core)' AS DbDriver, USER AS CURR_USER
                      ,(select max(name) from V$database) AS CURR_DB
                      ,(select host_name from v$instance) AS DB_HOST
                      ,(SELECT  max(banner) AS Version FROM v$version) AS DB_VERSION
                      , sysdate as CURR_TIME
                     FROM dual
                    "

            );

        }



        public long BulkCopy(DataTable dataTable, String destTable)
        {
            var paramList = new List<string>();

            for (int i = 1; i <= dataTable.Columns.Count; i++)
            {
                paramList.Add(":" + i);
            };

            String bcpCommand = String.Format("Insert into {0} values ({1})", destTable, String.Join(",", paramList));

            return BulkCopyCommand(dataTable, bcpCommand);
        }

        public long BulkCopy(DataTable dataTable, String destTable, Hashtable mapping)
        {
            if (mapping == null) throw new ArgumentNullException("mapping");

            var cmd = this.GetCommand("") as OracleCommand;
            cmd.ArrayBindCount = dataTable.Rows.Count;

            var paramList = new List<string>();
            var destColumns = new List<String>();

            var i = 1;

            foreach (var key in mapping.Keys)
            {
                var destColumn = mapping[key] as String;
                var srcColumn = key as String;
                destColumns.Add(destColumn);
                paramList.Add(":" + i);
                cmd.Parameters.Add(destColumn, "").DbType = (DbType)Enum.Parse(typeof(DbType), dataTable.Columns[srcColumn].DataType.Name, true);
                cmd.Parameters[destColumn].Value = DtLoader.SelectColumn(dataTable, srcColumn);
                i += 1;
            };

            cmd.CommandText = String.Format("Insert into {0} ({1}) values ({2})"
                      , destTable
                      , String.Join(",", destColumns)
                      , String.Join(",", paramList)
                    );

            return cmd.ExecuteNonQuery();
        }
        public long BulkCopy(IDataReader reader, String destTable)
        {
            var paramList = new List<string>();
            for (int i = 1; i <= reader.FieldCount; i++)
            {
                paramList.Add(":" + i);
            };

            String bcpCommand = String.Format("Insert into {0} values ({1})", destTable, String.Join(",", paramList));

            return BulkCopyCommand(reader, bcpCommand);

        }

        public long BulkCopyCommand(DataTable dataTable, String sqlCommand)
        {
            using (var cmd = this.GetCommand(sqlCommand) as OracleCommand)
            {
                cmd.ArrayBindCount = dataTable.Rows.Count;
                foreach (DataColumn col in dataTable.Columns)
                {
                    cmd.Parameters.Add(col.ColumnName, "").DbType = (DbType)Enum.Parse(typeof(DbType), col.DataType.Name, true);
                    cmd.Parameters[col.ColumnName].Value = DtLoader.SelectColumn(dataTable, col.ColumnName);
                }

                return cmd.ExecuteNonQuery();
            }

        }

        public long BulkCopyCommand(IDataReader reader, String sqlCommand)
        {
            long rowsCopied = 0;

            var dtLoader = new DtLoader(reader);
            while (dtLoader.ReadRows(this.BcpBatchSize) > 0)
            {
                rowsCopied += BulkCopyCommand(dtLoader.dt, sqlCommand);
                if(this.BcpVerbose) {Console.WriteLine(String.Format("[{0}] rows copied so far: {1}", DateTime.Now, rowsCopied));}
            }

            return rowsCopied;

        }

        public long BulkCopy(IDataReader reader, String destTable, Hashtable mapping)
        {
            long rowsCopied = 0;

            var dtLoader = new DtLoader(reader);
            while (dtLoader.ReadRows(this.BcpBatchSize) > 0)
            {
                rowsCopied += BulkCopy(dtLoader.dt, destTable, mapping);
                if(this.BcpVerbose) {Console.WriteLine(String.Format("[{0}] rows copied so far: {1}", DateTime.Now, rowsCopied));}
            }

            return rowsCopied;
        }

    }


}