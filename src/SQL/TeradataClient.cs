
using System;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Collections;
using Teradata.Client.Provider;

namespace ETL.SQL
{
    public class TeradataClient : BaseClient
    {
        public Int32 BcpBatchSize { get; set; } = 10000;
        public Int32 BcpTimeout { get; set; } = 0;
        public Boolean BcpVerbose { get; set; } = false;
        public TeradataClient(String connectionString) : base(new TdConnection(ETL.Util.ResolveString(connectionString)))
        {
            SetClientParameters(
                 dbDriver: "Teradata (Teradata.Client.Provider)",
                 paramChar: '@',
                 testSql: @"
 SELECT 'Teradata (Teradata.Client.Provider)' AS DbDriver
 , CURRENT_USER as CURR_USER
 , DATABASE AS CURRENT_DATABASE
 
 , (select max(ClientTdHostName) from DBC.SessionInfo where SessionNo=Session) as HOST
 , InfoData as DbVersion
 , CURRENT_TIMESTAMP as SERVER_TIME
 FROM DBC.DBCInfoV WHERE InfoKey = 'VERSION'
   "
            );

        }


    }


}