using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Octokit;
using Utils;
using RyuCLI.Templates;
using ParRepacker;
using ModLoadOrder.Mods;

using static ModLoadOrder.Generator;
using static Utils.GamePath;
using static Utils.Constants;

namespace RyuCLI
{
    public static class Program
    {
        public const string VERSION = "v1.7";
        public const string AUTHOR = "SutandoTsukai181";
        public const string REPO = "RyuModManager";

        private static bool externalModsOnly = true;
        private static bool looseFilesEnabled = false;
        private static bool checkForUpdates = true;
        private static bool isSilent = false;

        private static Task<ConsoleOutput> updateCheck = null;

        public static bool RebuildMLO = true;

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Ryu Mod Manager {VERSION}");
            Console.WriteLine($"By {AUTHOR}\n");

            // Parse arguments
            List<string> list = new List<string>(args);

            if (list.Contains("-s") || list.Contains("--silent"))
            {
                isSilent = true;
            }

            await RunGeneration(ConvertNewToOldModList(PreRun())).ConfigureAwait(true);
            await PostRun().ConfigureAwait(true);
        }

        public static List<ModInfo> PreRun()
        {
            var iniParser = new FileIniDataParser();
            iniParser.Parser.Configuration.AssigmentSpacer = string.Empty;

            IniData ini;
            if (File.Exists(INI))
            {
                ini = iniParser.ReadFile(INI);

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

                if (ini.TryGetKey("RyuModManager.ShowWarnings", out string showWarnings))
                {
                    ConsoleOutput.ShowWarnings = int.Parse(showWarnings) == 1;
                }

                if (ini.TryGetKey("RyuModManager.LoadExternalModsOnly", out string extMods))
                {
                    externalModsOnly = int.Parse(extMods) == 1;
                }

                if (ini.TryGetKey("Overrides.RebuildMLO", out string rebuildMLO))
                {
                    RebuildMLO = int.Parse(rebuildMLO) == 1;
                }

                if (!ini.TryGetKey("Parless.IniVersion", out string iniVersion) || int.Parse(iniVersion) < ParlessIni.CurrentVersion)
                {
                    // Update if ini version is old (or does not exist)
                    Console.Write(INI + " is outdated. Updating ini to the latest version... ");

                    if (int.Parse(iniVersion) <= 3)
                    {
                        // Force enable RebuildMLO option
                        ini.Sections["Overrides"]["RebuildMLO"] = "1";
                        RebuildMLO = true;
                    }

                    iniParser.WriteFile(INI, IniTemplate.UpdateIni(ini));
                    Console.WriteLine("DONE!\n");
                }
            }
            else
            {
                // Create ini if it does not exist
                Console.Write(INI + " was not found. Creating default ini... ");
                iniParser.WriteFile(INI, IniTemplate.NewIni());
                Console.WriteLine("DONE!\n");
            }

            if (isSilent)
            {
                // No need to check if console won't be shown anyway
                checkForUpdates = false;
            }
            else if (checkForUpdates)
            {
                // Start checking for updates before the actual generation is done
                updateCheck = Task.Run(() => CheckForUpdatesCLI());
            }

            if (GamePath.GetGame() != Game.Unsupported && !Directory.Exists(MODS))
            {
                // Create mods folder if it does not exist
                Console.Write($"\"{MODS}\" folder was not found. Creating empty folder... ");
                Directory.CreateDirectory(MODS);
                Console.WriteLine("DONE!\n");
            }

            // TODO: Maybe move this to a separate "Game patches" file
            // Virtua Fighter eSports crashes when used with dinput8.dll as the ASI loader
            if (GamePath.GetGame() == Game.eve && File.Exists(DINPUT8DLL))
            {
                if (File.Exists(VERSIONDLL))
                {
                    Console.Write($"Game specific patch: Deleting {DINPUT8DLL} because {VERSIONDLL} exists...");

                    // Remove dinput8.dll
                    File.Delete(DINPUT8DLL);
                }
                else
                {
                    Console.Write($"Game specific patch: Renaming {DINPUT8DLL} to {VERSIONDLL}...");

                    // Rename dinput8.dll to version.dll to prevent the game from crashing
                    File.Move(DINPUT8DLL, VERSIONDLL);
                }

                Console.WriteLine(" DONE!\n");
            }

            // Read ini (again) to check if we should try importing the old load order file
            ini = iniParser.ReadFile(INI);

            List<ModInfo> mods = new List<ModInfo>();

            if (ShouldBeExternalOnly())
            {
                // Only load the files inside the external mods path, and ignore the load order in the txt
                mods.Add(new ModInfo(EXTERNAL_MODS));
            }
            else
            {
                if (File.Exists(TXT_OLD) && ini.GetKey("SavedSettings.ModListImported") == null)
                {
                    Console.Write("Old format load order file (" + TXT_OLD + ") was found. Importing to the new format...");

                    // Migrate old format to new
                    mods.AddRange(ConvertOldToNewModList(ReadModLoadOrderTxt(TXT_OLD)).Where(n => !mods.Any(m => m.Name == n.Name)));

                    ini.Sections.AddSection("SavedSettings");
                    ini["SavedSettings"].AddKey("ModListImported", "true");
                    iniParser.WriteFile(INI, ini);

                    Console.WriteLine("DONE!\n");

                    try
                    {
                        File.Delete(TXT_OLD);
                    }
                    catch
                    {
                        Console.WriteLine("Could not delete " + TXT_OLD + ". This file should be deleted manually.");
                    }
                }

                if (File.Exists(TXT))
                {
                    mods.AddRange(ReadModListTxt(TXT).Where(n => !mods.Any(m => m.Name == n.Name)));
                }
                else
                {
                    Console.WriteLine(TXT + " was not found. Will load all existing mods.\n");
                }

                if (Directory.Exists(MODS))
                {
                    // Add all scanned mods that have not been added to the load order yet
                    mods.AddRange(ScanMods().Where(n => !mods.Any(m => m.Name == n)).Select(m => new ModInfo(m)));
                }
            }

            return mods;
        }

