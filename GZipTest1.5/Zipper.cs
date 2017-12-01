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
            while (!Source.endOfRead || Source.gzipQueue.Count() > 0)
            {
                bool inside = false;
                BlockInfo blockInfo = null;

                lock (locker)
                {
                    inside = Source.gzipQueue.Count() > 0;
                    if (inside)
                    {
                        blockInfo = Source.gzipQueue.Dequeue();
                    }
                }
                if (inside)
                {
                    Source.endOfZip = CompressBlock(blockInfo);
                    Source.queueToWriteEWH.Set();
                }
                else
                    Source.queueToCompressEWH.Reset();
                Source.queueToCompressEWH.WaitOne();
            }
        }

        static bool CompressBlock(BlockInfo blockInfo)
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

            lock(locker)
            {
                Source.compressDataInfo.Add(blockInfo.readBlockNumber, new WriteCompressBlock
                {
                    blockNumber = blockInfo.blockNumber,
                    length = length,
                    readBlockNumber = blockInfo.readBlockNumber
                });
            }

            return blockInfo.originalLength != Source.blockForCompress;
        }

        static bool DecompressBlock(BlockInfo blockInfo)
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
            
            lock (locker)
            {
                Source.compressDataInfo.Add(blockInfo.readBlockNumber, new WriteCompressBlock
                {
                    blockNumber = blockInfo.blockNumber,
                    length = blockInfo.originalLength,
                    readBlockNumber = blockInfo.readBlockNumber
                });
            }

            return blockInfo.originalLength != Source.blockForCompress;
        }
    }
}
