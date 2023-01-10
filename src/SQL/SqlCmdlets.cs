
using System.Management.Automation;
using System.Data.Common;
using System;
using ETL.File;

namespace ETL.SQL
{
    [Cmdlet(VerbsCommon.New, "BaseClient")]
    [OutputType(typeof(BaseClient))]
    [Alias("newbase")]
    public class NewBaseClient : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public DbConnection Connection { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new BaseClient(Connection));
        }
    }

    // ------- mssql

    [Cmdlet(VerbsCommon.New, "MSSqlClient")]
    [OutputType(typeof(BaseClient))]
    [Alias("newsql")]
    public class NewMSSqlClient : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public String ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new MSSqlClient(ConnectionString));
        }
    }

    // ------- oracle

    [Cmdlet(VerbsCommon.New, "OracleClient")]
    [OutputType(typeof(BaseClient))]
    [Alias("newora")]
    public class NewOracleClient : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public String ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new OracleClient(ConnectionString));
        }
    }

    // ------- MySQL

    [Cmdlet(VerbsCommon.New, "MySqlClient")]
    [OutputType(typeof(BaseClient))]
    [Alias("newmysql")]
    public class NewMySqlClient : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public String ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new MySqlClient(ConnectionString));
        }
    }

    // ------- Postgres

    [Cmdlet(VerbsCommon.New, "PgSqlClient")]
    [OutputType(typeof(BaseClient))]
    [Alias("newpg")]
    public class NewPgSqlClient : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public String ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new PgSqlClient(ConnectionString));
        }
    }

    // ------- Tera

    [Cmdlet(VerbsCommon.New, "TeradataClient")]
    [OutputType(typeof(BaseClient))]
    [Alias("newtera")]
    public class NewTeradataClient : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public String ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new TeradataClient(ConnectionString));
        }
    }

    // ------- Sqlite

    [Cmdlet(VerbsCommon.New, "SqliteClient")]
    [OutputType(typeof(BaseClient))]
    [Alias("newsqlite")]
    public class NewSqliteClient : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = false)]
        public String File { get; set; }

        [Parameter(Position = 1, Mandatory = false)]
        public String ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            if(ConnectionString is null) ConnectionString = "Data Source=" + (File ?? ":memory:");
            WriteObject(new SqliteClient(ConnectionString));
        }
    }

    // ------- Odbc

    [Cmdlet(VerbsCommon.New, "OdbcClient")]
    [OutputType(typeof(BaseClient))]
    [Alias("newodbc")]
    public class NewOdbcClient : PSCmdlet
    {

        [Parameter(Position = 0, Mandatory = true)]
        public String ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new OdbcClient(ConnectionString));
        }
    }

}