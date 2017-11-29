using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest1._5
{
    class Program
    {
        static string inputName = @"F:\test\input2.mkv", outputName = @"F:\test\test.gz";

        static void Main(string[] args)
        {
            for (var i = 0; i < Source.roundCount; i++)
            {
                Source.dataSource[i] = new byte[Source.blockForCompress];
            }
            Reader reader = new Reader(inputName);
            
            for (int i = 0; i < Source.threadCount; i++)
            {

                Thread zipThread = new Thread(new ThreadStart(Zipper.Compress));
                zipThread.Priority = ThreadPriority.Normal;
                zipThread.Name = $"zipThread {i}";
                zipThread.Start();
            }

            
            Thread thread = new Thread(new ThreadStart(reader.Read));
            thread.Priority = ThreadPriority.Highest;
            thread.Name = $"readerThread";
            thread.Start();
            //Thread.CurrentThread.Name = $"writerThread";
            //Thread.CurrentThread.Priority = ThreadPriority.Normal;
            Writer writ = new Writer(outputName);
            Thread writer = new Thread(new ThreadStart(writ.Write));
            writer.Priority = ThreadPriority.AboveNormal;
            writer.Start();

            thread.Join();
            Console.WriteLine("end");
            Console.ReadLine();
        }
    }
}
