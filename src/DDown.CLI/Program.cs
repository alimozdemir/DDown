using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DDown.CLI
{
    class Program
    {
        static List<Indicator> indicators = new List<Indicator>();
        async static Task Main(string[] args)
        {
            Console.Clear();

            Downloader downloader = new Downloader("https://addons-origin.cursecdn.com/files/2477/989/Bagnon_7.3.2.zip");

            Console.WriteLine("Preparing..!");
            var status = await downloader.PrepareAsync();
            
            for (int i = 0; i < status.PartitionCount; i++)
            {
                indicators.Add(new Indicator($"{i + 1}", 100));
            }

            var progressIndicator = new Progress<(int, int)>(ReportProgress);

            Console.WriteLine("Starting..!");
            await downloader.StartAsync(progressIndicator);

            Console.WriteLine("Merging..!");
            await downloader.MergeAsync();

            Console.WriteLine("Download is finished!");
        }

        static void ReportProgress((int, int) data)
        {
            indicators[data.Item1].Tick(data.Item2);
            //Console.WriteLine(data.Item1 + " " + data.Item2);
        }



    }
}
