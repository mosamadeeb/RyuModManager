using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using ModLoadOrder.Mods;
using static Utils.Constants;

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
                // Read the mod list (and execute other RMM stuff)
                List<ModInfo> mods = RyuHelpers.Program.PreRun();

                // This should be called only after PreRun() to make sure the ini value was loaded
                if (RyuHelpers.Program.ShouldBeExternalOnly())
                {
                    MessageBox.Show(
                        "External mods folder detected. Please run Ryu Mod Manager in CLI mode " +
                        "(use --cli parameter) and use the external mod manager instead.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (mods.Count == 0)
                {
                    MessageBox.Show("No mods were found. Add some mods to the \"\\mods\\\" directory first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (RyuHelpers.Program.ShouldCheckForUpdates())
                {
                    // Run the task and let it show the message whenever it's done
                    Task.Run(() => CheckForUpdatesGUI());
                }

                if (RyuHelpers.Program.ShowWarnings())
                {
                    // Check if the ASI loader is not in the directory (possibly due to incorrect zip extraction)
                    if (RyuHelpers.Program.MissingDLL())
                    {
                        MessageBox.Show(
                            DINPUT8DLL + " is missing from this directory. Mods will NOT be applied without this file.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Check if the ASI is not in the directory
                    if (RyuHelpers.Program.MissingASI())
                    {
                        MessageBox.Show(
                            ASI + " is missing from this directory. Mods will NOT be applied without this file.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Calculate the checksum for the game's exe to inform the user if their version might be unsupported
                    if (RyuHelpers.Program.InvalidGameExe())
                    {
                        MessageBox.Show(
                            "Game version is unsupported. Please use the latest Steam version of the game. " +
                            "The mod list will still be saved, but mods might not work.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                MainWindow window = new MainWindow();

                // Add the mod list to the listview
                window.SetupModList(mods);

                App app = new App();
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

                RyuHelpers.Program.Main(args).Wait();

                if (consoleEnabled)
                    FreeConsole();
            }
        }

        private static async Task CheckForUpdatesGUI()
        {
            var latestRelease = await RyuHelpers.Program.CheckForUpdates().ConfigureAwait(false);

            if (latestRelease != null && latestRelease.Name.Contains("Ryu Mod Manager") && latestRelease.TagName != RyuHelpers.Program.VERSION)
            {
                MessageBoxResult result = MessageBox.Show(
                    "New version available! Go to the release webpage?",
                    "Update", MessageBoxButton.OKCancel, MessageBoxImage.Question,
                    MessageBoxResult.OK);

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        Process.Start(latestRelease.HtmlUrl);
                    }
                    catch
                    {
                        MessageBox.Show(
                            "Browser could not be opened. Please go to this URL to download the update: " + latestRelease.HtmlUrl,
                            "Update", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
