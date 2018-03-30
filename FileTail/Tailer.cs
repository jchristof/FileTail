
using System.Collections.Concurrent;
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

        public async Task<(FileInfo[] fileInfo, ConcurrentBag<Task<(string path, int lines)>> snapShot)> Start(FileInfo[] fileInformation) {  
            ConcurrentBag<Task< (string path, int lines) >> snapShot = new ConcurrentBag<Task<(string path, int lines)>>(); 

            foreach (var fileInfo in fileInformation) {
                snapShot.Add(
                        Task.Factory.StartNew(
                            () => (fileInfo.Name, File.ReadLines(fileInfo.Name).Count()),
                            cancellationTokenSource.Token
                        )
                    );
            }

            await Task.WhenAll(snapShot);
            return (fileInformation, snapShot);
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
