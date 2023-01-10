
using System;
using System.Data;
using System.Collections.Generic;
using System.IO;

namespace ETL.Tools
{

    public class ReportBuilder
    {

        private DataTable _dt { get; set; }

        public ReportBuilder(DataTable table)
        {
            _dt = table;
        }

        public Func<String, DataRow, Object,String> ConditionalStyle { get; set; }

        private Dictionary<String, String> _css { get; set; } = new Dictionary<string, string>() {
            {"default", "bgcolor:red"},
            {"left", "padding:left"}
        };

        public void AddStyle(String name, String cssStyle)
        {
            _css.Add(name, cssStyle);
        }

        public void ListStyles()
        {
            foreach (var key in _css.Keys)
            {
                Console.WriteLine($"{key}: {_css[key]}");
            }
        }

        public String Print() {
            var sw = new StringWriter();

            var dt = _dt.Copy();
            // todo: select first 25 rows and convert to table (vs copy)

            foreach (DataColumn col in dt.Columns) { sw.Write(col.ColumnName + "|"); }
            sw.Write("\n");

            foreach(DataRow dr in dt.Rows) {
                foreach(DataColumn col in dt.Columns) {
                    var colName = col.ColumnName;
                    sw.Write(ConditionalStyle(colName, dr, dr[colName]) + '|');
                }
                sw.Write("\n");

            }

            return sw.ToString();
        }

    }
}