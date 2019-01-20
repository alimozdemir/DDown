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

        public static void SetDownload(Downloader download, string fileName = "")
        {
            var id = string.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString() : fileName;
            var saveModel = new Save();

            saveModel.Id = id;
            saveModel.Partitions = download.GetPartitions();
            saveModel.Url = download.GetUrl();
            saveModel.Length = download.Length;
            saveModel.IsRangeSupported = download.IsRangeSupported;
            var ser = JsonConvert.SerializeObject(saveModel, Formatting.Indented);
            File.WriteAllText(FileHelper.GetSavePath(id + ".json"), saveModel.Serialize());
        }

        public static Save GetSaveModel(string path)
        {
            var text = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Save>(text);
        }

        public static bool RemoveSaveModel(string fileName)
        {
            var path = FileHelper.GetSavePath(fileName + ".json");
            File.Delete(path);
            return !File.Exists(path);
        }
    }
}