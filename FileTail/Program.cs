
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        /// Runs the file watcher and reporting
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="pattern">Filter pattern</param>
        /// <returns>An awaitable</returns>
        private static async Task Run(string path, string pattern) {
            try {
                var fileWatcher = new FilesWatcher(path, pattern);
                fileWatcher.Initialize();

                var lastResult = await CollectFileSizes(fileWatcher.CurrentFileInfos, TEN_SECONDS);

#if DEBUG
                Report.FileLineCounts(lastResult);
#endif

                while (true) {
#if DEBUG
                    Console.WriteLine("X");
#endif

                    //inventory the directory again
                    fileWatcher.Scan();

                    Report.FilesAdded(fileWatcher.AddedFiles.ToList());
                    Report.FilesRemoved(fileWatcher.RemovedFiles.ToList());

                    //files with differing modified times
                    var changedFileInfos = fileWatcher.ModifiedFiles();

                    //collect the file sizes of the changed files
                    var changedFilesResult = await CollectFileSizes(changedFileInfos, TEN_SECONDS);

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
        /// Fetch file line count for each of the files.
        /// </summary>
        /// <param name="fileInformation">List of files to collect line sizes on</param>
        /// <param name="time">Complete in this number of milliseconds</param>
        /// <returns>Scanned files and their line sizes</returns>
        public static async Task<ConcurrentDictionary<string, int>> CollectFileSizes(FileInfo[] fileInformation, int time) {
            var snapShot = new List<Task>();
            var fileLines = new ConcurrentDictionary<string, int>();

            foreach (var fileInfo in fileInformation) {
                snapShot.Add(
                    Task.Factory.StartNew(
                        () => {
                            try {
                                fileLines[fileInfo.FullName] = File.ReadLines(fileInfo.FullName).Count();
                            }
                            catch (Exception e) {
                                Debug.WriteLine("Couldn't read file lines.");
                                Debug.WriteLine(e.Message);
                            }
                        }
                 )
                );
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            await Task.WhenAny(Task.WhenAll(snapShot), Task.Delay(time));
            await Task.Delay(Math.Max(0, time - (int)sw.ElapsedMilliseconds));

            sw.Stop();

            return fileLines;
        }

        /// <summary>
        /// Find any fileInfos files that are not accounted for in finalnameAndSize
        /// </summary>
        /// <param name="fileInfos">Files that were scanned</param>
        /// <param name="filenameAndSize">Scanned files and their line sizes</param>
        /// <returns>Files unaccounted for in the name and line size collection</returns>
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
        /// <returns>Merged dictionary</returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB) {
            return new ConcurrentDictionary<TKey, TValue>(dictA.Keys.Union(dictB.Keys).ToDictionary(k => k, k => dictA.ContainsKey(k) ? dictA[k] : dictB[k]));
        }
    }
}
