using System.IO;
using System.Collections.Generic;
using System.Linq;

using Utils;

namespace ModLoadOrder.Mods
{
    public class Mod
    {
        public string Name { get; set; }

        // Files that can be directly loaded from the mod path
        public List<string> Files { get; }

        // Folders that have to be repacked into pars before running the game
        public List<string> ParFolders { get; }

        public Mod(string name)
        {
            this.Name = name;
            this.Files = new List<string>();
            this.ParFolders = new List<string>();
        }

        public void AddFiles(string path, string check)
        {
            bool needsRepack = false;
            string basename = GamePath.GetBasename(path);

            // Check if this path does not need repacking
            switch (check)
            {
                case "chara":
                case "cse":
                case "map_":
                case "prep":
                case "light_anim":
                    needsRepack = GamePath.ExistsInDataAsPar(path);
                    break;
                case "effect":
                    needsRepack = basename.StartsWith("effect_always");
                    break;
                case "2dpar":
                    needsRepack = basename.StartsWith("sprite_");
                    break;
                case "pausepar":
                    needsRepack = !basename.StartsWith("pause_");
                    break;
                case "":
                    check = this.CheckFolder(basename);
                    break;
                default:
                    break;
            }

            if (needsRepack)
            {
                // Add this folder to the list of folders to be repacked and stop recursing
                this.ParFolders.Add(GamePath.GetDataPathFrom(path));
            }
            else
            {
                // Add files in current directory
                this.Files.AddRange(Directory.GetFiles(path).Select(f => GamePath.GetDataPathFrom(f)));

                // Get files for all subdirectories
                foreach (string folder in Directory.GetDirectories(path))
                {
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
