
using System.IO;

namespace FileTail {
    public class FilesWatcher {

        public FilesWatcher(string directory, string filter) {
            directoryInfo = new DirectoryInfo(directory);
            this.filter = filter;
        }

        private readonly string filter;
        private readonly DirectoryInfo directoryInfo;

    }
}
