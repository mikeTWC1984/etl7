
using System.Management.Automation;
using System;
using System.IO;
using System.Drawing;


namespace ETL
{

    [Cmdlet(VerbsData.Import, "EnvVariables")]
    [OutputType(typeof(void))]
    [Alias("importenv")]
    public class NewSqliteClient : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = false)]
        public String File { get; set; }

        protected override void BeginProcessing()
        {
            File = File ?? Path.Join(this.MyInvocation.PSScriptRoot, ".env");
            ETL.Util.LoadEnv(File);

        }
    }

    [Cmdlet(VerbsCommon.Show, "Colors")]
    [OutputType(typeof(String))]
    [Alias("colors")]
    public class ShowColors : PSCmdlet
    {
        [Parameter()]
        public SwitchParameter Plain;

        protected override void BeginProcessing()
        {
            foreach (String color in Enum.GetNames(typeof(System.Drawing.KnownColor)))
            {
                WriteObject(Plain.IsPresent ? color : ETL.Util.Fg(color, color));
            }
        }
    }

    /// <summary>
    /// Option to write to actuall stderr (so can external tools would treat it as stderr)
    /// </summary>
    /// 
    [Cmdlet(VerbsCommunications.Write, "Stderr")]
    [OutputType(typeof(void))]
    public class WriteStderr : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position =0)]
        public String Message;

        protected override void ProcessRecord()
        {
            Console.Error.WriteLine(Message);

        }
    }

    [Cmdlet(VerbsCommunications.Write, "Log")]
    [OutputType(typeof(void))]
    public class WriteLog : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public String Message;

        [Alias("fore", "fg")]
        public KnownColor? ForegroundColor;

        [Parameter()]
        [Alias("bg")]
        public KnownColor? BackgroundColor;

        [Parameter()]
        [Alias("OK")]
        public SwitchParameter Success;

        [Parameter()]
        [Alias("Error")]
        public SwitchParameter Err;

        [Parameter()]
        [Alias("Warn")]
        public SwitchParameter Warning;

        [Parameter()]
        public String Symbol = "-";

        [Parameter()]
        public SwitchParameter ResetTimer;

        // private DateTime? _log_time;
        // private DateTime log_time;
        // private DateTime _start_time = DateTime.Now;

        protected override void BeginProcessing()
        {
            //base.BeginProcessing();
            if (ResetTimer.IsPresent || this.GetVariableValue("global:log_time") is null)
            {
                this.SessionState.PSVariable.Set("global:log_time", DateTime.Now);                
            }
            if(this.GetVariableValue("global:start_time") is null)
            {
                this.SessionState.PSVariable.Set("global:start_time", DateTime.Now);
            }

        }

        protected override void ProcessRecord()
        {
            var d = DateTime.Now - (DateTime)this.GetVariableValue("global:log_time");
            // if(ForegroundColor) Message
            if(Success.IsPresent) Message = ETL.Util.Bg(" " + Message + " ", "Green");
            else if (Err.IsPresent) Message = ETL.Util.Bg(" " + Message + " ", "Red");
            else if (Warning.IsPresent) Message = ETL.Util.Fg(ETL.Util.Bg(" " + Message + " ", "Orange"), "Brown");
            else 
            { 
                if (ForegroundColor !=  null) Message = ETL.Util.Fg(Message, ForegroundColor.ToString());
                if (BackgroundColor != null ) Message = ETL.Util.Bg(Message, BackgroundColor.ToString());

            }
            WriteObject(DateTime.Now.ToString("[MM/dd hh:mm:ss]") + " - " + Message + " [" + Util.GetReadableTimespan(d) + "]");
            if(Success.IsPresent && this.GetVariableValue("global:start_time") is DateTime) {
                 var elp = DateTime.Now - (DateTime)this.GetVariableValue("global:start_time");
                 WriteObject(DateTime.Now.ToString("[MM/dd hh:mm:ss]") + " - " + "Elapsed in " + "[" + Util.GetReadableTimespan(elp) + "]");
            }
            this.SessionState.PSVariable.Set("global:log_time", DateTime.Now);


        }
    }



}