using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    class Source
    {

        public static int threadCount = Environment.ProcessorCount > 3 ? Environment.ProcessorCount-2 : 1;
        public static int memorySize = ((int)Process.GetCurrentProcess().VirtualMemorySize64*2);
        public static int roundCount = memorySize/ (4*1024*1024);
        public static int blockForCompress = (4 * 1024 * 1024);

        public static byte[][] dataSource = new byte[roundCount][];
    }
}
