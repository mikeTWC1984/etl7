using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Automation;
using ETL.File;
using System.Threading;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace ETL
{
    [Cmdlet(VerbsCommon.Get, "ZipContent")]
    [CmdletBinding(DefaultParameterSetName="file")]
    [OutputType(typeof(String), ParameterSetName = new String[]{"file"})]
    [OutputType(typeof(TarInputStream), ParameterSetName = new String[]{"stream"})]
    [Alias("catz")]
    public class GetZipContent : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public InStream Input { get; set; }

        [Parameter(ParameterSetName = "stream")]
        public SwitchParameter AsStream { get; set; }

        [Parameter(ParameterSetName = "file")]
        public SwitchParameter AsFile { get; set; }

        [Parameter()]
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private Stream _input;
        private StreamReader _reader;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            Console.CancelKeyPress += (_, e) => { e.Cancel = true; _cts.Cancel(); };

            _input = Input.Stream;
            var gz = new GZipInputStream(_input);
            var tar = new TarInputStream(gz, Encoding);
            var entry = tar.GetNextEntry();

            if(AsStream.IsPresent) {
                WriteObject(tar);
            }
            else
            {
                _reader = new StreamReader(tar);

                try
                {
                    while (!_reader.EndOfStream)
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        WriteObject(_reader.ReadLine());
                    }
                }
                catch (OperationCanceledException)
                {
                    this.EndProcessing();
                }
            }

        }

        // protected override void ProcessRecord()
        // {
        //     base.ProcessRecord();
        // }

        protected override void EndProcessing()

        {
            base.EndProcessing();
            _reader.Dispose();
            _input.Dispose();
        }

    }
}
