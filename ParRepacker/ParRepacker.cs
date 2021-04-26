using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using ParLibrary;
using ParLibrary.Converter;
using Utils;

using Yarhl.FileSystem;

namespace ParRepacker
{
    public static class Repacker
    {

        public static async Task RepackDictionary(Dictionary<string, List<string>> parDictionary)
        {
            var parTasks = new List<Task>();

            foreach (KeyValuePair<string, List<string>> parModPair in parDictionary)
            {
                parTasks.Add(Task.Run(() => RepackPar(parModPair.Key, parModPair.Value)));
            }

            await Task.WhenAll(parTasks).ConfigureAwait(false);
        }

        private static async Task RepackPar(string par, List<string> mods)
        {
            string pathToPar = Path.Combine(GamePath.GetDataPath(), par + ".par");
            string pathToModPar = Path.Combine(GamePath.GetModsPath(), "Parless", par + ".par");
            string pathToModParMeta = pathToModPar + "meta";

            // Dictionary of fileInPar, ModName
            Dictionary<string, string> fileDict = new Dictionary<string, string>();

            // Populate fileDict with the files inside each mod
            foreach (string mod in mods)
            {
                foreach (string modFile in GetModFiles(par, mod))
                {
                    fileDict.TryAdd(modFile, mod);
                }
            }

            int fileCount = 0;
            bool needsRepack = false;

            /*
            if (!File.Exists(pathToModParMeta))
            {
                needsRepack = true;
            }
            else
            {
                FileStream parMetaStream = new FileStream(pathToModParMeta, FileMode.Open);

                using Node parMeta = NodeFactory.FromFile(pathToModParMeta, Yarhl.IO.FileOpenMode.Read);
                parMeta.TransformWith<ParArchiveReader>();

                // Iterate on the nodes of the .parmeta
                foreach (Node node in Navigator.IterateNodes(parMeta))
                {
                    var file = node.GetFormatAs<ParFile>();

                    if (file != null)
                    {
                        // An actual file, not a folder node
                        ++fileCount;

                        if (fileDict.TryGetValue(node.Path, out string modName))
                        {
                            // Get the mod file's info to check the last modified date
                            FileInfo info = new FileInfo(GamePath.GetModPathFromDataPath(modName, node.Path));
                            info.Refresh();

                            if (!info.Exists || !file.FileDate.Equals(info.LastWriteTime))
                            {
                                // Par file last modified date does not match the new file's date, so par should be repacked
                                needsRepack = true;
                                break;
                            }
                        }
                        else
                        {
                            // Par file does not exist in new list of files, so par should be repacked
                            needsRepack = true;
                            break;
                        }
                    }
                }

                // No need to await for stream to be closed
                parMetaStream.DisposeAsync();
            }
            */

            needsRepack = true;

            if (needsRepack || fileCount != fileDict.Count)
            {
                string pathToTempPar = pathToModPar + "temp";

                // Make sure that the .partemp directory is empty
                Directory.Delete(pathToTempPar, true);

                // Copy each file in the mods to the .partemp directory
                foreach (KeyValuePair<string, string> fileModPair in fileDict)
                {
                    if (fileModPair.Value.StartsWith("Parless_"))
                    {
                        File.Copy(
                            Path.Combine(GamePath.GetDataPath(), par.Insert(int.Parse(fileModPair.Value.Substring(8)), ".parless"), fileModPair.Key),
                            Path.Combine(pathToTempPar, fileModPair.Key),
                            true);
                    }
                    else
                    {
                        File.Copy(
                            GamePath.GetModPathFromDataPath(fileModPair.Value, Path.Combine(par, fileModPair.Key)),
                            Path.Combine(pathToTempPar, fileModPair.Key),
                            true);
                    }
                }

                // Create a node from the .partemp directory and write the par to pathToModPar
                string nodeName = new DirectoryInfo(pathToTempPar).Name;
                Node node = ReadDirectory(pathToTempPar, nodeName);

                node.SortChildren((x, y) => string.CompareOrdinal(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant()));

                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(pathToModPar)));
                node.TransformWith<ParArchiveWriter, ParArchiveWriterParameters>(new ParArchiveWriterParameters {
                    CompressorVersion = 0,
                    OutputPath = pathToModPar,
                    IncludeDots = true,
                });

                // TODO: Write .parmeta file here

                node.Dispose();

                // Remove the .partemp directory
                Directory.Delete(pathToTempPar, true);
            }
        }

        private static List<string> GetModFiles(string par, string mod)
        {
            if (mod.StartsWith("Parless_"))
            {
                // Get index of ".parless" in par path
                return GetModFiles(Path.Combine(GamePath.GetDataPath(), par.Insert(int.Parse(mod.Substring(8)), ".parless")));
            }

            return GetModFiles(GamePath.GetModPathFromDataPath(mod, par));
        }

        private static List<string> GetModFiles(string path)
        {
            List<string> files = new List<string>();

            // Add files in current directory
            files.AddRange(Directory.GetFiles(path));

            // Get files for all subdirectories
            foreach (string folder in Directory.GetDirectories(path))
            {
                files.AddRange(GetModFiles(folder));
            }

            return files;
        }

        private static Node ReadDirectory(string dirPath, string nodeName = "")
        {
            dirPath = Path.GetFullPath(dirPath);

            if (string.IsNullOrEmpty(nodeName))
            {
                nodeName = Path.GetFileName(dirPath);
            }

            Node container = NodeFactory.CreateContainer(nodeName);
            var directoryInfo = new DirectoryInfo(dirPath);
            container.Tags["DirectoryInfo"] = directoryInfo;

            var files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                Node fileNode = NodeFactory.FromFile(file.FullName, Yarhl.IO.FileOpenMode.Read);
                container.Add(fileNode);
            }

            var directories = directoryInfo.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                Node directoryNode = ReadDirectory(directory.FullName);
                container.Add(directoryNode);
            }

            return container;
        }
    }
}
