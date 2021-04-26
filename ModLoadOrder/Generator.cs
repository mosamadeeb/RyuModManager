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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mods">Mod names sorted in the desired order, from highest priority to lowest.</param>
        /// <param name="looseFilesEnabled"></param>
        /// <param name="verbose"></param>
        public static async Task GenerateModLoadOrder(List<string> mods, bool looseFilesEnabled, bool verbose)
        {
            List<int> modIndices = new List<int> { 0 };
            OrderedSet<string> files = new OrderedSet<string>();

            // Dictionary of PathToPar, ListOfMods
            Dictionary<string, List<string>> parDictionary = new Dictionary<string, List<string>>();

            ParlessMod loose = new ParlessMod();

            Game game = GamePath.GetGame();

            if (looseFilesEnabled)
            {
                PrintLine("text", verbose);

                loose.AddFiles(GamePath.GetDataPath(), game >= Game.Yakuza6 ? "DE" : "");

                if (game < Game.Yakuza6)
                {
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
                }
            }

            Mod mod;

            // Use a reverse loop to be able to remove items from the list when necessary
            for (int i = mods.Count - 1; i >= 0; i--)
            {
                mod = new Mod(mods[i]);
                mod.AddFiles(Path.Combine(GamePath.GetModsPath(), mods[i]), game >= Game.Yakuza6 ? "DE" : "");

                if (mod.Files.Count > 0)
                {
                    files.UnionWith(mod.Files);
                    modIndices.Add(files.Count);
                }
                else
                {
                    mods.RemoveAt(i);
                }

                if (game < Game.Yakuza6)
                {
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
            }

            // Reverse the list because the last mod in the list should have the highest priority
            mods.Reverse();

            // Generate MLO
            ModLoadOrder mlo = new ModLoadOrder(modIndices, mods, files, loose.ParlessFolders);
            mlo.WriteMLO(Path.Combine(GamePath.GetGamePath(), Constants.MLO));

            if (game < Game.Yakuza6)
            {
                // Repack pars
                await Repacker.RepackDictionary(parDictionary).ConfigureAwait(false);
            }
        }
    }
}
