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
        private static bool _continued;

        async static Task Main(string[] args)
        {
            Console.Clear();
            var downloader = new Downloader("http://qc2.androidfilehost.com/dl/VPTMvX6eUAab7LpTFJvFEg/1527665057/24269982087012285/N910CQXXU2COJ5_5.1.1_TUR_Factory_Firmware_abdyasar.zip");

            Console.WriteLine("Preparing..!");
            var status = await downloader.PrepareAsync();
            _continued = status.Continued;

            if (status.Continued)
                Console.WriteLine($"Download is continued");

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
                //_cancelSource.Cancel();
                //await _downloader.PauseAsync();
            }*/
            Console.WriteLine($"Partition Index = {data.index}, Percentange  {data.percent}");
        }
    }
}