        public static async Task<bool> RunGeneration(List<string> mods)
        {
            if (File.Exists(MLO))
            {
                Console.Write("Removing old MLO...");

                // Remove existing MLO file to avoid it being used if a new MLO won't be generated
                File.Delete(MLO);

                Console.WriteLine(" DONE!\n");
            }

            // Remove previously repacked pars, to avoid unwanted side effects
            Repacker.RemoveOldRepackedPars();

            if (GamePath.GetGame() != Game.Unsupported)
            {
                if (mods?.Count > 0 || looseFilesEnabled)
                {
                    await GenerateModLoadOrder(mods, looseFilesEnabled).ConfigureAwait(false);
                    return true;
                }

                Console.WriteLine("Aborting: No mods were found, and .parless paths are disabled\n");
            }
            else
            {
                Console.WriteLine("Aborting: No supported game was found in this directory\n");
            }

            return false;
        }

        public static async Task PostRun()
        {
            // Check if the ASI loader is not in the directory (possibly due to incorrect zip extraction)
            if (MissingDLL())
            {
                Console.WriteLine($"Warning: \"{DINPUT8DLL}\" is missing from this directory. RyuModManager will NOT function properly without this file\n");
            }

            // Check if the ASI is not in the directory
            if (MissingASI())
            {
                Console.WriteLine($"Warning: \"{ASI}\" is missing from this directory. RyuModManager will NOT function properly without this file\n");
            }

            // Calculate the checksum for the game's exe to inform the user if their version might be unsupported
            if (ConsoleOutput.ShowWarnings && InvalidGameExe())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Warning: Game version is unsupported. Please use the latest Steam version of the game.");
                Console.WriteLine($"RyuModManager will still generate the load order, but the game might CRASH or not function properly\n");
                Console.ResetColor();
            }

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

        public static bool ShowWarnings()
        {
            return ConsoleOutput.ShowWarnings;
        }

        public static bool MissingDLL()
        {
            return !(File.Exists(DINPUT8DLL) || File.Exists(VERSIONDLL));
        }

        public static bool MissingASI()
        {
            return !File.Exists(ASI);
        }

