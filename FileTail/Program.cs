
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileTail {
    internal class Program {
        public static async Task Main() {
            Tailer tailer = new Tailer(".", "*.*", new DirectoryInfo("."));
            
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

                    var changedFilesResult = await tailer.Start(ChangedFiles(lastCurrentFiles, nextCurrentFiles));

                    Reconcile(lastResult, changedFilesResult);

                    lastCurrentFiles = nextCurrentFiles;
                    lastResult = Merge(changedFilesResult, lastResult);
                });
            }
        }

        public static ConcurrentDictionary<TKey, TValue> Merge<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dictA, ConcurrentDictionary<TKey, TValue> dictB){
            return new ConcurrentDictionary<TKey, TValue>(dictA.Keys.Union(dictB.Keys).ToDictionary(k => k, k => dictA.ContainsKey(k) ? dictA[k] : dictB[k]));
        }

        private static FileInfo[] ChangedFiles(FileInfo[] oldFiles, FileInfo[] newFiles) {
            var fileInfo = new List<FileInfo>();
            for (int i = 0; i < oldFiles.Length; i++) {
                var oldfile = oldFiles[i];
                for (int j = 0; j < newFiles.Length; j++) {
                    var newFile = newFiles[j];

                    if (oldfile.FullName == newFile.FullName && oldfile.LastWriteTime != newFile.LastWriteTime) {
                        fileInfo.Add(newFile);
                    }
                }
            }

            return fileInfo.ToArray();
        }

        private static void Reconcile(ConcurrentDictionary<string, int> allFilesStats, ConcurrentDictionary<string, int> changedFiles) {
            foreach (KeyValuePair<string, int> keyValuePair in changedFiles) {
                if(!allFilesStats.ContainsKey(keyValuePair.Key))
                    continue;

                var oldSize = allFilesStats[keyValuePair.Key];
                var newSize = keyValuePair.Value;

                var deltaSize = newSize - oldSize;

                if (deltaSize == 0)
                    return;

                Console.WriteLine($"{keyValuePair.Key} {newSize - oldSize:+0;-#}");
            }
        }

    }
}
