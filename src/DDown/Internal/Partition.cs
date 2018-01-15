using System;
using System.Net.Http.Headers;

namespace DDown.Internal
{
    internal class Partition
    {
        public Partition(int _id, string _path, long _start, long _end)
        {
            Id = _id;
            Length = (_end - _start) + 1;
            End = _end;
            Start = _start;
            Path = _path;
            _current = 0;
        }
        private long _current;
        public int Id { get; }
        public long Length { get; }
        public long Current { get => _current; }
        public long Start { get; }
        public long End { get; }
        public int Percent { get =>  (int)((this._current * 100) / (this.Length)); }
        public void Write(long value)
        {
            if (_current + value > Length)
                throw new ArgumentOutOfRangeException("Current can not exceed Length of partition");

            _current += value;
        }
        public bool IsFinished() => Length == Current;
        public RangeItemHeaderValue GetHeader() => new RangeItemHeaderValue(Start, End);
        public string Path { get; }
    }
}