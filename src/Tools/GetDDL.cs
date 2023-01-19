using System;
using System.Data;
using System.Management.Automation;
using ETL.SQL;

namespace ETL
{
    [Cmdlet(VerbsCommon.Get, "DDL")]
    [OutputType(typeof(String))]
    [Alias("ddl")]
    public class GetDDL : PSCmdlet
    {

        private Boolean _first = true;

        [Parameter(Mandatory = true, ValueFromPipeline = true)]

        public PSObject Input;

        [Parameter(Position = 0)]
        public SqlDialect dialect = SqlDialect.SQL;

        [Parameter(Position = 1)]
        public String Name = "TEST";


        private DataTable _dt;
        protected override void ProcessRecord()
        {
            if(! _first) { throw new PipelineStoppedException(); }

            Object obj = Input.ImmediateBaseObject;

            if(obj is DataRow) {
                _dt = (obj as DataRow).Table;
            }
            else if (obj is DataTable) {
                _dt = (obj as DataTable);
            }
            else {
                _dt = SqlUtil.PSObjectToDataTable(Input);
            }

            var sw = new System.IO.StringWriter();

            sw.WriteLine($"CREATE TABLE {Name} (");
            for (int i = 0; i < _dt.Columns.Count; i++) {
                var col = _dt.Columns[i];
                sw.WriteLine((i==0 ? "  " : ", ") + col.ColumnName + " " + SqlUtil.SQLTypeMap[dialect][col.DataType]);

            }

            sw.WriteLine(")");

            WriteObject(sw.ToString());

            sw.Dispose();

            _first = false;            

        }
                
    }
}