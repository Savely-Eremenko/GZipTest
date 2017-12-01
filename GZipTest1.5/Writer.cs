using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest1._5
{
    class Writer
    {
        public static Dictionary<int, BlockInfo> compressDataInfo = new Dictionary<int, BlockInfo>();
        public static EventWaitHandle queueToWriteEWH = new EventWaitHandle(false, EventResetMode.ManualReset);
        public static bool endOfZip = false;
        FileStream outputStream;
        int writeBlockNumber = 0;
        int writeSize = 0;
        byte[] writeArray = new byte[Source.memorySize / 2];

        public Writer(string outFileName)
        {  
            //outputStream = new FileStream(outFileName, FileMode.Create);
            outputStream =new FileStream(outFileName, FileMode.Create, FileAccess.Write, FileShare.Write, Source.blockForCompress,FileOptions.Asynchronous);
        }

        public void Write()
        {
            while (!endOfZip || compressDataInfo.Count() > 0)
            {
                if (compressDataInfo.Count() > 0)
                {
                    while (compressDataInfo.ContainsKey(writeBlockNumber))
                    {
                        if ((compressDataInfo.Count() > 10))
                        {
                            while (compressDataInfo.ContainsKey(writeBlockNumber) && writeSize < writeArray.Length - Source.blockForCompress * 2)
                            {
                                Array.Copy(Source.dataSource[compressDataInfo[writeBlockNumber].blockNumber], 0, writeArray, writeSize, compressDataInfo[writeBlockNumber].compressLength);
                                writeSize += compressDataInfo[writeBlockNumber].compressLength;

                                QueueStep(writeBlockNumber);
                                writeBlockNumber++;
                            }
                            outputStream.Write(writeArray, 0, writeSize);
                            writeSize = 0;
                        }
                        else
                        {
                            outputStream.Write(Source.dataSource[compressDataInfo[writeBlockNumber].blockNumber], 0, compressDataInfo[writeBlockNumber].compressLength);
                            QueueStep(writeBlockNumber);
                            writeBlockNumber++;
                        }
                    }
        }
                else
                {
                    queueToWriteEWH.Reset();
                    queueToWriteEWH.WaitOne();
                }
            }
            outputStream.Close();
            Console.WriteLine("All is complete");
        }

        private void QueueStep(int writeBlockNumber)
        {
            Reader.freeBlocksQueue.Enqueue(compressDataInfo[writeBlockNumber].blockNumber);
            compressDataInfo.Remove(writeBlockNumber);
            Reader.queueToReadEWH.Set();
        }
    }
}

//                
