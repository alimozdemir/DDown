using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using DDown.Internal;

namespace DDown
{
    public class Downloader
    {
        private static HttpClient _staticClient = new HttpClient();
        private static Options _staticOptions = new Options();
        HttpClient _client;
        Uri _uri;
        Options _options;
        Status _status;
        string _fileName, _fullPath;
        public Downloader(string url) : this(new Uri(url), _staticClient, _staticOptions)
        {
        }

        public Downloader(Uri uri) : this(uri, _staticClient, _staticOptions)
        {
        }

        public Downloader(Uri uri, HttpClient client, Options options)
        {
            _client = client;
            _options = options;
            _uri = uri;

            _fileName = Utility.GetFileName(_uri);
            _fullPath = Path.Combine(_options.OutputFolder, _fileName);
        }
        //CancellationToken token = default
        public async Task StartAsync(IProgress<(int, int)> progress = null)
        {
            using (var response = await _client.GetAsync(_uri, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                /*Parallel.ForEach(_partitions, async (p) =>
                {
                    await DownloadPartitionAsync(p);
                });*/

                List<Task> tasks = new List<Task>();

                foreach (var item in _status.Partitions)
                    tasks.Add(DownloadPartitionAsync(item, progress));

                Task.WaitAll(tasks.ToArray());
            }
        }
        public async Task<Status> PrepareAsync()
        {
            using (var response = await _client.GetAsync(_uri, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                _status = EnsureContentIsDownloadable(response);
                CalculatePartitions();

                if (_options.Override)
                    File.Delete(_fullPath);
            }

            return _status;
        }
        public async Task MergeAsync()
        {
            if (_status.Partitions.Any(i => !i.IsFinished()))
                throw new Exception("Someting went wrong, some of the partitions does not completed.");

            //merge all
            await MergePartitionsAsync();
        }
        private async Task DownloadPartitionAsync(Partition partition, IProgress<(int, int)> progress = null)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, _uri);
            message.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue();
            message.Headers.Range.Unit = "bytes";
            message.Headers.Range.Ranges.Add(partition.GetHeader());
            
            using (var response = await _client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var file = new FileStream(partition.Path, FileMode.Create, FileAccess.Write))
                using (Stream read = await response.Content.ReadAsStreamAsync())
                {
                    var buffer = new byte[8192];

                    do
                    {
                        int requestSize = 0;

                        if (partition.Current + buffer.Length > partition.Length)
                            requestSize = (int)partition.Length - (int)partition.Current;
                        else
                            requestSize = buffer.Length;

                        if (requestSize > 0)
                        {
                            var count = await read.ReadAsync(buffer, 0, requestSize);

                            if (count == 0)
                            {
                                throw new Exception("There exist a problem with source.");
                            }
                            else
                            {
                                await file.WriteAsync(buffer, 0, count);
                                partition.Write(count);
                                progress?.Report((partition.Id, partition.Percent));
                            }
                        }

                    }
                    while (!partition.IsFinished());

                }
            }

        }
        protected Status EnsureContentIsDownloadable(HttpResponseMessage response)
        {
            var result = new Status()
            {
                Length = response.Content.Headers.ContentLength.GetValueOrDefault(),
                IsRangeSupported = response.Headers.AcceptRanges.ToString() == "bytes"
            };

            if (result.Length == 0)
            {
                throw new ArgumentException("The content is not downloadable.");
            }

            return result;
        }
        protected void CalculatePartitions()
        {
            if (_status == null)
                throw new ArgumentException("EnsureContentIsDownloadable must be called before CalculatePartitions");
            _options.ConnectionCount = 4;

            var median = _status.Length / _options.ConnectionCount;
            long start = 0, end = 0;
            for (int i = 0; i < _options.ConnectionCount; i++)
            {
                // This is a quick fix, remain part could be lost, if iteratively sum up to end
                if (_options.ConnectionCount - 1 == i)
                    end = _status.Length - 1;
                else if (start + median >= _status.Length)
                    end = _status.Length;
                else
                    end = start + median - 1;

                var fileName = i + "." + _fileName;
                var path = Path.Combine(_options.OutputFolder, fileName);

                _status.Partitions.Add(
                    new Partition(i, path, start, end)
                );

                start += median;
            }

            /*if(_partitions.Sum(i => i.Length) != _status.Length)
                throw new Exception("Not equal part of sizes");*/
        }
        protected async Task MergePartitionsAsync()
        {
            using (var file = new FileStream(Path.Combine(_options.OutputFolder, _fileName), FileMode.Create, FileAccess.Write))
            {
                foreach (var p in _status.Partitions)
                {
                    var tempFilePath = Path.Combine(_options.OutputFolder, p.Id + "." + _fileName);
                    if (File.Exists(tempFilePath))
                    {
                        using (var partitionFile = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                        {
                            await partitionFile.CopyToAsync(file);
                        }

                        File.Delete(tempFilePath);
                    }
                }
            }
        }

        /// <summary>
        /// Save current partitions status of the downloader
        /// </summary>
        public void SavePartitions()
        {
            var id = Guid.NewGuid();
            var saveModel = new Save();
            saveModel.Id = id.ToString();
            saveModel.Partitions = _status.Partitions;
            saveModel.Url = _uri.OriginalString;

            File.WriteAllText(id + ".json", saveModel.ToString());
        }


        public async Task PauseAsync()
        {

        }

        public async Task ResumeAsync()
        {

        }

        public async Task StopAsync()
        {

        }
    }
}
