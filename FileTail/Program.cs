
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

                Report.FileLineCounts(lastResult);

                while (true) {
                    await Task.Delay(TEN_SECONDS);
                    Console.Write("X");

                    //interrupt any file line counting tasks which are pending
                    tailer.Interrupt();

                    //inventory the directory again
                    var nextCurrentFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

                    //identify added or removed files
                    var filesAdded = nextCurrentFiles.Select(x => x.FullName).Where(x => !lastCurrentFiles.Select(y => y.FullName).Contains(x)).ToList();
                    var filesRemoved = lastCurrentFiles.Select(x => x.FullName).Where(x => !nextCurrentFiles.Select(y => y.FullName).Contains(x)).ToList();

                    Report.FilesAdded(filesAdded);
                    Report.FilesRemoved(filesRemoved);

                    //files with differing modified times
                    var changedFileInfos = FileExtensions.FilesThatDifferByDateTime(lastCurrentFiles, nextCurrentFiles);

                    //collect the file sizes of the changed files
                    var changedFilesResult = await tailer.CollectFileSizes(changedFileInfos);

                    //report file size changes
                    Report.FileLineCountDelta(lastResult, changedFilesResult);

                    //identify files that were modified but couldn't be counted
                    var unaccountedForFiles = FileExtensions.UnaccountedForFiles(changedFileInfos, changedFilesResult);

                    //next set of files to check
                    lastCurrentFiles = FileExtensions.NextFilesToCheck(nextCurrentFiles, unaccountedForFiles);

                    //produce a current record of files and their line sizes
                    lastResult = FileExtensions.Merge(changedFilesResult, lastResult) as ConcurrentDictionary<string, int>;
                }
            }
            catch (Exception e) {
                Console.WriteLine("File tailing failed");
                Console.WriteLine(e.Message);
            }
        }
    }
}
