using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using RyuCLI;

namespace RyuGUI
{
    public static class Program
    {
        private const string Kernel32Dll = "kernel32.dll";

        [DllImport(Kernel32Dll)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32Dll)]
        private static extern bool FreeConsole();

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                App app = new App();
                app.Run(new MainWindow());
            }
            else
            {
                AllocConsole();
                RyuCLI.Program.Main(args).ConfigureAwait(true);
                FreeConsole();
            }
        }
    }
}
