
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileTail {
    public class Tailer {

        /// <summary>
        /// Fetch file line count for each of the files
        /// </summary>
        /// <param name="fileInformation"></param>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, int>> CollectFileSizes(FileInfo[] fileInformation) {  
            var snapShot = new List<Task>(); 
            var fileLines = new ConcurrentDictionary<string, int>();
            foreach (var fileInfo in fileInformation) {
                snapShot.Add(
                        Task.Factory.StartNew(
                                              () => {
                                                  try {
                                                      fileLines[fileInfo.Name] = File.ReadLines(fileInfo.Name).Count();
                                                  }
                                                  catch (Exception e) {
                                                        
                                                  }
                                              },
                            cancellationTokenSource.Token
                        )
                    );
            }

            await Task.WhenAll(snapShot);
            return fileLines;
        }

        public void Interrupt() {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
        }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static FileInfo[] ChangedFiles(FileInfo[] oldFiles, FileInfo[] newFiles) {
            var fileInfo = new List<FileInfo>();
            for (int i = 0; i < oldFiles.Length; i++) {
                var oldfile = oldFiles[i];
                for (int j = 0; j < newFiles.Length; j++) {
                    var newFile = newFiles[j];

                    if (oldfile.FullName == newFile.FullName && oldfile.LastWriteTime != newFile.LastWriteTime) {
                        fileInfo.Add(oldfile);
                    }
                }
            }

            return fileInfo.ToArray();
        }

        public static void Report(IDictionary<string, int> allFilesStats, IDictionary<string, int> changedFiles) {
            foreach (KeyValuePair<string, int> keyValuePair in changedFiles) {
                if (!allFilesStats.ContainsKey(keyValuePair.Key))
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
