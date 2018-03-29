
using System;
using System.IO;

namespace FileTail {
    class Program {
        static void Main() {
            //The program takes 2 arguments, the directory to watch and a file pattern, example: program.exe "c:\file folder" *.txt
            string[] args = System.Environment.GetCommandLineArgs();
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

            var filter = path == args[1] ? args[1] : args[2];

            FileSystemWatcher watcher = new FileSystemWatcher {
                Path = path,
                Filter = filter,
                //Use the modified date of the file as a trigger that the file has changed.
                NotifyFilter = NotifyFilters.LastWrite,

            };
        }
    }
}
