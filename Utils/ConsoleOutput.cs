using System;
using System.Collections.Generic;

namespace Utils
{
    public class ConsoleOutput
    {
        public static bool Verbose = false;
        private static readonly List<string> printQueue = new List<string>();

        public int Id { get; }

        public int Indent { get; set; }

        public ConsoleOutput()
        {
            this.Id = printQueue.Count;
            this.Indent = 0;
            printQueue.Add("");
        }

        public ConsoleOutput(int indent)
            : this()
        {
            this.Indent = indent;
        }

        public void Write(string text)
        {
            printQueue[Id] += new string(' ', this.Indent) + text;
        }

        public void WriteLine(string text = "")
        {
            this.Write(text + "\n");
        }

        public void WriteIfVerbose(string text)
        {
            if (Verbose)
            {
                printQueue[Id] += new string(' ', this.Indent) + text;
            }
        }

        public void WriteLineIfVerbose(string text = "")
        {
            WriteIfVerbose(text + "\n");
        }

        public void Flush()
        {
            Console.WriteLine(printQueue[Id]);
            printQueue[Id] = "";
        }

        public static void Clear()
        {
            printQueue.Clear();
        }
    }
}
