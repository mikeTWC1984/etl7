using System;
using Pastel;
using System.Drawing;
using HtmlAgilityPack;
using System.Data;
using ExcelDataReader;
using System.IO;
using ETL.SQL;
using CsvHelper;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using CsvHelper.Configuration;
using FastMember;
using System.Linq;
using dotenv.net;
using ETL.File.InputTypes;


namespace ETL
{
    public static class Util
    {
        public static String ResolveString(String str)
        {
            return ETL.Config.GetString(str) ?? Environment.GetEnvironmentVariable(str) ?? str;
        }
        public static String Fg(String message, String color = null)
        {
            if (color != null) { message = message.Pastel(Color.FromName(color)); }
            return message;
        }
        public static String Bg(String message, String color = null)
        {
            if (color != null) { message = message.PastelBg(Color.FromName(color)); }
            return message;
        }

        public static HtmlDocument ParseHtmlUrl(string url)
        {
            return (new HtmlWeb()).Load(url);
        }
        public static HtmlDocument ParseHtmlText(string htmlText)
        {
            var doc = new HtmlDocument();
            doc.Load(htmlText);
            return doc;
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static DataSet ExcelToDataSet(StreamData inputFile, bool useHeader = true)
        {
            return ExcelReaderFactory.CreateReader(inputFile.Stream).AsDataSet(new ExcelDataSetConfiguration()
            {
                UseColumnDataType = true,
                FilterSheet = (tableReader, sheetIndex) => true,
                ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                {
                    EmptyColumnNamePrefix = "Column",
                    UseHeaderRow = useHeader
                }
            });
        }

        public static DataSet ExcelToDataSet(StreamData inputFile, bool useHeader
        , Func<IExcelDataReader, bool> rowFilter, Func<IExcelDataReader, int, bool> columnFilter)
        {
            return ExcelReaderFactory.CreateReader(inputFile.Stream).AsDataSet(new ExcelDataSetConfiguration()
            {
                UseColumnDataType = true,
                FilterSheet = (tableReader, sheetIndex) => true,
                ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                {
                    EmptyColumnNamePrefix = "Column",
                    UseHeaderRow = useHeader,
                    ReadHeaderRow = (rowReader) => { rowReader.Read(); },
                    FilterRow = rowFilter,
                    FilterColumn = columnFilter
                }
            }
            );
        }

        // ----------- SQL to CSV helpers --------------------------------- //

        public static Byte[] ReaderToCsvBlob(IDataReader reader, string delimiter = ",", bool includeHeaders = true, bool quote = true)
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                SqlUtil.WriteReaderToStream(reader, sw, delimiter, includeHeaders, quote);
                sw.Flush();
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public static long ReaderToCsv(IDataReader reader, string filePath, string delimiter = ",", bool includeHeaders = true, bool quote = true)
        {
            return SqlUtil.WriteReaderToFile(reader, filePath, delimiter, includeHeaders, quote);
        }

        public static long ReaderToCsv(IDataReader reader, StreamWriter sw, string delimiter = ",", bool includeHeaders = true, bool quote = true)
        {
            return SqlUtil.WriteReaderToStream(reader, sw, delimiter, includeHeaders, quote);
        }


        // --------------------------  CSV to reader/table helpers ----------------------- //

        ///<summary>
        /// Default config to be used with DataTable/DataReader methods.
        ///</summary>
        private static CsvConfiguration defaultCsvConfig { get; set; } = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => Regex.Replace(args.Header.Trim().ToLower(), @"\s+", "_")
        };

        public static CsvConfiguration GetCsvConfig(CultureInfo ci = null)
        {
            if (ci == null) ci = CultureInfo.InvariantCulture;

            return new CsvConfiguration(ci)
            {
                PrepareHeaderForMatch = args => Regex.Replace(args.Header.Trim().ToLower(), @"\s+", "_")
            };
        }

        // public static void ResetDefaultCsvConfig()
        // {
        //     defaultCsvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        //     {
        //         PrepareHeaderForMatch = args => Regex.Replace(args.Header.Trim().ToLower(), @"\s+", "_")
        //     };

