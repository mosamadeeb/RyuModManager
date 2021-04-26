using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Yarhl.IO;
using Utils;

namespace ModLoadOrder
{
    public class ModLoadOrder
    {
        public const string MAGIC    = "_OLM"; // MLO_ but in little endian cause that's how the yakuza works
        public const uint ENDIANNESS = 0x21; // Little endian
        public const uint VERSION    = 0x010002; // 1.2
        public const uint FILESIZE   = 0x0; // Remaining faithful to RGG by adding a filesize that is not used

        public List<string> Mods;
        public List<(string, int)> Files;
        public List<(string, int)> ParlessFolders;

        public ModLoadOrder(List<int> modIndices, List<string> mods, OrderedSet<string> fileSet, List<(string, int)> parlessFolders)
        {
            List<string> files = fileSet.ToList();

            this.Mods = mods;
            this.Files = new List<(string, int)>();
            for (int i = 0; i < modIndices.Count - 1; i++)
            {
                for (int j = modIndices[i]; j < modIndices[i + 1]; j++)
                {
                    this.Files.Add((files[j].ToLower().Replace('\\', '/'), i));
                }
            }

            this.ParlessFolders = parlessFolders.Select(f => (f.Item1.ToLower().Replace('\\', '/'), f.Item2)).ToList();
        }

        public void WriteMLO(string path)
        {
            DataWriter writer = new DataWriter(new FileStream(path, FileMode.Create, FileAccess.Write));

            // Write header
            writer.Write(MAGIC, false);
            writer.Write(ENDIANNESS);
            writer.Write(VERSION);
            writer.Write(FILESIZE);

            writer.Write(0x30); // Mods start (size of header)
            writer.WriteOfType(typeof(uint), this.Mods.Count);

            writer.Write(0); // Files start (to be written later)
            writer.WriteOfType(typeof(uint), this.Files.Count);

            writer.Write(0); // Parless folders start (to be written later)
            writer.WriteOfType(typeof(uint), this.ParlessFolders.Count);

            // Align
            writer.WriteTimes(0, 8);

            // 0x0: length
            // 0x2: string
            foreach (string mod in this.Mods)
            {
                writer.WriteOfType(typeof(ushort), mod.Length + 1);
                writer.Write(mod);
            }

            long fileStartPos = writer.Stream.Position;

            // 0x0: index of mod
            // 0x2: length
            // 0x4: string
            foreach ((string file, int index) in this.Files)
            {
                writer.WriteOfType(typeof(ushort), index);
                writer.WriteOfType(typeof(ushort), file.Length + 1);
                writer.Write(file);
            }

            long parlessStartPos = writer.Stream.Position;

            // 0x0: index of .parless in string
            // 0x2: length
            // 0x4: string
            foreach ((string folder, int index) in this.ParlessFolders)
            {
                writer.WriteOfType(typeof(ushort), index);
                writer.WriteOfType(typeof(ushort), folder.Length + 1);
                writer.Write(folder);
            }

            // Write file size
            writer.Stream.Seek(0xC, SeekOrigin.Begin);
            writer.WriteOfType(typeof(uint), writer.Stream.Length);

            // Write files start position
            writer.Stream.Seek(0x18, SeekOrigin.Begin);
            writer.WriteOfType(typeof(uint), fileStartPos);

            // Write mods start position
            writer.Stream.Seek(0x20, SeekOrigin.Begin);
            writer.WriteOfType(typeof(uint), parlessStartPos);

            // Close the file stream
            writer.Stream.Dispose();
        }
    }
}
