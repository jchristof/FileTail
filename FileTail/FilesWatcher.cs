
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileTail {

    public class FilesWatcher {

        public FilesWatcher(string directory, string filter) {
            directoryInfo = new DirectoryInfo(directory);
            this.filter = filter;
        }

        private readonly string filter;
        private readonly DirectoryInfo directoryInfo;
        private FileInfo[] previousFiles;
        private FileInfo[] currentFiles;

        public void Initialize() {
            previousFiles = directoryInfo.GetFiles(filter, SearchOption.TopDirectoryOnly);
            currentFiles = (FileInfo[])previousFiles.Clone();
        }

        public FileInfo[] CurrentFileInfos => currentFiles;
        public IEnumerable<string> AddedFiles { get; private set; }
        public IEnumerable<string> RemovedFiles { get; private set; }

        /// <summary>
        /// Collect the directory's files. Update removed and added files
        /// </summary>
        public void Scan() {
            currentFiles = directoryInfo.GetFiles(filter, SearchOption.TopDirectoryOnly);

            AddedFiles = currentFiles.Select(x => x.FullName).Where(x => !previousFiles.Select(y => y.FullName).Contains(x));
            RemovedFiles = previousFiles.Select(x => x.FullName).Where(x => !currentFiles.Select(y => y.FullName).Contains(x));
        }

        /// <summary>
        /// Return a list of modified files
        /// </summary>
        /// <returns>Modified files</returns>
        public FileInfo[] ModifiedFiles() {
            var fileInfo = new List<FileInfo>();

            for (int i = 0; i < previousFiles.Length; i++) {
                var oldfile = previousFiles[i];
                for (int j = 0; j < currentFiles.Length; j++) {
                    var newFile = currentFiles[j];

                    if (oldfile.FullName == newFile.FullName && oldfile.LastWriteTime != newFile.LastWriteTime) {
                        fileInfo.Add(oldfile);
                    }
                }
            }

            fileInfo.AddRange(currentFiles.Where(x => AddedFiles.Contains(x.FullName)));

            return fileInfo.ToArray();
        }

        /// <summary>
        /// Give unaccounted for files the opportunity to be scanned again on the next collection pass
        /// </summary>
        /// <param name="unaccountedFiles"></param>
        public void UnaccountedFiles(FileInfo[] unaccountedFiles) {
            var currentFileList = currentFiles.ToList();

            foreach (FileInfo unaccountedForFile in unaccountedFiles) {
                var replaceFile = currentFileList.FirstOrDefault(x => x.FullName == unaccountedForFile.FullName);
                if (replaceFile != null) {
                    currentFileList.Remove(replaceFile);
                    currentFileList.Add(unaccountedForFile);
                }
            }
            previousFiles =  currentFileList.ToArray();
        }

    }
}
