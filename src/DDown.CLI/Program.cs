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
        static List<Indicator> indicators = new List<Indicator>();
        private static bool _continued;
        private static Downloader downloader;

        public static void Exit(object sender, ConsoleCancelEventArgs ev)
        {
            downloader.Pause();

            ev.Cancel = true;
        }

        private static int[] lefts;
        private static int top;
        private static int percent;
        public static object lockObject = new object();
        async static Task Main(string[] args)
        {
            Console.CancelKeyPress += Exit;

            string link = "https://github.com/OpenShot/openshot-qt/releases/download/v2.4.1/OpenShot-v2.4.1-x86_64.dmg";
            //string link = "http://www.itu.edu.tr/docs/default-source/KurumsalKimlik-2017/itu-sunum.rar?sfvrsn=2";
            //var link = "https://media.forgecdn.net/files/2573/89/DBM-Core-7.3.31.zip";
            Console.Clear();
            //downloader = new Downloader(new Uri(link), new System.Net.Http.HttpClient(), new Options() { PartitionCount = 16 });
            
            downloader = new Downloader(link);
            downloader.Progress += ReportProgress2;


            Console.WriteLine("Preparing..!");
            var status = await downloader.PrepareAsync();
            _continued = status.Continued;

            if (status.Continued)
                Console.WriteLine($"Download is continued");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            lefts = new int[downloader.PartitionCount];
            top = Console.CursorTop;
            percent = 0;
            await downloader.StartAsync();

            Console.SetCursorPosition(0, top + downloader.PartitionCount + 1);
            if (downloader.ConnectionLost)
            {
                Console.WriteLine("Connection is lost.");
            }

            if (downloader.SourceException)
            {
                Console.WriteLine("There exist a problem with source.");
            }

            if (!downloader.Canceled)
            {
                Console.WriteLine("Merging..");
                await downloader.MergeAsync();
            }

            sw.Stop();

            Console.WriteLine("Ended " + sw.ElapsedMilliseconds);
            Console.WriteLine("Download is finished!");
            Console.WriteLine("\r\n");
        }

        static void ReportProgress2(Report data)
        {
            lock (lockObject)
            {
                var left = lefts[data.PartitionId];
                var topPartition = top + data.PartitionId;
                string template = $"P{(data.PartitionId + 1).ToString().PadRight(2)}: {(data.Percent + "%").PadRight(4)}";
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
    }
}
