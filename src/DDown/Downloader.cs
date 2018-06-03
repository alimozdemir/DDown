using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using DDown.Infrastructures;
using DDown.Internal;

namespace DDown
{
    public class Downloader
    {
        #region Constructors and Variables
        private static HttpClient _staticClient = new HttpClient();
        private static Options _staticOptions = new Options();
        HttpClient _client;
        Uri _uri;
        Options _options;
        Status _status;
        string _fileName, _fullPath;
        bool _canceled = false;
        public bool Completed => _options.Completed;
        public IProgress<(int, int)> Progress { get; set; }
        public Downloader(string url)
                : this(new Uri(url), _staticClient, _staticOptions)
        {
        }

        public Downloader(Uri uri)
                : this(uri, _staticClient, _staticOptions)
        {
        }

        public Downloader(Uri uri, HttpClient client, Options options)
        {
            _client = client;
            _options = options;
            _uri = uri;

            _fileName = FileHelper.GetFileName(_uri);
            _fullPath = Path.Combine(_options.OutputFolder, _fileName);

            //Ensure the necessary folders are created
            FileHelper.EnsureFoldersCreated();
        }
        #endregion


        public async Task MergeAsync()
        {
            if (_status.Partitions.Any(i => !i.IsFinished()))
                throw new Exception("Something went wrong, some of the partitions does not completed. Also, you can't merge the paused or stopped downloads");

            //merge all
            await MergePartitionsAsync();
        }

        public void Pause()
        {
            _canceled = true;
            SavePartitions();
        }

        #region Core methods

        public async Task<Status> PrepareAsync()
        {
            using (var response = await _client.GetAsync(_uri, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                _status = EnsureContentIsDownloadable(response);

                var saveModel = ResumePartitions();

                if (saveModel == null)
                {
                    CalculatePartitions();

                    if (_options.Override)
                        File.Delete(_fullPath);
                }
                else
                    _status.Continued = true;

            }

            return _status;
        }

        public async Task StartAsync()
        {
            _canceled = false;

            using (var response = await _client.GetAsync(_uri, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                /*Parallel.ForEach(_partitions, async (p) =>
                {
                    await DownloadPartitionAsync(p);
                });*/

                List<Task> tasks = new List<Task>();

                foreach (var item in _status.Partitions)
                    tasks.Add(DownloadPartitionAsync(item));

                Task.WaitAll(tasks.ToArray());
            }
        }

        private async Task DownloadPartitionAsync(Partition partition)
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
                                Progress?.Report((partition.Id, partition.Percent));
                            }
                        }

                    }
                    while (!partition.IsFinished() && !_canceled);

                }
            }

        }

        protected async Task MergePartitionsAsync()
        {
            using (var file = new FileStream(_fullPath, FileMode.Create, FileAccess.Write))
            {
                foreach (var p in _status.Partitions)
                {
                    if (File.Exists(p.Path))
                    {
                        using (var partitionFile = new FileStream(p.Path, FileMode.Open, FileAccess.Read))
                        {
                            await partitionFile.CopyToAsync(file);
                        }

                        File.Delete(p.Path);
                    }
                }
            }

            _options.Completed = true;
        }
        #endregion

        #region Internal methods and properties

        internal List<Partition> GetPartitions()
        {
            return _status.Partitions;
        }
        internal string GetUrl()
        {
            return _uri.OriginalString;
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
                var path = FileHelper.GetPartitionPath(fileName);

                _status.Partitions.Add(
                    new Partition(i, path, start, end)
                );

                start += median;
            }

            /*if(_partitions.Sum(i => i.Length) != _status.Length)
                throw new Exception("Not equal part of sizes");*/
        }

        internal void SavePartitions()
        {
            var result = LookingPartitionFile();
            Infrastructures.SaveModelFactory.SetDownload(this, result.fileName);
        }

        internal Save ResumePartitions()
        {
            var result = LookingPartitionFile();

            if (result.model != null)
            {
                CalculatePartitionsRemain(result.model);
            }

            return result.model;
        }

        internal void CalculatePartitionsRemain(Save model)
        {
            if (_status == null)
                throw new ArgumentException("EnsureContentIsDownloadable must be called before CalculatePartitions");

            /*_status = new Status()
            {
                Length = model.Length,
                IsRangeSupported = model.IsRangeSupported
            };*/

            _status.Partitions = model.Partitions;

            /*if(_partitions.Sum(i => i.Length) != _status.Length)
                throw new Exception("Not equal part of sizes");*/
        }



        internal (String fileName, Save model) LookingPartitionFile()
        {
            var files = FileHelper.GetAllFilesInSavedFolder();

            foreach (var item in files)
            {
                var model = SaveModelFactory.GetSaveModel(item);
                if (model.Url.Equals(_uri.OriginalString))
                {
                    FileInfo info = new FileInfo(item);
                    return (info.Name, model);
                }
            }

            return (string.Empty, null);
        }
        #endregion
    }
}
