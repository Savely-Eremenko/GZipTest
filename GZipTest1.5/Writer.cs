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

        public Writer(string outFileName)
        {  
            outputStream = new FileStream(outFileName, FileMode.Create);
            //outputStream =new FileStream(outFileName, FileMode.Create, FileAccess.Write, FileShare.Write, Source.readBlockSize/2,FileOptions.Asynchronous);
        }

        public void Write()
        {
            while (!Source.endOfZip || Source.compressDataInfo.Count() > 0)
            {
                if (Source.compressDataInfo.Count() > 0)
                {
                    while (Source.compressDataInfo.ContainsKey(writeBlockNumber))
                    {
                        Debug.WriteLine($"Reader :  {Source.freeBlocksQueue.Count()}");
                        Debug.WriteLine($"Compress :  {Source.compressQueue.Count()}");
                        Debug.WriteLine($"Writer :  {Source.compressDataInfo.Count()}");

                        WriteToFile(writeBlockNumber);
                        writeBlockNumber++;
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
