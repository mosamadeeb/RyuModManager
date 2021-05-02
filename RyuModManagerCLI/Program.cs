using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Octokit;
using Utils;

using static ModLoadOrder.Generator;
using static Utils.GamePath;
using static Utils.Constants;

namespace RyuCLI
{
    public static class Program
    {
        private const string VERSION = "v1.0";
        private const string AUTHOR = "SutandoTsukai181";
        private const string REPO = "RyuModManager";

        public static async Task Main(string[] args)
        {
            bool looseFilesEnabled = false;
            bool checkForUpdates = true;
            bool isSilent = false;

            Console.WriteLine($"Ryu Mod Manager CLI {VERSION}");
            Console.WriteLine($"By {AUTHOR}\n");

            if (args.Length == 0)
            {
                Console.WriteLine($"No arguments were passed. Will generate Mod Load Order and repack pars...\n");
            }
            else
            {
                List<string> list = new List<string>(args);
                if (list.Contains("/s"))
                {
                    isSilent = true;
                }
            }

            Task<ConsoleOutput> updateCheck = Task.Run(() => CheckForUpdates());

            if (File.Exists(INI))
            {
                IniData ini = new FileIniDataParser().ReadFile(INI);

                if (ini.TryGetKey("Overrides.LooseFilesEnabled", out string looseFiles))
                {
                    looseFilesEnabled = int.Parse(looseFiles) == 1;
                }

                if (ini.TryGetKey("RyuModManager.Verbose", out string verbose))
                {
                    ConsoleOutput.Verbose = int.Parse(verbose) == 1;
                }

                if (ini.TryGetKey("RyuModManager.CheckForUpdates", out string check))
                {
                    checkForUpdates = int.Parse(check) == 1;
                }
            }

            List<string> mods = new List<string>();

            if (File.Exists(TXT))
            {
                StreamReader file = new StreamReader(new FileInfo(TXT).FullName);

                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.StartsWith(';'))
                    {
                        line = line.Split(';', 1)[0];

                        // Only add existing mods that are not duplicates
                        if (line.Length > 0 && Directory.Exists(Path.Combine(MODS, line)) && !mods.Contains(line))
                        {
                            mods.Add(line);
                        }
                    }
                }

                file.Close();
            }

            await GenerateModLoadOrder(mods, looseFilesEnabled).ConfigureAwait(false);

            if (checkForUpdates)
            {
                Console.WriteLine("Checking for updates...");

                // Wait for a maximum of 5 seconds for the update check if it was not finished
                updateCheck.Wait(5000);
                var updateConsole = await updateCheck.ConfigureAwait(false);

                if (updateConsole != null)
                {
                    updateConsole.Flush();
                }
                else
                {
                    Console.WriteLine("Unable to check for updates\n");
                }
            }
            if (!isSilent)
            {
                Console.WriteLine("Program finished. Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static async Task<ConsoleOutput> CheckForUpdates()
        {
            try
            {
                ConsoleOutput console = new ConsoleOutput();
                var client = new GitHubClient(new ProductHeaderValue(REPO));
                var latestRelease = await client.Repository.Release.GetLatest(AUTHOR, REPO).ConfigureAwait(false);

                if (latestRelease != null && latestRelease.Name.Contains("CLI") && latestRelease.TagName != VERSION)
                {
                    console.WriteLine("New version detected!");
                    console.WriteLine($"Current version: {VERSION}");
                    console.WriteLine($"New version: {latestRelease.TagName}\n");

                    console.WriteLine($"Please update by going to {latestRelease.HtmlUrl}");
                }
                else
                {
                    console.WriteLine("Current version is up to date");
                }

                return console;
            }
            catch
            {

            }

            return null;
        }
    }
}
