﻿
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileTail {
    internal class Program {
        public static async Task Main() {
              
            //The program takes 2 arguments, the directory to watch and a file pattern, example: program.exe "c:\file folder" *.txt
            var args = Environment.GetCommandLineArgs();
            if (args.Length != 3) {
                Console.WriteLine("Program needs both a directory path and filter string arguments.");
                return;
            }

            //The path may be an absolute path, relative to the current directory, or UNC.
            var path =
                Directory.Exists(args[1]) ? args[1] : Directory.Exists(args[2]) ? args[2] : string.Empty;

            if (string.IsNullOrEmpty(path)) {
                Console.WriteLine("Neither argument is a valid directory.");
                return;
            }

            var searchPattern = path == args[1] ? args[2] : args[1];

            await Run(path, searchPattern);
        }

        private const int TEN_SECONDS = 10000;

        private static async Task Run(string path, string pattern) {
            try {
                Tailer tailer = new Tailer();

                var fileWatcher = new FilesWatcher(path, pattern);
                fileWatcher.Initialize();

                var lastResult = await tailer.CollectFileSizes(fileWatcher.CurrentFileInfos);

#if DEBUG
                Report.FileLineCounts(lastResult);
#endif

                while (true) {
                    await Task.Delay(TEN_SECONDS);

#if DEBUG
                    Console.WriteLine("X");
#endif
                    //interrupt any file line counting tasks which are pending
                    tailer.Interrupt();

                    //inventory the directory again
                    fileWatcher.Scan();

                    Report.FilesAdded(fileWatcher.AddedFiles.ToList());
                    Report.FilesRemoved(fileWatcher.RemovedFiles.ToList());

                    //files with differing modified times
                    var changedFileInfos = fileWatcher.ModifiedFiles();

                    //collect the file sizes of the changed files
                    var changedFilesResult = await tailer.CollectFileSizes(changedFileInfos);

                    //report file size changes
                    Report.FileLineCountDelta(lastResult, changedFilesResult);

                    //identify files that were modified but couldn't be counted
                    var unaccountedForFiles = UnaccountedForFiles(changedFileInfos, changedFilesResult);

                    //next set of files to check
                    fileWatcher.UnaccountedFiles(unaccountedForFiles);

                    //produce a current record of files and their line sizes
                    lastResult = Merge(changedFilesResult, lastResult) as ConcurrentDictionary<string, int>;
                }
            }
            catch (Exception e) {
                Console.WriteLine("File tailing failed");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Find any fileInfos files that are not accounted for in finalnameAndSize
        /// </summary>
        /// <param name="fileInfos"></param>
        /// <param name="filenameAndSize"></param>
        /// <returns></returns>
        public static FileInfo[] UnaccountedForFiles(FileInfo[] fileInfos, ConcurrentDictionary<string, int> filenameAndSize) {

            var unaccountedForFiles = new List<FileInfo>();

            foreach (FileInfo fileInfo in fileInfos) {
                if (!filenameAndSize.ContainsKey(fileInfo.FullName))
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
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB) {
            return new ConcurrentDictionary<TKey, TValue>(dictA.Keys.Union(dictB.Keys).ToDictionary(k => k, k => dictA.ContainsKey(k) ? dictA[k] : dictB[k]));
        }
    }
}