        // }


        ///<summary>
        /// returns plain CsvHelper.CsvReader object. Could be used to retreive parser (Parser property) or to use custom Config (different from defaultCsvConfig)
        ///</summary>
        public static CsvReader GetCsvReader(StreamData data, CsvConfiguration conf)
        {
            return new CsvReader(new StreamReader(data.Stream), conf ?? defaultCsvConfig);
        }

        public static CsvReader GetCsvReader(StreamData data, String delim = ",", bool hasHeaders = true)
        {
            var conf = GetCsvConfig();
            conf.Delimiter = delim;
            conf.HasHeaderRecord = hasHeaders;
            return new CsvReader(new StreamReader(data.Stream), conf);
        }

        ///<summary>
        /// Convert CSV file/stream/bytes/filepath to datareader (string only columns)
        ///</summary>
        public static IDataReader CsvToDataReader(StreamData data, CsvConfiguration conf)
        {
            var csv = new CsvReader(new StreamReader(data.Stream), conf ?? defaultCsvConfig);
            return new CsvDataReader(csv);
        }

        public static IDataReader CsvToDataReader(StreamData data, String delim = ",", bool hasHeaders = true)
        {
            var conf = GetCsvConfig();
            conf.Delimiter = delim;
            conf.HasHeaderRecord = hasHeaders;
            var csv = new CsvReader(new StreamReader(data.Stream), conf);
            return new CsvDataReader(csv);
        }


        public static IDataReader CsvToDataReaderGeneric<T>(StreamData data, string delim = ",", bool hasHeaders = true)
        {
            var conf = GetCsvConfig();
            conf.Delimiter = delim;
            conf.HasHeaderRecord = hasHeaders;
            var csv = new CsvReader(new StreamReader(data.Stream), conf);
            var ie = csv.GetRecords<T>();
            return ObjectReader.Create(ie);
        }

        public static IDataReader CsvToDataReaderGeneric<T>(StreamData data, CsvConfiguration conf)
        {
            var csv = new CsvReader(new StreamReader(data.Stream), conf ?? defaultCsvConfig);
            var ie = csv.GetRecords<T>();
            return ObjectReader.Create(ie);
        }

        ///<summary>
        /// Converts CSV file/stream/bytes/filepath to datareader using provided schema (type)
        ///</summary>
        public static IDataReader CsvToDataReaderT(Type schema, StreamData data, CsvConfiguration conf)
        {
            var GetRecordMethod = typeof(Util).GetMethod("CsvToDataReaderGeneric", new Type[] { typeof(StreamData), typeof(CsvConfiguration) });
            return GetRecordMethod.MakeGenericMethod(schema).Invoke(null, new Object[] { data, conf }) as IDataReader;
        }

        public static IDataReader CsvToDataReaderT(Type schema, StreamData data, string delim = ",", bool hasHeaders = true)
        {
            var conf = GetCsvConfig();
            conf.Delimiter = delim;
            conf.HasHeaderRecord = hasHeaders;
            return CsvToDataReaderT(schema, data, conf);
        }

        ///<summary>
        /// Converts CSV file/stream/bytes/filepath to datatable (all columns as string)
        ///</summary>
        public static DataTable CsvToDataTable(StreamData data)
        {
            using (data.Stream)
            using (var csv = new CsvReader(new StreamReader(data.Stream), defaultCsvConfig))
            {
                using (var dr = new CsvDataReader(csv))
                {
                    var dt = new DataTable();
                    dt.Load(dr);
                    return dt;
                }
            }
        }

