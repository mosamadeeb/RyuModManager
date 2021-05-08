using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModLoadOrder.Mods;
using Utils;
using ParRepacker;
using static Utils.ConsoleOutput;

namespace ModLoadOrder
{
    public static class Generator
    {
        public static async Task GenerateModLoadOrder(List<string> mods, bool looseFilesEnabled)
        {
            List<int> modIndices = new List<int> { 0 };
            OrderedSet<string> files = new OrderedSet<string>();

            // Dictionary of PathToPar, ListOfMods
            Dictionary<string, List<string>> parDictionary = new Dictionary<string, List<string>>();

            ParlessMod loose = new ParlessMod();

            Game game = GamePath.GetGame();

            if (looseFilesEnabled)
            {
                loose.AddFiles(GamePath.GetDataPath(), "");

                loose.PrintInfo();

                // Add all pars to the dictionary
                foreach (string par in loose.ParFolders)
                {
                    int index = par.IndexOf(".parless");

                    if (index != -1)
                    {
                        // Remove .parless from the par's path
                        // Since .parless loose files are processed first, we can be sure that the dictionary won't have duplicates
                        parDictionary.Add(par.Remove(index, 8), new List<string> { loose.Name + "_" + index });
                    }
                }

                Console.WriteLine($"Done reading {Constants.PARLESS_NAME}\n");
            }

            Mod mod;
            Console.WriteLine("Reading mods...\n");

            // TODO: Make mod reading async

            // Use a reverse loop to be able to remove items from the list when necessary
            for (int i = mods.Count - 1; i >= 0; i--)
            {
                mod = new Mod(mods[i]);
                mod.AddFiles(Path.Combine(GamePath.GetModsPath(), mods[i]), "");

                mod.PrintInfo();

                if (mod.Files.Count > 0 || mod.ParFolders.Count > 0)
                {
                    files.UnionWith(mod.Files);
                    modIndices.Add(files.Count);
                }
                else
                {
                    mods.RemoveAt(i);
                }

                // Add all pars to the dictionary
                foreach (string par in mod.ParFolders)
                {
                    // If a par is not in the dictionary, make a new list for it
                    if (!parDictionary.TryAdd(par, new List<string> { mod.Name }))
                    {
                        // Add the mod's name to the par's list
                        parDictionary.GetValueOrDefault(par).Add(mod.Name);
                    }
                }
            }

            Console.WriteLine($"Added {mods.Count} mod(s) and {files.Count} file(s)!\n");

            // Reverse the list because the last mod in the list should have the highest priority
            mods.Reverse();

            Console.Write($"Generating {Constants.MLO} file...");

            // Generate MLO
            ModLoadOrder mlo = new ModLoadOrder(modIndices, mods, files, loose.ParlessFolders);
            mlo.WriteMLO(Path.Combine(GamePath.GetGamePath(), Constants.MLO));

            Console.WriteLine(" DONE!\n");

            // Check if a mod has a par that will override the repacked par, and skip repacking it in that case
            int matchIndex;
            foreach (string key in parDictionary.Keys.ToList())
            {
                List<string> value = parDictionary[key];

                // Faster lookup by checking in the OrderedSet
                if (files.Contains(key + ".par"))
                {
                    // Get the mod's index from the ModLoadOrder's Files
                    matchIndex = mlo.Files.Find(f => f.Item1 == key.Replace('\\', '/') + ".par").Item2;

                    // Avoid repacking pars which exist as a file in mods that have a higher priority than the first mod in the par to be repacked
                    if (mods.IndexOf(value[0]) > matchIndex)
                    {
                        parDictionary.Remove(key);
                    }
                }
            }

            // Repack pars
            await Repacker.RepackDictionary(parDictionary).ConfigureAwait(false);
        }
    }
}
