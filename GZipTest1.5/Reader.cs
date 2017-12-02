using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    class Reader
    {
        public static EventWaitHandle queueToReadEWH = new EventWaitHandle(true, EventResetMode.ManualReset);
        public static Queue<int> freeBlocksQueue = new Queue<int>(Source.roundCount);
        public static FileStream inputStream;

        int readBlockNumber = 0;

        public Reader(string inputFile)
        {
            inputStream = File.OpenRead(inputFile);
            for (int i = 0; i < Source.roundCount; i++)
            {
                freeBlocksQueue.Enqueue(i);
            }
        }

        public void Read(object compress)
        {
            while (inputStream.Position < inputStream.Length)
            {
                if (freeBlocksQueue.Count() > 0)
                {
                    if((bool)compress)
                    {
                        Zipper.gzipQueue.Enqueue(ReadToCompress(freeBlocksQueue.Dequeue()));
                    }
                    else
                    {
                        Zipper.gzipQueue.Enqueue(ReadToDecompress(freeBlocksQueue.Dequeue()));
                    }
                    Zipper.queueToCompressEWH.Set();
                }
                else
                    queueToReadEWH.Reset();
                queueToReadEWH.WaitOne();
            }
            Zipper.endOfRead = true;
            Zipper.queueToCompressEWH.Set();
            inputStream.Close();
        }
        

        private BlockInfo ReadToCompress(int blockNumber)
        {
            int length = (int)((inputStream.Length - inputStream.Position) < Source.blockForCompress ? inputStream.Length - inputStream.Position : Source.blockForCompress);

            inputStream.Read(Source.dataSource[blockNumber], 0, length);

            BlockInfo compressBlock = new BlockInfo()
            {
                blockNumber = blockNumber,
                originalLength = length,
                readBlockNumber = readBlockNumber++
            };
            return compressBlock;
        }

        private BlockInfo ReadToDecompress(int blockNumber)
        {
            inputStream.Read(Source.dataSource[blockNumber], 0, 8);

            int compressedBlockSize = (BitConverter.ToInt32(Source.dataSource[blockNumber], 4));
            inputStream.Read(Source.dataSource[blockNumber], 8, compressedBlockSize - 8);

            var BlockInfo = new BlockInfo()
            {
                blockNumber = blockNumber,
                compressLength = compressedBlockSize,
                originalLength = BitConverter.ToInt32(Source.dataSource[blockNumber], compressedBlockSize - 4),
                readBlockNumber = readBlockNumber++
            };
            return BlockInfo;
        }

    }
}
