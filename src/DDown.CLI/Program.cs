using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DDown.CLI
{
    class Program
    {
        static List<Indicator> indicators = new List<Indicator>();
        private static CancellationTokenSource _cancelSource;
        private static bool _continued;

        async static Task Main(string[] args)
        {
            Console.Clear();
            _cancelSource = new CancellationTokenSource();

            var downloader = new Downloader("https://addons-origin.cursecdn.com/files/2477/989/Bagnon_7.3.2.zip", _cancelSource.Token);

            Console.WriteLine("Preparing..!");
            var status = await downloader.PrepareAsync();
            _continued = status.Continued;

            if (status.Continued)
                Console.WriteLine($"Download is continued");

            //downloader.SavePartitions();
            /*for (int i = 0; i < status.PartitionCount; i++)
            {
                indicators.Add(new Indicator($"{i + 1}", 100));
            }*/
            //return;
            var progressIndicator = new Progress<(int, int)>(ReportProgress);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await downloader.StartAsync(progressIndicator);
            sw.Stop();

            Console.WriteLine("Ended " + sw.ElapsedMilliseconds);

            Console.WriteLine("Merging..!");
            await downloader.MergeAsync();

            Console.WriteLine("Download is finished!");

        }

        static async void ReportProgress((int index, int percent) data)
        {
            /*if (!_continued && data.percent > 80)
            {
                _cancelSource.Cancel();
                //await _downloader.PauseAsync();
            }*/
            Console.WriteLine($"Partition Index = {data.index}, Percentange  {data.percent}");
        }
    }
}
