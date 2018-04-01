
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileTail {
    internal class Program {
        public static async Task Main() {
            Tailer tailer = new Tailer();
            
            await Run(tailer);
            //The program takes 2 arguments, the directory to watch and a file pattern, example: program.exe "c:\file folder" *.txt
//            var args = System.Environment.GetCommandLineArgs();
//            if (args.Length != 3) {
//                Console.WriteLine("Program needs both a directory path and filter string arguments.");
//                return;
//            }
//
//            //The path may be an absolute path, relative to the current directory, or UNC.
//            var path =
//                Directory.Exists(args[1]) ? args[1] : Directory.Exists(args[2]) ? args[2] : string.Empty;
//
//            if (string.IsNullOrEmpty(path)) {
//                Console.WriteLine("Neither argument is a valid directory.");
//                return;
//            }
//
//            var searchPattern = path == args[1] ? args[1] : args[2];
        }

        private const int TEN_SECONDS = 10000;

        private static async Task Run(Tailer tailer) {
            try {
                var directoryInfo = new DirectoryInfo(@".");
                var lastCurrentFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

                var lastResult = await tailer.CollectFileSizes(lastCurrentFiles);

                Console.WriteLine("Initial files:");
                foreach (KeyValuePair<string, int> keyValuePair in lastResult) {
                    Console.WriteLine($"{keyValuePair.Key} {keyValuePair.Value}");
                }

                while (true) {
                    await Task.Delay(TEN_SECONDS);
                    Console.Write("X");

                    //interrupt any file line counting tasks which are pending
                    tailer.Interrupt();

                    //inventory the directory again
                    var nextCurrentFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

                    //identify added or removed files
                    var addedFiles = nextCurrentFiles.Select(x => x.Name).Where(x => !lastCurrentFiles.Select(y => y.Name).Contains(x)).ToList();
                    var removedFiles = lastCurrentFiles.Select(x => x.Name).Where(x => !nextCurrentFiles.Select(y => y.Name).Contains(x)).ToList();

                    if (addedFiles.Any()) {
                        Console.WriteLine("Added");
                        foreach (string addedFile in addedFiles) {
                            Console.WriteLine($"{addedFile}");
                        }
                    }

                    if (removedFiles.Any()) {
                        Console.WriteLine("Removed");
                        foreach (string removed in removedFiles) {
                            Console.WriteLine($"{removed}");
                        }
                    }

                    //files with differing modified times
                    var changedFileInfos = Tailer.ChangedFiles(lastCurrentFiles, nextCurrentFiles);

                    //collect the file sizes of the changed files
                    var changedFilesResult = await tailer.CollectFileSizes(changedFileInfos);

                    //report file size changes
                    Tailer.Report(lastResult, changedFilesResult);

                    //identify files that were modified but couldn't be counted
                    var unaccountedForFiles = UnaccountedForFiles(changedFileInfos, changedFilesResult);

                    //next set of files to check
                    lastCurrentFiles = NextFilesToCheck(nextCurrentFiles, unaccountedForFiles);

                    //produce a current record of files and their line sizes
                    lastResult = Merge(changedFilesResult, lastResult) as ConcurrentDictionary<string, int>;
                    
                }
            }
            catch (Exception e) {
                
            }
        }

        /// <summary>
        /// Create a new list of files to check by mering all current files plus an files that weren't account for 
        /// in the previous check. FileInfo from an unaccounted for file replaces the current FileInfo for that file.
        /// </summary>
        /// <param name="currentFiles"></param>
        /// <param name="unaccountedForFiles"></param>
        /// <returns></returns>
        static FileInfo[] NextFilesToCheck(FileInfo[] currentFiles, FileInfo[] unaccountedForFiles) {
            var currentFileList = currentFiles.ToList();

            foreach (FileInfo unaccountedForFile in unaccountedForFiles) {
                var replaceFile = currentFileList.FirstOrDefault(x => x.FullName == unaccountedForFile.FullName);
                if (replaceFile != null) {
                    currentFileList.Remove(replaceFile);
                    currentFileList.Add(unaccountedForFile);
                }
            }
            return currentFileList.ToArray();
        }

        /// <summary>
        /// Find any fileInfos files that are not accounted for in finalnameAndSize
        /// </summary>
        /// <param name="fileInfos"></param>
        /// <param name="filenameAndSize"></param>
        /// <returns></returns>
        static FileInfo[] UnaccountedForFiles(FileInfo[] fileInfos, ConcurrentDictionary<string, int> filenameAndSize) {

            var unaccountedForFiles = new List<FileInfo>();

            foreach (FileInfo fileInfo in fileInfos) {
                if(!filenameAndSize.ContainsKey(fileInfo.Name))
                    unaccountedForFiles.Add(fileInfo);
            }

            return unaccountedForFiles.ToArray();
        }

        /// <summary>
        /// Merge dictionaries. When keys collide, dictA kvp wins.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictA"></param>
        /// <param name="dictB"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB){
            return new ConcurrentDictionary<TKey, TValue>(dictA.Keys.Union(dictB.Keys).ToDictionary(k => k, k => dictA.ContainsKey(k) ? dictA[k] : dictB[k]));
        }

    }
}
