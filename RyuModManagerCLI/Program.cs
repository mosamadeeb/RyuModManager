namespace RyuCLI
{
    using System.Collections.Generic;
    using System.IO;
    using IniParser;
    using IniParser.Model;

    using static ModLoadOrder.Constants;
    using static ModLoadOrder.Generator;
    using static Utils.GamePath;

    /// <summary>
    /// Main program.
    /// </summary>
    internal static partial class Program
    {
        // Will print info to console
        private const bool VERBOSE = true;

        private static void Main(string[] args)
        {
            bool looseFilesEnabled = false;

            if (File.Exists(INI))
            {
                IniData ini = new FileIniDataParser().ReadFile(INI);

                if (ini.TryGetKey("Overrides.LooseFilesEnabled", out string looseFiles))
                {
                    looseFilesEnabled = int.Parse(looseFiles) == 1;
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

            GenerateModLoadOrder(mods, looseFilesEnabled, VERBOSE);
        }
    }
}
