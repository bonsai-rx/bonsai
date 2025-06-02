using System;

namespace Bonsai.Configuration
{
    internal class ProgressBar : IProgressBar
    {
        int value;

        public ProgressBar()
        {
            if (!Console.IsOutputRedirected)
                Write(value);
        }

        public void Report(int percent)
        {
            if (Console.IsOutputRedirected || percent == value)
                return;

            Console.CursorLeft = 0;
            Write(percent);
            value = percent;
        }

        static void Write(int percent)
        {
            const int Length = 30;
            Console.Write($"\r[{new String('■', percent * Length / 100),-Length}] {percent,3:##0}%");
        }

        public void Dispose()
        {
            if (Console.IsOutputRedirected)
                return;

            Console.WriteLine();
        }
    }
}
