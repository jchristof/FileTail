
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileTail {
    public static class Report {

        /// <summary>
        /// Write file names and line sizes
        /// </summary>
        /// <param name="fileLineSizeCollection"></param>
        public static void FileLineCounts(IDictionary<string, int> fileLineSizeCollection) {
            Console.WriteLine("Initial files:");
            foreach (KeyValuePair<string, int> keyValuePair in fileLineSizeCollection) {
                Console.WriteLine($"{keyValuePair.Key} {keyValuePair.Value}");
            }
        }

        /// <summary>
        /// Write files added to the directory
        /// </summary>
        /// <param name="filesAdded"></param>
        public static void FilesAdded(IList<string> filesAdded) {
            if (filesAdded == null || !filesAdded.Any())
                return;


            Console.WriteLine("Added");
            foreach (string addedFile in filesAdded) {
                Console.WriteLine($"{addedFile}");
            }
        }

        /// <summary>
        /// Write files removed from the directory
        /// </summary>
        /// <param name="filesRemoved"></param>
        public static void FilesRemoved(IEnumerable<string> filesRemoved) {
            if (filesRemoved == null || !filesRemoved.Any())
                return;

            Console.WriteLine("Removed");
            foreach (string removed in filesRemoved) {
                Console.WriteLine($"{removed}");
            }
        }

        /// <summary>
        /// Write line count changes of modified files
        /// </summary>
        /// <param name="allFiles"></param>
        /// <param name="changedFiles"></param>
        public static void FileLineCountDelta(IDictionary<string, int> allFiles, IDictionary<string, int> changedFiles) {
            foreach (KeyValuePair<string, int> keyValuePair in changedFiles) {
                if (!allFiles.ContainsKey(keyValuePair.Key))
                    continue;

                var oldSize = allFiles[keyValuePair.Key];
                var newSize = keyValuePair.Value;

                var deltaSize = newSize - oldSize;

                if (deltaSize == 0)
                    return;

                Console.WriteLine($"{keyValuePair.Key} {newSize - oldSize:+0;-#}");
            }
        }
    }
}
