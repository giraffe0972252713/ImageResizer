using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 同步進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgName + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            }
        }

        /// <summary>
        /// 非同步進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            Task[] tasks = new Task[allFiles.Count];
            var i = 0;
            foreach (var filePath in allFiles)
            {
                //var index = string.Format("{0:D2}", (i + 1));

                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                tasks[i] = Task.Run(async () =>
                {
                    //ShowDebugInfo(index, 1, String.Format("{0:D2}", Thread.CurrentThread.ManagedThreadId), ">>>>");
                    int sourceWidth = imgPhoto.Width;
                    int sourceHeight = imgPhoto.Height;

                    int destionatonWidth = (int)(sourceWidth * scale);
                    int destionatonHeight = (int)(sourceHeight * scale);

                    var processedImage = await processBitmapAsync((Bitmap)imgPhoto,
                        sourceWidth, sourceHeight,
                        destionatonWidth, destionatonHeight);
                    string destFile = Path.Combine(destPath, imgName + ".jpg");
                    //await Task.Run(() => 
                    processedImage.Save(destFile, ImageFormat.Jpeg);

                    //ShowDebugInfo(index, 1, String.Format("{0:D2}", Thread.CurrentThread.ManagedThreadId), "<<<<", "");
                });


                i++;
            }
            Task.WaitAll(tasks);
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }

        /// <summary>
        /// 針對指定圖片進行同步縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }
        /// <summary>
        /// 針對指定圖片進行非同步縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Task<Bitmap> processBitmapAsync(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return Task.FromResult(resizedbitmap);
        }


        static object __lockObj = new object();
        private static void ShowDebugInfo(string index, int trial, string tid, string sep, string result = null)
        {
            lock (__lockObj)
            {
                ConsoleColor orig = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{index}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" << ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{trial}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" >> 測試 (TID: ");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{tid}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($")");

                if (result != null)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                Console.Write($" {sep} ");

                if (result != null)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                Console.Write($"{DateTime.Now.ToString("hh:mm:ss.fff")}");

                Console.ForegroundColor = ConsoleColor.Cyan;
                if (result != null)
                {
                    Console.Write($" {result}");
                }
                Console.WriteLine();

                Console.ForegroundColor = orig;
            }
        }

    }

    public static class Tool
    {
        /// <summary>
        /// 使用異步遍歷處理數據
        /// </summary>
        /// <typeparam name="T">需要遍歷的基類</typeparam>
        /// <param name="list">集合</param>
        /// <param name="func">Lambda表達式</param>
        /// <returns></returns>
        public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        {
            foreach (T value in list)
            {

                await func(value);
            }
        }




    }

}
