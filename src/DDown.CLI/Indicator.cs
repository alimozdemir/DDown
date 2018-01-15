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

        private static int _lastLeft, _lastTop;

        public static void Clear()
        {
            Console.SetCursorPosition(_lastLeft, _lastTop);
        }

        public Indicator(string title, int max)
        {
            Max = max;
            Title = title;
            _current = 0;
            Initialize();
        }

        public void Tick(int val)
        {
            if (val > _current)
            {
                _current = val;
                Display();
            }
        }
        private void Initialize()
        {
            //lock (_lock)
            {
                left = Console.CursorLeft;
                top = Console.CursorTop;

                Console.Write($"{Title} : {_current} / {Max}" + Environment.NewLine);

                _lastLeft = Console.CursorLeft;
                _lastTop = Console.CursorTop;
            }
        }

        private void Display()
        {
            //lock (_lock)
            {
                int tempLeft = Console.CursorLeft;
                int tempTop = Console.CursorTop;

                Console.SetCursorPosition(left, top);
                Console.Write($"{Title} : {_current} / {Max}"+ Environment.NewLine);
                Console.SetCursorPosition(tempLeft, tempTop);

            }
        }

    }
}