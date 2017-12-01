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
        static string inputName = @"F:\test\input1.mkv", zipName = @"F:\test\test.gz", outputName = @"F:\test\output.mkv" ;

        static void Main(string[] args)
        {
            for (var i = 0; i < Source.roundCount; i++)
            {
                Source.dataSource[i] = new byte[Source.blockForCompress];
            }
            Reader reader = new Reader(zipName);
            
            for (int i = 0; i < Source.threadCount; i++)
            {

                Thread zipThread = new Thread(new ParameterizedThreadStart(Zipper.Compress));
                zipThread.Priority = ThreadPriority.BelowNormal;
                zipThread.Name = $"zipThread {i}";
                zipThread.Start(false);
            }

            Thread thread = new Thread(new ParameterizedThreadStart(reader.Read));
            thread.Priority = ThreadPriority.AboveNormal;
            thread.Name = $"readerThread";
            thread.Start(false);
            //Thread.CurrentThread.Name = $"writerThread";
            //Thread.CurrentThread.Priority = ThreadPriority.Normal;
            Writer writ = new Writer(outputName);
            Thread writer = new Thread(new ThreadStart(writ.Write));
            writer.Priority = ThreadPriority.Normal;
            writer.Start();

            thread.Join();
            Console.WriteLine("end");
            Console.ReadLine();
        }
    }
}
