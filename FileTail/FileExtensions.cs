
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileTail {
    public static class FileExtensions {
        public static FileInfo[] FilesThatDifferByDateTime(FileInfo[] oldFiles, FileInfo[] newFiles) {
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

        /// <summary>
        /// Create a new list of files to check by mering all current files plus an files that weren't account for 
        /// in the previous check. FileInfo from an unaccounted for file replaces the current FileInfo for that file.
        /// </summary>
        /// <param name="currentFiles"></param>
        /// <param name="unaccountedForFiles"></param>
        /// <returns></returns>
        public static FileInfo[] NextFilesToCheck(FileInfo[] currentFiles, FileInfo[] unaccountedForFiles) {
            var currentFileList = currentFiles.ToList();

            foreach (FileInfo unaccountedForFile in unaccountedForFiles) {
                var replaceFile = currentFileList.FirstOrDefault(x => x.FullName == unaccountedForFile.FullName);
                if (replaceFile != null) {
                    currentFileList.Remove(replaceFile);
                    currentFileList.Add(unaccountedForFile);
                }
            }
            return currentFileList.ToArray();
        }

        /// <summary>
        /// Find any fileInfos files that are not accounted for in finalnameAndSize
        /// </summary>
        /// <param name="fileInfos"></param>
        /// <param name="filenameAndSize"></param>
        /// <returns></returns>
        public static FileInfo[] UnaccountedForFiles(FileInfo[] fileInfos, ConcurrentDictionary<string, int> filenameAndSize) {

            var unaccountedForFiles = new List<FileInfo>();

            foreach (FileInfo fileInfo in fileInfos) {
                if (!filenameAndSize.ContainsKey(fileInfo.FullName))
                    unaccountedForFiles.Add(fileInfo);
            }

            return unaccountedForFiles.ToArray();
        }

        /// <summary>
        /// Merge dictionaries. When keys collide, dictA kvp wins.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictA"></param>
        /// <param name="dictB"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB) {
            return new ConcurrentDictionary<TKey, TValue>(dictA.Keys.Union(dictB.Keys).ToDictionary(k => k, k => dictA.ContainsKey(k) ? dictA[k] : dictB[k]));
        }

    }
}
