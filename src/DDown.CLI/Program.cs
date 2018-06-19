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
            Console.WriteLine("Exiting.. {0} {1}", ev, sender);

            ev.Cancel = true;
        }

        async static Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
            {
                Console.WriteLine("process exit");
            };
            
            Console.CancelKeyPress += Exit;
            //AppDomain.CurrentDomain.ProcessExit += Exit;

            //string link = "https://github.com/OpenShot/openshot-qt/releases/download/v2.4.1/OpenShot-v2.4.1-x86_64.dmg";
            //string link = "http://www.itu.edu.tr/docs/default-source/KurumsalKimlik-2017/itu-sunum.rar?sfvrsn=2";
            var link = "https://media.forgecdn.net/files/2573/89/DBM-Core-7.3.31.zip";
            Console.Clear();
            downloader = new Downloader(link);
            downloader.Progress += ReportProgress;


            Console.WriteLine("Preparing..!");
            var status = await downloader.PrepareAsync();
            _continued = status.Continued;

            if (status.Continued)
                Console.WriteLine($"Download is continued");



            Stopwatch sw = new Stopwatch();
            sw.Start();
            await downloader.StartAsync();

            if (!downloader.Canceled)
            {
                Console.WriteLine("Merging..");
                await downloader.MergeAsync();

                sw.Stop();

                Console.WriteLine("Ended " + sw.ElapsedMilliseconds);

                Console.WriteLine("Download is finished!");
            }

        }


        static ConcurrentDictionary<int, int> Parts = new ConcurrentDictionary<int, int>();
        static void ReportProgress(Report data)
        {
            /*if (!_continued && data.percent > 80)
            {
                //_cancelSource.Cancel();
                //await _downloader.PauseAsync();
            }*/
            if (Parts.ContainsKey(data.PartitionId))
            {
                if (Parts[data.PartitionId] != data.Percent)
                {
                    Console.WriteLine($"Partition Index = {data.PartitionId}, Percentange  {data.Percent}");
                }
            }
            else
            {
                Parts.TryAdd(data.PartitionId, data.Percent);
                Console.WriteLine($"Partition Index = {data.PartitionId}, Percentange  {data.Percent}");
            }

        }
    }
}
