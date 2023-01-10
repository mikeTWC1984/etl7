using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Automation;
using System.Text;
using ETL.Tools;

namespace ETL
{
    [Cmdlet(VerbsCommon.Get, "RandomTable")]
    [OutputType(typeof(DataTable))]
    [Alias("grt")]
    public class GetRandomTable : PSCmdlet
    {

        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public int RowQuantity { get; set; } = 10;

        readonly DataTable table = new DataTable("Person");
        readonly Random random = new Random();

        // public GetRandomTable()
        // {
        //     table = new DataTable("Person");
        //     random = new Random();
        // }
        
        protected override void EndProcessing()
        {
            table.AddColumnsFromClass(typeof(Person));

            for(var i=0; i < RowQuantity; i++)
            {
                var p = new Person(random);
                table.Rows.Add(
                    p.Id, p.FirstName, p.MiddleName, p.LastName, p.DateOfBirth, p.Gender, p.Age, p.Street, p.City
                    ,p.State, p.ZipCode, p.PhoneNumber, p.Account, p.Rate, p.Amount, p.Balance, p.LastUpdate, p.StrNumber

                );

            }

            WriteObject(table);
        }
    }
}
