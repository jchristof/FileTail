
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileTail2 {
    class Program {
        public static async Task Main() {
            var directoryInfo = new DirectoryInfo(".");
            var lastCurrentFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            Dictionary<string, (FileInfo, int) > fileAndLines = new Dictionary<string, (FileInfo, int)>();

            while (true) {
                foreach (var lastCurrentFile in lastCurrentFiles) {
                    var fileLines = File.ReadLines(lastCurrentFile.Name).Count();
                    fileAndLines[lastCurrentFile.Name] = (lastCurrentFile, fileLines);

                    Console.WriteLine($"File name {lastCurrentFile.Name} lines: {fileLines}");
                }

                await Task.Delay(2000);

                var currentFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
               
            }
        }
    }
}
