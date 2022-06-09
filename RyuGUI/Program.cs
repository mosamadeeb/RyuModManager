using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using ModLoadOrder.Mods;

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
                MainWindow window = new MainWindow();

                List<ModInfo> mods = RyuCLI.Program.PreRun();

                // This should be called only after PreRun() to make sure the ini value was loaded
                if (RyuCLI.Program.ShouldBeExternalOnly())
                {
                    MessageBox.Show(
                        "External mods folder detected. Please run Ryu Mod Manager in CLI mode " +
                        "(use --cli parameter) and use the external mod manager instead.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Add the mod list to the listview
                window.SetupModList(mods);

                app.Run(window);
            }
            else
            {
                bool consoleEnabled = true;

                foreach (string a in args)
                {
                    if (a == "-s" || a == "--silent")
                    {
                        consoleEnabled = false;
                        break;
                    }
                }

                if (consoleEnabled)
                    AllocConsole();

                RyuCLI.Program.Main(args).Wait();

                if (consoleEnabled)
                    FreeConsole();
            }
        }
    }
}
