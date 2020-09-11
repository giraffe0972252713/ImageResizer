using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();

            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            ImageProcess imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);

            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
            sw.Stop();

            var Orig = sw.ElapsedMilliseconds;

            Console.WriteLine($"同步(Synchronous)花費時間: {sw.ElapsedMilliseconds} ms");

            sw.Reset();
            imageProcess.Clean(destinationPath);
            sw.Start();
            imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
            sw.Stop();
            var New = sw.ElapsedMilliseconds;
            Console.WriteLine($"非同步(Asynchronous)花費時間: {sw.ElapsedMilliseconds} ms");


            Console.WriteLine("Performance ratio :" + ((Orig - New) / (double)Orig * 100).ToString("0.00") + "%");


            Console.ReadKey();
           
        }


    }
}
