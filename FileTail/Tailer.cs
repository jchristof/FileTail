
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileTail {
    public class Tailer {
        public Tailer(string path, string pattern, DirectoryInfo directoryInfo) {
            this.path = path;
            this.pattern = pattern;
            this.directoryInfo = directoryInfo;
        }

        public async Task<ConcurrentDictionary<string, int>> Start(FileInfo[] fileInformation) {  
            var snapShot = new List<Task>(); 
            ConcurrentDictionary<string, int> fileLines = new ConcurrentDictionary<string, int>();
            foreach (var fileInfo in fileInformation) {
                snapShot.Add(
                        Task.Factory.StartNew(
                            () => { fileLines[fileInfo.Name] = File.ReadLines(fileInfo.Name).Count(); },
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
        private readonly string path;
        private readonly string pattern;
        private readonly DirectoryInfo directoryInfo;

        public void Reconcile((FileInfo[] fileInfo, ConcurrentBag<Task<(string path, int lines)>> snapShot) results) {
            
        }
    }
}
