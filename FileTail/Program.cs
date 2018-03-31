
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

        private const int TenSeconds = 10000;

        private static async Task Run(Tailer tailer) {
            var directoryInfo = new DirectoryInfo(".");
            var lastCurrentFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            var lastResult = await tailer.Start(lastCurrentFiles);

            Console.WriteLine("Initial files:");
            foreach (KeyValuePair<string, int> keyValuePair in lastResult) {
                Console.WriteLine($"{keyValuePair.Key} {keyValuePair.Value}");
            }

            while (true) {
                await Task.Delay(TenSeconds).ContinueWith(async task => {
                    tailer.Interrupt();

                    var nextCurrentFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

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

                    var changedFileInfos = Tailer.ChangedFiles(lastCurrentFiles, nextCurrentFiles);
                    var changedFilesResult = await tailer.Start(changedFileInfos);

                    Tailer.Report(lastResult, changedFilesResult);

                    var unaccountedForFiles = UnaccountedForFiles(changedFileInfos, changedFilesResult);

                    lastCurrentFiles = NextFilesToCheck(nextCurrentFiles, unaccountedForFiles);

                    lastResult = Merge(changedFilesResult, lastResult) as ConcurrentDictionary<string, int>;
                });
            }
        }

        static FileInfo[] NextFilesToCheck(FileInfo[] currentFiles, FileInfo[] unaccountedForFiles) {
            return currentFiles;
        }

        static FileInfo[] UnaccountedForFiles(FileInfo[] fileInfos, ConcurrentDictionary<string, int> changedFiles) {
            var unaccountedForFiles = new List<FileInfo>();

            foreach (FileInfo fileInfo in fileInfos) {
                if(!changedFiles.ContainsKey(fileInfo.Name))
                    unaccountedForFiles.Add(fileInfo);
            }

            return unaccountedForFiles.ToArray();
        }

        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB){
            return new ConcurrentDictionary<TKey, TValue>(dictA.Keys.Union(dictB.Keys).ToDictionary(k => k, k => dictA.ContainsKey(k) ? dictA[k] : dictB[k]));
        }

    }
}
