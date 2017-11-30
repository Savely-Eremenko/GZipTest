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
            int index = 0;
            while (inputStream.Position < inputStream.Length)
            {
                if (Source.freeBlocksQueue.Count() > 0)
                {
                    int blockNumber = Source.freeBlocksQueue.Dequeue();
                    int length = (int)((inputStream.Length - inputStream.Position) < Source.blockForCompress ? inputStream.Length - inputStream.Position : Source.blockForCompress);

                    inputStream.Read(Source.dataSource[blockNumber], 0, length);
                    //index++;
                    //if (index == 10)
                    //{
                    //    Debug.WriteLine($"Reader :  {Source.freeBlocksQueue.Count()}");
                    //    Debug.WriteLine($"Compress :  {Source.compressQueue.Count()}");
                    //    Debug.WriteLine($"Writer :  {Source.compressDataInfo.Count()}");
                    //    index = 0;
                    //}

                    //Debug.WriteLine($"% :  {((double)inputStream.Position) / inputStream.Length * 100}");

                    var compressBlock = new CompressBlockInfo()
                    {
                        blockNumber = blockNumber,
                        length = length,
                        readBlockNumber = readBlockNumber++
                    };

                    Source.compressQueue.Enqueue(compressBlock);
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
