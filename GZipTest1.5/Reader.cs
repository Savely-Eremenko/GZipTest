using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest1._5
{
    class Reader
    {
        public static FileStream inputStream;
        int readBlockNumber = 0;

        public Reader(string inputFile)
        {
            inputStream = File.OpenRead(inputFile);
            for (int i = 0; i < Source.roundCount; i++)
            {
                Source.freeBlocksQueue.Enqueue(i);
            }
        }

        public void Read()
        {
            while (inputStream.Position < inputStream.Length)
            {
                if (Source.freeBlocksQueue.Count() > 0)
                {
                    int blockNumber = Source.freeBlocksQueue.Dequeue();
                    int length = (int)((inputStream.Length - inputStream.Position) < Source.blockForCompress ? inputStream.Length - inputStream.Position : Source.blockForCompress);

                    inputStream.Read(Source.dataSource[blockNumber], 0, length);

                    var compressBlock = new BlockInfo()
                    {
                        blockNumber = blockNumber,
                        originalLength = length,
                        readBlockNumber = readBlockNumber++
                    };

                    Source.gzipQueue.Enqueue(compressBlock);
                    Source.queueToCompressEWH.Set();
                }
                else
                    Source.compressQueueIsFullEWH.Reset();
                Source.compressQueueIsFullEWH.WaitOne();
            }
            Source.endOfRead = true;
            inputStream.Close();
        }

        public void ReadCompressInfo()
        {
            byte[] buffer = new byte[8];
            int compressedBlockSize;

            while (inputStream.Position < inputStream.Length)
            {
                if (Source.freeBlocksQueue.Count() > 0)
                {
                    int blockNumber = Source.freeBlocksQueue.Dequeue();
                    inputStream.Read(Source.dataSource[blockNumber], 0, 8);

                    compressedBlockSize = (BitConverter.ToInt32(Source.dataSource[blockNumber], 4));
                    inputStream.Read(Source.dataSource[blockNumber], 8, compressedBlockSize - 8);
                    int daw = BitConverter.ToInt32(Source.dataSource[blockNumber], compressedBlockSize - 4);
                    var decompressBlock = new BlockInfo()
                    {
                        blockNumber = blockNumber,
                        compressLength = compressedBlockSize,
                        originalLength = BitConverter.ToInt32(Source.dataSource[blockNumber], compressedBlockSize - 4),
                        readBlockNumber = readBlockNumber++
                    };

                    Source.gzipQueue.Enqueue(decompressBlock);
                    Source.queueToCompressEWH.Set();
                }
                else
                    Source.compressQueueIsFullEWH.Reset();
                Source.compressQueueIsFullEWH.WaitOne();
            }
            Source.endOfRead = true;
            inputStream.Close();
        }   
    }
}
