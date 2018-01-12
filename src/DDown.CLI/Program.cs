using System;
using System.Threading.Tasks;

namespace DDown.CLI
{
    class Program
    {
        async static Task Main(string[] args)
        {
            Downloader downloader = new Downloader("https://addons-origin.cursecdn.com/files/2477/989/Bagnon_7.3.2.zip");

            Console.WriteLine("Preparing..!");
            var status = await downloader.PrepareAsync();
            
            Console.WriteLine("Starting..!");
            await downloader.StartAsync();

            Console.WriteLine("Merging..!");
            await downloader.MergeAsync();

            Console.WriteLine("Download is finished!");
        }
    }
}
