
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;


namespace ETL.SQL
{
    public class BaseClient : IDisposable
    {

        public String DbDriver = "Generic";
        public Char ParamChar = '@';
        public Int32 PreviewLimit { get; set; } = 50;
        private Int16 _tabCounter = 0;

        public Int32 timeout { get; set; } = 0;

        public Boolean HasOpenTransaction { get; private set; } = false;
        private DbTransaction _transaction;

        private DbConnection Connection;

        private String _testSql { get; set; }

        public BaseClient(DbConnection connection)
        {
            if (connection.State.ToString() != "Open")
            {
                try
                {
                    connection.Open();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(e.Message);
                }
            }

            this.Connection = connection;
        }

        public BaseClient(DbConnection connection, String dbDriver, Char paramChar) : this(connection)
        {
            this.DbDriver = dbDriver;
            this.ParamChar = paramChar;
        }

        public void SetClientParameters(String dbDriver, Char paramChar, String testSql)
        {
            this.DbDriver = dbDriver;
            this.ParamChar = paramChar;
            this._testSql = testSql;
        }

        public DbCommand GetCommand(String command)
        {
            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = command;
            cmd.CommandTimeout = timeout;
            if (this.HasOpenTransaction)
            {
                cmd.Transaction = _transaction;
            }
            return cmd;
        }

        public void BeginTransaction()
        {
            if (!this.HasOpenTransaction)
            {
                this._transaction = this.Connection.BeginTransaction();
                this.HasOpenTransaction = true;
            }
            else { throw new Exception("There is already open tansaction. Invoke either Commit or Rollback method"); }
        }
        public void Commit()
        {
            if (this.HasOpenTransaction)
            {
                this._transaction.Commit();
                this.HasOpenTransaction = false;
            }
            else { throw new Exception("There is no open transaction. Invoke BeginTransaction() first"); }
        }
        public void Rollback()
        {
            if (this.HasOpenTransaction)
            {
                this._transaction.Rollback();
                this.HasOpenTransaction = false;
            }
            else { throw new Exception("There is no open transaction. Invoke BeginTransaction() first"); }
        }
        public DbTransaction GetTransaction() {
            return this._transaction;
        }

        public IDataReader ExecuteReader(String query)
        {
            return this.GetCommand(query).ExecuteReader();
        }
        // public IDataReader ExecuteReader(String query, DbParameter param)
        // {
        //     var cmd = this.GetCommand(query);
        //     cmd.Parameters.Add(param);
        //     return cmd.ExecuteReader();
        // }

        public Task<IDataReader> ExecuteReaderAsync(String query)
        {
           return Task<IDataReader>.Run(() => { return this.ExecuteReader(query); });
        }

        public DataTable Query(String query)
        {
            var r = this.ExecuteReader(query);
            _tabCounter += 1;
            var dt = new DataTable("Table" + _tabCounter);
            dt.Load(r);
            return dt;
        }

        public DataTable Query(DbCommand cmd)
        {
            var r = cmd.ExecuteReader();
            _tabCounter += 1;
            var dt = new DataTable("Table" + _tabCounter);
            dt.Load(r);
            return dt;
        }

        public void Query(String query, String filePath)
        {
            var r = this.ExecuteReader(query);
            ETL.SQL.SqlUtil.WriteReaderToFile(r, filePath);
        }

        public void Query(String query, StreamWriter stream)
        {
            var r = this.ExecuteReader(query);
            ETL.SQL.SqlUtil.WriteReaderToStream(r, stream);
        }

        public Task<DataTable> QueryAsync(String query)
        {
            var t = Task<DataTable>.Run(() =>
            {
                return this.Query(query);
            });
            return t;
        }

        public Task<DataTable> QueryAsync(DbCommand cmd)
        {
            var t = Task<DataTable>.Run(() =>
            {
                return this.Query(cmd);
            });
            return t;
        }

        public async Task QueryAsync(String query, String filePath)
        {
            await Task.Run(() => this.Query(query, filePath));
        }

        public async Task QueryAsync(String query, StreamWriter stream)
        {
            await Task.Run(() => this.Query(query, stream));
        }

        public DataTable QueryParams(String query, params Object[] PS)
        {
            
            var cmd = this.GetCommand(query);
            int i = 1;
            foreach(var p in PS) {
                var param =  cmd.CreateParameter();
                param.Value = p;
                param.ParameterName = "p" + i++;
                cmd.Parameters.Add(param);
            }
            var r = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(r);
            return dt;
        }

        public Object ExecuteScalar(String query)
        {
            return this.GetCommand(query).ExecuteScalar();
        }
        public Task<Object> ExecuteScalarAsync(String query)
        {
            return this.GetCommand(query).ExecuteScalarAsync();
        }

        public int Execute(String command)
        {
            return this.GetCommand(command).ExecuteNonQuery();
        }

        public Task<int> ExecuteAsync(String command)
        {
            return this.GetCommand(command).ExecuteNonQueryAsync();
        }

        public int FastInsert(DataTable dt, String destTable)
        {
            int counter = 0;
            Boolean closeTransaction = false;

            if (!HasOpenTransaction)
            { // if there is no open transaction, open one and close upon completion
                this.BeginTransaction();
                closeTransaction = true;
            }

            if (dt.Rows.Count == 0) { throw new Exception("DataTable cannot be empty"); }

            // String pmts = String.Join("," , new String(this.ParamChar, dt.Columns.Count).ToCharArray());
            var cmd = this.GetCommand("");

            try
            {
                List<String> paramNames = new List<String>();


                foreach (DataColumn column in dt.Columns)
                {
                    DbParameter p = cmd.CreateParameter();

                    p.ParameterName = column.ColumnName;

                    if (ParamChar == '?')
                    {   // MyDb, Dbite - insert into table1 values (?, ?, ?)
                        paramNames.Add("?");
                    }
                    else
                    {   // DbServer -  insert into table 1 Values (@id, @name, @age)
                        // Oracle, PgDb, Db2 insert into table1 (:id, :name, :age)                    
                        paramNames.Add(ParamChar + column.ColumnName);
                    }

                    p.DbType = (DbType)DbType.Parse(typeof(DbType), column.DataType.Name);
                    cmd.Parameters.Add(p);

                }

                String pmts = String.Join(",", paramNames);
                cmd.CommandText = String.Format("INSERT INTO {0} VALUES ({1})", destTable, pmts);

                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        cmd.Parameters[column.ColumnName].Value = row[column.ColumnName];

                    }

                    cmd.ExecuteNonQuery();
                    counter += 1;
                }

                if (closeTransaction) { this.Commit(); }
            }
            catch (Exception e)
            {
                if (closeTransaction) { Console.WriteLine("rolling back transaction"); this.Rollback(); }
                Console.WriteLine("command: \n" + cmd.CommandText);
                throw new Exception(e.Message);
            }

            return counter;
        }

        public Object Test()
        {
            Object result = null;
            try
            {
                result = this.Query(_testSql);
            }
            catch
            {
                result = this.Connection;
            }

            return result;
        }

        public DbConnection GetConnection()
        {
            return this.Connection;
        }

        public void Dispose()
        {
            this.Connection.Dispose();
            if (_transaction != null)
            {
                _transaction.Dispose();
            }
        }

    }

}

