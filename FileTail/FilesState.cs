
using System.IO;

namespace FileTail {
    public class FilesState {
        public FilesState(FileInfo[] fileInfo) {
            this.fileInfo = fileInfo;
        }

        public void SnapShot() { }

        private readonly FileInfo[] fileInfo;
    }
}
