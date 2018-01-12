using System;

namespace DDown.CLI
{
    public class Indicator
    {
        private int _current;
        private int left, top;
        public int Max { get; }
        public int Current { get => _current; }
        public string Title { get; } 
        private static object _lock = new object();
        public Indicator(string title, int max)
        {
            Max = max;
            Title = title;
            Initialize();
        }

        public void Tick(int val)
        {
            _current = val;
            Display();
        }
        private void Initialize()
        {
            lock (_lock)
            {
                left = Console.CursorLeft;
                top = Console.CursorTop;

                Console.WriteLine($"{Title} : {_current} / {Max}");
            }
        }

        private void Display()
        {
            lock (_lock)
            {
                int tempLeft = Console.CursorLeft;
                int tempTop = Console.CursorTop;

                Console.SetCursorPosition(left, top);
                Console.WriteLine($"{Title} : {_current} / {Max}");
                Console.SetCursorPosition(tempLeft, tempTop);

            }
        }

    }
}