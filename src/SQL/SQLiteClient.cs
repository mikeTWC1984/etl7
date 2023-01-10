
using System;
using System.Data.SQLite;

namespace ETL.SQL
{
    public class SqliteClient : BaseClient
    {
        public SqliteClient(String connectionString) : base( new SQLiteConnection(connectionString))
        {
            // Data Source=:memory:;Version=3;New=True;
            // Data Source=c:\mydb.db;Version=3;UseUTF16Encoding=True;
            SetClientParameters(
                 dbDriver: "SQLite (System.Data.Sqlite.Core)",
                 paramChar: '@',
                 testSql: String.Format(@"
                     SELECT 'SQLite (System.Data.Sqlite.Core)' AS DbDriver
                       ,'{0}' AS CURR_USER
                       ,'{1}' AS CURR_DB
                       ,'localhost' AS DB_HOST
                       ,sqlite_version() AS DB_VERSION
                       ,datetime('now','localtime') as CURR_TIME
                     ", Environment.UserName, connectionString)

            );

        }
    }


}