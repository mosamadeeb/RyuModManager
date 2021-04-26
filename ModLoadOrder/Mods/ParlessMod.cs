using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utils;

namespace ModLoadOrder.Mods
{
    public class ParlessMod : Mod
    {
        public const string NAME = "Parless";

        public List<(string, int)> ParlessFolders { get; }

        public ParlessMod()
            : base(NAME)
        {
            this.ParlessFolders = new List<(string, int)>();
        }

        /// <summary>
        /// Adds all files inside .parless folders in this path into the Files list.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="check"></param>
        public new void AddFiles(string path, string check)
        {
            if (string.IsNullOrEmpty(check))
            {
                check = this.CheckFolder(path);
            }

            int index = path.IndexOf(".parless");

            if (index != -1)
            {
                // Call the base class AddFiles method
                base.AddFiles(path, check);

                // Remove ".parless" from the path
                path = path.Remove(index, 8);

                // Add .parless folders to the list to make it easier to check for them in the ASI
                this.ParlessFolders.Add((GamePath.RemoveParlessPath(path), index - GamePath.GetDataPath().Length));
            }
            else
            {
                // Continue recursing until we find the next ".parless"
                foreach (string folder in Directory.GetDirectories(path))
                {
                    this.AddFiles(folder, check);
                }
            }
        }
    }
}
