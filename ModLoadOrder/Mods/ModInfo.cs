using System.IO;

using Utils;

namespace ModLoadOrder.Mods
{
    public class ModInfo
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public ModInfo(string name, bool enabled = true)
        {
            this.Name = name;
            this.Enabled = enabled;
        }

        public static bool IsValid(ModInfo info)
        {
            return (info != null) && Directory.Exists(Path.Combine(GamePath.MODS, info.Name));
        }
    }
}
