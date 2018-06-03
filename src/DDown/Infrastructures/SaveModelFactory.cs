using System;
using System.IO;
using DDown.Internal;
using Newtonsoft.Json;

namespace DDown.Infrastructures
{
    internal class SaveModelFactory
    {
        public SaveModelFactory()
        {
            
        }
        
        public static void SetDownload(Downloader download)
        {
            var id = Guid.NewGuid();
            var saveModel = new Save();

            saveModel.Id = id.ToString();
            saveModel.Partitions = download.GetPartitions();
            saveModel.Url = download.GetUrl();

            File.WriteAllText(FileHelper.GetSavePath(id + ".json"), saveModel.Serialize());
        }

        public static Save GetSaveModel(string path)
        {
            var text = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Save>(text);
        }
    }
}