        public static bool InvalidGameExe()
        {
            return GetGame() != Game.Unsupported && !GameHash.ValidateFile(Path.Combine(GetGamePath(), GetGameExe()), GetGame());
        }

        /// <summary>
        /// Read the load order from ModLoadOrder.txt (old format).
        /// </summary>
        /// <param name="txt">expected to be "ModLoadOrder.txt".</param>
        /// <returns>list of strings containing mod names according to the load order in the file.</returns>
        public static List<string> ReadModLoadOrderTxt(string txt)
        {
            List<string> mods = new List<string>();

            if (!File.Exists(txt))
            {
                return mods;
            }

            StreamReader file = new StreamReader(new FileInfo(txt).FullName);

            string line;
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith(";"))
                {
                    line = line.Split(new char[] { ';' }, 1)[0];

                    // Only add existing mods that are not duplicates
                    if (line.Length > 0 && Directory.Exists(Path.Combine(MODS, line)) && !mods.Contains(line))
                    {
                        mods.Add(line);
                    }
                }
            }

            file.Close();

            return mods;
        }

        /// <summary>
        /// Read the mod list from ModList.txt (current format).
        /// </summary>
        /// <param name="txt">expected to be "ModList.txt".</param>
        /// <returns>list of ModInfo for each mod in the file.</returns>
        public static List<ModInfo> ReadModListTxt(string txt)
        {
            List<ModInfo> mods = new List<ModInfo>();

            if (!File.Exists(txt))
            {
                return mods;
            }

            StreamReader file = new StreamReader(new FileInfo(txt).FullName);

            string line = file.ReadLine();

            if (line != null)
            {
                foreach (string mod in line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (mod.StartsWith("<") || mod.StartsWith(">"))
                    {
                        ModInfo info = new ModInfo(mod.Substring(1), mod[0] == '<');

                        if (ModInfo.IsValid(info) && !mods.Contains(info))
                        {
                            mods.Add(info);
                        }
                    }
                }
            }

            file.Close();

            return mods;
        }

        public static bool WriteModListTxt(List<ModInfo> mods)
        {
            // No need to write the file if it's going to be empty
            if (mods?.Count > 0)
            {
                string content = "";

                foreach (ModInfo m in mods)
                {
                    content += "|" + (m.Enabled ? "<" : ">") + m.Name;
                }

                File.WriteAllText(TXT, content.Substring(1));

                return true;
            }

            return false;
        }

        public static List<ModInfo> ConvertOldToNewModList(List<string> mods)
        {
            return mods.Select(m => new ModInfo(m)).ToList();
        }

        public static List<string> ConvertNewToOldModList(List<ModInfo> mods)
        {
            return mods.Select(m => m.Name).ToList();
        }

        public static bool ShouldBeExternalOnly()
        {
            return externalModsOnly && Directory.Exists(GetExternalModsPath());
        }

        public static bool ShouldCheckForUpdates()
        {
            return checkForUpdates;
        }

        private static List<string> ScanMods()
        {
            return Directory.GetDirectories(GetModsPath())
                .Select(d => Path.GetFileName(d.TrimEnd(new char[] { Path.DirectorySeparatorChar })))
                .Where(m => (m != "Parless") && (m != EXTERNAL_MODS))
                .ToList();
        }

        public static async Task<Release> CheckForUpdates()
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue(REPO));
                return await client.Repository.Release.GetLatest(AUTHOR, REPO).ConfigureAwait(false);
            }
            catch
            {

            }

            return null;
        }

        private static async Task<ConsoleOutput> CheckForUpdatesCLI()
        {
            ConsoleOutput console = new ConsoleOutput();

            var latestRelease = await CheckForUpdates().ConfigureAwait(true);

            if (latestRelease != null && latestRelease.Name.Contains("Ryu Mod Manager") && latestRelease.TagName != VERSION)
            {
                console.WriteLine("New version detected!\n");
                console.WriteLine($"Current version: {VERSION}");
                console.WriteLine($"Latest version: {latestRelease.TagName}\n");

                console.WriteLine($"Please update by going to {latestRelease.HtmlUrl}");
            }
            else
            {
                console.WriteLine("Current version is up to date");
            }

            return console;
        }
    }
}
