using System;
using CommandLine;

namespace DDown.CLI
{
    public class CommandOptions
    {
        [Option('p', "partition", Default = 0, Required = false, HelpText = "Set partition count.")]
        public int PartitionCount { get; set; }

        [Option('o', "output", Required = false, HelpText = "Set output folder.")]
        public string OutputFolder { get; set; }

        [Option('r', "override", Default = false, Required = false, HelpText = "Override the lastest download file with same name.")]
        public bool Override { get; set; }

        [Option('b', "buffersize", Default = 8192, Required = false, HelpText = "Set buffer size.")]
        public int BufferSize { get; set; }

        [Option('t', "timeout", Default = 10000, Required = false, HelpText = "Set timeout parameter in miliseconds.")]
        public int Timeout { get; set; }

        /* [Option('c', "clear", Default = true, Required = false, HelpText = "Clear console on start.")]
        public bool ClearConsole { get; set; } */
    }
}