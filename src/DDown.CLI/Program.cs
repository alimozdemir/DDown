using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace DDown.CLI
{
    class Program
    {
        private static bool _continued;
        private static Downloader downloader;

        public static void Exit(object sender, ConsoleCancelEventArgs ev)
        {
            downloader.Pause();

            ev.Cancel = true;
        }

        private static int[] lefts;
        private static int top;
        public static object lockObject = new object();
        async static Task Main(string[] args)
        {
            if (args.Length < 1)
                ArgumentError("Please give a link to download.");

            string link = args[0];

            //string link = "https://github.com/OpenShot/openshot-qt/releases/download/v2.4.1/OpenShot-v2.4.1-x86_64.dmg";
            //string link = "http://www.itu.edu.tr/docs/default-source/KurumsalKimlik-2017/itu-sunum.rar?sfvrsn=2";
            //var link = "https://media.forgecdn.net/files/2573/89/DBM-Core-7.3.31.zip";
            Console.Clear();
            //downloader = new Downloader(new Uri(link), new System.Net.Http.HttpClient(), new Options() { PartitionCount = 16 });

            Console.CancelKeyPress += Exit;
            downloader = new Downloader(link);
            downloader.Progress += ReportProgress2;

            TimeLog.WriteLine("Preparing");
            var status = await downloader.PrepareAsync();

            TimeLog.WriteLine($"Source {downloader.Url}");
            TimeLog.WriteLine($"File Name {downloader.FileName}, Size {downloader.Length.AsReadable()}");
            _continued = status.Continued;

            if (status.Continued)
                TimeLog.WriteLine($"Download is continued");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            lefts = new int[downloader.PartitionCount];
            top = Console.CursorTop;

            await downloader.StartAsync();

            Console.SetCursorPosition(0, top + downloader.PartitionCount);
            if (downloader.ConnectionLost)
            {
                TimeLog.WriteLine("Connection is lost.");
            }

            if (downloader.SourceException)
            {
                TimeLog.WriteLine("There exist a problem with source.");
            }

            if (!downloader.Canceled)
            {
                TimeLog.WriteLine("Merging partitions");
                await downloader.MergeAsync();
            }

            sw.Stop();

            TimeLog.WriteLine($"Total elapsed time {sw.ElapsedMilliseconds} ms");
            TimeLog.WriteLine("Download is finished");
        }

        static void ReportProgress2(Report data)
        {
            lock (lockObject)
            {
                var left = lefts[data.PartitionId];
                var topPartition = top + data.PartitionId;
                string template = $"[{DateTime.Now.ToShortTimeString()}] P{(data.PartitionId + 1).ToString().PadRight(2)}: {(data.Percent + "%").PadRight(4)}";
                var howMuch = data.Percent - left;

                // set the header
                Console.SetCursorPosition(0, topPartition);
                Console.Write(template);

                // set the bars
                Console.SetCursorPosition(left + template.Length, topPartition);
                Console.Write(new string('|', howMuch));

                // update the last state
                lefts[data.PartitionId] = left + (howMuch);
            }
        }

        static void ArgumentError(string text)
        {
            TimeLog.WriteLine(text);
            Environment.Exit(-1);
        }
    }
}
