using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Automation;
using System.Threading;
using System.Text;
using ETL.Tools;

namespace ETL
{
    [Cmdlet(VerbsCommon.Get, "RandomObject")]
    [OutputType(typeof(Person))]
    [Alias("gro")]
    public class GetRandomObject : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public WorkModes WorkMode { get; set; } = WorkModes.LimitByQuntity;

        [Parameter(Position = 0, Mandatory = false)]
        public int Limit { get; set; } = 10;

        [Parameter(Mandatory = false)]
        public int MinTimeLag { get; set; } = 1;

        [Parameter(Mandatory = false)]
        public int MaxTimeLag { get; set; } = 1000;

        readonly Random random;

        public GetRandomObject()
        {
            random = new Random();
        }
        
        protected override void ProcessRecord()
        {
            var startTime = DateTime.Now;
            var itterationCounter = 0;

            while(CanContinue(startTime, itterationCounter))
            {
                WriteObject(new Person(random));
                itterationCounter++;
                if(WorkMode==WorkModes.Infinite || WorkMode == WorkModes.LimitByTime)
                {
                    Thread.Sleep(random.Next(MinTimeLag, MaxTimeLag));
                }
            }
        }
        private bool CanContinue(DateTime startTime, int itterationCounter)
        {
            switch (WorkMode)
            {
                case WorkModes.Infinite:
                    return true;
                case WorkModes.LimitByQuntity:
                    return itterationCounter<Limit? true : false;
                case WorkModes.LimitByTime:
                    return DateTime.Now.Subtract(startTime).TotalSeconds < Limit; //? true : false;
                default:
                    return false;
            }
}
    }
}
