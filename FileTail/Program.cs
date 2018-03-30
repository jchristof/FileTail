
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
            var lastResult = await tailer.Start();

            while (true) {
                await Task.Delay(TenSeconds).ContinueWith(async task => {
                    tailer.Interrupt();

                    var newResult = await tailer.Start();
                    Reconcile(lastResult, newResult);

                    lastResult = newResult;
                });
            }
        }

        private static void Reconcile((FileInfo[] fileInfo, ConcurrentBag<Task<(string path, int lines)>> snapShot) oldResult, (FileInfo[] fileInfo, ConcurrentBag<Task<(string path, int lines)>> snapShot) newResult) {
            var addedFiles = newResult.fileInfo.Select(x => x.Name).Where(x => !oldResult.fileInfo.Select(y => y.Name).Contains(x));
            var deletedFiles = oldResult.fileInfo.Select(x => x.Name).Where(x => !newResult.fileInfo.Select(y => y.Name).Contains(x));

            var adds = addedFiles.ToList();
            var deletes = deletedFiles.ToList();

            for (int i = 0; i < oldResult.fileInfo.Length; i++) {
                var oldfile = oldResult.fileInfo[i];
                for (int j = 0; j < newResult.fileInfo.Length; j++) {
                    var newFile = newResult.fileInfo[j];

                    if (oldfile.FullName == newFile.FullName && oldfile.LastWriteTime != newFile.LastWriteTime) {

                    }
                }
            }
            Console.WriteLine($"Added {adds.Count}");
            Console.WriteLine($"Removed {deletes.Count}");
            var changedFiles = "";
        }

    }
}
