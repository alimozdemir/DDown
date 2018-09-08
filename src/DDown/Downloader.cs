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
        string _fileName, _fullPath, _originalName;
        volatile bool _canceled = false, _connectionLost = false, _sourceException = false;
        bool _completed = false;
        public bool Completed => _completed;
        public bool Canceled => _canceled;
        public bool ConnectionLost => _connectionLost;
        public bool SourceException => _sourceException;
        public long Length => _status.Length;
        public int PartitionCount => _options.PartitionCount;

        public delegate void ProgressHandler(Report report);
        public event ProgressHandler Progress;
        public string FileName => _fileName;
        public string Url => _uri.ToString();

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

            if (_options.BufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(_options.BufferSize));

            if (_options.PartitionCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(_options.PartitionCount));

            _fileName = FileHelper.GetFileName(_uri);
            _fullPath = Path.Combine(_options.OutputFolder, _fileName);
            _originalName = Path.GetFileNameWithoutExtension(_fileName);
            //Ensure the necessary folders are created
            FileHelper.EnsureFoldersCreated();
        }
        #endregion


        public async Task MergeAsync()
        {
            if (_status.Partitions.Any(i => !i.IsFinished()))
                throw new Exception("Something went wrong, some of the partitions does not completed. Also, you can't merge the paused or stopped downloads.");

            //merge all
            await MergePartitionsAsync();
        }

        public void Pause()
        {
            _canceled = true;
        }

        public void Delete()
        {
            if (_status.OK)
            {
                DeleteAll();
            }
            else
            {
                throw new Exception("You have to call PrepareAsync method or something is wrong (e.g. PartitionCount, Length of the file)");
            }
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
                }
            }

            return _status;
        }

        public async Task StartAsync()
        {
            _canceled = false;

            using (var response = await _client.GetAsync(_uri, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                List<Task> tasks = new List<Task>();

                foreach (var item in _status.Partitions)
                {
                    if (item.Current == item.Length)
                        continue;

                    tasks.Add(DownloadPartitionAsync(item));
                }

                Task.WaitAll(tasks.ToArray());

                if (_canceled)
                {
                    SavePartitions();
                }
            }
        }

        private async Task DownloadPartitionAsync(Partition partition)
        {
            if (partition.Current == partition.Length)
                return;

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, _uri);

            if (_status.IsRangeSupported)
            {
                message.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue();
                message.Headers.Range.Unit = "bytes";
                message.Headers.Range.Ranges.Add(partition.GetHeader());
            }

            using (var response = await _client.SendAsync(message))
            {
                response.EnsureSuccessStatusCode();
                // System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable
                using (var file = new FileStream(partition.Path, FileMode.OpenOrCreate, FileAccess.Write))
                using (Stream read = await response.Content.ReadAsStreamAsync())
                {
                    var buffer = new byte[_options.BufferSize];

                    // if it is continued download, seek to end.
                    if (_status.Continued)
                        file.Seek(0, SeekOrigin.End);

                    do
                    {
                        int requestSize = 0;

                        if (partition.Current + buffer.Length > partition.Length)
                            requestSize = (int)partition.Length - (int)partition.Current;
                        else
                            requestSize = buffer.Length;

                        if (requestSize > 0)
                        {
                            // prepare tasks
                            var readRequest = read.ReadAsync(buffer, 0, requestSize); // task cancel
                            var timeout = Task.Delay(_options.Timeout);

                            // wait for a task to complete
                            Task.WaitAny(readRequest, timeout);

                            // if readRequest is not completed then timeout will be.
                            if (timeout.IsCompleted)
                            {
                                // cancel the all processes

                                _canceled = true;
                                _connectionLost = true;
                            }
                            else
                            {
                                // get the result of the already completed task for reading
                                var count = await readRequest;

                                if (count == 0) // 0 bytes means; something is not right
                                {
                                    _canceled = true;
                                    _sourceException = true;
                                }
                                else
                                {
                                    await file.WriteAsync(buffer, 0, count);
                                    partition.Write(count);
                                    partition.Notify(this.Progress);
                                }
                            }
                        }

                    }
                    while (!partition.IsFinished() && !_canceled);
                }
            }


        }

        protected async Task MergePartitionsAsync()
        {
            if (_options.Override)
            {
                if (File.Exists(_fullPath))
                    File.Delete(_fullPath);
            }

            _fullPath = PrepareFileName();

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

            var saved = LookingPartitionFile();

            if (saved.model != null)
            {
                SaveModelFactory.RemoveSaveModel(saved.fileName);
            }

            _completed = true;
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

            if (!_status.IsRangeSupported) // if range is not supported then it shouldn't divide the file into partitions
                _options.PartitionCount = 1;

            var median = _status.Length / _options.PartitionCount;
            long start = 0, end = 0;
            for (int i = 0; i < _options.PartitionCount; i++)
            {
                // This is a quick fix, remain part could be lost, if iteratively sum up to end
                if (_options.PartitionCount - 1 == i)
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

        internal void DeleteAll()
        {
            if (_status.Continued)
            {
                foreach (var item in _status.Partitions)
                {
                    if (File.Exists(item.Path))
                        File.Delete(item.Path);
                }
            }

            // remove Json file.
            var saved = LookingPartitionFile();

            if (saved.model != null)
            {
                SaveModelFactory.RemoveSaveModel(saved.fileName);
            }

        }

        internal void CalculatePartitionsRemain(Save model)
        {
            if (_status == null)
                throw new ArgumentException("EnsureContentIsDownloadable must be called before CalculatePartitions");

            _status.Continued = true;
            _status.Partitions = model.Partitions;
            _status.Length = model.Length;
            _status.IsRangeSupported = model.IsRangeSupported;
            _status.Partitions.ForEach(i => i.Start = i.Start + i.Current);

            //TODO: ensure json file is not changed and match with source.

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
                    return (Path.GetFileNameWithoutExtension(info.Name), model);
                }
            }

            return (string.Empty, null);
        }

        internal string PrepareFileName()
        {
            string fullPath = _fullPath;

            var name = _originalName;
            var extension = Path.GetExtension(_fileName);
            int counter = 0;

            while (File.Exists(fullPath))
            {
                counter++;
                name = $"{_originalName} ({counter})";
                fullPath = Path.Combine(_options.OutputFolder, name + extension);
            }

            return fullPath;
        }
        #endregion
    }
}
