using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using ParLibrary.Converter;
using Utils;

using Yarhl.FileSystem;

namespace ParRepacker
{
    public static class Repacker
    {

        public static async Task RepackDictionary(Dictionary<string, List<string>> parDictionary)
        {
            var parTasks = new List<Task<ConsoleOutput>>();

            string pathToParlessMods = Path.Combine(GamePath.GetModsPath(), "Parless");

            if (Directory.Exists(pathToParlessMods))
            {
                Console.Write("Removing old pars...");

                // Clean up the /mods/Parless/ directory before processing the dictionary
                // TODO: If .parmeta checks are implemented, this should be changed
                Directory.Delete(pathToParlessMods, true);

                Console.WriteLine(" DONE!\n");
            }

            if (parDictionary.Count == 0)
            {
                Console.WriteLine("No pars to repack\n");
            }
            else
            {
                Console.WriteLine("Repacking pars...\n");

                foreach (KeyValuePair<string, List<string>> parModPair in parDictionary)
                {
                    ConsoleOutput consoleOutput = new ConsoleOutput(2);
                    parTasks.Add(Task.Run(() => RepackPar(parModPair.Key, parModPair.Value, consoleOutput)));
                }

                while (parTasks.Count > 0)
                {
                    var console = await Task.WhenAny(parTasks).ConfigureAwait(false);

                    if (console != null)
                    {
                        console.Result.Flush();
                    }

                    parTasks.Remove(console);
                }

                Console.WriteLine($"Repacked {parDictionary.Count} par(s)!\n");
            }
        }

        private static async Task<ConsoleOutput> RepackPar(string parName, List<string> mods, ConsoleOutput console)
        {
            parName = parName.TrimStart(Path.DirectorySeparatorChar);

            string pathToPar = Path.Combine(GamePath.GetDataPath(), parName + ".par");
            string pathToModPar = Path.Combine(GamePath.GetModsPath(), "Parless", parName + ".par");
            string pathToModParMeta = pathToModPar + "meta";

            // Dictionary of fileInPar, ModName
            Dictionary<string, string> fileDict = new Dictionary<string, string>();

            console.WriteLine($"Repacking {parName + ".par"} ...");

            // Populate fileDict with the files inside each mod
            foreach (string mod in mods)
            {
                foreach (string modFile in GetModFiles(parName, mod, console))
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

                if (File.Exists(pathToModPar))
                {
                    // Make sure that the .partemp directory is empty
                    File.Delete(pathToModPar);
                }

                if (Directory.Exists(pathToTempPar))
                {
                    // Make sure that the .partemp directory is empty
                    Directory.Delete(pathToTempPar, true);
                    Directory.CreateDirectory(pathToTempPar);
                }
                else
                {
                    Directory.CreateDirectory(pathToTempPar);
                }

                // Copy each file in the mods to the .partemp directory
                foreach (KeyValuePair<string, string> fileModPair in fileDict)
                {
                    if (fileModPair.Value.StartsWith(Constants.PARLESS_NAME))
                    {
                        // 15 = ParlessMod.NAME.Length + 1
                        File.Copy(
                            Path.Combine(GamePath.GetDataPath(), parName.Insert(int.Parse(fileModPair.Value.Substring(15)) - 1, ".parless"), fileModPair.Key),
                            Path.Combine(pathToTempPar, fileModPair.Key),
                            true);
                    }
                    else
                    {
                        File.Copy(
                            GamePath.GetModPathFromDataPath(fileModPair.Value, Path.Combine(parName, fileModPair.Key)),
                            Path.Combine(pathToTempPar, fileModPair.Key),
                            true);
                    }
                }

                ParArchiveReaderParameters readerParameters = new ParArchiveReaderParameters
                {
                    Recursive = false,
                };

                ParArchiveWriterParameters writerParameters = new ParArchiveWriterParameters
                {
                    CompressorVersion = 0,
                    OutputPath = pathToModPar,
                    IncludeDots = true,

                };

                // Create a node from the .partemp directory and write the par to pathToModPar
                string nodeName = new DirectoryInfo(pathToTempPar).Name;
                Node node = ReadDirectory(pathToTempPar, nodeName);

                bool overwrite = false;

                // Store a reference to the nodes in the container to dispose of them later, as they are not disposed properly
                NodeContainerFormat containerNode = node.GetFormatAs<NodeContainerFormat>();

                Node par = NodeFactory.FromFile(pathToPar, Yarhl.IO.FileOpenMode.Read);
                par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParameters);
                writerParameters.IncludeDots = par.Children[0].Name == ".";

                node.GetFormatAs<NodeContainerFormat>().MoveChildrenTo(writerParameters.IncludeDots ? par.Children[0] : par, true);
                par.SortChildren((x, y) => string.CompareOrdinal(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant()));

                writerParameters.IncludeDots = false;

                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(pathToModPar)));
                par.TransformWith<ParArchiveWriter, ParArchiveWriterParameters>(writerParameters);
                par.Dispose();

                // TODO: Write .parmeta file here, using the containerNode

                node.Dispose();
                containerNode.Root.Dispose();

                // Remove the .partemp directory
                Directory.Delete(pathToTempPar, true);

                console.WriteLineIfVerbose();
                console.WriteLine($"Repacked {fileDict.Count} file(s) in {parName + ".par"}!");
            }

            return console;
        }

        private static List<string> GetModFiles(string par, string mod, ConsoleOutput console)
        {
            List<string> result;
            if (mod.StartsWith(Constants.PARLESS_NAME))
            {
                // Get index of ".parless" in par path
                // 15 = ParlessMod.NAME.Length + 1
                result = GetModFiles(Path.Combine(GamePath.GetDataPath(), par.Insert(int.Parse(mod.Substring(15)) - 1, ".parless")), console);
            }
            else
            {
                result = GetModFiles(GamePath.GetModPathFromDataPath(mod, par), console);
            }

            // Get file path relative to par
            return result.Select(f => f.Replace(".parless", "").Substring(f.Replace(".parless", "").IndexOf(par) + par.Length + 1)).ToList();
        }

        private static List<string> GetModFiles(string path, ConsoleOutput console)
        {
            List<string> files = new List<string>();

            // Add files in current directory
            foreach (string p in Directory.GetFiles(path).Select(f => GamePath.GetDataPathFrom(f)))
            {
                files.Add(p);
                console.WriteLineIfVerbose($"Adding file: {p}");
            }

            // Get files for all subdirectories
            foreach (string folder in Directory.GetDirectories(path))
            {
                files.AddRange(GetModFiles(folder, console));
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
