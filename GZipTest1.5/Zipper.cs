using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest1._5
{
    static class Zipper
    {
        public static Queue<BlockInfo> gzipQueue = new Queue<BlockInfo>(Source.roundCount);
        public static EventWaitHandle queueToCompressEWH = new EventWaitHandle(false, EventResetMode.ManualReset);
        public static bool endOfRead = false;
        static object locker = new object();

        public static void Compress(object compress)
        {
            while (!endOfRead || gzipQueue.Count() > 0)
            {
                bool inside = false;
                BlockInfo blockInfo = null;

                lock (locker)
                {
                    inside = gzipQueue.Count() > 0;
                    if (inside)
                    {
                        blockInfo = gzipQueue.Dequeue();
                    }
                }
                if (inside)
                {
                    int length;
                    if ((bool)compress)
                    {
                        length = CompressBlock(blockInfo);
                    }else
                    {
                        length = DecompressBlock(blockInfo);
                    }

                    lock (locker)
                    {
                        blockInfo.compressLength = length;
                        Writer.compressDataInfo.Add(blockInfo.readBlockNumber, blockInfo);
                    }
                    Writer.endOfZip = blockInfo.originalLength != Source.blockForCompress;
                    Writer.queueToWriteEWH.Set();
                }
                else
                    queueToCompressEWH.Reset();
                queueToCompressEWH.WaitOne();
            }
        }

        static int CompressBlock(BlockInfo blockInfo)
        {
            int length;
            using (MemoryStream outStream = new MemoryStream(blockInfo.originalLength))
            {
                using (GZipStream zip = new GZipStream(outStream, CompressionMode.Compress))
                {
                    zip.Write(Source.dataSource[blockInfo.blockNumber], 0, blockInfo.originalLength);
                }
                length = (int)(outStream.ToArray().Length);
                Array.Copy(outStream.ToArray(), 0, Source.dataSource[blockInfo.blockNumber], 0, length);
            }

            BitConverter.GetBytes(length)
                        .CopyTo(Source.dataSource[blockInfo.blockNumber], 4);

            return length; 
        }

        static int DecompressBlock(BlockInfo blockInfo)
        {
            byte[] bufer = new byte[Source.blockForCompress];
            using (MemoryStream outStream = new MemoryStream(Source.dataSource[blockInfo.blockNumber]))
            {
                using (GZipStream zip = new GZipStream(outStream, CompressionMode.Decompress))
                {
                    zip.Read(bufer, 0, blockInfo.originalLength);
                }
                
                Array.Copy(bufer, 0, Source.dataSource[blockInfo.blockNumber], 0, blockInfo.originalLength);
            }
            return blockInfo.originalLength;
            
        }

    }
}
