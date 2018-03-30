
using System;
using System.Collections.Concurrent;
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

            while (true) {
                await Task.Delay(TenSeconds).ContinueWith(async task => {
                    tailer.Interrupt();

                    var nextCurrentFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

                    var addedFiles = nextCurrentFiles.Select(x => x.Name).Where(x => !lastCurrentFiles.Select(y => y.Name).Contains(x));
                    var deletedFiles = lastCurrentFiles.Select(x => x.Name).Where(x => !nextCurrentFiles.Select(y => y.Name).Contains(x));

                    Console.WriteLine($"Added {addedFiles.Count()}");
                    Console.WriteLine($"Removed {deletedFiles.Count()}");

                    var newResult = await tailer.Start(ChangedFiles(lastCurrentFiles, nextCurrentFiles));
                    Reconcile(lastResult, newResult);

                    lastResult = newResult;
                });
            }
        }

        private static FileInfo[] ChangedFiles(FileInfo[] oldFiles, FileInfo[] newFiles) {
            return new FileInfo[0];
        }

        private static void Reconcile((FileInfo[] fileInfo, ConcurrentBag<Task<(string path, int lines)>> snapShot) oldResult, (FileInfo[] fileInfo, ConcurrentBag<Task<(string path, int lines)>> snapShot) newResult) {

            for (int i = 0; i < oldResult.fileInfo.Length; i++) {
                var oldfile = oldResult.fileInfo[i];
                for (int j = 0; j < newResult.fileInfo.Length; j++) {
                    var newFile = newResult.fileInfo[j];

                    if (oldfile.FullName == newFile.FullName && oldfile.LastWriteTime != newFile.LastWriteTime) {

                    }
                }
            }

        }

    }
}
