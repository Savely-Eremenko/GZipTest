using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest1._5
{
    class Writer
    {
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

            int index = 0;
            while (!Source.endOfZip || Source.compressDataInfo.Count() > 0)
            {

                if (Source.compressDataInfo.Count() > 0)
                {
                    while (Source.compressDataInfo.ContainsKey(writeBlockNumber))
                    {
                        if ((Source.compressDataInfo.Count() > 10))
                        {
                            while (Source.compressDataInfo.ContainsKey(writeBlockNumber) && writeSize < writeArray.Length - Source.blockForCompress * 2)
                            {
                                Array.Copy(Source.dataSource[Source.compressDataInfo[writeBlockNumber].blockNumber], 0, writeArray, writeSize, Source.compressDataInfo[writeBlockNumber].length);
                                writeSize += Source.compressDataInfo[writeBlockNumber].length;

                                Source.freeBlocksQueue.Enqueue(Source.compressDataInfo[writeBlockNumber].blockNumber);
                                Source.compressDataInfo.Remove(writeBlockNumber);
                                Source.compressQueueIsFullEWH.Set();

                                index++;
                                writeBlockNumber++;
                            }
                            outputStream.Write(writeArray, 0, writeSize);
                            writeSize = 0;
                            index = 0;
                        }
                        else
                        {
                            WriteToFile(writeBlockNumber);
                            writeBlockNumber++;
                        }
                    }
        }
                else
                {
                    Source.queueToWriteEWH.Reset();
                    Source.queueToWriteEWH.WaitOne();
                }
            }
            outputStream.Close();
            Console.WriteLine("All is complete");
        }

        private void WriteToFile(int writeBlockNumber)
        {
            outputStream.Write(Source.dataSource[Source.compressDataInfo[writeBlockNumber].blockNumber], 0, Source.compressDataInfo[writeBlockNumber].length);
            Source.freeBlocksQueue.Enqueue(Source.compressDataInfo[writeBlockNumber].blockNumber);
            Source.compressDataInfo.Remove(writeBlockNumber);
            Source.compressQueueIsFullEWH.Set();
        }
    }
}

//                
