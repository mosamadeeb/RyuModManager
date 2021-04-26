using System;

namespace Utils
{
    public static class ConsoleOutput
    {
        public static void PrintLine(string text, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine(text);
            }
        }
    }
}
