using System;
using System.IO;
using DDown.Internal;

namespace DDown.Infrastructures
{
    internal class SaveModelFactory
    {
        public static void SetDownload(Downloader download)
        {
            var id = Guid.NewGuid();
            var saveModel = new Save();

            saveModel.Id = id.ToString();
            saveModel.Partitions = download.GetPartitions();
            saveModel.Url = download.GetUrl();

            File.WriteAllText(FileHelper.GetSavePath(id + ".json"), saveModel.Serialize());
        }
    }
}