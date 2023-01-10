

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
using System.Management.Automation;

namespace ETL.SQL
{
    static class Extensions
    {

        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            return items.Contains(item);
        }

    }

    public enum SqlDialect
    {
        SQL, Oracle, Mysql, Postgres, Sqlite
    }
    public static class SqlUtil
    {
        public static Dictionary<SqlDialect, Dictionary<Type, String>> SQLTypeMap = new Dictionary<SqlDialect, Dictionary<Type, String>> {


       {SqlDialect.SQL, new Dictionary<Type, string>()
        {
                 {typeof(String), "nvarchar(256)"}
                ,{typeof(Boolean), "bit"}
                ,{typeof(Byte[]), "varbinary(max)"}
                ,{typeof(SByte), "tinyint"}
                ,{typeof(Byte), "tinyint"}
                ,{typeof(Char), "nvarchar(1)"}
                ,{typeof(DateTime), "datetime"}
                ,{typeof(DateTimeOffset), "datetimeoffset"}
                ,{typeof(TimeSpan), "time"}
                ,{typeof(Decimal), "decimal"}
                ,{typeof(Double), "float"}
                ,{typeof(Guid), "uniqueidentifier"}
                ,{typeof(Int16), "smallint"}
                ,{typeof(Int32), "int"}
                ,{typeof(Int64), "bigint"}
                ,{typeof(Single), "real"}
                ,{typeof(UInt16), "smallint"}
                ,{typeof(UInt32), "int"}
                ,{typeof(UInt64), "bigint"}
        }}

        , {SqlDialect.Oracle, new Dictionary<Type, string>()
        {
                 {typeof(String), "VARCHAR(256)"}
                ,{typeof(Boolean), "NUMBER(1)"}
                ,{typeof(Byte[]), "BLOB"}
                ,{typeof(SByte), "NUMBER(3)"}
                ,{typeof(Byte), "NUMBER(3)"}
                ,{typeof(Char), "VARCHAR(1)"}
                ,{typeof(DateTime), "TIMESTAMP"}
                ,{typeof(DateTimeOffset), "TIMESTAMP WITH TIME ZONE"}
                ,{typeof(TimeSpan), "INTERVAL DAT TO SECOND"}
                ,{typeof(Decimal), "NUMBER"}
                ,{typeof(Double), "NUMBER"}
                ,{typeof(Guid), "RAW(16)"}
                ,{typeof(Int16), "NUMBER(5)"}
                ,{typeof(Int32), "NUMBER(10)"}
                ,{typeof(Int64), "NUMBER(19)"}
                ,{typeof(Single), "NUMBER(15,5)"}
                ,{typeof(UInt16), "NUMBER(5)"}
                ,{typeof(UInt32), "NUMBER(10)"}
                ,{typeof(UInt64), "NUMBER(19)"}
        }}
        , {SqlDialect.Sqlite, new Dictionary<Type, string>()
        {
                 {typeof(String), "TEXT"}
                ,{typeof(Boolean), "boolean"}
                ,{typeof(Byte[]), "BLOB"}
                ,{typeof(SByte), "INTEGER"}
                ,{typeof(Byte), "INTEGER"}
                ,{typeof(Char), "TEXT"}
                ,{typeof(DateTime), "datetime"}
                ,{typeof(DateTimeOffset), "datetimeoffset"}
                ,{typeof(TimeSpan), "time"}
                ,{typeof(Decimal), "NUMERIC"}
                ,{typeof(Double), "REAL"}
                ,{typeof(Guid), "guid"}
                ,{typeof(Int16), "INTEGER"}
                ,{typeof(Int32), "INTEGER"}
                ,{typeof(Int64), "INTEGER"}
                ,{typeof(Single), "REAL"}
                ,{typeof(UInt16), "INTEGER"}
                ,{typeof(UInt32), "INTEGER"}
                ,{typeof(UInt64), "INTEGER"}
        }}
        , {SqlDialect.Mysql, new Dictionary<Type, string>()
        {
                 {typeof(String), "varchar(256)"}
                ,{typeof(Boolean), "boolean"}
                ,{typeof(Byte[]), "blob"}
                ,{typeof(SByte), "tinyint"}
                ,{typeof(Byte), "tinyint unsigned"}
                ,{typeof(Char), "varchar(1)"}
                ,{typeof(DateTime), "datetime"}
                ,{typeof(DateTimeOffset), "datetime"}
                ,{typeof(TimeSpan), "time"}
                ,{typeof(Decimal), "decimal"}
                ,{typeof(Double), "double"}
                ,{typeof(Guid), "char(36)"}
                ,{typeof(Int16), "smallint"}
                ,{typeof(Int32), "int"}
                ,{typeof(Int64), "bigint"}
                ,{typeof(Single), "float"}
                ,{typeof(UInt16), "smallint"}
                ,{typeof(UInt32), "int"}
                ,{typeof(UInt64), "bigint"}
        }}
        , {SqlDialect.Postgres, new Dictionary<Type, string>()
        {
                 {typeof(String), "varchar(256)"}
                ,{typeof(Boolean), "boolean"}
                ,{typeof(Byte[]), "bytea"}
                ,{typeof(SByte), "smallint"}
                ,{typeof(Byte), "smallint"}
                ,{typeof(Char), "varchar(1)"}
                ,{typeof(DateTime), "timestamp"}
                ,{typeof(DateTimeOffset), "timestamp with time zone"}
                ,{typeof(TimeSpan), "timestamp"}
                ,{typeof(Decimal), "numeric"}
                ,{typeof(Double), "double precision"}
                ,{typeof(Guid), "uuid"}
                ,{typeof(Int16), "smallint"}
                ,{typeof(Int32), "integer"}
                ,{typeof(Int64), "bigint"}
                ,{typeof(Single), "real"}
                ,{typeof(UInt16), "smallint"}
                ,{typeof(UInt32), "integer"}
                ,{typeof(UInt64), "bigint"}
        }}

    };
    
/// <summary>
/// Checks if inoput object type belong to valid SqlType. If so returns that type, otherwise typeof(String)
/// </summary>
/// <param name="val"></param>
/// <returns></returns>
/// 
       public static Type GetODTType( Object val = null) {

            Type t;

            if (val is null || val is DBNull) { return typeof(String); }

            var types = new HashSet<Type>  {
                typeof(Boolean),
                typeof(Byte[]),
                typeof(Byte),
                typeof(Char),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Decimal),
                typeof(Double),
                typeof(Guid),
                typeof(Int16),
                typeof(Int32),
                typeof(Int64),
                typeof(Single),
                typeof(UInt16),
                typeof(UInt32),
                typeof(UInt64)
        };

            return types.TryGetValue(val.GetType(), out t) ? t : typeof(String);

        }

