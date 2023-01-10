
using System;
using Npgsql;
using System.Data;

namespace ETL.SQL
{
    public class PgSqlClient : BaseClient
    {
        public PgSqlClient(String connectionString) : base(new NpgsqlConnection(ETL.Util.ResolveString(connectionString)))
        {
            SetClientParameters(
                 dbDriver: "Postgres (Npgsql)",
                 paramChar: '?',
                 testSql: String.Format(@"
                     SELECT 'Postgres (Npgsql)' AS DbDriver
                      , CURRENT_USER as CURR_USER
                      , CURRENT_DATABASE() AS CURR_DB
                      , '{0}' AS DB_HOST
                      , version() AS DB_VERSION
                      , CURRENT_TIMESTAMP AS  CURR_TIME
                    ", (GetConnection() as NpgsqlConnection).Host)

            );

        }


    }


}