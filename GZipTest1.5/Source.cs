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
        static int memorySize = ((int)Process.GetCurrentProcess().VirtualMemorySize64 * 7);
        public static int roundCount = memorySize/ (40*1024*1024);
        public static int blockForCompress = memorySize / roundCount;

        public static byte[][] dataSource = new byte[roundCount][];

        public static Queue<CompressBlockInfo> compressQueue = new Queue<CompressBlockInfo>(roundCount);
        public static Queue<int> freeBlocksQueue = new Queue<int>(roundCount);
        public static Dictionary<int, WriteCompressBlock> compressDataInfo = new Dictionary<int, WriteCompressBlock>();

        public static EventWaitHandle queueToCompressEWH = new EventWaitHandle(false, EventResetMode.ManualReset);
        public static EventWaitHandle compressQueueIsFullEWH = new EventWaitHandle(true, EventResetMode.ManualReset);
        public static EventWaitHandle queueToWriteEWH = new EventWaitHandle(false, EventResetMode.ManualReset);

        public static bool endOfRead = false;
        public static bool endOfZip = false;
    }
}
