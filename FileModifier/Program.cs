using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileModifier {
    class Program {
        static void Main(string[] args) {
           var fs = new FileStream(@"../../../FileTail/bin/debug/TEST.txt", FileMode.Open,
                                FileAccess.ReadWrite, FileShare.None);

            while (true) {
                
            }
        }
    }
}
