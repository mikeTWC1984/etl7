using System;
using System.Data;
using System.Management.Automation;
using ETL.SQL;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ETL
{

    /// <summary>
    /// <para type="synopsis">This is the cmdlet synopsis.</para>
    /// <para type="description">This is part of the longer cmdlet description.</para>
    /// <para type="description">Also part of the longer cmdlet description.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "HtmlTable")]
    [CmdletBinding(DefaultParameterSetName = "default")]
    [OutputType(typeof(String), ParameterSetName = new String[] { "default" })]
    [OutputType(typeof(Dictionary<String, String>), ParameterSetName = new String[] { "list" })]
    [OutputType(typeof(void), ParameterSetName = new String[] { "help" })]

    [Alias("htmltab")]
    public class NewHtmlTable : PSCmdlet
    {

        // private Boolean _first = true;

        [Parameter(ParameterSetName = "default", Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public DataTable dt;

        [Parameter(ParameterSetName = "default")]
        public String[] Bold = new String[] { };
        [Parameter(ParameterSetName = "default")]
        public String[] Left = new String[] { };
        [Parameter(ParameterSetName = "default")]
        public String[] High = new String[] { };

        [Parameter(ParameterSetName = "default")]
        public Func<DataRow, DataColumn, Object, String> Condition;

        [Parameter(ParameterSetName = "default")]
        public String DateFormat = "yyyy-MM-dd HH:mm:ss";

        [Parameter(ParameterSetName = "default")]
        public int NumberPrecision = 5;

        [Parameter(ParameterSetName = "list")]
        public SwitchParameter ListStyles;

        [Parameter(ParameterSetName = "help")]
        public SwitchParameter Help;


        // private DataTable _dt;

        private static Dictionary<String, String> css = new Dictionary<string, string>();

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }


        protected override void ProcessRecord()
        {


            var center = "text-align:center;vertical-align:center";
            var dfsz = "font-size:12px";
            var bgc = "background-color";
            var td_base = $"{dfsz};font-weight:normal;color:#222b35;border:1px solid #A9A9A9;padding:2px 4px";
            var css = new Dictionary<string, string>();
            css.Add("table", "border-collapse:collapse;border-spacing:0;font-family:sans-serif;");
            css.Add("head", "font-size:14px;font-weight:bold;background-color:#C0C0C0;color:#222b35;border:1px solid #A9A9A9;padding:2px 4px;height:0.5in;");
            css.Add("td", $"{td_base};{center};");
            css.Add("td_bold", $"{td_base};font-weight:bold;{center};");
            css.Add("td_left", $"{td_base}; text-align:left;text-indent:4px;");
            css.Add("td_bold_left", $"{td_base};font-weight:bold;text-align:left;text-indent:4px;");
            css.Add("td_dang", $"{dfsz};{bgc}:#EE2C2C;color:white;font-weight:bold;border-color:#EE2C2C;{center}");
            css.Add("td_warn", $"{dfsz};{bgc}:orange;color:brown;font-weight:bold;border-color:orange;{center}");
            css.Add("td_ok", $"{dfsz};{bgc}:lightgreen;color:darkgreen;font-weight:bold;border-color:green;{center}");
            css.Add("td_high", $"{td_base};{bgc}:#DCDCDC;font-weight:bold;{center};");
            css.Add("td_head", $"{dfsz};{bgc}:#C0C0C0;color:#222b35;font-weight:bold;border:1px solid #A9A9A9;padding:2px 2px;{center}");
            css.Add("tr_dang_left", $"{dfsz};{bgc}:#EE2C2C;color:white;font-weight:bold;border-color:#EE2C2C;text-align:left;text-indent:4px;");
            css.Add("td_warn_left", $"{dfsz};{bgc}:orange;color:brown;font-weight:bold;border-color:orange;text-align:left;text-indent:4px;");
            css.Add("td_ok_left", $"{dfsz};{bgc}:lightgreen;color:darkgreen;font-weight:bold;border-color:green;text-align:left;text-indent:4px;");
            css.Add("td_high_left", $"{td_base};{bgc}:#DCDCDC;font-weight:bold;text-align:left;text-indent:4px;");
            css.Add("td_head_left", $"{dfsz};{bgc}:#C0C0C0;color:#222b35;border:1px solid #A9A9A9;padding:2px 5px;text-align:left;text-indent:4px;");
            css.Add("tr_odd", $"{bgc}:white;height:0.2in;color:#010066;{center}");
            css.Add("tr_even", $"{bgc}:#f2f2f2;height:0.2in;color:#010066;{center}");
            css.Add("tr_high", $"{bgc}:#DCDCDC;height:0.2in;color:#010066;{center}");

            if (ListStyles.IsPresent)
            {
                WriteObject(css);
                return;
            }

            if (Help.IsPresent)
            {
                var helpmsg = @"
$rule = { param([System.Data.DataRow]$row, [System.Data.DataColumn]$col, [ETL.RowContext]$ctx)

   if($ctx.Column -eq 'FirstName') {
      $ctx.Value = $ctx.Value.toUpper()
   }

   if($ctx.Column -eq 'DateOfBirth') {
      $ctx.Value = $ctx.Value ? $ctx.Value.toString('MM/dd/yyyy') : 'N/A'
   }

   if($row['Age'] -gt 70) {
      if($col.ColumnName -eq 'Age') {
       return 'td_ok'      
      }
      return 'td_dang' 
   }
}

grt 3 | New-HtmlTable  -NumberPrecision 3 -Bold StrNumber  -Condition $rule
                ";

                Console.WriteLine(helpmsg);
                return;

            }

            var table = XElement.Parse("<TABLE></TABLE>");
            table.SetAttributeValue("style", css["table"]);
            var head = XElement.Parse("<TR></TR>");
            head.Add(XElement.Parse("<TH></TH>"));
            var th_rownum = XElement.Parse("<TH>#</TH>");
            th_rownum.SetAttributeValue("style", css["head"]);
            head.Add(XElement.Parse("""<TH style="font-size:14px;font-weight:bold;background-color:#C0C0C0;color:#222b35;border:1px solid #A9A9A9;padding:2px 4px;height:0.5in;">#</TH>"""));


            foreach (DataColumn col in dt.Columns)
            {
                var th = XElement.Parse("<TH></TH>");
                th.SetAttributeValue("style", css["head"]);
                th.Value = col.ColumnName;
                head.Add(th);
            }
            table.Add(head);

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];

                var tr = new XElement("TR");
                // row banding
                tr.SetAttributeValue("style", i % 2 == 0 ? css["tr_odd"] : css["tr_even"]);
                tr.Add(new XElement("TD"));
                // row number 
                var td = new XElement("TD", i);
                td.SetAttributeValue("style", css["td_head"]);
                tr.Add(td);


                // --------  data cells 
                foreach (DataColumn col in dt.Columns)
                {

                    var style = css["td"];

                    var ctx = new RowContext(col.ColumnName, i, row[col.ColumnName]);

                    if (Left.Contains<String>(col.ColumnName)) style = css["td_left"];
                    if (Bold.Contains<String>(col.ColumnName)) style = css["td_bold"];
                    if (High.Contains<String>(col.ColumnName)) style = css["td_high"];
                    if (Condition != null)
                    {
                        var customStyle = Condition(row, col, ctx);
                        if (!String.IsNullOrEmpty(customStyle)) style = css.ContainsKey(customStyle) ? css[customStyle] : customStyle;
                    }

                    var val = ctx.Value;

                    if (val is null || val is DBNull) val = "";
                    if (val is float || val is decimal || val is double) val = Math.Round(double.Parse(val.ToString()), NumberPrecision);
                    if (val is DateTime) val = ((DateTime)val).ToString(DateFormat);
                    var cell = new XElement("TD", val.ToString());
                    cell.SetAttributeValue("style", style);
                    tr.Add(cell);
                }
                // -------- 
                table.Add(tr);

            }

            WriteObject(table.ToString());
        }

    }

    public class RowContext
    {

        public String Column { get; set; }
        public int RowNumber { get; set; }
        public object Value { get; set; }
        public RowContext(String col, int rownum, object val)
        {
            this.Column = col;
            this.RowNumber = rownum;
            this.Value = val;
        }
    }
}