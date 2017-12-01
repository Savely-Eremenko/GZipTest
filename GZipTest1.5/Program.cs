using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    class Program
    {
        //static string inputName = @"F:\test\input2.mkv", zipName = @"F:\test\test.gz", outputName = @"F:\test\output.mkv" ;

        static void Main(string[] args)
        {
            for (var i = 0; i < Source.roundCount; i++)
            {
                Source.dataSource[i] = new byte[Source.blockForCompress];
            }

            if (args.Length == 3)
            {
                string inFileName = args[1].ToString();
                string outFileName = args[2].ToString();


                if (File.Exists(inFileName))
                {
                    try
                    {
                        

                        switch (args[0].ToString())
                        {
                            case "compress":
                                {
                                    Console.WriteLine("Идет архивация...");
                                    StartPocess(args[1].ToString(), args[2].ToString(), true);
                                    Console.WriteLine("Успешно.");
                                    break;
                                }
                            case "decompress":
                                {
                                    Console.WriteLine("Идут разархивация...");
                                    StartPocess(args[1].ToString(), args[2].ToString(), false);
                                    Console.WriteLine("Успешно.");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Неверно введена команда. Поддерживаются только команды \"compress\" и \"decompress\"");
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Writer.outputStream != null)
                            Writer.outputStream.Close();
                        //for (int i = 0; i < WorkingThread.tPool.Length; i++)
                        //{
                        //    if (WorkingThread.tPool[i] != null)
                        //        WorkingThread.tPool[i].Abort();
                        //}

                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Исходный файл не найден. Проверьте правильность введенного пути и названия файла.");
                }
            }
            else
            {
                Console.WriteLine("Неверный формат входных данных. Ожидается формат \"compress\\decompress [имя исходного файла] [имя результирующего файла]\"");
            }
        }

        private static void StartPocess(string inFileName, string outFileName, bool compress)
        {
            Reader reader = new Reader(inFileName);
            Writer writer = new Writer(outFileName);
            for (int i = 0; i < Source.threadCount; i++)
            {

                Thread zipThread = new Thread(new ParameterizedThreadStart(Zipper.Compress));
                zipThread.Priority = ThreadPriority.BelowNormal;
                //zipThread.Name = $"zipThread {i}";
                zipThread.Start(compress);
            }

            Thread thread = new Thread(new ParameterizedThreadStart(reader.Read));
            thread.Priority = ThreadPriority.AboveNormal;
            //thread.Name = $"readerThread";
            thread.Start(compress);
            //Thread.CurrentThread.Name = $"writerThread";
            //Thread.CurrentThread.Priority = ThreadPriority.Normal;

            Thread writerThread = new Thread(new ThreadStart(writer.Write));
            writerThread.Priority = ThreadPriority.Normal;
            writerThread.Start();

            writerThread.Join();
            //Console.WriteLine("end");
            //Console.ReadLine();
        }
    }
}


