
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileTail {
    public class Tailer : IDisposable {

        /// <summary>
        /// Fetch file line count for each of the files.
        /// </summary>
        /// <param name="fileInformation">List of files to inspect.</param>
        /// <returns>Collection of filenames and their lines sizes</returns>
        
        public async Task<ConcurrentDictionary<string, int>> CollectFileSizes(FileInfo[] fileInformation) {  
            var snapShot = new List<Task>(); 
            var fileLines = new ConcurrentDictionary<string, int>();

            cancellationTokenSource = new CancellationTokenSource();

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
                                              },
                            cancellationTokenSource.Token
                        )
                    );
            }

            await Task.WhenAll(snapShot);
            return fileLines;
        }

        /// <summary>
        /// Cancel any pending file reading
        /// </summary>
        public void Interrupt() {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();



        protected virtual void Dispose(bool disposing) {
            if (!disposing)
                return;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
