
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileTail {
    public static class Report {

        public static void FileLineCounts(IDictionary<string, int> fileLineSizeCollection) {
            Console.WriteLine("Initial files:");
            foreach (KeyValuePair<string, int> keyValuePair in fileLineSizeCollection) {
                Console.WriteLine($"{keyValuePair.Key} {keyValuePair.Value}");
            }
        }

        public static void FilesAdded(IList<string> filesAdded) {
            if (filesAdded == null || !filesAdded.Any())
                return;


            Console.WriteLine("Added");
            foreach (string addedFile in filesAdded) {
                Console.WriteLine($"{addedFile}");
            }
        }

        public static void FilesRemoved(IEnumerable<string> filesRemoved) {
            if (filesRemoved == null || !filesRemoved.Any())
                return;

            Console.WriteLine("Removed");
            foreach (string removed in filesRemoved) {
                Console.WriteLine($"{removed}");
            }
        }

        public static void FileLineCountDelta(IDictionary<string, int> allFilesStats, IDictionary<string, int> changedFiles) {
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
