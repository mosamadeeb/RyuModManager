using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
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

        public static async Task Main(string[] args)
        {
            bool looseFilesEnabled = false;
            bool checkForUpdates = true;

            Console.WriteLine($"Ryu Mod Manager CLI {VERSION}");
            Console.WriteLine($"By {AUTHOR}\n");

            if (args.Length == 0)
            {
                Console.WriteLine($"No arguments were passed. Will generate Mod Load Order and repack pars...\n");
            }

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

            Console.WriteLine("Program finished. Press any key to exit...");
            Console.ReadKey();
        }
        }
    }
}
