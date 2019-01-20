using System;
using CommandLine;

namespace DDown.CLI
{
    public class CommandOptions
    {
        [Value(0, HelpText = "Link to download", Required = true)]
        public string Link { get; set; }

        [Option('p', "partition", Default = 0, Required = false, HelpText = "Set partition count. Default zero means system's processor count")]
        public int PartitionCount { get; set; }

        [Option('o', "output", Required = false, HelpText = "Default value is current folder that command runs.")]
        public string OutputFolder { get; set; }

        [Option('r', "override", Default = false, Required = false, HelpText = "Override the lastest download file with same name. Otherwise it will download the file with numbers (e.g. File (1).exe, File (2).exe).")]
        public bool Override { get; set; }

        [Option('b', "buffersize", Default = 8192, Required = false, HelpText = "Set buffer size.")]
        public int BufferSize { get; set; }

        [Option('t', "timeout", Default = 10000, Required = false, HelpText = "Set timeout parameter in miliseconds.")]
        public int Timeout { get; set; }

        [Option('d', "downloadFolder", Default = false, Required = false, HelpText = "The file download location set to User's download folder.")]
        public bool DownloadFolder {get; set;}
        
        [Option('s', "startover", Default = false, Required = false, HelpText = "Startover the download")]
        public bool Startover { get; set; }

        /* [Option('i', "info", Default = false, Required = false, HelpText = "Information about this version.")]
        public bool Information {get; set;}
        [Option('c', "clear", Default = true, Required = false, HelpText = "Clear console on start.")]
        public bool ClearConsole { get; set; } */
    }
}