using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Automation;
using System.Linq;
using ETL.SQL;

namespace ETL
{
    [Cmdlet(VerbsData.Out, "DataTable")]
    [OutputType(typeof(DataTable))]
    [Alias("odt")]
    public class OutDataTable : PSCmdlet
    {

        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]

        public DataTable dt = new DataTable();

        private Boolean _first = true;

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public PSObject[] InputObject { get; set; }

        [Parameter()]
        public String[] NonNullable = new String[] { };

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            //WriteObject(typeof(InputObject));
            //Console.WriteLine(InputObject[0].GetType());

            foreach (var obj in InputObject)
            {
                if (obj.ImmediateBaseObject is DataTable)
                {
                    WriteObject(obj);
                    return;
                }

                // if input is data row array - just clone table and copy record data into

                if (obj.ImmediateBaseObject is DataRow)
                {
                    var row = obj.ImmediateBaseObject as DataRow;
                    if (_first) { this.dt = row.Table.Clone(); }
                    dt.Rows.Add(row.ItemArray);
                }

                else
                {
                    var dr = this.dt.NewRow();
                    foreach (var prop in obj.Properties)
                    {
                        var name = prop.Name;
                        var val = prop.Value;
                        // Console.WriteLine( obj.ImmediateBaseObject.GetType());
                        // Console.WriteLine(name + ": " + val);

                        if (_first)
                        {
                            var col = new DataColumn();
                            col.ColumnName = name;
                            col.DataType = SqlUtil.GetODTType(val);
                            // if (val != null && val is not DBNull)
                            // {
                            //     col.DataType = GetODTType(val.GetType());
                            // }
                            if (this.NonNullable.Contains<String>(name)) col.AllowDBNull = false;
                            this.dt.Columns.Add(col);
                        }

                        dr[name] = val is null ? DBNull.Value : val;

                    }
                    this.dt.Rows.Add(dr);
                }
                this._first = false;
            }

        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteObject(this.dt);
        }

        public static Type GetODTType(Type type)
        {
            Type[] types = {
                typeof(Boolean),
                typeof(Byte[]),
                typeof(Byte),
                typeof(Char),
                typeof(DateTime),
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
            return types.Contains<Type>(type) ? type : typeof(String);

        }
    }

}