        ///<summary>
        /// Converts CSV file/stream/bytes/filepath to datatable using provided schema (type)
        ///</summary>
        public static DataTable CsvToDataTableT(StreamData data, Type schema, CsvConfiguration conf)
        {
            using (var reader = CsvToDataReaderT(schema, data, conf))
            {
                var dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }

        public static DataTable CsvToDataTableT(StreamData data, Type schema, string delim, bool hasHeaders)
        {
            var conf = GetCsvConfig();
            conf.Delimiter = delim;
            conf.HasHeaderRecord = hasHeaders;

            using (var reader = CsvToDataReaderT(schema, data, conf))
            {
                var dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }

        ///<summary>
        /// Converts csv file/stream/path to IEnumerable<T> using type/schema
        ///</summary>
        public static IEnumerable<dynamic> CsvToObjectsT(Type schema, StreamData data, CsvConfiguration conf)
        {
            var csv = new CsvReader(new StreamReader(data.Stream), conf ?? defaultCsvConfig);
            var GetRecordMethod = typeof(CsvReader).GetMethod("GetRecords", new Type[] { });
            var GetRecordMethodGeneric = GetRecordMethod.MakeGenericMethod(schema);
            return GetRecordMethodGeneric.Invoke(csv, null) as IEnumerable<dynamic>;
        }

        public static IEnumerable<dynamic> CsvToObjectsT(Type schema, StreamData data, string delim = ",", bool hasHeaders = true)
        {
            var conf = GetCsvConfig();
            conf.Delimiter = delim;
            conf.HasHeaderRecord = hasHeaders;
            var csv = new CsvReader(new StreamReader(data.Stream), conf);
            var GetRecordMethod = typeof(CsvReader).GetMethod("GetRecords", new Type[] { });
            var GetRecordMethodGeneric = GetRecordMethod.MakeGenericMethod(schema);
            return GetRecordMethodGeneric.Invoke(csv, null) as IEnumerable<dynamic>;
        }


        public static Stream ConvertToSeekableStream(Stream underlyingStream, int buffer = 32768)
        {
            return new File.ReadSeekableStream(underlyingStream, buffer);
        }

        public static TableDiff GetTableDiff(DataTable oldTable, DataTable newTable)
        {
            var ds = new DataSet("Diff");

            var rowsRemoved = oldTable.Clone(); rowsRemoved.TableName = "rows_removed";
            var rowsAdded = newTable.Copy(); rowsAdded.TableName = "rows_added";
            var diffTable = newTable.Clone(); diffTable.TableName = "rows_changed";

            foreach (DataRow row in oldTable.Rows)
            {
                var matchingRow = rowsAdded.Rows.Find(GetDataRowIndex(row));
                if (matchingRow != null)
                {
                    if (!DataRowComparer.Default.Equals(row, matchingRow)) { diffTable.Rows.Add(matchingRow.ItemArray); }
                    rowsAdded.Rows.Remove(matchingRow);
                }
                else
                {
                    rowsRemoved.Rows.Add(row.ItemArray);
                }
            }
            ds.Tables.Add(rowsAdded);
            ds.Tables.Add(rowsRemoved);
            ds.Tables.Add(diffTable);

            return new TableDiff(ds);
        }

        public static String PrintTable(DataTable dt, int limit = 25)
        {
            var sb = new System.Text.StringBuilder();
            var rowId = 0;
            var ctNew = new ConsoleTables.ConsoleTable();
            ctNew.AddColumn(dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            foreach (DataRow r in dt.Rows)
            {
                ctNew.AddRow(r.ItemArray);
                rowId += 1;
                if (rowId >= limit) break;
            }
            sb.AppendLine(ctNew.ToStringAlternative());
            if (dt.Rows.Count > limit) sb.AppendLine("   + " + (dt.Rows.Count - limit) + " MORE ROWS");
            return sb.ToString();

        }

        public class TableDiff
        {
            public int TotalChanges { get => this.RowsAdded + this.RowsRemoved + this.RowsChanged; private set { } }
            public int RowsAdded = 0;
            public int RowsRemoved = 0;
            public int RowsChanged = 0;
            public DataSet data;

            public TableDiff(DataSet ds)
            {
                this.data = ds;
                this.RowsAdded = ds.Tables[0].Rows.Count;
                this.RowsRemoved = ds.Tables[1].Rows.Count;
                this.RowsChanged = ds.Tables[2].Rows.Count;
            }

            public String GetReport(int limit = 25)
            {
                var sb = new System.Text.StringBuilder();
                if (this.data.Tables[0].Rows.Count > 0)
                {
                    sb.AppendLine("Rows added: " + this.RowsAdded);
                    sb.AppendLine(PrintTable(this.data.Tables[0], limit));
                }
                if (this.data.Tables[1].Rows.Count > 0)
                {
                    sb.AppendLine("Rows Removed: " + this.RowsRemoved);
                    sb.AppendLine(PrintTable(this.data.Tables[1], limit));
                }
                if (this.data.Tables[2].Rows.Count > 0)
                {
                    sb.AppendLine("Rows Changed: " + this.RowsChanged);
                    sb.AppendLine(PrintTable(this.data.Tables[2], limit));
                }
                return sb.ToString();
            }

        }

        public static object[] GetDataRowIndex(DataRow row)
        {
            var arr = new List<object>();
            foreach (var column in row.Table.PrimaryKey) { arr.Add(row[column]); }
            return arr.ToArray();
        }

        public static Object ParseJson(String data)
        {
            return Nest.Utf8Json.JsonSerializer.Deserialize<dynamic>(data);
        }

        public static Object[] ParseJsonLines(String[] lines)
        {
            var jsonArray = new dynamic[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                jsonArray[i] = Nest.Utf8Json.JsonSerializer.Deserialize<dynamic>(lines[i]);
            }
            //return Nest.Utf8Json.JsonSerializer.Deserialize<dynamic>(data);
            return jsonArray;
        }

        public static void LoadEnv(String filePath = null)
        {
            filePath = filePath ?? Environment.GetEnvironmentVariable("ENV_FILES");
            if (filePath is null)
            {
                DotEnv.Load();
            }
            else
            {
                DotEnv.Load(options: new DotEnvOptions(envFilePaths: filePath.Split(",")));
            }

        }

        public static string GetReadableTimespan(TimeSpan ts)
        {
            // formats and its cutoffs based on totalseconds
            var cutoff = new SortedList<long, string> {
       {59, "{3:S}" },
       {60, "{2:M}" },
       {60*60-1, "{2:M}, {3:S}"},
       {60*60, "{1:H}"},
       {24*60*60-1, "{1:H}, {2:M}"},
       {24*60*60, "{0:D}"},
       {Int64.MaxValue , "{0:D}, {1:H}"}
     };

            // find nearest best match
            var find = cutoff.Keys.ToList()
                          .BinarySearch((long)ts.TotalSeconds);
            // negative values indicate a nearest match
            var near = find < 0 ? Math.Abs(find) - 1 : find;
            // use custom formatter to get the string
            return String.Format(
                new HMSFormatter(),
                cutoff[cutoff.Keys[near]],
                ts.Days,
                ts.Hours,
                ts.Minutes,
                ts.Seconds);
        }
    }

    // formatter for forms of
    // seconds/hours/day
    public class HMSFormatter : ICustomFormatter, IFormatProvider
    {
        // list of Formats, with a P customformat for pluralization
        static Dictionary<string, string> timeformats = new Dictionary<string, string> {
        {"S", "{0:P:Seconds:Second}"},
        {"M", "{0:P:Minutes:Minute}"},
        {"H","{0:P:Hours:Hour}"},
        {"D", "{0:P:Days:Day}"}
    };

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return String.Format(new PluralFormatter(), timeformats[format], arg);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }

    // formats a numeric value based on a format P:Plural:Singular
    public class PluralFormatter : ICustomFormatter, IFormatProvider
    {

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg != null)
            {
                var parts = format.Split(':'); // ["P", "Plural", "Singular"]

                if (parts[0] == "P") // correct format?
                {
                    // which index postion to use
                    int partIndex = (arg.ToString() == "1") ? 2 : 1;
                    // pick string (safe guard for array bounds) and format
                    return String.Format("{0} {1}", arg, (parts.Length > partIndex ? parts[partIndex] : ""));
                }
            }
            return String.Format(format, arg);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }
}