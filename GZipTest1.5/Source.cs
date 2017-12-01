using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest1._5
{
    class Source
    {
        public static int threadCount = Environment.ProcessorCount -2; // количество потоков выбирается по количеству ядер процессора
        public static int memorySize = ((int)Process.GetCurrentProcess().VirtualMemorySize64);
        public static int roundCount = memorySize/ (4*1024*1024);
        public static int blockForCompress = (4 * 1024 * 1024);

        public static byte[][] dataSource = new byte[roundCount][];
    }
}
