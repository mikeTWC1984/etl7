using System;
using System.Data.Odbc;

namespace ETL.SQL
{
    public class OdbcClient : BaseClient
    {
        public OdbcClient(String connectionString) : base(new OdbcConnection(ETL.Util.ResolveString(connectionString)))
        {
            SetClientParameters("Odbc (System.Data.Odbc)", '?', null);
        }
    }
}
