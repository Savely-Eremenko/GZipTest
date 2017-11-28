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
        static object locker = new object();

        public static void Compress()
        {
            byte[] buffer = new byte[Source.blockForCompress];
            while (!Source.endOfRead || Source.compressQueue.Count() > 0)
            {
                bool inside = false;
                CompressBlockInfo blockInfo = null;

                //Debug.WriteLine($"{Thread.CurrentThread.Name}");
                lock (locker)
                {
                    inside = Source.compressQueue.Count() > 0;
                    if (inside)
                    {
                        blockInfo = Source.compressQueue.Dequeue();
                    }
                }
                if (inside)
                {
                    Source.endOfZip = CompressBlock(blockInfo, buffer);
                    Source.queueToWriteEWH.Set();
                }
                else
                    Source.queueToCompressEWH.Reset();
                Source.queueToCompressEWH.WaitOne();
            }
        }

        static bool CompressBlock(CompressBlockInfo blockInfo, byte[] buffer)
        {
            int length;
            using (MemoryStream outStream = new MemoryStream(blockInfo.length))
            {
                using (GZipStream zip = new GZipStream(outStream, CompressionMode.Compress))
                {
                    zip.Write(Source.dataSource[blockInfo.blockNumber], 0, blockInfo.length);
                }
                length = (int)(outStream.ToArray().Length);
                Array.Copy(outStream.ToArray(), 0, Source.dataSource[blockInfo.blockNumber], 0, length);
            }

            BitConverter.GetBytes(length)
                        .CopyTo(Source.dataSource[blockInfo.blockNumber], 4);

            lock(locker)
            {
                Source.compressDataInfo.Add(blockInfo.readBlockNumber, new WriteCompressBlock
                {
                    blockNumber = blockInfo.blockNumber,
                    length = length,
                    readBlockNumber = blockInfo.readBlockNumber
                });
            }
            
            if (!Source.compressDataInfo.ContainsKey(blockInfo.readBlockNumber))
                Console.WriteLine("error");

            return blockInfo.length != Source.blockForCompress;
        }
    }
}
