using System.IO;
using System.Collections.Generic;
using System.Linq;

using Utils;

namespace ModLoadOrder.Mods
{
    public class Mod
    {
        protected readonly ConsoleOutput console;

        public string Name { get; set; }

        // Files that can be directly loaded from the mod path
        public List<string> Files { get; }

        // Folders that have to be repacked into pars before running the game
        public List<string> ParFolders { get; }

        public Mod(string name, int indent = 2)
        {
            this.Name = name;
            this.Files = new List<string>();
            this.ParFolders = new List<string>();

            this.console = new ConsoleOutput(indent);
            this.console.WriteLine($"Reading directory: {name} ...");
        }

        public void PrintInfo()
        {
            this.console.WriteLineIfVerbose();

            if (this.Files.Count > 0 || this.ParFolders.Count > 0)
            {
                if (this.Files.Count > 0)
                {
                    this.console.WriteLine($"Added {this.Files.Count} file(s)");
                }

                if (this.ParFolders.Count > 0)
                {
                    this.console.WriteLine($"Added {this.ParFolders.Count} folder(s) to be repacked");
                }
            }
            else
            {
                this.console.WriteLine($"Nothing found for {this.Name}, skipping");
            }

            this.console.Flush();
        }

        public void AddFiles(string path, string check)
        {
            bool needsRepack = false;
            string basename = GamePath.GetBasename(path);

            // Check if this path does not need repacking
            switch (check)
            {
                case "chara":
                case "map_":
                    needsRepack = GamePath.ExistsInDataAsPar(path);
                    break;
                case "prep":
                case "light_anim":
                    needsRepack = GamePath.GetGame() < Game.Yakuza0 && GamePath.ExistsInDataAsPar(path);
                    break;
                case "effect":
                    needsRepack = basename.StartsWith("effect_always") && GamePath.ExistsInDataAsPar(path);
                    break;
                case "2d":
                case "cse":
                    needsRepack = (basename.StartsWith("sprite") || basename.StartsWith("pj")) && GamePath.ExistsInDataAsParNested(path);
                    break;
                case "pausepar":
                    needsRepack = !basename.StartsWith("pause") && GamePath.ExistsInDataAsPar(path);
                    break;
                case "particle":
                    if (GamePath.GetGame() >= Game.Yakuza6 && basename == "arc")
                    {
                        check = "particle/arc";
                    }
                    break;
                case "particle/arc":
                    needsRepack = GamePath.ExistsInDataAsParNested(path);
                    break;
                case "":
                    check = this.CheckFolder(basename);
                    break;
                default:
                    break;
            }

            if (needsRepack)
            {
                string dataPath = GamePath.GetDataPathFrom(path);

                // Add this folder to the list of folders to be repacked and stop recursing
                this.ParFolders.Add(dataPath);
                this.console.WriteLineIfVerbose($"Adding repackable folder: {dataPath}");
            }
            else
            {
                // Add files in current directory
                foreach (string p in Directory.GetFiles(path).Select(f => GamePath.GetDataPathFrom(f)))
                {
                    this.Files.Add(p);
                    this.console.WriteLineIfVerbose($"Adding file: {p}");
                }

                // Get files for all subdirectories
                foreach (string folder in Directory.GetDirectories(path))
                {
                    // Break an important rule in the concept of inheritance to make the program function correctly
                    if (this.GetType() == typeof(ParlessMod))
                    {
                        ((ParlessMod)this).AddFiles(folder, check);
                    }
                    else
                    {
                        this.AddFiles(folder, check);
                    }
                }
            }
        }

        protected string CheckFolder(string name)
        {
            foreach (string folder in Constants.IncompatiblePars)
            {
                if (name.StartsWith(folder))
                {
                    return folder;
                }
            }

            return "";
        }
    }
}
