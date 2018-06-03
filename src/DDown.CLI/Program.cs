using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            Console.WriteLine("Exiting.. {0} {1}", ev, sender);
            
            if (downloader != null && !downloader.Completed)
            {
                downloader.Pause();
            }
            ev.Cancel = false;
        }

        async static Task Main(string[] args)
        {
            Console.CancelKeyPress += Exit;
            //AppDomain.CurrentDomain.ProcessExit += Exit;
            string link = "https://github.com/OpenShot/openshot-qt/releases/download/v2.4.1/OpenShot-v2.4.1-x86_64.dmg";
            //string link = "http://www.itu.edu.tr/docs/default-source/KurumsalKimlik-2017/itu-sunum.rar?sfvrsn=2";
            Console.Clear();
            downloader = new Downloader(link);

            Console.WriteLine("Preparing..!");
            var status = await downloader.PrepareAsync();
            _continued = status.Continued;

            if (status.Continued)
                Console.WriteLine($"Download is continued");

            var progressIndicator = new Progress<(int, int)>(ReportProgress);
            downloader.Progress = progressIndicator;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await downloader.StartAsync();
            sw.Stop();

            Console.WriteLine("Ended " + sw.ElapsedMilliseconds);

            Console.WriteLine("Merging..");
            await downloader.MergeAsync();

            Console.WriteLine("Download is finished!");
        }
        static ConcurrentDictionary<int, int> Parts = new ConcurrentDictionary<int, int>();
        static void ReportProgress((int index, int percent) data)
        {
            /*if (!_continued && data.percent > 80)
            {
                //_cancelSource.Cancel();
                //await _downloader.PauseAsync();
            }*/
            if (Parts.ContainsKey(data.index))
            {
                if (Parts[data.index] != data.percent)
                {
                    Console.WriteLine($"Partition Index = {data.index}, Percentange  {data.percent}");
                }
            }
            else
            {
                Parts.TryAdd(data.index, data.percent);
                Console.WriteLine($"Partition Index = {data.index}, Percentange  {data.percent}");
            }

        }
    }
}