/// <summary>
/// Converts arbitrary PSObject into empty DataTable (as Schema)
/// </summary>
/// <param name="obj"></param>
/// <returns></returns>
/// 
        public static DataTable PSObjectToDataTable(PSObject obj) {
            var dt = new DataTable();
            foreach (var prop in obj.Properties)
            {
                var name = prop.Name;
                var val = prop.Value;

                var col = new DataColumn();
                col.ColumnName = name;
                col.DataType = GetODTType(val);
                dt.Columns.Add(col);
            }

            return dt;
        }


        // ---------------------------- Export CSV ---------------------------------------------------------------

        // Write reader data to stream, keep stream open. Return # of rows parsed 

        public static long WriteReaderToStream(
              IDataReader reader
            , StreamWriter sw
            , String csvDelimiter = ","
            , Boolean csvIncludeHeaders = true
            , Boolean csvQuote = true
            )
        {
            var fc = reader.FieldCount;
            var rowCount = 0;

            if (csvIncludeHeaders)
            {
                if (csvQuote)
                {
                    sw.WriteLine(string.Join(csvDelimiter, Enumerable.Range(0, fc).Select(reader.GetName).Select(x => "\"" + x.Replace("\"", "\"\"") + "\"").ToArray()));
                }
                else
                {
                    sw.WriteLine(string.Join(csvDelimiter, Enumerable.Range(0, fc).Select(reader.GetName).ToArray()));
                }
            }
            while (reader.Read())
            {
                rowCount += 1;
                if (csvQuote)
                {
                    sw.WriteLine(string.Join(csvDelimiter, Enumerable.Range(0, fc).Select(reader.GetValue).Select(x => "\"" + x.ToString().Replace("\"", "\"\"") + "\"").ToArray()));
                }
                else
                {
                    sw.WriteLine(string.Join(csvDelimiter, Enumerable.Range(0, fc).Select(reader.GetValue).ToArray()));
                }
            }

            reader.Close();
            return rowCount;
        }


        // export data to File and dispose streams upon completion. Return # of rows parsed

        public static long WriteReaderToFile(
              IDataReader reader
            , String outFile
            , String csvDelimiter = ","
            , Boolean csvIncludeHeaders = true
            , Boolean csvQuote = true
            )
        {

            using (var sw = new StreamWriter(outFile))
            {
                var rowCount = WriteReaderToStream(reader, sw);
                reader.Close();
                return rowCount;
            }

        }


    }
}